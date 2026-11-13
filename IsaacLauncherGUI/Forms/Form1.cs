//Form1.cs
using IsaacLauncherGUI.Classes;
using Microsoft.WindowsAPICodePack.Dialogs;
using ReaLTaiizor.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static IsaacLauncherGUI.Launcher;

namespace IsaacLauncherGUI
{
    public partial class IsaacLauncher : Form
    {
        private AppSettings _settings;
        private bool _loadingSettings = false;
        private ModmanagerForm _modManagerForm = null;
        private PrivateFontCollection customFont = new PrivateFontCollection();
        private Dictionary<Button, TabPage> buttonTabPages = new Dictionary<Button, TabPage>();
        ToolTip mainToolTip = new ToolTip();
        private enum PlayButtonState { Play, Stop, Starting }
        public IsaacLauncher()
        {
            InitializeComponent();
            buttonTabPages.Add(launcherButton, launcherPage);
            buttonTabPages.Add(settingsButton, settingsPage);
            buttonTabPages.Add(toolsButton, toolsPage);
            buttonTabPages.Add(aboutButton, aboutPage);

#if !DEBUG
            label6.Visible = false;
            portableCreate.Visible = false;
            label6.Dispose();
            portableCreate.Dispose();

#elif DEBUG
            label6.Visible = true;
            portableCreate.Visible = true;
            portableCreate.Click += StartPortable;
#endif
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();

            customFont.AddFontFile("upheavtt.ttf");
            this.Icon = Properties.Resources.beast;
            UIHelper.ApplyCustomFont(this, customFont.Families[0]);
        }


#if DEBUG

private async void StartPortable(object sender, EventArgs e)
        {
            portableCreate.Enabled = false;
            if (string.IsNullOrWhiteSpace(pathText.Text) || !Directory.Exists(pathText.Text))
            {
                MessageBox.Show("Please select a valid Isaac game folder before creating a portable version.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                portableCreate.Enabled = true;
                return;
            }
            PortableIsaacLauncher startPortable = new PortableIsaacLauncher(pathText.Text);
            string err = await Task.Run(() => startPortable.MakeIsaacPortable());

            if (string.IsNullOrEmpty(err))
            {
                MessageBox.Show("Portable version created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(err, "Portable Creation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            portableCreate.Enabled = true;
        
        } 

#endif

        private static readonly Color DisabledColor = Color.FromArgb(110, 110, 110);

        private void SetCheckBoxEnabled(CheckBox box, bool enabled)
        {
            if (!enabled) box.Checked = false;
            box.AutoCheck = enabled;
            box.ForeColor = enabled ? Color.White : DisabledColor;
        }

        public void UpdateUI()
        {
            // 1. custom <-> portable kölcsönös kizárás
            SetCheckBoxEnabled(portable, !custom.Checked);
            SetCheckBoxEnabled(custom, !portable.Checked);

            // az előző két sor után az állapotok megváltozhattak, ezért itt olvassuk ki
            bool isSmart = smart.Checked;
            bool isPortable = portable.Checked;
            bool isSteam = !isPortable && !custom.Checked;

            // 2. mi mit tilt
            //SetCheckBoxEnabled(mods, !isSmart);
            SetCheckBoxEnabled(repentogonCheckbox, isSteam);

            // 3. láthatóság
            label9.Visible = isSmart;
            label10.Visible = !isSmart;

            label9.Text = _settings.DailyStreak.ToString();
            label10.Text = _settings.Streak.ToString();

            ghost.Visible = isPortable;
            ghostModeTooltip.Visible = isPortable;

            gamePathToolTip.Visible = !isPortable;
            pathText.Visible = !isPortable;
            pathLine.Visible = !isPortable;
            gameExeLabel.Visible = !isPortable;

            //repentogonPathToolTip.Visible = !isPortable && !isCustom;
            //repentogonExePathLabel.Visible = !isPortable && !isCustom;
            //repentogonPathText.Visible = !isPortable && !isCustom;
            //pathLine2.Visible = !isPortable && !isCustom;

            portableCreate.Enabled = !isPortable;

            if (!isPortable) ghost.Checked = false;
        }

        private void SetActiveButton(Button selectedButton) { UIHelper.SetActiveNavigationButton(selectedButton, tabControl1, buttonTabPages); }
        private void NavigationButton_Click(object sender, EventArgs e) { SetActiveButton(sender as Button); }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        private void SaveSettings()
        {
            _settings.Mods = mods.Checked;
            _settings.SmartStart = smart.Checked;
            _settings.GamePath = pathText.Text;
            _settings.Custom = custom.Checked;
            _settings.Borderless = borderless.Checked;
            _settings.InstantLaunch = instantLaunch.Checked;
            _settings.Portable = portable.Checked;
            _settings.OnTray = trayBox.Checked;
            _settings.StartOnTray = startTray.Checked;
            _settings.GhostMode = ghost.Checked;
            _settings.RepentogonChecked = repentogonCheckbox.Checked;
            _settings.RepentogonPath = repentogonPathText.Text;

            Point location = this.WindowState == FormWindowState.Normal ? this.Location : this.RestoreBounds.Location;
            _settings.OpenX = location.X;
            _settings.OpenY = location.Y;

            AppConfig.Save(_settings);
        }

        private void LoadSettings()
        {
            _settings = AppConfig.Load();

            mods.Checked = _settings.Mods;
            smart.Checked = _settings.SmartStart;
            pathText.Text = _settings.GamePath;
            custom.Checked = _settings.Custom;
            borderless.Checked = _settings.Borderless;
            instantLaunch.Checked = _settings.InstantLaunch;
            portable.Checked = _settings.Portable;
            trayBox.Checked = _settings.OnTray;
            startTray.Checked = _settings.StartOnTray;
            ghost.Checked = _settings.GhostMode;
            repentogonCheckbox.Checked = _settings.RepentogonChecked;
            repentogonPathText.Text = _settings.RepentogonPath;

            if (_settings.OpenX.HasValue && _settings.OpenY.HasValue)
            {
                Point savedLocation = new Point(
                    _settings.OpenX.Value,
                    _settings.OpenY.Value
                );

                Rectangle windowBounds = new Rectangle(savedLocation, this.Size);

                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen.WorkingArea.Contains(windowBounds))
                    {
                        this.StartPosition = FormStartPosition.Manual;
                        this.Location = savedLocation;
                        break;
                    }
                }
            }

            label9.Text = _settings.DailyStreak.ToString();
            label10.Text = _settings.Streak.ToString();
        }

        private void SetPlayButtonState(PlayButtonState playButtonState)
        {
            switch (playButtonState)
            {
                case PlayButtonState.Play:
                    playButton.Text = "Play";
                    playButton.Enabled = true;
                    playButton.BackgroundImage = Properties.Resources.button;
                    playButton.ForeColor = Color.FromArgb(84, 76, 74);
                    break;
                case PlayButtonState.Stop:
                    playButton.Text = "Stop";
                    playButton.Enabled = true;
                    playButton.BackgroundImage = Properties.Resources.redbutton;
                    playButton.ForeColor = Color.White;
                    break;
                case PlayButtonState.Starting:
                    playButton.Text = "Starting...";
                    playButton.Enabled = false;
                    playButton.BackgroundImage = Properties.Resources.button;
                    playButton.ForeColor = Color.FromArgb(84, 76, 74);
                    break;
            }
        }

        public async Task<string> FindIsaac(bool enableFullscreen, Launcher launcher)
        {
            bool hasExited = false;
            bool ghostMode = ghost.Checked;
            bool isPortable = GetLaunchMode() == LaunchMode.Portable;

            try
            {
                // 1. Várjuk meg, hogy a játék és a Repentogon elinduljon
                GameStartupResult gameProcessResult = await GameStartupMonitor.WaitForGameStartupAsync(launcher.IsRepentogonEnabled);
                Process Isaac = gameProcessResult.Process;
                if (Isaac == null) return gameProcessResult.ErrorMessage;

                // Most már biztonságosan méretezzük a futó ablakot
                if (enableFullscreen)
                {
                    bool result = GameWindowManager.ApplyBorderlessFullscreen(Isaac);
                    if (!result) Debug.WriteLine("Failed to apply borderless fullscreen.");
                }

                // Megvárjuk, amíg a játék bezáródik (ez is háttérben megy)
                if (!Isaac.HasExited) SetPlayButtonState(PlayButtonState.Stop);
                UpdateUI();

                // Várjuk meg, amíg a játék bezáródik, és ha a Ghost Mode be van kapcsolva, próbáljuk meg kitakarítani a My Games mappát
                bool exitResult = await GameProcessMonitor.WaitForExitAsync(Isaac);
                if (!exitResult) Debug.WriteLine("Failed to wait for Isaac process exit.");
                else hasExited = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FindIsaac error: {ex}");
                return $"Failed to monitor Isaac: {ex.Message}";
            }
            finally
            {
                // Visszaállítjuk a UI-t, függetlenül attól, hogy sikerült-e a játékot elindítani vagy sem
                UpdateUI();
                SetPlayButtonState(PlayButtonState.Play);

                // Ha a játék bezárult, és a Ghost Mode be van kapcsolva, próbáljuk meg kitakarítani a My Games mappát
                bool cleanupResult = GameCleanupManager.TryCleanup(hasExited, ghostMode, isPortable, launcher);
                if (!cleanupResult && ghostMode && isPortable && Process.GetProcessesByName("isaac-ng").Length == 0) Debug.WriteLine("Failed to cleanup after Isaac.");
            }
            return null;
        }

        public LaunchMode GetLaunchMode()
        {
            if (custom.Checked) return LaunchMode.Custom;
            else if (portable.Checked) return LaunchMode.Portable;
            else return LaunchMode.Steam;
        }

        private bool CheckForErrors(string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            return false;
        }

        private async Task<DialogResult> ShowUpdateDialogAsync()
        {
            // We create a custom user control for the update prompt, and use a TaskCompletionSource to await the user's choice.
            UpdateUserControl updateUserControl = new UpdateUserControl();
            TaskCompletionSource<DialogResult> result = new TaskCompletionSource<DialogResult>();
            updateUserControl.ChoiceSelected += (choice) => result.TrySetResult(choice);

            // We take a screenshot of the current form to use as the background for the modal panel.
            Bitmap screenshot = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            Point topLeft = PointToScreen(Point.Empty);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(topLeft, Point.Empty, this.ClientSize);
                using (SolidBrush semiTransparentBrush = new SolidBrush(Color.FromArgb(170, 40, 30, 28)))
                {
                    g.FillRectangle(semiTransparentBrush, new Rectangle(Point.Empty, this.ClientSize));
                }
            }

            // We create a modal panel to host the update user control, and add it to the form.
            Panel Modal = new Panel();

            Modal.Dock = DockStyle.Fill;
            Modal.BackgroundImage = screenshot;
            Modal.TabStop = false;

            this.Controls.Add(Modal);
            Modal.Controls.Add(updateUserControl);

            updateUserControl.Location = new Point(this.ClientSize.Width / 2 - updateUserControl.Width / 2, this.ClientSize.Height / 2 - updateUserControl.Height / 2);

            // We bring the modal panel and the update user control to the front, and play a sound to alert the user.
            Modal.BringToFront();
            updateUserControl.BringToFront();
            SystemSounds.Asterisk.Play();

            DialogResult choiceSelected = await result.Task;

            this.Controls.Remove(Modal);
            Modal.BackgroundImage = null;
            Modal.Dispose();
            screenshot.Dispose();
            return choiceSelected;
        }

        private async Task ShowUpdateProgressDialogAsync(Func<IProgress<int>, IProgress<string>, Task> operation)
        {
            // We take a screenshot of the current form to use as the background for the modal panel.
            Bitmap screenshot = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            Point topLeft = PointToScreen(Point.Empty);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(topLeft, Point.Empty, this.ClientSize);
                using (SolidBrush semiTransparentBrush = new SolidBrush(Color.FromArgb(170, 40, 30, 28)))
                {
                    g.FillRectangle(semiTransparentBrush, new Rectangle(Point.Empty, this.ClientSize));
                }
            }
            bool isActive = true;
            UpdateProgressUserControl updateProgressUserControl = new UpdateProgressUserControl();
            Progress<int> progressReporter = new Progress<int>(percentage => { if (isActive) updateProgressUserControl.UpdateProgress(percentage); });
            Progress<string> statusReporter = new Progress<string>(status => { if (isActive) updateProgressUserControl.UpdateStatus(status); });

            // We create a modal panel to host the update user control, and add it to the form.
            Panel Modal = new Panel();

            Modal.Dock = DockStyle.Fill;
            Modal.BackgroundImage = screenshot;
            Modal.TabStop = false;

            this.Controls.Add(Modal);
            Modal.Controls.Add(updateProgressUserControl);

            updateProgressUserControl.Location = new Point(this.ClientSize.Width / 2 - updateProgressUserControl.Width / 2, this.ClientSize.Height / 2 - updateProgressUserControl.Height / 2);

            // We bring the modal panel and the update user control to the front, and play a sound to alert the user.
            Modal.BringToFront();
            updateProgressUserControl.BringToFront();
            try { await operation(progressReporter, statusReporter); }
            finally
            {
                isActive = false;
                this.Controls.Remove(Modal);
                Modal.BackgroundImage = null;
                Modal.Dispose();
                screenshot.Dispose();
            }
        }

        private async Task<bool> CheckForUpdate(bool autoCheck)
        {
            // We check for updates only if the last check was more than 36 seconds ago (0.01 hours)
            if (!string.IsNullOrEmpty(_settings.LastUpdateCheck) && !_settings.Reminder && autoCheck)
            {
                DateTime lastCheck;
                if (DateTime.TryParse(_settings.LastUpdateCheck, out lastCheck))
                {
                    TimeSpan timeSinceLastCheck = DateTime.Now - lastCheck;
                    if (timeSinceLastCheck.TotalHours < 0.01) return false;
                }
            }

            // We get the remote version and compare it with the local version. If the remote version is newer, we prompt the user to update.
            Updater updater = new Updater();
            bool isSuccess = await updater.GetRemoteVersion();
            string remoteVersion = null;
            Version checkForNull = updater.RemoteVersion;
            if (isSuccess && checkForNull != null)
            {
                // Update the last check time
                _settings.LastUpdateCheck = DateTime.Now.ToString("o");
                AppConfig.Save(_settings);
                remoteVersion = updater.RemoteVersion.ToString();
            }
            // If the user has chosen to skip this version, we don't prompt them again unless they manually check for updates.
            if (!_settings.Reminder && _settings.SkippedUpdateVersion == remoteVersion && autoCheck) return false;

            if (isSuccess && updater.IsUpdateAvailable())
            {
                Task<DialogResult> choiceTask = ShowUpdateDialogAsync();
                DialogResult choiceSelected = await choiceTask;

                if (choiceSelected == DialogResult.Yes)
                {
                    _settings.Reminder = false;
                    AppConfig.Save(_settings);
                    try { await ShowUpdateProgressDialogAsync((progressReporter, statusReporter) => updater.StartUpdateProcess(progressReporter, statusReporter)); }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"The update failed: {ex.Message}", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else if (choiceSelected == DialogResult.No)
                {
                    _settings.Reminder = false;
                    _settings.SkippedUpdateVersion = updater.RemoteVersion.ToString();
                    AppConfig.Save(_settings);
                }
                else if (choiceSelected == DialogResult.Cancel)
                {
                    _settings.Reminder = true;
                    AppConfig.Save(_settings);
                }
            }
            else if (isSuccess && !autoCheck) MessageBox.Show(this, $"You are using the latest version of the launcher: {updater.LocalVersion}.", "No Update Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (!isSuccess && !autoCheck) MessageBox.Show(this, "Failed to check for updates. Please check your internet connection and try again.", "Update Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return isSuccess;
        }

        public async Task StartGame(Launcher launcher)
        {
            bool BorderlessEnabled = borderless.Checked;
            bool ModsEnabled = mods.Checked;
            bool RepentogonEnabled = repentogonCheckbox.Checked;

            string hiba = await Task.Run(() => launcher.InitializeAndLaunch(BorderlessEnabled, ModsEnabled, RepentogonEnabled, _settings));
            UpdateUI();
            if (CheckForErrors(hiba))
            {
                SetPlayButtonState(PlayButtonState.Play);
                return;
            }

            hiba = await FindIsaac(BorderlessEnabled, launcher);
            if (CheckForErrors(hiba)) return;
        }



        private async void PlayButton_Click(object sender, EventArgs e)
        {
            UpdateUI();
            try
            {
                Process[] Isaac = Process.GetProcessesByName("isaac-ng");
                if (Isaac.Length > 0)
                {
                    try
                    {
                        Isaac[0].Kill();
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to stop the game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else SetPlayButtonState(PlayButtonState.Starting);

                LaunchMode ModeRequested = GetLaunchMode();
                Launcher isaacLauncher = new Launcher(ModeRequested, mods.Checked, pathText.Text, repentogonCheckbox.Checked, repentogonPathText.Text, smart.Checked);

                await StartGame(isaacLauncher);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetPlayButtonState(PlayButtonState.Play);
            }
        }

        private void BackupButton_Click(object sender, EventArgs e)
        {
            SaveManager saveManager = new SaveManager(portable.Checked);
            saveManager.Save();
            MessageBox.Show("Your saves have been backed up successfully!", "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ghost_CheckedChanged(object sender, EventArgs e)
        {
            if (!ghost.Checked || _loadingSettings) return;

            DialogResult result = MessageBox.Show(
                this,
                "Ghost Mode automatically deletes temporary Isaac files from the My Games folder after the portable game closes.\n\n" +
                "This includes options.ini, logs, and save backups stored in that folder.\n\n" +
                "IMPORTANT: Only use Ghost Mode with a supported portable installation. Make sure your saves are backed up!\n\n" +
                "Do you want to enable Ghost Mode?",
                "Enable Ghost Mode?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );
            if (result != DialogResult.Yes) ghost.Checked = false;
        }
        private void SmartStartChecked(object sender, EventArgs e) { UpdateUI(); }
        private void UsePortableVersionClick(object sender, EventArgs e) { UpdateUI(); }
        private void UseCustomVersion(object sender, EventArgs e) { UpdateUI(); }
        private void BrowseGamePath(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Title = "Search for your Isaac folder with the isaac.ng.exe in it!";

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string isaacExePath = Path.Combine(dialog.FileName, "isaac-ng.exe");

                    if (File.Exists(isaacExePath)) pathText.Text = dialog.FileName;
                    else
                    {
                        MessageBox.Show("There is no isaac-ng.exe in this folder!", "No exe found!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        pathText.Text = _settings.GamePath;
                    }
                }
            }
        }

        private void BrowseRepentogonPath(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.Title = "Search for your Repentogon Launcher exe!";
                dialog.Filters.Add(new CommonFileDialogFilter("Executable", "*.exe"));

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string selected = dialog.FileName;

                    if (Path.GetFileName(selected).Equals("REPENTOGONLauncher.exe", StringComparison.OrdinalIgnoreCase)) repentogonPathText.Text = selected;
                    else
                    {
                        MessageBox.Show("Please select REPENTOGONLauncher.exe!", "Wrong file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        repentogonPathText.Text = _settings.RepentogonPath;
                    }
                }
            }
        }


        private void OpenBackupFolderButton(object sender, EventArgs e)
        {
            SaveManager saveManager = new SaveManager(portable.Checked);
            saveManager.OpenBackupFolder();
        }
        private void OpenModManagerButton_Click(object sender, EventArgs e)
        {
            string targetPath = pathText.Text;
            if (portable.Checked) targetPath = Path.Combine(Application.StartupPath, "portable-saac");

            // Ha még nincs nyitva, VAGY bezárták (IsDisposed), akkor újat nyitunk:
            if (_modManagerForm == null || _modManagerForm.IsDisposed)
            {
                _modManagerForm = new ModmanagerForm(targetPath);
                _modManagerForm.Show(this);
            }
            else
            {
                // Ha már nyitva van, csak fókuszba rakjuk és előrehozzuk!
                if (_modManagerForm.WindowState == FormWindowState.Minimized) _modManagerForm.WindowState = FormWindowState.Normal;

                _modManagerForm.BringToFront();
                _modManagerForm.Focus();
            }
        }

        private void OpenLinkOnClick(object sender, EventArgs e)
        {
            if (sender is Button clickedbutton)
            {
                string url = clickedbutton.Tag as string;
                if (!string.IsNullOrWhiteSpace(url)) BrowserHelper.OpenUrl(url);
            }
        }

        private async void CheckForUpdatesButton_Click(object sender, EventArgs e)
        {
            checkForUpdatesButton.Enabled = false;
            try
            {
                await CheckForUpdate(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while checking for updates: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                checkForUpdatesButton.Enabled = true;
            }
        }



        public void ShowMainWindow()
        {
            this.Show();

            if (this.WindowState == FormWindowState.Minimized) this.WindowState = FormWindowState.Normal;

            notifyIcon1.Visible = false;
            this.BringToFront();
            this.Activate();
        }

        private void ShowApp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.Activate();
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }
        }

        private void ShowApp(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
            notifyIcon1.Visible = false;
        }
        private void Close_Click(object sender, EventArgs e)
        {
            exit = true;
            Application.Exit();
        }
        bool exit;
        private void IsaacLauncher_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (trayBox.Checked && !exit)
            {
                e.Cancel = true;
                this.Hide();
                notifyIcon1.Visible = true;
                //notifyIcon1.ShowBalloonTip(1000, "Isaac Launcher", "The launcher is still running in the background.", ToolTipIcon.Info);
            }
            else SaveSettings();
        }

        private async void IsaacLauncher_Shown(object sender, EventArgs e)
        {

            await CheckForUpdate(true);
            if (startTray.Checked)
            {
                this.Hide();
                notifyIcon1.Visible = true;
                //notifyIcon1.ShowBalloonTip(1000, "Isaac Launcher", "The launcher is still running in the background.", ToolTipIcon.Info);
            }
            else
            {
                this.Show();
                this.Activate();
            }
            try
            {
                if (instantLaunch.Checked)
                {
                    Process[] Isaac = Process.GetProcessesByName("isaac-ng");
                    if (Isaac.Length > 0) return;

                    //A play gombot letiltjuk, hogy ne lehessen újra kattintani, amíg a játék elindul
                    SetPlayButtonState(PlayButtonState.Starting);
                    LaunchMode ModeRequested = GetLaunchMode();
                    Launcher instantLauncher = new Launcher(ModeRequested, mods.Checked, pathText.Text, repentogonCheckbox.Checked, repentogonPathText.Text, smart.Checked);

                    await StartGame(instantLauncher);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An unexpected error occurred during instant launch: {ex.Message}");
                MessageBox.Show($"An unexpected error occurred during instant launch: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetPlayButtonState(PlayButtonState.Play);
            }
        }

        private void IsaacLauncher_Load(object sender, EventArgs e)
        {
            string version = Application.ProductVersion;
            label7.Text = "Made by Haltiru - v" + version;

            mainToolTip.AutoPopDelay = 5000;
            mainToolTip.InitialDelay = 500;
            mainToolTip.ReshowDelay = 100;
            mainToolTip.ToolTipTitle = "Info";

            mainToolTip.SetToolTip(modsToolTip, "If checked Isaac is going to start with mods.");
            mainToolTip.SetToolTip(smartStartTooltip, "If checked the launcher will try to start Isaac according to the daily challenge timer. (without mods and repentogon when a new daily is available on the first start)");
            mainToolTip.SetToolTip(borderlessFullscreenTooltip, "If checked Isaac will start in borderless fullscreen mode.");
            mainToolTip.SetToolTip(customToolTip, "If checked the launcher will try to start the custom version of Isaac.");
            mainToolTip.SetToolTip(portableToolTip, "If checked the launcher will try to start the portable version of Isaac.");
            mainToolTip.SetToolTip(instantLaunchToolTip, "If checked the launcher will try to start Isaac instantly when starting the app.");
            mainToolTip.SetToolTip(ghostModeTooltip, "If checked the launcher will delete My Games and/or Binding of Isaac Repentance+ folders if they did not exist before. (only works with portable version)");
            mainToolTip.SetToolTip(systemTrayToolTip, "If checked the launcher will minimize to tray instead of closing.");
            mainToolTip.SetToolTip(startInSystemTrayToolTip, "If checked the launcher will start minimized to tray.");
            mainToolTip.SetToolTip(gamePathToolTip, "The path to the Isaac game folder. The folder must contain the isaac-ng.exe file.");
            mainToolTip.SetToolTip(repentogonToolTip, "If checked the launcher will try to start the Repentogon launcher.");
            mainToolTip.SetToolTip(repentogonPathToolTip, "The path to the Repentogon Launcher exe.");

            var _ = ghostModeTooltip.Handle;
            var __ = gamePathToolTip.Handle;

            SetPlayButtonState(PlayButtonState.Play);
            UIHelper.ApplyHoverEffects(this);
            SetActiveButton(launcherButton);
            _loadingSettings = true;
            try { LoadSettings(); }
            finally { _loadingSettings = false; }

            UpdateUI();
        }

       
    }
}