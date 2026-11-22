
using System;
using System.Diagnostics;
using System.IO;

namespace IsaacLauncherGUI
{
    internal static class StartupLog
    {
        private static readonly object Lock = new object();
        private static readonly string LogPath = Path.Combine(AppConfig.GetDataPath(), "log.txt");

        [Conditional("DEBUG")]
        public static void Write(string message)
        {
            try
            {
                lock (Lock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
                    File.AppendAllText(LogPath,$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
                }
            }
            catch (Exception ex)
            {
               Debug.WriteLine($"Logging failed: {ex}");
            }
        }
    }
}
