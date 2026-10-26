using System;
using System.Drawing;
using System.Windows.Forms;

namespace IsaacLauncherGUI
{
    public static class UIHelper
    {
        public static void MouseHovered(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Text != "Stop") btn.BackgroundImage = Properties.Resources.hoverbutton;
            else if (btn.ForeColor == Color.White && btn.Text != "Stop") btn.BackgroundImage = Properties.Resources.blackbutton;
            else if (btn.Text == "Stop") { btn.BackgroundImage = Properties.Resources.redbuttonhover; btn.ForeColor = Color.White; }
        }

        public static void MouseLeft(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.ForeColor != Color.White && btn.Text != "Stop") btn.BackgroundImage = Properties.Resources.button;
            else if (btn.ForeColor == Color.White && btn.Text != "Stop") btn.BackgroundImage = Properties.Resources.blackbutton;
            else if (btn.Text == "Stop") { btn.BackgroundImage = Properties.Resources.redbutton; btn.ForeColor = Color.White; }
        }

        public static void ApplyCustomFont(Control parentControl, FontFamily customFamily)
        {
            foreach (Control c in parentControl.Controls)
            {
                if (c.Tag?.ToString() != "KeepComic")
                {
                    c.Font = new Font(customFamily, c.Font.Size, c.Font.Style);
                }

                if (c.HasChildren) ApplyCustomFont(c, customFamily);
            }
        }

        public static void ApplyHoverEffects(Control parentControl)
        {
            foreach (Control c in parentControl.Controls)
            {
                if (c is Button btn)
                {
                    btn.MouseEnter += MouseHovered;
                    btn.MouseLeave += MouseLeft;
                }
                if (c is TabPage || c is Panel || c is GroupBox || c is NoBorderTabControl) // Ha van konténer, amiben a gombok ülnek
                {
                    ApplyHoverEffects(c);
                }
            }
        }

    }
}