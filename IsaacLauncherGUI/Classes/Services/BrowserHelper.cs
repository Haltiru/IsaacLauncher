using System;
using System.Diagnostics;
using System.Security.Policy;

namespace IsaacLauncherGUI
{
    public class BrowserHelper
    {
        public static void OpenUrl(string url)
        {
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && uriResult.Scheme == Uri.UriSchemeHttps)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = uriResult.ToString(),
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open URL: {ex.Message}");
            }
        }
    }
}
