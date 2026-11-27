//UpdateProgressUserControl.cs

namespace IsaacLauncherGUI
{
    public partial class UpdateProgressUserControl : UserControl
    {
        public UpdateProgressUserControl()
        {
            InitializeComponent();
            progressBar.Width = 0;
        }

        public void UpdateProgress(int percentage)
        {
            progressBar.Width = (int)(backPanel.Width * (percentage / 100.0));
            percentageLabel.Text = $"{percentage}%";
        }
        public void UpdateStatus(string status)
        {
            statusLabel.Text = status;
            if (status == "Preparing installation...")
            {
                updaterTitle.Text = "UPDATING LAUNCHER";
            }
        }
    }
}
