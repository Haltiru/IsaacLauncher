using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IsaacLauncherGUI
{
    public static class PlaytimeTracker
    {

        public static void UpdateSessionTime(Stopwatch stopwatch, Label label)
        {
            TimeSpan elapsed = stopwatch.Elapsed;
            label.Text = FormatSessionTime(elapsed);
        }
        public static string FormatSessionTime(TimeSpan elapsed)
        {
            int hours = (int)elapsed.TotalHours;
            int minutes = elapsed.Minutes;
            int seconds = elapsed.Seconds;

            if (hours == 0) return $"{minutes:00}:{seconds:00}";
            else return $"{hours:00}:{minutes:00}:{seconds:00}";
        }

        public static string FormatTotalPlaytime(long totalSeconds)
        {
            return $"{(totalSeconds / 3600.0):F1} hours";
        }

        public static void UpdateTotalPlaytime(Stopwatch stopwatch, Label label, AppSettings settings)
        {
            long sessionTotalSeconds = (long)stopwatch.Elapsed.TotalSeconds;
            long savedTotalSeconds = settings.LauncherPlaytimeSeconds;
            if (stopwatch.IsRunning) savedTotalSeconds += sessionTotalSeconds;
            label.Text = FormatTotalPlaytime(savedTotalSeconds);
        }
    }
}
