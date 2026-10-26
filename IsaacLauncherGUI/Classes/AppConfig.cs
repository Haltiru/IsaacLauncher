//Appconfig.cs
using System;
using System.IO;
using System.Text.Json;


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

        public static string GetConfigPath(bool forcePortable = false)
        {
            if (forcePortable || File.Exists("config.json")) return "config.json";

            Directory.CreateDirectory(path);
            return Path.Combine(path, "config.json");
        }
        public static void Save(AppSettings settings)
        {
            string filePath = GetConfigPath(settings.Portable);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);

            File.WriteAllText(filePath, json);
        }

        public static AppSettings Load()
        {
            string filePath = GetConfigPath();
            if (!File.Exists(filePath)) return new AppSettings();
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
    }
}
