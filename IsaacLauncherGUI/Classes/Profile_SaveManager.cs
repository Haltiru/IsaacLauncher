//Profile_SaveManager.cs
using System;
using System.IO;
using System.Windows.Forms;

namespace IsaacLauncherGUI
{
    class SaveManager
    {
        public bool IsPortable { get; set; }
        public string SavePath { get; set; }

        public SaveManager(bool isPortable)
        {
            string user = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string correctPath = Path.Combine(user, "My Games", "Binding of Isaac Repentance+");
            string portableDir = Path.Combine(Application.StartupPath, "portable-saac", "portable_saves", "250900", "remote");
            IsPortable = isPortable;

            if (isPortable)
            {
                SavePath = portableDir;
                if (!Directory.Exists(SavePath)) throw new ArgumentException("Portable save path does not exist.", nameof(SavePath));
            }
            else SavePath = correctPath;

            if (string.IsNullOrEmpty(SavePath)) throw new ArgumentException("Save path cannot be null or empty.", nameof(SavePath));
            if (!Directory.Exists(SavePath)) throw new ArgumentException("Save path does not exist.", nameof(SavePath));
        }

        public void Save()
        {
            string pathToSave = IsPortable ? Path.Combine(Application.StartupPath, "Backups", "Portable") : Path.Combine(Application.StartupPath, "Backups", "Steam");
            string correctedPath = IsPortable ? SavePath : Path.Combine(SavePath, "save_backups");
            if (!Directory.Exists(pathToSave)) Directory.CreateDirectory(pathToSave);

            for (int i = 1; i <= 3; i++)
            {

                string today = DateTime.Now.ToString("yyyyMMdd");
                DateTime todayInt = DateTime.ParseExact(today, "yyyyMMdd", null);
                string saveFileName = "";
                if (IsPortable) saveFileName = $"rep+persistentgamedata{i}.dat";
                else
                {
                    while ((todayInt > DateTime.ParseExact("20241118", "yyyyMMdd", null)))
                    {
                        saveFileName = $"{today}.rep+persistentgamedata{i}.dat";
                        if (File.Exists(Path.Combine(correctedPath, saveFileName)))
                        {
                            break;
                        }
                        todayInt = todayInt.AddDays(-1);
                        today = todayInt.ToString("yyyyMMdd");
                    }
                }

                string gameStateFileName = $"gamestate{i}.dat";
                if (!File.Exists(Path.Combine(correctedPath, saveFileName)))
                {
                    MessageBox.Show($"Save files for slot {i} not found. Please ensure the game has been played and saved at least once. {saveFileName}, {correctedPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                string saveFileNameAbsPath = Path.Combine(correctedPath, saveFileName);
                string destPath = Path.Combine(pathToSave, saveFileName);
                File.Copy(saveFileNameAbsPath, destPath, true);


                string gameStateFileNameAbsPath = Path.Combine(SavePath, gameStateFileName);
                destPath = Path.Combine(pathToSave, gameStateFileName);
                if (!File.Exists(gameStateFileNameAbsPath)) continue;
                File.Copy(gameStateFileNameAbsPath, destPath, true);

            }
        }

        public void OpenBackupFolder()
        {
            string pathToSave = Path.Combine(Application.StartupPath, "Backups");
            if (!Directory.Exists(pathToSave)) Directory.CreateDirectory(pathToSave);
            System.Diagnostics.Process.Start("explorer.exe", pathToSave);
        }
    }

    internal class Profile_SaveManager
    {
    }
}
