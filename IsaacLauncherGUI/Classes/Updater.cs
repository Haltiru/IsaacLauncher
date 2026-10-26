// Updater.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IsaacLauncherGUI.Classes
{
    public class Updater
    {
        private const string MainExe = "IsaacLauncherGUI.exe";
        private const long MaxUnpackedBytes = 512L * 1024 * 1024;
        private static readonly string[] ProtectedFolders =
            { "portable-saac", "Backups", "Saves", "UserData", ".update-backup" };

        private readonly string installPath;
        private readonly string workPath;
        private readonly List<string> createdFiles = new List<string>();

        public Version LocalVersion { get; set; }
        public Version RemoteVersion { get; set; }
        public string GithubOwner { get; set; } = "Haltiru";
        public string GithubRepoName { get; set; } = "IsaacLauncher";
        public string downloadUrl { get; set; } = "";
        public string BackupPath { get; private set; }

        public Updater() : this(AppDomain.CurrentDomain.BaseDirectory, null) { }

        // The helper runs from %TEMP%, but edits the real installation directory.
        public Updater(string installationDirectory, string updateDirectory)
        {
            installPath = Path.GetFullPath(installationDirectory);
            workPath = updateDirectory == null
                ? Path.Combine(Path.GetTempPath(), "IsaacLauncher_Update_" + Guid.NewGuid().ToString("N"))
                : Path.GetFullPath(updateDirectory);
            LocalVersion = new Version(Application.ProductVersion);
        }

        private string ZipPath => Path.Combine(workPath, "package.zip");
        private string ExtractPath => Path.Combine(workPath, "extracted");

        public async Task<bool> GetRemoteVersion()
        {
            downloadUrl = "";
            RemoteVersion = null;
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("IsaacLauncherGUI/1.0");
                    string url = "https://api.github.com/repos/" + GithubOwner + "/" + GithubRepoName + "/releases/latest";
                    string json = await client.GetStringAsync(url);
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        JsonElement root = doc.RootElement;
                        string tag = root.GetProperty("tag_name").GetString();
                        Version version;
                        if (string.IsNullOrWhiteSpace(tag) ||
                            !Version.TryParse(tag.TrimStart('v', 'V', '.'), out version)) return false;

                        foreach (JsonElement asset in root.GetProperty("assets").EnumerateArray())
                        {
                            string name = asset.GetProperty("name").GetString();
                            if (name != null && name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                            {
                                string candidate = asset.GetProperty("browser_download_url").GetString();
                                Uri uri;
                                if (!Uri.TryCreate(candidate, UriKind.Absolute, out uri) ||
                                    uri.Scheme != Uri.UriSchemeHttps) continue;
                                downloadUrl = candidate;
                                RemoteVersion = version;
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException ||
                                        ex is JsonException || ex is KeyNotFoundException ||
                                        ex is InvalidOperationException || ex is ArgumentException)
            {
                return false;
            }
            return false;
        }

        public bool IsUpdateAvailable() => RemoteVersion != null && RemoteVersion > LocalVersion;

        public bool IsProtectedPath(string relativePath)
        {
            string[] parts = relativePath.Replace('\\', '/').Split('/');
            return parts.Any(part => ProtectedFolders.Any(folder =>
                string.Equals(part, folder, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool IsBelow(string root, string path)
        {
            string prefix = Path.GetFullPath(root).TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
            return Path.GetFullPath(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        private string CheckedDestination(string relative)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || IsProtectedPath(relative))
                throw new InvalidOperationException("Unsafe update path: " + relative);
            string destination = Path.GetFullPath(Path.Combine(installPath, relative));
            if (File.Exists(destination) && (File.GetAttributes(destination) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException("Update targets a symbolic link: " + relative);
            if (!IsBelow(installPath, destination))
                throw new InvalidOperationException("Path escapes installation directory: " + relative);
            // Avoid traversing junctions / symlinks that point outside the installation.
            string parent = Path.GetDirectoryName(destination);
            while (IsBelow(installPath, parent))
            {
                if (Directory.Exists(parent) &&
                    (File.GetAttributes(parent) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidOperationException("Update crosses a junction: " + relative);
                parent = Path.GetDirectoryName(parent);
            }
            return destination;
        }

        public async Task DownloadNewVersion()
        {
            Uri uri;
            if (!Uri.TryCreate(downloadUrl, UriKind.Absolute, out uri) || uri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("Missing or insecure update URL.");
            Directory.CreateDirectory(workPath);
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("IsaacLauncherGUI/1.0");
                using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    using (var source = await response.Content.ReadAsStreamAsync())
                    using (var target = new FileStream(ZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[81920];
                        long total = 0;
                        int count;
                        while ((count = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            total += count;
                            if (total > MaxUnpackedBytes)
                                throw new InvalidOperationException("Update ZIP is too large.");
                            await target.WriteAsync(buffer, 0, count);
                        }
                    }
                }
            }
        }

        public async Task ExtractNewVersion()
        {
            if (!File.Exists(ZipPath)) throw new FileNotFoundException("Update ZIP missing.", ZipPath);
            if (Directory.Exists(ExtractPath)) Directory.Delete(ExtractPath, true);
            Directory.CreateDirectory(ExtractPath);
            await Task.Run(() =>
            {
                using (var archive = ZipFile.OpenRead(ZipPath))
                {
                    long extractedBytes = 0;
                    var duplicates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (((entry.ExternalAttributes >> 16) & 0xF000) == 0xA000)
                            throw new InvalidOperationException("ZIP contains a symbolic link: " + entry.FullName);
                        string relative = entry.FullName.Replace('/', '\\');
                        if (string.IsNullOrEmpty(relative)) continue;
                        bool isDirectory = relative.EndsWith("\\", StringComparison.Ordinal);
                        string normalized = relative.TrimEnd('\\');
                        if (string.IsNullOrEmpty(normalized)) continue;
                        if (!duplicates.Add(normalized))
                            throw new InvalidOperationException("Duplicate ZIP entry: " + relative);
                        string destination = CheckedDestination(relative.TrimEnd('\\'));
                        // CheckedDestination validates the install-root equivalent; extract separately.
                        string extractedFile = Path.GetFullPath(Path.Combine(ExtractPath, normalized));
                        if (!IsBelow(ExtractPath, extractedFile))
                            throw new InvalidOperationException("ZIP escapes extraction directory: " + relative);
                        if (entry.Length > MaxUnpackedBytes - extractedBytes)
                            throw new InvalidOperationException("ZIP exceeds maximum expanded size.");
                        extractedBytes += entry.Length;
                        if (isDirectory)
                        {
                            Directory.CreateDirectory(extractedFile);
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(extractedFile));
                            entry.ExtractToFile(extractedFile, false);
                        }
                    }
                }
            });
            ValidateUpdateFiles();
        }

        public void ValidateUpdateFiles()
        {
            if (!Directory.Exists(ExtractPath))
                throw new DirectoryNotFoundException("Extracted update is missing.");
            if (!File.Exists(Path.Combine(ExtractPath, MainExe)))
                throw new InvalidOperationException("Invalid update: " + MainExe + " is missing from ZIP root.");
            foreach (string file in Directory.GetFiles(ExtractPath, "*", SearchOption.AllDirectories))
            {
                string relative = file.Substring(ExtractPath.TrimEnd('\\', '/').Length + 1);
                CheckedDestination(relative);
            }
        }

        private IEnumerable<string> UpdateFiles()
        {
            return Directory.GetFiles(ExtractPath, "*", SearchOption.AllDirectories)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
        }

        public async Task RenameFilesToOld()
        {
            ValidateUpdateFiles();
            BackupPath = Path.Combine(installPath, ".update-backup", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(BackupPath);
            foreach (string file in UpdateFiles())
            {
                string relative = file.Substring(ExtractPath.TrimEnd('\\', '/').Length + 1);
                string destination = CheckedDestination(relative);
                if (!File.Exists(destination)) continue;
                string backup = Path.Combine(BackupPath, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(backup));
                // Copy, do not move: if backup creation fails, existing app remains intact.
                await Task.Run(() => File.Copy(destination, backup, false));
            }
        }

        public async Task CopyNewFilesToCurrent()
        {
            foreach (string file in UpdateFiles())
            {
                string relative = file.Substring(ExtractPath.TrimEnd('\\', '/').Length + 1);
                string destination = CheckedDestination(relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                if (!File.Exists(destination)) createdFiles.Add(destination);
                // Copy to a sibling staging file, then swap where possible.
                string staged = destination + ".updating_" + Guid.NewGuid().ToString("N");
                try
                {
                    await Task.Run(() => File.Copy(file, staged));
                    if (File.Exists(destination))
                    {
                        // Files being overwritten are already separately backed up.
                        File.Replace(staged, destination, null);
                    }
                    else File.Move(staged, destination);
                }
                finally
                {
                    if (File.Exists(staged)) File.Delete(staged);
                }
            }
        }

        public void RestoreBackup()
        {
            if (string.IsNullOrEmpty(BackupPath) || !Directory.Exists(BackupPath))
                throw new DirectoryNotFoundException("Cannot restore update: backup directory was not found.");
            // Restore older files first, keeping the backup intact for manual recovery.
            foreach (string backupFile in Directory.GetFiles(BackupPath, "*", SearchOption.AllDirectories))
            {
                string relative = backupFile.Substring(BackupPath.TrimEnd('\\', '/').Length + 1);
                string destination = CheckedDestination(relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                string staged = destination + ".rollback_" + Guid.NewGuid().ToString("N");
                try
                {
                    File.Copy(backupFile, staged);
                    if (File.Exists(destination)) File.Replace(staged, destination, null);
                    else File.Move(staged, destination);
                }
                finally { if (File.Exists(staged)) File.Delete(staged); }
            }
            foreach (string created in createdFiles)
            {
                string relative = created.Substring(installPath.TrimEnd('\\', '/').Length + 1);
                CheckedDestination(relative);
                if (File.Exists(created)) File.Delete(created);
            }
            createdFiles.Clear();
        }

        public async Task FinishUpdateProcess()
        {
            ValidateUpdateFiles();
            createdFiles.Clear();
            try
            {
                await RenameFilesToOld();
                await CopyNewFilesToCurrent();
            }
            catch (Exception updateError)
            {
                if (string.IsNullOrEmpty(BackupPath)) throw;
                try { RestoreBackup(); }
                catch (Exception rollbackError)
                {
                    throw new AggregateException("Update failed and rollback was incomplete; backups are retained.",
                        updateError, rollbackError);
                }
                throw new IOException("Update failed; previous files restored from backup.", updateError);
            }
        }

        public void StartNewVersion()
        {
            string exePath = Path.Combine(installPath, MainExe);
            if (!File.Exists(exePath)) throw new FileNotFoundException("Launcher missing", exePath);
            string helperDirectory = Path.Combine(workPath, "helper");
            Directory.CreateDirectory(helperDirectory);
            string helperExe = Path.Combine(helperDirectory, MainExe);
            File.Copy(exePath, helperExe, true);
            // Copy dependencies into helper directory to avoid locking the installed DLLs.
            foreach (string dependency in Directory.GetFiles(installPath, "*.dll", SearchOption.TopDirectoryOnly))
                File.Copy(dependency, Path.Combine(helperDirectory, Path.GetFileName(dependency)), true);
            string config = exePath + ".config";
            if (File.Exists(config)) File.Copy(config, helperExe + ".config", true);
            string runtimes = Path.Combine(installPath, "runtimes");
            if (Directory.Exists(runtimes)) CopyDirectory(runtimes, Path.Combine(helperDirectory, "runtimes"));

            var start = new ProcessStartInfo
            {
                FileName = helperExe,
                Arguments = "--update " + Process.GetCurrentProcess().Id + " \"" + installPath + "\" \"" + workPath + "\"",
                WorkingDirectory = helperDirectory,
                UseShellExecute = true
            };
            if (Process.Start(start) == null) throw new InvalidOperationException("Updater process could not start.");
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (string file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            foreach (string directory in Directory.GetDirectories(source))
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
        }

        public void CloseCurrentVersion() => Application.Exit();

        public async Task StartUpdateProcess()
        {
            await DownloadNewVersion();
            await ExtractNewVersion();
            StartNewVersion();
            CloseCurrentVersion();
        }

        public static void CleanupOldWork()
        {
            // Delete only stale work directories, never the running updater's directory.
            foreach (string directory in Directory.GetDirectories(Path.GetTempPath(), "IsaacLauncher_Update_*"))
            {
                try
                {
                    if (Directory.GetCreationTimeUtc(directory) < DateTime.UtcNow.AddDays(-7))
                        Directory.Delete(directory, true);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }
}
