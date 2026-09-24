//Form1.cs
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static IsaacLauncherGUI.Launcher;


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










        private void launcherButton_Click_1(object sender, EventArgs e) {tabControl1.SelectedIndex = 0;}
        private void settingsButton_Click(object sender, EventArgs e) {tabControl1.SelectedIndex = 1;}

        private void toolsButton_Click(object sender, EventArgs e) { tabControl1.SelectedIndex = 2; }

        private async void playButton_Click(object sender, EventArgs e)
        {

            // A dinamikus útvonalak, amiket te is profin összeraktál
            string docsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string originalRepPath = Path.Combine(docsFolder, "My Games", "Binding of Isaac Repentance+");
            string originalOptionsIni = Path.Combine(originalRepPath, "options.ini");

            string portableDir = Path.Combine(Application.StartupPath, "portable-saac");
            string portableOptionsIni = Path.Combine(portableDir, "options.ini");
            string portableExe = Path.Combine(portableDir, "isaac-ng.exe");


            bool modsRequested = checkBox1.Checked;
            bool smartModeRequested = checkBox2.Checked;
            LaunchMode ModeRequested;
            if (checkBox3.Checked) ModeRequested = LaunchMode.Cracked;
            else if (checkBox6.Checked) ModeRequested = LaunchMode.Portable;
            else { ModeRequested = LaunchMode.Steam; }
                bool borderlessFullscreenRequested = checkBox4.Checked;
            Launcher isaacLauncher = new Launcher(ModeRequested, modsRequested, textBox1.Text);

            System.Diagnostics.Process[] Isaac = Process.GetProcessesByName("isaac-ng");
            if (Isaac.Length > 0)
            {
                MessageBox.Show("The Binding of Isaac is already running. Please close the game before launching it again.", "Game Already Running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (smartModeRequested)
            {
                isaacLauncher.SmartStart(borderlessFullscreenRequested, ModeRequested);
                updateStreak();
            }
            else
            {
                isaacLauncher.changeIni(ModeRequested, modsRequested, borderlessFullscreenRequested);
                isaacLauncher.LaunchGame();
            }
            if (borderlessFullscreenRequested) await FindIsaac();
        }

        private void smartStartChecked(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Enabled = false;
                checkBox1.Checked = false;
            }
            else checkBox1.Enabled = true;
        }

        private void usePortableVersionClick(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                checkBox3.Enabled = false;
                checkBox3.Checked = false;
            }
            else checkBox3.Enabled = true;
        }

        private void useCrackedVersion(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                checkBox6.Enabled = false;
                checkBox6.Checked = false;
            }
            else checkBox6.Enabled = true;
        }
        private void BrowseGamePath(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK) textBox1.Text = fbd.SelectedPath;
            }
        }
        private async void startPortable(object sender, EventArgs e)
        {
            button3.Enabled = false;
            PortableIsaacLauncher startPortable = new PortableIsaacLauncher(textBox1.Text);
            await Task.Run(() => startPortable.MakeIsaacPortable());
            button3.Enabled = true;
        }











        private void IsaacLauncher_FormClosing(object sender, FormClosingEventArgs e)
        {
            string settingsData = $"mods={checkBox1.Checked}\nsmart={checkBox2.Checked}\ngamePath={textBox1.Text}\ncracked={checkBox3.Checked}\nborderless={checkBox4.Checked}\noriginalFullscreen={savedOriginalFullscreen}\ninstantLaunch={checkBox5.Checked}\nportable={checkBox6.Checked}";
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
                        case string s when s.StartsWith("portable="):
                            checkBox6.Checked = bool.Parse(s.Substring("portable=".Length)); break;
                    }
                }
            }

            if (checkBox5.Checked) 
            {
                LaunchMode ModeRequested = LaunchMode.Steam;
                if (checkBox3.Checked) ModeRequested = LaunchMode.Cracked;
                else if (checkBox6.Checked) ModeRequested = LaunchMode.Portable;
                Launcher instantLauncher = new Launcher(ModeRequested, checkBox1.Checked, textBox1.Text);
                instantLauncher.InstantLaunch(checkBox2.Checked, checkBox1.Checked, checkBox4.Checked, ModeRequested);
                if (checkBox4.Checked) await FindIsaac();
            };
            updateStreak();
        }

        
    }
}