namespace IsaacLauncherGUI
{
    public class GameCleanupManager
    {
        public static bool TryCleanup(bool hasExited, bool ghostMode, bool isPortable, Launcher launcher)
        {
            try
            {
                if (!ghostMode || !isPortable || !hasExited || Process.GetProcessesByName("isaac-ng").Length > 0) return false;
                return launcher.DeleteNotExistedFolders();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cleanup: {ex.Message}");
                return false;
            }
        }
    }
}
