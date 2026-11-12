using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace IsaacLauncherGUI
{
    public class GameWindowManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public static bool ApplyBorderlessFullscreen(Process process)
        {
            try
            {
                // Check if the process is valid and has a main window handle
                if (process == null || process.HasExited || process.MainWindowHandle == IntPtr.Zero)
                {
                    return false;
                }
                // Get the current window style
                int style = GetWindowLong(process.MainWindowHandle, -16);
                int errorCode = Marshal.GetLastWin32Error();
                if (style == 0 && errorCode != 0)
                {
                    Debug.WriteLine($"GetWindowLong failed with error code: {errorCode}");
                    return false;
                }

                // Remove the window borders and title bar 
                int resultSetWindowLong = SetWindowLong(process.MainWindowHandle, -16, style & ~0x00C00000 & ~0x00040000);
                errorCode = Marshal.GetLastWin32Error();
                if (resultSetWindowLong == 0 && errorCode != 0)
                {
                    Debug.WriteLine($"SetWindowLong failed with error code: {errorCode}");
                    return false;
                }

                // Set the window position and size to cover the entire screen
                bool result = SetWindowPos(process.MainWindowHandle, IntPtr.Zero, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, 0x0020);
                errorCode = Marshal.GetLastWin32Error();
                if (!result)
                {
                    Debug.WriteLine($"SetWindowPos failed with error code: {errorCode}");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception occurred while applying borderless fullscreen: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
