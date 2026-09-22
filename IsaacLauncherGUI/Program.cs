//Progran.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace IsaacLauncherGUI
{
    class Launcher
    {
        public bool IsCracked { get; set; }
        public bool AreModsEnabled { get; set; }
        public string GamePath { get; set; }
        public string IniPath { get; set; }


        public Launcher(bool isCracked, bool areModsEnabled, string gamePath)
        {
            string user = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string correctPath = Path.Combine(user, "My Games", "Binding of Isaac Repentance+", "options.ini");
            IsCracked = isCracked;
            AreModsEnabled = areModsEnabled;
            GamePath = gamePath;
            IniPath = correctPath;
        }
        public void LaunchGame()
        {
            if (IsCracked)
            {
                ProcessStartInfo AdminStartCracked = new ProcessStartInfo
                {
                    FileName = GamePath + "\\isaac-ng.exe",
                    UseShellExecute = true,
                };
                Process.Start(AdminStartCracked);
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
        }

        public void changeIni(bool enableMods, bool forceWindowed)
        {
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

        public void SmartStart(bool checksFullScreen)
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
            if (checksFullScreen) changeIni(true, true);
            else changeIni(true, false);
            LaunchGame();
        }
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ez indítja el a te Form1-edet!
            Application.Run(new IsaacLauncher());
        }
    }
}