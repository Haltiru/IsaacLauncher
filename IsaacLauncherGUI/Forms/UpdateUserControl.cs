using IsaacLauncherGUI.Classes;
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
    public partial class UpdateUserControl : UserControl
    {
        public event Action<DialogResult> ChoiceSelected;
        public UpdateUserControl()
        {
            InitializeComponent();
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            ChoiceSelected?.Invoke(DialogResult.Yes);
        }

        private void laterButton_Click(object sender, EventArgs e)
        {
            ChoiceSelected?.Invoke(DialogResult.Cancel);
        }

        private void skipButton_Click(object sender, EventArgs e)
        {
            ChoiceSelected?.Invoke(DialogResult.No);
        }

        private async void onUpdateUserControlLoad(object sender, EventArgs e)
        {
            UIHelper.ApplyHoverEffects(this);
            Updater updater = new Updater();
            await updater.GetRemoteVersion(); 
            if (updater.RemoteVersion != null) label1.Text = $"A new version is available: v{updater.RemoteVersion} . Would you like to update now?";        
        }
    }
}
