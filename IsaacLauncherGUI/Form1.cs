//Form1.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        public IsaacLauncher()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.beast;
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
                    await Task.Delay(500);
                }
            }
            while (!singleIsaac.HasExited && singleIsaac.MainWindowHandle == IntPtr.Zero)
            {
                await Task.Delay(500);
                singleIsaac.Refresh();
            }
            while (!singleIsaac.HasExited && !singleIsaac.Responding)
            {
                singleIsaac.Refresh();
                await Task.Delay(500);
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
        private async void playButton_Click(object sender, EventArgs e)
        {
            bool smartModeRequested = checkBox2.Checked;
            bool crackedModeRequested = checkBox3.Checked;
            Launcher isaacLauncher = new Launcher(crackedModeRequested, checkBox1.Checked, textBox1.Text);

            System.Diagnostics.Process[] Isaac = Process.GetProcessesByName("isaac-ng");
            if (Isaac.Length > 0)
            {
                MessageBox.Show("The Binding of Isaac is already running. Please close the game before launching it again.", "Game Already Running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (smartModeRequested)
            {
                isaacLauncher.SmartStart(checkBox4.Checked);
                updateStreak();
            }
            else
            {
                isaacLauncher.changeIni(checkBox1.Checked, checkBox4.Checked);
                isaacLauncher.LaunchGame();
            }
            if (checkBox4.Checked)
            {
                await FindIsaac();
            }
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
            string settingsData = $"mods={checkBox1.Checked}\nsmart={checkBox2.Checked}\ngamePath={textBox1.Text}\ncracked={checkBox3.Checked}\nborderless={checkBox4.Checked}\noriginalFullscreen={savedOriginalFullscreen}";
            File.WriteAllText("settings.txt", settingsData);
        }

        private void IsaacLauncher_Load(object sender, EventArgs e)
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
                    }
                }
            }

            updateStreak();
        }

    }
}

