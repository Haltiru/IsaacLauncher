//Program.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using System.IO.Compression;

namespace IsaacLauncherGUI
{
    class Launcher
    {
        public enum LaunchMode { Steam, Cracked, Portable }
        public LaunchMode CurrentMode { get; set; }
        public bool AreModsEnabled { get; set; }
        public string GamePath { get; set; }
        public string IniPath { get; set; }


        public Launcher(LaunchMode currentMode, bool areModsEnabled, string gamePath)
        {
            string user = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string correctPath = Path.Combine(user, "My Games", "Binding of Isaac Repentance+", "options.ini");
            AreModsEnabled = areModsEnabled;
            GamePath = gamePath;
            IniPath = correctPath;
            CurrentMode = currentMode;
        } 
        public void LaunchGame()
        {
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");
            switch (CurrentMode)
            {
                case LaunchMode.Steam:
                    ProcessStartInfo AdminStart = new ProcessStartInfo
                    {
                        FileName = "steam://rungameid/250900",
                        UseShellExecute = true,
                    };
                    Process.Start(AdminStart);
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
            }
        }



        public void changeIni(LaunchMode startMode, bool enableMods, bool forceWindowed)
        {
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");
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
            
            string[] Inilines = File.ReadAllLines(IniPath);
            string originalFullscreen = "1";
            if (File.Exists("settings.txt"))
            {
                string[] settingLines = File.ReadAllLines("settings.txt");
                bool foundInSettings = false;

                foreach (string line in settingLines)
                {
                    if (line.StartsWith("originalFullscreen="))
                    {
                        originalFullscreen = line.Substring("originalFullscreen=".Length);
                        foundInSettings = true;
                        break;
                    }
                }
                if (!foundInSettings)
                {
                    foreach (string line in Inilines)
                    {
                        if (line.StartsWith("Fullscreen="))
                        {
                            originalFullscreen = line.Split('=')[1];
                            break;
                        }
                    }
                    File.AppendAllText("settings.txt", $"\noriginalFullscreen={originalFullscreen}");
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
                    else
                    {
                        Inilines[i] = $"Fullscreen={originalFullscreen}";
                    }
                }
            }

            File.WriteAllLines(IniPath, Inilines);
        }

        public void EnableFullscreen(bool enable, LaunchMode checkMode)
        {
            if (enable) changeIni(checkMode , true, true);
            else changeIni(checkMode, true, false);
            LaunchGame();
        }
        public void SmartStart(bool checksFullScreen, LaunchMode checkMode)
        {
            if (!File.Exists("DailyStreak.txt"))
            {
                File.WriteAllText("DailyStreak.txt", "1");
            }
            DateTime today = DateTime.Now.Date;
            DateTime lastReset = DateTime.Now.Hour >= 12
             ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0)
             : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0).AddDays(-1);
            if (File.Exists("last_run.txt"))
            {
                DateTime Content = DateTime.Parse(File.ReadAllText("last_run.txt"));
                if (Content < lastReset)
                {
                    // If the last run was before today, disable mods and launch the game
                    AreModsEnabled = false;
                    File.WriteAllText("last_run.txt", DateTime.Now.ToString());
                    File.WriteAllText("DailyStreak.txt", (int.Parse(File.ReadAllText("DailyStreak.txt")) + 1).ToString());
                }
                // If the last run was today, enable mods and launch the game
                else AreModsEnabled = true;
            }
            else
            {
                File.WriteAllText("last_run.txt", DateTime.Now.ToString());
                AreModsEnabled = false;
            }
            EnableFullscreen(checksFullScreen, checkMode);
        }

        public void InstantLaunch(bool smartStart, bool modsEnabled, bool fullscreenEnabled, LaunchMode checkMode)
        {
            if (smartStart) SmartStart(fullscreenEnabled, checkMode);
            else if (modsEnabled) EnableFullscreen(fullscreenEnabled, checkMode);
            else LaunchGame();
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

        public void CopyBaseGameFiles(string gamePath)
        {
            string exePath = Path.Combine(gamePath, "isaac-ng.exe");
            if (!File.Exists(exePath))
            {
                MessageBox.Show("Please add an acceptable folder!");
                return;
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
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating directory: {ex.Message}");
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
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error copying file: {ex.Message}");
                    }
                }
                
            }
        }

        public void SetupSteamEmulator()
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
            catch (Exception ex)
            {
                MessageBox.Show($"Valami gáz van a crackeléssel, bátyja: {ex.Message}");
            }
        }

        public void PortModsToEmulator()
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
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating directory: {ex.Message}");
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
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error copying file: {ex.Message}");
                    }
                }

            }
        }

        public void ConfigurePortableOptions()
        {
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");

            string optionsIniPath = Path.Combine(RepPlusPath, "options.ini");
            string portableIniPath = Path.Combine(portableDir, "options.ini");

            if (File.Exists(optionsIniPath))
            {
            File.Copy(optionsIniPath, portableIniPath);

                string optionsContent = File.ReadAllText(portableIniPath);

                if (optionsContent.Contains("SteamCloud=0"))
                {
                    optionsContent = optionsContent.Replace("SteamCloud=0", "SteamCloud=1");
                    File.WriteAllText(portableIniPath, optionsContent);
                }
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

        public void MakeIsaacPortable()
        {
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");
            if (!Directory.Exists(portableDir)) Directory.CreateDirectory(portableDir);

            // 1. Létrehozza a mappát és átmásolja a nyers gamet
            CopyBaseGameFiles(GamePath);

            // 2. Bepattintja a steam_api.dll-t és megcsinálja a steam_settings mappát a txt-kkel
            SetupSteamEmulator();

            // 3. Átrakja a modokat és levágja a nevüket számokra
            PortModsToEmulator();

            //4.Legenerálja a portable options.ini - t(SteamCloud = 1)
            ConfigurePortableOptions();

            MessageBox.Show("Portable Isaac is Ready to play!");
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
