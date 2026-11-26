using System;
using System.Diagnostics;

namespace IsaacLauncherGUI
{
    public class BrowserHelper
    {
        public static void OpenUrl(string url)
        {
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && uri.Scheme == Uri.UriSchemeHttps) Process.Start(new ProcessStartInfo{FileName = uri.ToString(),UseShellExecute = true});
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open URL: {ex.Message}");
            }
        }
    }
}
