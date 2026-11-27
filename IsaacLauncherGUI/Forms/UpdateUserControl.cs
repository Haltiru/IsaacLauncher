//UpdateUserControl.cs

namespace IsaacLauncherGUI
{
    public partial class UpdateUserControl : UserControl
    {
        public event Action<DialogResult> ChoiceSelected;
        public UpdateUserControl(string version)
        {
            InitializeComponent();
            label1.Text = $"A new version is available: v{version}. Would you like to update now?";
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

        private void onUpdateUserControlLoad(object sender, EventArgs e)
        {
            UIHelper.ApplyHoverEffects(this);
        }

    }
}
