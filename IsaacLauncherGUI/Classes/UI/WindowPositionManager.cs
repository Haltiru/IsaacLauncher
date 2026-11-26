using System.Windows.Forms;
using System.Drawing;

namespace IsaacLauncherGUI
{ 
    public static class WindowPositionManager
    {
        public static void SaveWindowPosition(Form form, AppSettings settings)
        {
            Point location = form.WindowState == FormWindowState.Normal ? form.Location : form.RestoreBounds.Location;
            settings.OpenX = location.X;
            settings.OpenY = location.Y;
        }

        public static void LoadWindowPosition(Form form, AppSettings settings)
        {
            if (!settings.OpenX.HasValue || !settings.OpenY.HasValue) return;   
            Point savedLocation = new Point(settings.OpenX.Value, settings.OpenY.Value);
            Rectangle windowBounds = new Rectangle(savedLocation, form.Size);

            foreach (Screen screen in Screen.AllScreens)
            {
                if (!screen.WorkingArea.Contains(windowBounds)) continue;
                
                form.StartPosition = FormStartPosition.Manual;
                form.Location = savedLocation;
                return;
            }
            
        }
    }
}
