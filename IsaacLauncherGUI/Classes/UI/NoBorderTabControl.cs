using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace IsaacLauncherGUI
{
    public class NoBorderTabControl : TabControl
    {
        // 🛑 A legbrutálisabb anti-designer védelem
        private bool IsInDesignMode
        {
            get
            {
                if (this.DesignMode) return true;
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;

                // Ha a Visual Studio-ban futunk, dobjuk el a custom logikát a picsába 💀
                string processName = Process.GetCurrentProcess().ProcessName.ToLower();
                return processName.Contains("devenv") || processName.Contains("designtoolsserver");
            }
        }

        protected override void WndProc(ref Message m)
        {
            // 0x1328 a TCM_ADJUSTRECT native üzenet. Ha ezt megfogjuk, a Windows nem rajzolja be a 3D keretet.
            if (m.Msg == 0x1328 && !this.DesignMode)
            {
                m.Result = (IntPtr)1;
                return;
            }
            base.WndProc(ref m);
        }
    }
}