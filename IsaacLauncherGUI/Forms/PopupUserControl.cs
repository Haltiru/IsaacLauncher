using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace IsaacLauncherGUI
{
    public partial class PopupUserControl : UserControl
    {
        public enum PopupChoice{Primary,Secondary,Tertiary}
        public event Action<PopupChoice> ChoiceSelected;
        public PopupUserControl()
        {
            InitializeComponent();
            UIHelper.ApplyHoverEffects(this);
        }

        public void Configure(string title, string message, string primaryText, string secondaryText = null, string tertiaryText = null)
        {
            this.title.Text = title;
            this.message.Text = message;

            this.primaryButton.Text = primaryText;
            this.primaryButton.Visible = true;

            if (!string.IsNullOrWhiteSpace(secondaryText))
            {
                this.secondaryButton.Text = secondaryText;
                this.secondaryButton.Visible = true;
            }
            else this.secondaryButton.Visible = false;

            if (!string.IsNullOrWhiteSpace(tertiaryText))
            {
                this.tertiaryButton.Text = tertiaryText;
                this.tertiaryButton.Visible = true;
            }
            else this.tertiaryButton.Visible = false;

            tableLayoutPanel1.PerformLayout();

            int textWidth = this.message.ClientSize.Width - this.message.Padding.Horizontal;

            int textHeight = TextRenderer.MeasureText(this.message.Text,this.message.Font,new Size(Math.Max(1, textWidth), int.MaxValue),TextFormatFlags.WordBreak).Height;
            
            int singleLineHeight = TextRenderer.MeasureText("Ag",this.message.Font).Height;

            bool isMultiline = textHeight > singleLineHeight;
            this.message.BackColor = isMultiline? Color.FromArgb(109, 67, 56) : Color.Transparent;

            this.Height = tableLayoutPanel1.Bottom;

        }

        private void PrimaryButton_click(object sender, EventArgs e)
        {
            ChoiceSelected?.Invoke(PopupChoice.Primary);
        }

        private void SecondaryButton_click(object sender, EventArgs e)
        {
            ChoiceSelected?.Invoke(PopupChoice.Secondary);
        }

        private void TertiaryButton_click(object sender, EventArgs e)
        {
            ChoiceSelected?.Invoke(PopupChoice.Tertiary);
        }
    }
}
