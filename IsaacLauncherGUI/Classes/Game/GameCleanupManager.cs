using System;
using System.Diagnostics;

namespace IsaacLauncherGUI
{
    public class GameCleanupManager
    {
        public static bool TryCleanup(bool hasExited, bool ghostMode, bool isPortable, Launcher launcher)
        {
            try
            {
                if  (ghostMode && isPortable && Process.GetProcessesByName("isaac-ng").Length == 0 && hasExited) return launcher.DeleteNotExistedFolders();
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cleanup: {ex.Message}");
                return false;
            }
        }
    }
}
