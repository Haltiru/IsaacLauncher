//Profile_SaveManager.cs

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

        public void Save(bool autoBackup = false)
        {
            string pathToSave = IsPortable ? Path.Combine(Application.StartupPath, "Backups", "Portable") : Path.Combine(Application.StartupPath, "Backups", "Steam");
            if (autoBackup)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
                pathToSave = Path.Combine(pathToSave, "Auto", timestamp);
            }

            string correctedPath = IsPortable ? SavePath : Path.Combine(SavePath, "save_backups");
            if (!Directory.Exists(pathToSave)) Directory.CreateDirectory(pathToSave);

            int backedUpSlots = 0;
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
                if (!File.Exists(Path.Combine(correctedPath, saveFileName))) continue;

                string saveFileNameAbsPath = Path.Combine(correctedPath, saveFileName);
                string destPath = Path.Combine(pathToSave, saveFileName);
                File.Copy(saveFileNameAbsPath, destPath, true);
                backedUpSlots++;


                string gameStateFileNameAbsPath = Path.Combine(SavePath, gameStateFileName);
                destPath = Path.Combine(pathToSave, gameStateFileName);
                if (!File.Exists(gameStateFileNameAbsPath)) continue;
                File.Copy(gameStateFileNameAbsPath, destPath, true);

            }
            if (backedUpSlots == 0) throw new InvalidOperationException("No valid save files were found to back up.");
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
