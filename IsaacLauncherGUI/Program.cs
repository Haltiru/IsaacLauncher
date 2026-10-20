//Program.cs
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
        public enum LaunchMode { Steam, Cracked, Portable }
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
                case LaunchMode.Cracked:
                    ProcessStartInfo AdminStartCracked = new ProcessStartInfo
                    {
                        FileName = Path.Combine(GamePath, "isaac-ng.exe"),
                        UseShellExecute = true,
                    };
                    Process.Start(AdminStartCracked);
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
                string steamErr = await EnsureSteamRunning();
                if (steamErr != null) return steamErr;
                SmartStart(AreModsEnabled, IsRepentogonEnabled, settings);
            }
            ChangeIni(CurrentMode, fullscreenEnabled, settings);
            return await LaunchGame();
        }
    }

    class PortableIsaacLauncher
    {
        public string GamePath { get; set; }
        public string RepPlusPath { get; set; }

        public PortableIsaacLauncher(string gamePath)
        {
            string user = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string correctPath = Path.Combine(user, "My Games", "Binding of Isaac Repentance+");
            GamePath = gamePath;
            RepPlusPath = correctPath;
        }

        public string CopyBaseGameFiles(string gamePath)
        {
            string exePath = Path.Combine(gamePath, "isaac-ng.exe");
            if (!File.Exists(exePath))
            {
                return "isaac-ng.exe not found in the specified game folder!";
            }
            else
            {
                string sourcePath = gamePath;
                string destinationPath = Path.Combine(Application.StartupPath, "portable-saac");
                // Copy all files and subdirectories from source to destination
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {

                    if (dirPath.Contains("portable-saac")) continue;
                    try
                    {
                        string newDirPath = dirPath.Replace(sourcePath, destinationPath);
                        Directory.CreateDirectory(@"\\?\" + Path.GetFullPath(newDirPath));
                    }
                    catch
                    {
                        return "Failed to create directory: " + dirPath;
                    }
                }
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    if (newPath.Contains("portable-saac")) continue;
                    try
                    {
                        string whereToPaste = newPath.Replace(sourcePath, destinationPath);

                        string fromWhere = @"\\?\" + Path.GetFullPath(newPath);
                        string whereToPasteLong = @"\\?\" + Path.GetFullPath(whereToPaste);
                        File.Copy(fromWhere, whereToPasteLong, true);
                    }
                    catch
                    {
                       return "Failed to copy file: " + newPath;
                    }
                }

            }
            return null;
        }

        public string SetupSteamEmulator()
        {
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");

            string targetDllPath = Path.Combine(portableDir, "steam_api.dll");
            File.WriteAllBytes(targetDllPath, Properties.Resources.steam_api);

            string settingsDir = Path.Combine(portableDir, "steam_settings");
            Directory.CreateDirectory(settingsDir);
            File.WriteAllText(Path.Combine(settingsDir, "steam_appid.txt"), "250900");

            string modsDir = Path.Combine(settingsDir, "mods");
            Directory.CreateDirectory(modsDir);

            string iniPath = Path.Combine(settingsDir, "configs.user.ini");
            string iniContent = @"[user::saves]
                local_save_path=portable_saves";
            File.WriteAllText(iniPath, iniContent);

            string interfacesPath = Path.Combine(settingsDir, "steam_interfaces.txt");
            string interfacesContent = @"STEAMAPPS_INTERFACE_VERSION008
                                        SteamClient017
                                        SteamController008
                                        SteamFriends017
                                        SteamGameServerStats001
                                        SteamGameServer015
                                        STEAMHTMLSURFACE_INTERFACE_VERSION_005
                                        STEAMHTTP_INTERFACE_VERSION003
                                        SteamInput006
                                        STEAMINVENTORY_INTERFACE_V003
                                        SteamMatchMakingServers002
                                        SteamMatchMaking009
                                        SteamMatchGameSearch001
                                        SteamParties002
                                        STEAMMUSIC_INTERFACE_VERSION001
                                        STEAMMUSICREMOTE_INTERFACE_VERSION001
                                        SteamNetworkingMessages002
                                        SteamNetworkingSockets012
                                        SteamNetworkingUtils004
                                        SteamNetworking006
                                        STEAMPARENTALSETTINGS_INTERFACE_VERSION001
                                        STEAMREMOTEPLAY_INTERFACE_VERSION002
                                        STEAMREMOTESTORAGE_INTERFACE_VERSION016
                                        STEAMSCREENSHOTS_INTERFACE_VERSION003
                                        STEAMTIMELINE_INTERFACE_V004
                                        STEAMUGC_INTERFACE_VERSION020
                                        SteamUser023
                                        STEAMUSERSTATS_INTERFACE_VERSION013
                                        SteamUtils010
                                        STEAMVIDEO_INTERFACE_V007
                                        ";

            File.WriteAllText(interfacesPath, interfacesContent);

            string tempFolder = Path.Combine(Path.GetTempPath(), "SteamlessTemp_Isaac");
            string zipPath = Path.Combine(tempFolder, "steamless.zip");
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
            Directory.CreateDirectory(tempFolder);


            File.WriteAllBytes(zipPath, Properties.Resources.steamless);
            ZipFile.ExtractToDirectory(zipPath, tempFolder);


            string steamlessCli = Path.Combine(tempFolder, "Steamless.CLI.exe");
            string exeToCrack = Path.Combine(portableDir, "isaac-ng.exe");

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = steamlessCli,
                    Arguments = $"\"{exeToCrack}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process process = Process.Start(psi)) process.WaitForExit();

                string unpackedExe = exeToCrack + ".unpacked.exe";

                if (File.Exists(unpackedExe))
                {
                    File.Delete(exeToCrack);
                    File.Move(unpackedExe, exeToCrack);
                }

                if (Directory.Exists(tempFolder)) Directory.Delete(tempFolder, true);
            }
            catch
            {
                return "Failed to crack the game executable with Steamless!";
            }
            return null;
        }

        public string PortModsToEmulator()
        {
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");
            string settingsDir = Path.Combine(portableDir, "steam_settings");
            string modsDir = Path.Combine(settingsDir, "mods");

            string sourceModsDir = Path.Combine(GamePath, "mods");

            foreach (string currentModPath in Directory.GetDirectories(sourceModsDir))
            {
                string originalFolderName = Path.GetFileName(currentModPath);

                int lastUnderscoreIndex = originalFolderName.LastIndexOf('_');

                string modId = originalFolderName;

                if (lastUnderscoreIndex != -1)
                {
                    modId = originalFolderName.Substring(lastUnderscoreIndex + 1);
                }

                string targetModDir = Path.Combine(modsDir, modId);
                Directory.CreateDirectory(@"\\?\" + Path.GetFullPath(targetModDir));

                foreach (string dirPath in Directory.GetDirectories(currentModPath, "*", SearchOption.AllDirectories))
                {

                    if (dirPath.Contains("portable-saac")) continue;
                    try
                    {
                        string newDirPath = dirPath.Replace(currentModPath, targetModDir);
                        Directory.CreateDirectory(@"\\?\" + Path.GetFullPath(newDirPath));
                    }
                    catch
                    {
                        return "Failed to create directory: " + dirPath;
                    }
                }
                foreach (string newPath in Directory.GetFiles(currentModPath, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        string whereToPaste = newPath.Replace(currentModPath, targetModDir);

                        string fromWhere = @"\\?\" + Path.GetFullPath(newPath);
                        string whereToPasteLong = @"\\?\" + Path.GetFullPath(whereToPaste);
                        File.Copy(fromWhere, whereToPasteLong, true);
                    }
                    catch
                    {
                        return "Failed to copy file: " + newPath;
                    }
                }

            }
            return null;
        }

        public void ConfigurePortableOptions()
        {
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");

            string optionsIniPath = Path.Combine(RepPlusPath, "options.ini");
            string portableIniPath = Path.Combine(portableDir, "options.ini");

            if (File.Exists(optionsIniPath))
            {
                File.Copy(optionsIniPath, portableIniPath, true);

                string optionsContent = File.ReadAllText(portableIniPath);

                if (optionsContent.Contains("SteamCloud=0"))
                {
                    optionsContent = optionsContent.Replace("SteamCloud=0", "SteamCloud=1");
                    File.WriteAllText(portableIniPath, optionsContent);
                }
            }
            else
            {
                File.WriteAllBytes(portableIniPath, Properties.Resources.iniOptions);
            }

            string remoteSavesDir = Path.Combine(portableDir, "portable_saves", "250900", "remote");
            Directory.CreateDirectory(remoteSavesDir);

            for (int i = 1; i <= 3; i++)
            {
                string saveName = $"persistentgamedata{i}.dat";
                string repPlusSaveName = $"rep+persistentgamedata{i}.dat";

                string sourceSave = Path.Combine(RepPlusPath, saveName);

                if (File.Exists(sourceSave))
                {
                    File.Copy(sourceSave, Path.Combine(remoteSavesDir, repPlusSaveName), true);
                }
            }
        }

        public string MakeIsaacPortable()
        {
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");
            if (!Directory.Exists(portableDir)) Directory.CreateDirectory(portableDir);

            string err = CopyBaseGameFiles(GamePath);
            if (err != null) return err;

            err = SetupSteamEmulator();
            if (err != null) return err;

            err = PortModsToEmulator();
            if (err != null) return err;

            ConfigurePortableOptions();
            return null;
        }

    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Csekkoljuk, hogy adminok vagyunk-e már
            if (!IsRunningAsAdmin())
            {
                try
                {
                    // Megpróbáljuk újraindítani magunkat Admin joggal
                    ProcessStartInfo proc = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = Application.ExecutablePath,
                        Verb = "runas" // Ez kéri ki a UAC ablakot!
                    };
                    Process.Start(proc);

                    // Ha a user rányomott az "Igen"-re, a fő folyamat kilép, 
                    // mert az új, adminként indult folyamat átveszi a helyét.
                    return;
                }
                catch
                {
                    // 💀 HA A USER A "MÉGSE" / "NEM" GOMBRA KATTINTOTT:
                    // Nem történik semmi, a catch elkapja a hibát, 
                    // és a kód fut tovább sima user joggal!
                }
            }

            // Innen folytatódik a normál Form indítás
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new IsaacLauncher());
        }

        // Segédfüggvény az admin jog csekkolására
        private static bool IsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}