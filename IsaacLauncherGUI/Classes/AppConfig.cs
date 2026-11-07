//Appconfig.cs
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;


namespace IsaacLauncherGUI
{
    public class AppSettings
    {
        public bool Mods { get; set; } = true;
        public bool SmartStart { get; set; } = false;
        public string GamePath { get; set; } = "path to isaac-ng.exe";
        public bool Custom { get; set; } = false;
        public bool Borderless { get; set; } = false;
        public string OriginalFullscreen { get; set; } = string.Empty;
        public string SkippedUpdateVersion { get; set; } = string.Empty;
        public bool Reminder { get; set; } = false;
        public int? OpenX { get; set; }
        public int? OpenY { get; set; }
        public bool InstantLaunch { get; set; } = false;
        public bool Portable { get; set; } = false;
        public bool OnTray { get; set; } = false;
        public bool StartOnTray { get; set; } = false;
        public bool GhostMode { get; set; } = false;
        public bool RepentogonChecked { get; set; } = false;
        public string RepentogonPath { get; set; } = "path to repentogon.exe";
        public string LastRun { get; set; } = string.Empty;
        public string LastUpdateCheck { get; set; } = string.Empty;
        public int DailyStreak { get; set; } = 0;
        public int Streak { get; set; } = 0;

    }

    internal class AppConfig
    {
        public static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IsaacLauncherGUI");

        public static string GetConfigPath()
        {
            string localConfig = Path.Combine(Application.StartupPath, "config.json");
            if (File.Exists(localConfig)) return localConfig;

            Directory.CreateDirectory(path);
            return Path.Combine(path, "config.json");
        }
        public static void Save(AppSettings settings)
        {
            string filePath = GetConfigPath();
            string tempPath = filePath + ".tmp";
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);

            File.WriteAllText(tempPath, json);
            if (File.Exists(filePath))
            {
                File.Replace(tempPath, filePath, filePath + ".bak");
            }
            else
            {
                File.Move(tempPath, filePath);
            }
        }

        public static AppSettings Load()
        {
            string filePath = GetConfigPath();
            if (!File.Exists(filePath)) return new AppSettings();
           try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch (JsonException ex)
            {
                string backupPath = filePath + ".corrupted-" +DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + ".bak";

                File.Copy(filePath, backupPath);

                MessageBox.Show(
                    $"The configuration file is corrupted.\n\n" +
                    $"A backup was saved to:\n{backupPath}\n\n" +
                    $"Default settings will be loaded.\n\nError: {ex.Message}",
                    "Configuration Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return new AppSettings();
            }
        }
    }
}
