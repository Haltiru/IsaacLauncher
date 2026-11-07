using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
