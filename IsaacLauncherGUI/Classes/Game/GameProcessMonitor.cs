using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IsaacLauncherGUI
{
    public class GameProcessMonitor
    {

        public static async Task<Process> WaitForProcessAsync(string processName, int timeoutMs)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                Process[] processses = Process.GetProcessesByName(processName);
                if (processses.Length > 0) return processses[0];
                else if (stopwatch.ElapsedMilliseconds >= timeoutMs) return null;
                await Task.Delay(Math.Min(250, Math.Max(0, timeoutMs - (int)stopwatch.ElapsedMilliseconds)));
            }
            while (stopwatch.ElapsedMilliseconds < timeoutMs);

            return null;
        }

        public static async Task<bool> WaitForWindowAsync(Process process, int timeoutMs)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                do
                {
                    process.Refresh();
                    if (process.HasExited) return false;
                    bool hasWindow = process.MainWindowHandle != IntPtr.Zero;
                    if (hasWindow) return true;
                    else if (stopwatch.ElapsedMilliseconds >= timeoutMs) return false;
                    await Task.Delay(Math.Min(250, Math.Max(0, timeoutMs - (int)stopwatch.ElapsedMilliseconds)));
                }
                while (!process.HasExited && stopwatch.ElapsedMilliseconds < timeoutMs);
                return process.MainWindowHandle != IntPtr.Zero && !process.HasExited;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in WaitForWindowAsync: {ex.Message}");
                return false;
            }

        }
        public static async Task<bool> WaitForResponsiveAsync(Process process, int timeoutMs)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                do
                {
                    process.Refresh();
                    if (process.HasExited) return false;
                    if (process.Responding) return true;
                    else if (stopwatch.ElapsedMilliseconds >= timeoutMs) return false;
                    await Task.Delay(Math.Min(250, Math.Max(0, timeoutMs - (int)stopwatch.ElapsedMilliseconds)));
                }
                while (stopwatch.ElapsedMilliseconds < timeoutMs);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in WaitForResponsiveAsync: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> WaitForExitAsync(Process process)
        {
            try
            {
                if (process == null)
                {
                    Debug.WriteLine("Process is null in WaitForExitAsync");
                    return false;
                }
                await Task.Run(() => process.WaitForExit());
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in WaitForExitAsync: {ex.Message}");
                return false;
            }
        }
    }
}
