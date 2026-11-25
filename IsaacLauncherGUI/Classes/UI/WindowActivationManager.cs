using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IsaacLauncherGUI
{
    public static class WindowActivationManager
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public static bool IsForegroundWindow(Form form)
        {
            return GetForegroundWindow() == form.Handle;
        }

        public static bool TryBringToForeground(Form form)
        {
            if (!form.Visible) form.Show();
            if (form.WindowState == FormWindowState.Minimized) form.WindowState = FormWindowState.Normal;
            if (IsForegroundWindow(form)) return true;

            SetForegroundWindow(form.Handle);
            return IsForegroundWindow(form);

        }
    }
}
