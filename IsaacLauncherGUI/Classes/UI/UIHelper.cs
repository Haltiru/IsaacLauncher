using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        public static void SetActiveNavigationButton(Button selectedButton, TabControl tabControl, Dictionary<Button, TabPage> navigationMap)
        {
            if (!navigationMap.TryGetValue(selectedButton, out TabPage selectedPage)) return;
            tabControl.SelectedTab = selectedPage;

            foreach(var button in navigationMap.Keys)
            {
                if (button == selectedButton)
                {
                    button.BackgroundImage = Properties.Resources.blackbutton;
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackgroundImage = Properties.Resources.button;
                    button.ForeColor = Color.FromArgb(84, 76, 74);
                }
            }
        }
    }
}