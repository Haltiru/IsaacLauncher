// Updater.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.Diagnostics;


namespace IsaacLauncherGUI.Classes
{
    public class Updater
    {
        public Version LocalVersion { get; set; }
        public Version RemoteVersion { get; set; }

        public string GithubOwner { get; set; } = "Haltiru";
        public string GithubRepoName { get; set; } = "IsaacLauncher";
        public string downloadUrl { get; set; } = "";

        public Updater() 
        {
            LocalVersion = new Version(Application.ProductVersion);
        }

        public async Task<bool> GetRemoteVersion()
        {

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("IsaacLauncherGUI/1.0");

                string url = $"https://api.github.com/repos/{GithubOwner}/{GithubRepoName}/releases/latest";

                try
                {
                    string jsonResponse = await client.GetStringAsync(url);
                    if (jsonResponse != null)
                    {
                        using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                        {
                            JsonElement root = doc.RootElement;
                            string tagName = root.GetProperty("tag_name").GetString().TrimStart('v');
                            RemoteVersion = new Version(tagName);

                            JsonElement assets = root.GetProperty("assets");
                            foreach (JsonElement asset in assets.EnumerateArray())
                            {
                                string name = asset.GetProperty("name").GetString();
                                if (name != null && name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                                {
                                    downloadUrl = asset.GetProperty("browser_download_url").GetString();
                                    break;
                                }
                            }

                            if (string.IsNullOrEmpty(downloadUrl) && assets.GetArrayLength() > 0)
                            {
                                downloadUrl = assets[0].GetProperty("browser_download_url").GetString();
                            }

                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }

                }
                catch { return false; }
            }
        }

        public bool IsUpdateAvailable()
        {
            return RemoteVersion > LocalVersion;
        }

        public async Task DownloadNewVersion()
        {
            string tempPath = Path.GetTempPath();
            string tempZipPath = Path.Combine(Path.GetTempPath(), "IsaacLauncher_Update.zip");
            using (HttpClient client = new HttpClient())
            {
                byte[] zipBytes = await client.GetByteArrayAsync(downloadUrl);
                await Task.Run(() => File.WriteAllBytes(tempZipPath, zipBytes));
            }
        }

        public async Task ExtractNewVersion()
        {
            string tempZipPath = Path.Combine(Path.GetTempPath(), "IsaacLauncher_Update.zip");
            string extractPath = Path.Combine(Path.GetTempPath(), "IsaacLauncher_Update");
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }
            await Task.Run(() => ZipFile.ExtractToDirectory(tempZipPath, extractPath));
        }

        public async Task RenameFilesToOld()
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string[] files = Directory.GetFiles(currentPath, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (file.EndsWith(".exe") || file.EndsWith(".dll"))
                {
                    string newFileName = file + ".old";
                    File.Move(file, newFileName);
                }
            }
        }

        public async Task CopyNewFilesToCurrent()
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string extractPath = Path.Combine(Path.GetTempPath(), "IsaacLauncher_Update");
            string[] files = Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string relativePath = Path.GetFullPath(file).Substring(Path.GetFullPath(extractPath).Length + 1);
                string destinationPath = Path.Combine(currentPath, relativePath);
                string destinationDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }
                File.Copy(file, destinationPath, true);
            }
        }

        public void StartNewVersion()
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = Path.Combine(currentPath, "IsaacLauncherGUI.exe");
            ProcessStartInfo Start = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = "--update",
                UseShellExecute = true,
            };
            Process.Start(Start);
        }

        public void CloseCurrentVersion()
        {
            Application.Exit();
        }

        public async Task StartUpdateProcess()
        {
            await DownloadNewVersion();
            await ExtractNewVersion();
            StartNewVersion();
            CloseCurrentVersion();

        }



        public async Task FinishUpdateProcess()
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (string oldFile in Directory.GetFiles(currentPath, "*.old", SearchOption.AllDirectories))
            {
                try { File.Delete(oldFile); } catch { }
            }
            await RenameFilesToOld();
            await CopyNewFilesToCurrent();
        }
    }
}
