using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace IsaacLauncherGUI
{
    public static class FontManager
    {
        private static readonly PrivateFontCollection _customFont = new PrivateFontCollection();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;
            _customFont.AddFontFile(Path.Combine(Application.StartupPath, "upheavtt.ttf"));
            _initialized = true;
        }

        public static void ApplyFont(Control control)
        {
            Initialize();
            UIHelper.ApplyCustomFont(control, _customFont.Families[0]);
        }
    }
}
