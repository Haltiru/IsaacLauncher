//Program.cs
using IsaacLauncherGUI.Classes;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IsaacLauncherGUI
{
    public class Launcher
    {
        public enum LaunchMode { Steam, Custom, Portable }
        public LaunchMode CurrentMode { get; set; }
        public bool AreModsEnabled { get; set; }
        public bool IsRepentogonEnabled { get; set; }
        public string GamePath { get; set; }
        public string RepentogonPath { get; set; }
        public string IniPath { get; set; }
        public string MyGames { get; set; }
        public string RepPlus { get; set; }
        public bool DoesMyGamesExist { get; set; }
        public bool DoesRepPlusExist { get; set; }
        public bool IsSmartStart { get; set; }


        public Launcher(LaunchMode currentMode, bool areModsEnabled, string gamePath, bool isRepentogonEnabled, string repentogonPath, bool isSmartStart)
        {
            string user = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string correctPath = Path.Combine(user, "My Games", "Binding of Isaac Repentance+", "options.ini");
            AreModsEnabled = areModsEnabled;
            IsSmartStart = isSmartStart;
            GamePath = gamePath;
            IniPath = correctPath;
            CurrentMode = currentMode;
            IsRepentogonEnabled = isRepentogonEnabled;
            RepentogonPath = repentogonPath;
            MyGames = Path.Combine(user, "My Games");
            RepPlus = Path.Combine(MyGames, "Binding of Isaac Repentance+");
            DoesMyGamesExist = Directory.Exists(MyGames);
            DoesRepPlusExist = Directory.Exists(RepPlus);
        }

        public bool IsSteamProcessRunning()
        {
            Process[] steamProcesses = Process.GetProcessesByName("steam");
            return steamProcesses.Length > 0;
        }

        private bool IsSteamReady()
        {
            // 1. Csekkoljuk, hogy létezik-e egyáltalán a steam processz a gépén
            if (!IsSteamProcessRunning())
            {
                return false;
            }

            // 2. Kiolvassuk az ActiveUser értéket a registry-ből (alapértelmezett 0, ha nincs)
            object activeUserObj = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam\ActiveProcess", "ActiveUser", 0);

            // 3. Biztonságos castolás: kezeljük a null-t és az esetleges string/int konverziót
            int activeUser = 0;
            if (activeUserObj != null)
            {
                // A TryParse megvéd attól, hogy elszálljon, ha valami fura típust ad vissza
                int.TryParse(activeUserObj.ToString(), out activeUser);
            }

            // 4. Akkor kész a Steam, ha a processz is fut ÉS az activeUser NEM nulla!
            return activeUser != 0;
        }

        public async Task<string> EnsureSteamRunning()
        {
            if (IsSteamReady())
            {
                return null;
            }
            if (!IsSteamProcessRunning())
            {
                string steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamExe", null) as string;
                if (File.Exists(steamPath))
                {
                    await Task.Delay(3000);
                    steamPath = Path.GetFullPath(steamPath);
                    ProcessStartInfo AdminSteamStart = new ProcessStartInfo
                    {
                        FileName = steamPath,
                        Arguments = "-silent",
                        UseShellExecute = true,
                    };
                    Process.Start(AdminSteamStart);
                }
                else
                {
                    return "Steam executable not found!";
                }
            }



            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                while (!IsSteamReady() && stopwatch.ElapsedMilliseconds < 60000)
                {
                    await Task.Delay(250);
                }

                if (!IsSteamReady())
                {
                    return "Timeout: Steam didn't start in time!";
                }
            }
            catch
            {
                return "Timeout: Steam didn't start in time!";
            }
            return null;
        }

        public async Task<string> LaunchGame()
        {
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");
            switch (CurrentMode)
            {
                case LaunchMode.Steam:
                    if (IsRepentogonEnabled)
                    {
                        string isaacExe = Path.Combine(GamePath, "isaac-ng.exe");

                        if (!File.Exists(RepentogonPath)) return "RepentogonLauncher.exe not found!";
                        if (!File.Exists(isaacExe)) return "isaac-ng.exe not found in game folder!";

                        string steamErr = await EnsureSteamRunning();
                        if (steamErr != null) return steamErr;


                        ProcessStartInfo AdminStart = new ProcessStartInfo
                        {
                            FileName = $"{RepentogonPath}",
                            Arguments = $"--basement-renovator --isaac=\"{isaacExe}\"",
                            UseShellExecute = true,
                            WorkingDirectory = Path.GetDirectoryName(RepentogonPath)
                        };
                        Process.Start(AdminStart);
                    }
                    else
                    {
                        ProcessStartInfo AdminStart = new ProcessStartInfo
                        {
                            FileName = "steam://rungameid/250900",
                            UseShellExecute = true,
                        };
                        Process.Start(AdminStart);
                    }
                    break;
                case LaunchMode.Custom:
                    ProcessStartInfo AdminStartCustom = new ProcessStartInfo
                    {
                        FileName = Path.Combine(GamePath, "isaac-ng.exe"),
                        WorkingDirectory = GamePath,
                        UseShellExecute = true,
                    };
                    Process.Start(AdminStartCustom);
                    break;
                case LaunchMode.Portable:
                    ProcessStartInfo AdminStartPortable = new ProcessStartInfo
                    {
                        FileName = Path.Combine(portableDir, "isaac-ng.exe"),
                        WorkingDirectory = portableDir,
                        UseShellExecute = true,
                    };
                    Process.Start(AdminStartPortable);
                    break;
                default: return "Unknown launch mode selected!";
            }
            return null;
        }


        public void DeleteNotExistedFolders()
        {
            if (!DoesMyGamesExist && Directory.Exists(MyGames))
                Directory.Delete(MyGames, true);

            if (!DoesRepPlusExist && Directory.Exists(RepPlus))
                Directory.Delete(RepPlus, true);
        }

        public void ChangeIni(LaunchMode startMode, bool forceWindowed, AppSettings settings)
        {
            DeleteNotExistedFolders();

            string user = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string correctPath = Path.Combine(user, "My Games", "Binding of Isaac Repentance+");
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");
            if (!Directory.Exists(correctPath)) Directory.CreateDirectory(correctPath);
            switch (startMode)
            {
                case LaunchMode.Portable:
                    string portableIniPath = Path.Combine(portableDir, "options.ini");
                    if (File.Exists(portableIniPath))
                    {
                        File.Copy(portableIniPath, IniPath, true);
                    }
                    break;
                default: break;
            }
            if (!File.Exists(IniPath))
            {
                File.WriteAllBytes(IniPath, Properties.Resources.iniOptions);
            }
            string[] Inilines = File.ReadAllLines(IniPath);
            string originalFullscreen = settings.OriginalFullscreen;
            if (forceWindowed)
            {
                if (string.IsNullOrWhiteSpace(settings.OriginalFullscreen))
                {
                    foreach (string line in Inilines)
                    {
                        if (line.StartsWith("Fullscreen="))
                        {
                            originalFullscreen = line.Split('=')[1];
                            settings.OriginalFullscreen = originalFullscreen;
                            AppConfig.Save(settings);
                            break;
                        }
                    }
                }
                else
                {
                    originalFullscreen = settings.OriginalFullscreen;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(settings.OriginalFullscreen))
                {
                    originalFullscreen = settings.OriginalFullscreen;
                    settings.OriginalFullscreen = string.Empty;
                    AppConfig.Save(settings);

                }
            }

            for (int i = 0; i < Inilines.Length; i++)
            {
                if (Inilines[i].StartsWith("EnableMods="))
                {
                    if (AreModsEnabled == true) Inilines[i] = "EnableMods=1";
                    else Inilines[i] = "EnableMods=0";
                }
                if (Inilines[i].StartsWith("Fullscreen="))

                {
                    if (forceWindowed == true) Inilines[i] = "Fullscreen=0";
                    else if (forceWindowed == false && !string.IsNullOrWhiteSpace(originalFullscreen))
                    {
                        Inilines[i] = $"Fullscreen={originalFullscreen}";
                    }
                }
            }

            File.WriteAllLines(IniPath, Inilines);
        }

        public void StreakChecker(AppSettings settings)
        {
            DateTime lastRun = DateTime.TryParse(settings.LastRun, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime content) ? content : DateTime.MinValue;
            DateTime today = DateTime.Now.Date;
            DateTime yesterday = today.AddDays(-1);
            if ((content.Date == yesterday) && !string.IsNullOrWhiteSpace(settings.LastRun))
            {
                settings.Streak++;
            }
            else if ((content.Date < yesterday) && !string.IsNullOrWhiteSpace(settings.LastRun))
            {
                settings.Streak = 1;
            }
            else if ((content.Date == today) && !string.IsNullOrWhiteSpace(settings.LastRun))
            {
                settings.LastRun = DateTime.Now.ToString("o");
                AppConfig.Save(settings);
                return;
            }
            else if (string.IsNullOrWhiteSpace(settings.LastRun))
            {
                settings.Streak = 1;

            }
            settings.LastRun = DateTime.Now.ToString("o");
            AppConfig.Save(settings);
            return;
        }

        public void SmartStart(bool areModsEnabled, bool isRepentogonEnabled, AppSettings settings)
        {
            DateTime lastReset = DateTime.Now.Hour >= 12
             ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0)
             : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0).AddDays(-1);
            if (!string.IsNullOrWhiteSpace(settings.LastRun) && DateTime.TryParse(settings.LastRun, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime content))
            {
                if (content < lastReset)
                {
                    // If the last run was before today, disable mods and launch the game
                    AreModsEnabled = false;
                    IsRepentogonEnabled = false;
                    settings.LastRun = DateTime.Now.ToString("o");
                    settings.DailyStreak++;
                    AppConfig.Save(settings);
                }
                // If the last run was today, enable mods and launch the game
                else
                {
                    AreModsEnabled = areModsEnabled;
                    IsRepentogonEnabled = isRepentogonEnabled;
                }
            }
            else
            {
                settings.LastRun = DateTime.Now.ToString("o");
                settings.DailyStreak = 1;
                AppConfig.Save(settings);
                AreModsEnabled = false;
                IsRepentogonEnabled = false;
            }
        }

        public async Task<string> InitializeAndLaunch(bool fullscreenEnabled, bool areModsEnabled, bool isRepentogonEnabled, AppSettings settings)
        {
            if (IsSmartStart)
            {
                if (CurrentMode == LaunchMode.Steam)
                {
                    string steamErr = await EnsureSteamRunning();
                    if (steamErr != null) return steamErr;
                }
                SmartStart(AreModsEnabled, IsRepentogonEnabled, settings);
            }
            StreakChecker(settings);
            ChangeIni(CurrentMode, fullscreenEnabled, settings);
            return await LaunchGame();
        }
    }

    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--update")
            {
                try
                {
                    if (args.Length != 4 || !int.TryParse(args[1], out int originalPid) || originalPid <= 0)
                        throw new InvalidOperationException("Invalid updater arguments.");

                    try
                    {
                        using (Process original = Process.GetProcessById(originalPid))
                        {
                            if (!original.WaitForExit(60000))
                                throw new TimeoutException("The original launcher did not close within 60 seconds.");
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Original process has already exited.
                    }

                    string installDirectory = Path.GetFullPath(args[2]);
                    string updateDirectory = Path.GetFullPath(args[3]);
                    new Updater(installDirectory, updateDirectory)
                        .FinishUpdateProcess().GetAwaiter().GetResult();

                    string updatedLauncher = Path.Combine(installDirectory, "IsaacLauncherGUI.exe");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = updatedLauncher,
                        WorkingDirectory = installDirectory,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The update failed.\n\n" + ex,
                        "IsaacLauncher updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // Clean up old temporary updater copies; do not delete arbitrary .old files.
            Updater.CleanupOldWork();

            if (!IsRunningAsAdmin())
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = Application.ExecutablePath,
                        Verb = "runas"
                    });
                    return;
                }
                catch
                {
                    // User declined elevation; run normally.
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new IsaacLauncher());
        }

        private static bool IsRunningAsAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
