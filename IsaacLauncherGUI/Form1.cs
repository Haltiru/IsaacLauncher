//Form1.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Text;
using System.Drawing;

namespace IsaacLauncherGUI
{
    public partial class IsaacLauncher : Form
    {
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private string savedOriginalFullscreen = "1";

        private PrivateFontCollection customFont = new PrivateFontCollection();
        public IsaacLauncher()
        {
            InitializeComponent();
            customFont.AddFontFile("upheavtt.ttf");
            this.Icon = Properties.Resources.beast;
            ApplyCustomFont(this, customFont.Families[0]);
        }

        private void ApplyCustomFont(Control parentControl, FontFamily customFamily)
        {
            foreach (Control c in parentControl.Controls)
            {
                // Ráhúzzuk az új fontot, de a designerben beállított méretet és stílust meghagyjuk
                c.Font = new Font(customFamily, c.Font.Size, c.Font.Style);

                // Ha a controlban vannak további controlok (pl. a TabControl fülei)
                if (c.HasChildren)
                {
                    ApplyCustomFont(c, customFamily);
                }
            }
        }
        public void updateStreak()
        {
            if (File.Exists("DailyStreak.txt")) label1.Text = "Current Daily Streak: " + File.ReadAllText("DailyStreak.txt") + " 🔥";
            else label1.Text = "Current Daily Streak: 0" + " 🔥";
        }

        public async Task FindIsaac()
        {
            Process singleIsaac = null;
            while (singleIsaac == null)
            {
                System.Diagnostics.Process[] Isaac = Process.GetProcessesByName("isaac-ng");
                if (Isaac.Length > 0)
                {
                    singleIsaac = Isaac[0];
                }
                else
                {
                    await Task.Delay(250);
                }
            }
            while (!singleIsaac.HasExited && singleIsaac.MainWindowHandle == IntPtr.Zero)
            {
                await Task.Delay(250);
                singleIsaac.Refresh();
            }
            while (!singleIsaac.HasExited && !singleIsaac.Responding)
            {
                singleIsaac.Refresh();
                await Task.Delay(250);
            }
            //MessageBox.Show("Isaac sikeresen felébredt bátyja! 💀");
            int style = GetWindowLong(singleIsaac.MainWindowHandle, -16);
            SetWindowLong(singleIsaac.MainWindowHandle, -16, style & ~0x00C00000 & ~0x00040000);
            SetWindowPos(singleIsaac.MainWindowHandle, IntPtr.Zero, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, 0x0020);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Enabled = false;
                checkBox1.Checked = false;
            }
            else checkBox1.Enabled = true;
        }

        private void launcherButton_Click_1(object sender, EventArgs e) {tabControl1.SelectedIndex = 0;}
        private void settingsButton_Click(object sender, EventArgs e) {tabControl1.SelectedIndex = 1;}

        private void toolsButton_Click(object sender, EventArgs e) { tabControl1.SelectedIndex = 2; }

        private async void playButton_Click(object sender, EventArgs e)
        {
            bool modsRequested = checkBox1.Checked;
            bool smartModeRequested = checkBox2.Checked;
            bool crackedModeRequested = checkBox3.Checked;
            bool borderlessFullscreenRequested = checkBox4.Checked;
            Launcher isaacLauncher = new Launcher(crackedModeRequested, modsRequested, textBox1.Text);

            System.Diagnostics.Process[] Isaac = Process.GetProcessesByName("isaac-ng");
            if (Isaac.Length > 0)
            {
                MessageBox.Show("The Binding of Isaac is already running. Please close the game before launching it again.", "Game Already Running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (smartModeRequested)
            {
                isaacLauncher.SmartStart(borderlessFullscreenRequested);
                updateStreak();
            }
            else
            {
                isaacLauncher.changeIni(modsRequested, borderlessFullscreenRequested);
                isaacLauncher.LaunchGame();
            }
            if (borderlessFullscreenRequested) await FindIsaac();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK) textBox1.Text = fbd.SelectedPath;
            }
        }

        private void IsaacLauncher_FormClosing(object sender, FormClosingEventArgs e)
        {
            string settingsData = $"mods={checkBox1.Checked}\nsmart={checkBox2.Checked}\ngamePath={textBox1.Text}\ncracked={checkBox3.Checked}\nborderless={checkBox4.Checked}\noriginalFullscreen={savedOriginalFullscreen}\ninstantLaunch={checkBox5.Checked}";
            File.WriteAllText("settings.txt", settingsData);
        }

        private async void IsaacLauncher_Load(object sender, EventArgs e)
        {
            if (File.Exists("settings.txt"))
            {
                string[] lines = File.ReadAllLines("settings.txt");
                foreach (string line in lines)
                {
                    switch (line)
                    {
                        case string s when s.StartsWith("mods="):
                            checkBox1.Checked = bool.Parse(s.Substring("mods=".Length)); break;
                        case string s when s.StartsWith("smart="):
                            checkBox2.Checked = bool.Parse(s.Substring("smart=".Length)); break;
                        case string s when s.StartsWith("gamePath="):
                            textBox1.Text = s.Substring("gamePath=".Length); break;
                        case string s when s.StartsWith("cracked="):
                            checkBox3.Checked = bool.Parse(s.Substring("cracked=".Length)); break;
                        case string s when s.StartsWith("borderless="):
                            checkBox4.Checked = bool.Parse(s.Substring("borderless=".Length)); break;
                        case string s when s.StartsWith("originalFullscreen="):
                            savedOriginalFullscreen = s.Substring("originalFullscreen=".Length); break;
                        case string s when s .StartsWith("instantLaunch="):
                            checkBox5.Checked = bool.Parse(s.Substring("instantLaunch=".Length)); break;
                    }
                }
            }

            if (checkBox5.Checked) 
            {
                Launcher instantLauncher = new Launcher(checkBox3.Checked, checkBox1.Checked, textBox1.Text);
                instantLauncher.InstantLaunch(checkBox2.Checked, checkBox1.Checked, checkBox4.Checked);
                if (checkBox4.Checked) await FindIsaac();
            };
            updateStreak();
        }
    }
}