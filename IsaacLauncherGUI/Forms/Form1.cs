//Form1.cs
using System.Runtime.InteropServices;
using static IsaacLauncherGUI.Launcher;

namespace IsaacLauncherGUI
{
    public partial class IsaacLauncher : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
        private AppSettings _settings;
        private bool _loadingSettings = false;
        private ModmanagerForm _modManagerForm = null;
        private bool _updatingModeUI = false;

        private UpdateManager _updateManager;
        private Dictionary<Button, TabPage> buttonTabPages = new Dictionary<Button, TabPage>();
        ToolTip mainToolTip = new ToolTip();
        private readonly Timer _sessionTimer = new Timer();
        private readonly Stopwatch _sessionStopwatch = new Stopwatch();
        private enum PlayButtonState { Play, Stop, Starting }
        private enum GameStatus { Stopped, Starting, Running, Stopping }
        private GameStatus _currentGameStatus = GameStatus.Stopped;
        private static readonly Color DisabledColor = Color.FromArgb(110, 110, 110);
        public IsaacLauncher()
        {
            InitializeComponent();

            _sessionTimer.Interval = 1000;
            _sessionTimer.Tick += SessionTimer_Tick;

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

            FontManager.ApplyFont(this);
            UIHelper.ApplyHoverEffects(this);
            this.Icon = Properties.Resources.IsaacLauncher_Icon;
        }


#if DEBUG

    private async void StartPortable(object sender, EventArgs e)
            {
                portableCreate.Enabled = false;
                if (string.IsNullOrWhiteSpace(pathText.Text) || !Directory.Exists(pathText.Text))
                {
                    await PopupManager.ShowAsync(this, "INVALID PATH", "Please select a valid Isaac game folder before creating a portable version.", "OK", null, null, SystemSounds.Hand);
                    portableCreate.Enabled = true;
                    return;
                }
                PortableIsaacLauncher startPortable = new PortableIsaacLauncher(pathText.Text);
                string err = await Task.Run(() => startPortable.MakeIsaacPortable());

                if (string.IsNullOrEmpty(err))
                {
                    await PopupManager.ShowAsync(this, "SUCCESS", "Portable version created successfully!", "OK", null, null, SystemSounds.Asterisk);

                }
                else
                {
                    await PopupManager.ShowAsync(this, "PORTABLE CREATION ERROR", err, "OK", null, null, SystemSounds.Hand);
                }
                portableCreate.Enabled = true;
        
            } 

#endif

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        private void ShowAndFocusWindow()
        {
            if (InvokeRequired)
            {
                BeginInvoke(ShowAndFocusWindow);
                return;
            }

            if (!Visible)Show();
            if (WindowState == FormWindowState.Minimized)ShowWindow(Handle, SW_RESTORE);

            SetForegroundWindow(Handle);
            Activate();
        }

        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            PlaytimeTracker.UpdateSessionTime(_sessionStopwatch, sessionTime);
            PlaytimeTracker.UpdateTotalPlaytime(_sessionStopwatch, totalTime, _settings);
        }
        private void InitializeToolTips()
        {
            var tooltips = new Dictionary<Control, string>
            {
                { modsToolTip, "If checked Isaac is going to start with mods." },
                { smartStartTooltip, "If checked the launcher will try to start Isaac according to the daily challenge timer. (without mods and repentogon when a new daily is available on the first start)" },
                { borderlessFullscreenTooltip, "If checked Isaac will start in borderless fullscreen mode." },
                { customToolTip, "If checked the launcher will try to start the custom version of Isaac." },
                { portableToolTip, "If checked the launcher will try to start the portable version of Isaac." },
                { instantLaunchToolTip, "If checked Isaac will start instantly when starting the app." },
                { ghostModeTooltip, "If checked the launcher will delete My Games and/or Binding of Isaac Repentance+ folders if they did not exist before. (only works with portable version)" },
                { systemTrayToolTip, "If checked the launcher will minimize to tray instead of closing." },
                { startInSystemTrayToolTip, "If checked the launcher will start minimized to tray." },
                { repentogonToolTip, "If checked the launcher will try to start the Repentogon launcher." },
                { repentogonPathToolTip, "The path to the Repentogon Launcher exe." },
                { gamePathToolTip, "The path to the Isaac game folder. The folder must contain the isaac-ng.exe file." },
                {backupBeforePlayToolip, "If checked the launcher will backup your saves before starting the game." }
            };

            UIHelper.SetToolTips(mainToolTip, tooltips);
        }

        private void SetCheckBoxEnabled(CheckBox box, bool enabled)
        {
            if (!enabled) box.Checked = false;
            box.AutoCheck = enabled;
            box.ForeColor = enabled ? Color.White : DisabledColor;
        }

        private void UpdateLaunchModeUI()
        {
            SetCheckBoxEnabled(portable, !custom.Checked);
            SetCheckBoxEnabled(custom, !portable.Checked);

            bool isSteam = GetLaunchMode() == LaunchMode.Steam;

            SetCheckBoxEnabled(repentogonCheckbox, isSteam);
        }

        private void UpdateStreakUI()
        {
            bool isSmart = smart.Checked;

            label9.Visible = isSmart;
            label10.Visible = !isSmart;

            label9.Text = _settings.DailyStreak.ToString();
            label10.Text = _settings.Streak.ToString();
        }

        private void UpdatePortableUI()
        {
            bool isPortable = portable.Checked;

            ghost.Visible = isPortable;
            ghostModeTooltip.Visible = isPortable;

            gamePathToolTip.Visible = !isPortable;
            pathText.Visible = !isPortable;
            pathLine.Visible = !isPortable;
            gameExeLabel.Visible = !isPortable;

            portableCreate.Enabled = !isPortable;

            if (!isPortable) ghost.Checked = false;
        }

        private void UpdateModeUI()
        {
            if (_updatingModeUI) return;

            _updatingModeUI = true;
            try
            {
                UpdateLaunchModeUI();
                UpdatePortableUI();
            }
            finally { _updatingModeUI = false; }
        }

        public void UpdateUI()
        {
            UpdateModeUI();
            UpdateStreakUI();
            UpdateGameInfo();
        }

        private void SetActiveButton(Button selectedButton) { UIHelper.SetActiveNavigationButton(selectedButton, tabControl1, buttonTabPages); }
        private void NavigationButton_Click(object sender, EventArgs e) { SetActiveButton(sender as Button); }

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
            _settings.AutoBackupBeforePlay = backupBeforePlayCheckbox.Checked;
            WindowPositionManager.SaveWindowPosition(this, _settings);

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
            backupBeforePlayCheckbox.Checked = _settings.AutoBackupBeforePlay;
            sessionTime.Text = PlaytimeTracker.FormatSessionTime(TimeSpan.FromSeconds(_settings.LastSessionSeconds));
            totalTime.Text = PlaytimeTracker.FormatTotalPlaytime(_settings.LauncherPlaytimeSeconds);
            BetaReleaseCheckbox.Checked = _settings.UpdateChannel == UpdateChannel.Beta;
            WindowPositionManager.LoadWindowPosition(this, _settings);
        }

        private void SetPlayButtonState(PlayButtonState playButtonState)
        {
            switch (playButtonState)
            {
                case PlayButtonState.Play:
                    playButton.Text = "Play";
                    playButton.Enabled = true;
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
                    break;
            }
            if (playButtonState != PlayButtonState.Stop)
            {
                playButton.BackgroundImage = Properties.Resources.button;
                playButton.ForeColor = Color.FromArgb(84, 76, 74);
            }
        }

        private void SetGameStatus(GameStatus gamestatus)
        {
            _currentGameStatus = gamestatus;
            switch (gamestatus)
            {
                case GameStatus.Stopped:
                    label17.Text = "Stopped";
                    label17.ForeColor = Color.FromArgb(84, 76, 74);
                    break;
                case GameStatus.Starting:
                    label17.Text = "Starting";
                    label17.ForeColor = Color.FromArgb(125, 72, 0);
                    break;
                case GameStatus.Running:
                    label17.Text = "Running";
                    label17.ForeColor = Color.Green;
                    break;
                case GameStatus.Stopping:
                    label17.Text = "Stopping";
                    label17.ForeColor = Color.Orange;
                    break;
            }
            if (_currentGameStatus == GameStatus.Stopped && _settings != null && !_loadingSettings) UpdateGameInfo();
        }

        public async Task<string> FindIsaac(bool enableFullscreen, Launcher launcher, bool ghostMode)
        {
            bool hasExited = false;
            bool isPortable = launcher.CurrentMode == LaunchMode.Portable;

            try
            {
                // 1. Várjuk meg, hogy a játék és a Repentogon elinduljon
                StartupLog.Write($"GameStartupMonitor START | Repentogon={launcher.IsRepentogonEnabled}");
                GameStartupResult gameProcessResult = await Task.Run(() => GameStartupMonitor.WaitForGameStartupAsync(launcher.IsRepentogonEnabled));
                Process Isaac = gameProcessResult.Process;

                if (Isaac == null)
                {
                    StartupLog.Write($"GameStartupMonitor FAILED: {gameProcessResult.ErrorMessage}");
                    return gameProcessResult.ErrorMessage;
                }

                StartupLog.Write($"Isaac detected successfully | PID={Isaac.Id}");


                // Most már biztonságosan méretezzük a futó ablakot
                if (enableFullscreen)
                {
                    bool result = GameWindowManager.ApplyBorderlessFullscreen(Isaac);
                    if (!result) Debug.WriteLine("Failed to apply borderless fullscreen.");
                }

                // Megvárjuk, amíg a játék bezáródik (ez is háttérben megy)
                if (!Isaac.HasExited)
                {
                    SetPlayButtonState(PlayButtonState.Stop);
                    SetGameStatus(GameStatus.Running);
                    _sessionStopwatch.Restart();
                    PlaytimeTracker.UpdateSessionTime(_sessionStopwatch, sessionTime);
                    _sessionTimer.Start();
                }

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
                _sessionTimer.Stop();

                if (_sessionStopwatch.IsRunning)
                {
                    _sessionStopwatch.Stop();
                    _settings.LastSessionSeconds = (long)_sessionStopwatch.Elapsed.TotalSeconds;
                    _settings.LauncherPlaytimeSeconds += (long)_sessionStopwatch.Elapsed.TotalSeconds;

                    AppConfig.Save(_settings);

                    PlaytimeTracker.UpdateSessionTime(_sessionStopwatch, sessionTime);
                    PlaytimeTracker.UpdateTotalPlaytime(_sessionStopwatch, totalTime, _settings);

                }
                StartupLog.Write($"Game monitoring finished | GameExited={hasExited}");
                SetPlayButtonState(PlayButtonState.Play);
                SetGameStatus(GameStatus.Stopped);

                // Ha a játék bezárult, és a Ghost Mode be van kapcsolva, próbáljuk meg kitakarítani a My Games mappát
                bool cleanupResult = GameCleanupManager.TryCleanup(hasExited, ghostMode, isPortable, launcher);
                if (!cleanupResult && ghostMode && isPortable && Process.GetProcessesByName("isaac-ng").Length == 0) Debug.WriteLine("Failed to cleanup after Isaac.");
            }
            return null;
        }

        public LaunchMode GetLaunchMode()
        {
            if (custom.Checked) return LaunchMode.Custom;
            if (portable.Checked) return LaunchMode.Portable;
            return LaunchMode.Steam;
        }

        private async Task<bool> CheckForErrors(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage)) return false;
            {
                ShowMainWindow();
                await PopupManager.ShowAsync(this, "LAUNCH ERROR", errorMessage, "OK", null, null, SystemSounds.Hand);
                return true;
            }
        }

        public async Task StartGame(Launcher launcher)
        {
            bool BorderlessEnabled = borderless.Checked;
            bool ModsEnabled = mods.Checked;
            bool RepentogonEnabled = repentogonCheckbox.Checked;
            bool isBackupBeforePlayEnabled = backupBeforePlayCheckbox.Checked;
            bool ghostMode = ghost.Checked;

            if (isBackupBeforePlayEnabled)
            {
                try
                {
                    SaveManager saveManager = new SaveManager(portable.Checked);
                    saveManager.Save(true);
                }
                catch (Exception ex)
                {
                    ShowMainWindow();
                    await PopupManager.ShowAsync(this, "BACKUP FAILED", $"An error occurred while backing up your saves: {ex.Message}", "OK", null, null, SystemSounds.Hand);
                    SetPlayButtonState(PlayButtonState.Play);
                    SetGameStatus(GameStatus.Stopped);
                    return;
                }
            }


            string hiba = await Task.Run(() => launcher.InitializeAndLaunch(BorderlessEnabled, ModsEnabled, RepentogonEnabled, _settings));
            UpdateStreakUI();
            if (await CheckForErrors(hiba))
            {
                SetPlayButtonState(PlayButtonState.Play);
                SetGameStatus(GameStatus.Stopped);
                return;
            }

            UpdateRunningGameInfo(launcher);
            hiba = await FindIsaac(BorderlessEnabled, launcher, ghostMode);
            if (await CheckForErrors(hiba)) return;
        }

        private async Task LaunchGameAsync(bool isInstantLaunch)
        {
            try
            {

                StartupLog.Write("========== NEW GAME LAUNCH ==========");
                StartupLog.Write(
                    $"Mode={GetLaunchMode()} | Repentogon={repentogonCheckbox.Checked} | " +
                    $"SmartStart={smart.Checked} | Mods={mods.Checked}");

                Process[] Isaac = Process.GetProcessesByName("isaac-ng");
                if (Isaac.Length > 0)
                {
                    if (isInstantLaunch) return;

                    try
                    {
                        Isaac[0].Kill();
                        return;
                    }
                    catch (Exception ex)
                    {
                        await PopupManager.ShowAsync(this, "ERROR", $"Failed to stop the game: {ex.Message}", "OK", null, null, SystemSounds.Hand);
                        return;
                    }
                }
                SetPlayButtonState(PlayButtonState.Starting);
                SetGameStatus(GameStatus.Starting);
                LaunchMode ModeRequested = GetLaunchMode();
                Launcher isaacLauncher = new Launcher(ModeRequested, mods.Checked, pathText.Text, repentogonCheckbox.Checked, repentogonPathText.Text, smart.Checked);
                await StartGame(isaacLauncher);

            }
            catch (Exception ex)
            {
                StartupLog.Write($"LAUNCH ERROR: {ex}");
                ShowMainWindow();
                await PopupManager.ShowAsync(this, "ERROR", $"An unexpected error occurred: {ex.Message}", "OK", null, null, SystemSounds.Hand);
                SetPlayButtonState(PlayButtonState.Play);
                SetGameStatus(GameStatus.Stopped);
            }
        }

        private async void PlayButton_Click(object sender, EventArgs e) { await LaunchGameAsync(false); }

        private async void BackupButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveManager saveManager = new SaveManager(portable.Checked);
                saveManager.Save();
                await PopupManager.ShowAsync(this, "BACKUP COMPLETE", "Your saves have been backed up successfully!", "OK", null, null, SystemSounds.Asterisk);
            }
            catch (Exception ex)
            {
                await PopupManager.ShowAsync(this, "BACKUP FAILED", $"An error occurred while backing up your saves: {ex.Message}", "OK", null, null, SystemSounds.Hand);
            }
        }

        private async void OpenBackupFolderButton(object sender, EventArgs e)
        {
            try
            {
                SaveManager saveManager = new SaveManager(portable.Checked);
                saveManager.OpenBackupFolder();
            }
            catch (Exception ex) { await PopupManager.ShowAsync(this, "ERROR", $"An error occurred while trying to open the backup folder: {ex.Message}", "OK", null, null, SystemSounds.Hand); }
        }

        private async void ghost_CheckedChanged(object sender, EventArgs e)
        {
            if (!ghost.Checked || _loadingSettings) return;

            var choice = await PopupManager.ShowAsync(this, "ENABLE GHOST MODE?", "Ghost Mode automatically deletes temporary Isaac files from the My Games folder after the portable game closes.\n\n" + "This includes options.ini, logs, and save backups stored in that folder.\n\n" + "IMPORTANT: Only use Ghost Mode with a supported portable installation. Make sure your saves are backed up!", "YES", "NO", null, SystemSounds.Exclamation);
            if (choice != PopupUserControl.PopupChoice.Primary) ghost.Checked = false;
        }

        private void UpdateGameInfo()
        {
            if (_currentGameStatus != GameStatus.Stopped) return;
            SetGameInfo(GetLaunchMode(), mods.Checked, repentogonCheckbox.Checked);
        }

        private void UpdateRunningGameInfo(Launcher launcher)
        {
            LaunchMode mode = launcher.CurrentMode;
            SetGameInfo(mode, launcher.AreModsEnabled, launcher.IsRepentogonEnabled);
        }

        private void SetGameInfo(LaunchMode mode, bool modsEnabled, bool repentogonEnabled)
        {
            label23.Text = mode.ToString();
            label25.Text = $"{(repentogonEnabled ? "ON" : "OFF")} / {(modsEnabled ? "ON" : "OFF")}";
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings || _settings == null) return;

            if (sender == portable || sender == custom) UpdateModeUI();
            else if (sender == smart) UpdateStreakUI();

            UpdateGameInfo();
        }
        private async void BrowseGamePath(object sender, EventArgs e)
        {
            string selectedFolder = FileDialogHelper.SelectFolder("Select your Isaac game folder");
            if (selectedFolder == null) return;
            if (File.Exists(Path.Combine(selectedFolder, "isaac-ng.exe"))) pathText.Text = selectedFolder;
            else
            {
                await PopupManager.ShowAsync(this, "INVALID FOLDER", "The selected folder does not contain isaac-ng.exe. Please select the correct Isaac game folder.", "OK", null, null, SystemSounds.Hand);
                pathText.Text = _settings.GamePath;
            }
        }

        private async void BrowseRepentogonPath(object sender, EventArgs e)
        {
            string repentogonExePath = FileDialogHelper.SelectFile("Search for your Repentogon Launcher exe!", "Executable", "*.exe");
            if (repentogonExePath == null) return;
            if (Path.GetFileName(repentogonExePath).Equals("REPENTOGONLauncher.exe", StringComparison.OrdinalIgnoreCase)) repentogonPathText.Text = repentogonExePath;
            else
            {
                await PopupManager.ShowAsync(this, "INVALID FILE", "The selected file is not REPENTOGONLauncher.exe. Please select the correct Repentogon Launcher executable.", "OK", null, null, SystemSounds.Hand);
                repentogonPathText.Text = _settings.RepentogonPath;
            }
        }
        private void OpenModManagerButton_Click(object sender, EventArgs e)
        {
            string targetPath = pathText.Text;
            if (portable.Checked) targetPath = Path.Combine(Application.StartupPath, "portable-saac");

            // Ha még nincs nyitva, VAGY bezárták (IsDisposed), akkor újat nyitunk:
            if (_modManagerForm == null || _modManagerForm.IsDisposed)
            {
                _modManagerForm = new ModmanagerForm(targetPath);
                _modManagerForm.Show();
            }
            // Ha már nyitva van, csak fókuszba rakjuk és előrehozzuk!
            else WindowActivationManager.TryBringToForeground(_modManagerForm);
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
            try { await _updateManager.CheckForUpdateAsync(false); }
            catch (Exception ex) { await PopupManager.ShowAsync(this, "ERROR", $"An unexpected error occurred while checking for updates: {ex.Message}", "OK", null, null, SystemSounds.Hand); }
            finally { checkForUpdatesButton.Enabled = true; }
        }

        private async void betaUpdatesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadingSettings || _settings == null) return;

            UpdateChannel newChannel = BetaReleaseCheckbox.Checked ? UpdateChannel.Beta : UpdateChannel.Stable;

            if (_settings.UpdateChannel == newChannel) return;

            _settings.UpdateChannel = newChannel;

            // Reset update preferences when switching channels.
            _settings.LastUpdateCheck = string.Empty;
            _settings.Reminder = false;
            _settings.SkippedUpdateVersion = string.Empty;

            AppConfig.Save(_settings);

            // Immediately check for updates on the selected channel.
            BetaReleaseCheckbox.Enabled = false;

            try { await _updateManager.CheckForUpdateAsync(true); }
            finally { BetaReleaseCheckbox.Enabled = true; }
        }

        public void ShowMainWindow()
        {
            WindowActivationManager.TryBringToForeground(this);
            notifyIcon1.Visible = false;
        }
        private void ShowApp(object sender, EventArgs e)
        {
            if (e is MouseEventArgs mouseEvent && mouseEvent.Button != MouseButtons.Left) return;
            ShowMainWindow();
        }
        private void Close_Click(object sender, EventArgs e)
        {
            exit = true;
            Application.Exit();
        }
        bool exit;

        private void MinimizeToTray()
        {
            this.Hide();
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(1000, "Isaac Launcher", "The launcher is still running in the background.", ToolTipIcon.Info);
        }
        private void IsaacLauncher_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (trayBox.Checked && !exit)
            {
                e.Cancel = true;
                MinimizeToTray();
            }
            else SaveSettings();
        }

        private async void IsaacLauncher_Shown(object sender, EventArgs e)
        {
            if (!startTray.Checked) ShowMainWindow();
            await _updateManager.CheckForUpdateAsync(true);

            if (startTray.Checked) MinimizeToTray();

            else ShowMainWindow();
            if (instantLaunch.Checked) await LaunchGameAsync(true);
        }

        private void IsaacLauncher_Load(object sender, EventArgs e)
        {
            string version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            label7.Text = "Made by Haltiru - v" + version;
            var _ = ghostModeTooltip.Handle;
            var __ = gamePathToolTip.Handle;
            InitializeToolTips();
            SetPlayButtonState(PlayButtonState.Play);
            SetGameStatus(GameStatus.Stopped);
            SetActiveButton(launcherButton);
            _loadingSettings = true;
            try { LoadSettings(); }
            finally { _loadingSettings = false; }
            _updateManager = new UpdateManager(_settings, this);
            UpdateUI();
        }
    }
}