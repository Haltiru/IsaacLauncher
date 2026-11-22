//Form1.cs
using Microsoft.WindowsAPICodePack.Dialogs;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Media;
using System.Reflection;
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
            private bool _updatingModeUI = false;
            private PrivateFontCollection customFont = new PrivateFontCollection();
            private Dictionary<Button, TabPage> buttonTabPages = new Dictionary<Button, TabPage>();
            ToolTip mainToolTip = new ToolTip();
            private readonly Timer _sessionTimer = new Timer();
            private readonly Stopwatch _sessionStopwatch = new Stopwatch();
            private enum PlayButtonState { Play, Stop, Starting }
            private enum GameStatus { Stopped,Starting,Running,Stopping }
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

                customFont.AddFontFile("upheavtt.ttf");
                this.Icon = Properties.Resources.IsaacLauncher_Icon;
                UIHelper.ApplyCustomFont(this, customFont.Families[0]);
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

            private void SessionTimer_Tick(object sender, EventArgs e)
            {
                UpdateSessionTime();
                UpdateTotalPlaytime();
            }

            private void UpdateSessionTime()
            {
                TimeSpan elapsed = _sessionStopwatch.Elapsed;
                label20.Text = FormatSessionTime(elapsed);
            }
            private string FormatSessionTime(TimeSpan elapsed)
            {
                int hours = (int)elapsed.TotalHours;
                int minutes = elapsed.Minutes;
                int seconds = elapsed.Seconds;

                if (hours == 0) return $"{minutes:00}:{seconds:00}";
                else return $"{hours:00}:{minutes:00}:{seconds:00}";
            }

            private string FormatTotalPlaytime(long totalSeconds)
            {
               return $"{(totalSeconds / 3600.0):F1} hours";
            }

            private void UpdateTotalPlaytime()
            {
                long sessionTotalSeconds = (long)_sessionStopwatch.Elapsed.TotalSeconds;
                long savedTotalSeconds = _settings.LauncherPlaytimeSeconds;
                if (_sessionStopwatch.IsRunning) savedTotalSeconds += sessionTotalSeconds;
                label21.Text = FormatTotalPlaytime(savedTotalSeconds);
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

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x02000000;
                    return cp;
                }
            }
            private void SaveWindowPosition()
            {
                Point location = this.WindowState == FormWindowState.Normal ? this.Location : this.RestoreBounds.Location;
                _settings.OpenX = location.X;
                _settings.OpenY = location.Y;
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
                _settings.AutoBackupBeforePlay = backupBeforePlayCheckbox.Checked;
                SaveWindowPosition();

                AppConfig.Save(_settings);
            }
            private void LoadWindowPosition()
            {
                if (!_settings.OpenX.HasValue || !_settings.OpenY.HasValue) return;
                {
                    Point savedLocation = new Point(_settings.OpenX.Value,_settings.OpenY.Value);
                    Rectangle windowBounds = new Rectangle(savedLocation, this.Size);

                    foreach (Screen screen in Screen.AllScreens)
                    {
                        if (!screen.WorkingArea.Contains(windowBounds)) continue;
                    
                        this.StartPosition = FormStartPosition.Manual;
                        this.Location = savedLocation;
                        break;
                    }
                }
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
                label20.Text = FormatSessionTime(TimeSpan.FromSeconds(_settings.LastSessionSeconds));
                label21.Text = FormatTotalPlaytime(_settings.LauncherPlaytimeSeconds);
                BetaReleaseCheckbox.Checked =_settings.UpdateChannel == UpdateChannel.Beta;


                LoadWindowPosition();
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
                switch (gamestatus)
                {
                    case GameStatus.Stopped:
                        label17.Text = "Stopped";
                        label17.ForeColor = Color.FromArgb(84, 76, 74); 
                        _currentGameStatus = GameStatus.Stopped;
                        break;
                    case GameStatus.Starting:
                        label17.Text = "Starting";
                        label17.ForeColor = Color.FromArgb(125, 72, 0);
                        _currentGameStatus = GameStatus.Starting;
                        break;
                    case GameStatus.Running:
                        label17.Text = "Running";
                        label17.ForeColor = Color.Green;
                        _currentGameStatus = GameStatus.Running;
                        break;
                    case GameStatus.Stopping:
                        label17.Text = "Stopping";
                        label17.ForeColor = Color.Orange;
                        _currentGameStatus = GameStatus.Stopping;
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
                        UpdateSessionTime();
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
                    
                        UpdateSessionTime();
                        UpdateTotalPlaytime();

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

            private async Task<Panel> CreateUpdateOverlay(Control content)
            {

                Bitmap screenshot = null;
                Panel Modal = null;
                try
                {
                    await Task.Delay(100);
                    screenshot = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                    Point topLeft = PointToScreen(Point.Empty);
                    using (Graphics g = Graphics.FromImage(screenshot))
                    {;
                        g.CopyFromScreen(topLeft, Point.Empty, this.ClientSize);
                        using (SolidBrush semiTransparentBrush = new SolidBrush(Color.FromArgb(170, 40, 30, 28))) g.FillRectangle(semiTransparentBrush, new Rectangle(Point.Empty, this.ClientSize));
                    }

                    // We create a modal panel to host the update user control, and add it to the form.
                    Modal = new Panel();

                    Modal.Dock = DockStyle.Fill;
                    Modal.BackgroundImage = screenshot;
                    Modal.TabStop = false;

                    this.Controls.Add(Modal);
                    Modal.Controls.Add(content);

                    content.Location = new Point(this.ClientSize.Width / 2 - content.Width / 2, this.ClientSize.Height / 2 - content.Height / 2);

                    Modal.BringToFront();
                    content.BringToFront();
                    return Modal;
                }
                catch
                {
                    try
                    {
                        if (Modal != null)
                        {
                            Modal.BackgroundImage = null;
                            this.Controls.Remove(Modal);
                            Modal.Dispose();
                        }
                    }
                    finally {if (screenshot != null) screenshot.Dispose();}
                    throw;
                }
            }

            private void RemoveUpdateOverlay(Panel overlay)
            {
                Image img = overlay.BackgroundImage;
                try
                {
                    this.Controls.Remove(overlay);
                    overlay.BackgroundImage = null;
                    overlay.Dispose();
                }
                finally { img?.Dispose(); }
            }


        private async Task<DialogResult> ShowUpdateDialogAsync(string version)
        {
            UpdateUserControl updateUserControl = new UpdateUserControl(version);
            TaskCompletionSource<DialogResult> result = new TaskCompletionSource<DialogResult>();
            updateUserControl.ChoiceSelected += choice => result.TrySetResult(choice);

            Panel modal = await CreateUpdateOverlay(updateUserControl);

            try
            {
                SystemSounds.Asterisk.Play();
                return await result.Task;
            }
            finally
            {
                RemoveUpdateOverlay(modal);
            }
        }


        private async Task ShowUpdateProgressDialogAsync(Func<IProgress<int>, IProgress<string>, Task> operation)
            {
                // We take a screenshot of the current form to use as the background for the modal panel.
                bool isActive = true;
                UpdateProgressUserControl updateProgressUserControl = new UpdateProgressUserControl();
                Progress<int> progressReporter = new Progress<int>(percentage => { if (isActive) updateProgressUserControl.UpdateProgress(percentage); });
                Progress<string> statusReporter = new Progress<string>(status => { if (isActive) updateProgressUserControl.UpdateStatus(status); });

                Panel Modal = await CreateUpdateOverlay(updateProgressUserControl);

                try { await operation(progressReporter, statusReporter); }
                finally
                {
                    isActive = false;
                    RemoveUpdateOverlay(Modal);
                }
            }

            private bool CooldownCheck(bool autoCheck)
            {
                if (!autoCheck) return true;
                if (_settings.Reminder) return true;
                if (string.IsNullOrEmpty(_settings.LastUpdateCheck)) return true;

                DateTime lastCheck;
                if (DateTime.TryParse(_settings.LastUpdateCheck, out lastCheck))
                {
                    TimeSpan timeSinceLastCheck = DateTime.Now - lastCheck;
                    if (timeSinceLastCheck.TotalHours < 0.01) return false;
                }
                return true;
            }
            private async Task<bool> CheckUpdateAvailabilityAsync(Updater updater)
            {
                bool isSuccess = await updater.GetRemoteVersion(_settings.UpdateChannel);
                NuGetVersion checkForNull = updater.RemoteVersion;
                if (isSuccess && checkForNull != null)
                {
                    // Update the last check time
                    _settings.LastUpdateCheck = DateTime.Now.ToString("o");
                    AppConfig.Save(_settings);
                }
                return isSuccess && checkForNull != null;
            }
            private async Task<bool> HandleUpdateDialogAsync(Updater updater)
            {

                DialogResult choiceSelected = await ShowUpdateDialogAsync(updater.RemoteVersion.ToString());
                switch (choiceSelected)
                    {
                    case DialogResult.Yes:
                        _settings.Reminder = false;
                        AppConfig.Save(_settings);
                        try { await ShowUpdateProgressDialogAsync((progressReporter, statusReporter) => updater.StartUpdateProcess(progressReporter, statusReporter)); }
                        catch (Exception ex)
                        {
                            await PopupManager.ShowAsync(this, "UPDATE FAILED", $"The update failed: {ex.Message}", "OK", null, null, SystemSounds.Hand);
                            return false;
                        }
                        break;
                    case DialogResult.No:
                        _settings.Reminder = false;
                        _settings.SkippedUpdateVersion = updater.RemoteVersion.ToString();
                        AppConfig.Save(_settings);
                        break;
                    case DialogResult.Cancel:
                        _settings.Reminder = true;
                        AppConfig.Save(_settings);
                        break;
                    default:
                        return false;
                }
                return true;
            }
            private async Task<bool> CheckForUpdate(bool autoCheck)
            {
                try
                {
                    // We check for updates only if the last check was more than 36 seconds ago (0.01 hours)
                    bool cooldownPassed = CooldownCheck(autoCheck);
                    if (!cooldownPassed) return false;

                    // We get the remote version and compare it with the local version. If the remote version is newer, we prompt the user to update.
                    Updater updater = new Updater();
                    bool checkSucceeded = await CheckUpdateAvailabilityAsync(updater);
                    if (!checkSucceeded)
                    {
                        if (!autoCheck) await PopupManager.ShowAsync(this, "UPDATE CHECK FAILED", "Failed to check for updates. Please check your internet connection and try again.", "OK", null, null, SystemSounds.Hand);
                        return false;
                    }

                // If the user has chosen to skip this version, we don't prompt them again unless they manually check for updates.
                if (updater.IsUpdateAvailable() &&!_settings.Reminder &&_settings.SkippedUpdateVersion == updater.RemoteVersion.ToString() &&autoCheck) return false;
                
                if (updater.IsUpdateAvailable()) return await HandleUpdateDialogAsync(updater);
                    else
                    {
                        if (!autoCheck) await PopupManager.ShowAsync(this, "NO UPDATE AVAILABLE", $"You are using the latest version of the launcher: {updater.LocalVersion}.", "OK", null, null, SystemSounds.Asterisk);
                    
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (!autoCheck) await PopupManager.ShowAsync(this, "UPDATE CHECK ERROR", $"An error occurred while checking for updates: {ex.Message}", "OK", null, null, SystemSounds.Hand);
                    Debug.WriteLine($"CheckForUpdate error: {ex}");
                    return false;
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
                            await PopupManager.ShowAsync(this, "ERROR",$"Failed to stop the game: {ex.Message}","OK", null, null, SystemSounds.Hand);
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

            private async void PlayButton_Click(object sender, EventArgs e) {await LaunchGameAsync(false);}

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

                var choice = await PopupManager.ShowAsync(
                    this,
                    "ENABLE GHOST MODE?",
                    "Ghost Mode automatically deletes temporary Isaac files from the My Games folder after the portable game closes.\n\n" +
                    "This includes options.ini, logs, and save backups stored in that folder.\n\n" +
                    "IMPORTANT: Only use Ghost Mode with a supported portable installation. Make sure your saves are backed up!",
                    "YES",
                    "NO",
                    null,
                    SystemSounds.Exclamation
                );
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
                            await PopupManager.ShowAsync(this, "INVALID FOLDER", "The selected folder does not contain isaac-ng.exe. Please select the correct Isaac game folder.", "OK", null, null, SystemSounds.Hand);
                            pathText.Text = _settings.GamePath;
                        }
                    }
                }
            }

            private async void BrowseRepentogonPath(object sender, EventArgs e)
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
                            await PopupManager.ShowAsync(this, "INVALID FILE", "The selected file is not REPENTOGONLauncher.exe. Please select the correct Repentogon Launcher executable.", "OK", null, null, SystemSounds.Hand);
                            repentogonPathText.Text = _settings.RepentogonPath;
                        }
                    }
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
                try { await CheckForUpdate(false); }
                catch (Exception ex) {await PopupManager.ShowAsync(this, "ERROR", $"An unexpected error occurred while checking for updates: {ex.Message}", "OK", null, null, SystemSounds.Hand); }
                finally {checkForUpdatesButton.Enabled = true;}
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

                try
                {
                    await CheckForUpdate(true);
                }
                finally
                {
                    BetaReleaseCheckbox.Enabled = true;
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
            private void IsaacLauncher_FormClosing(object sender, FormClosingEventArgs e)
            {
                if (trayBox.Checked && !exit)
                {
                    e.Cancel = true;
                    this.Hide();
                    notifyIcon1.Visible = true;
                    notifyIcon1.ShowBalloonTip(1000, "Isaac Launcher", "The launcher is still running in the background.", ToolTipIcon.Info);
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
                    notifyIcon1.ShowBalloonTip(1000, "Isaac Launcher", "The launcher is still running in the background.", ToolTipIcon.Info);
                }
                else
                {
                    ShowMainWindow();
                }
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
                UIHelper.ApplyHoverEffects(this);
                SetActiveButton(launcherButton);
                _loadingSettings = true;
                try { LoadSettings(); }
                finally { _loadingSettings = false; }
                UpdateUI();
            }
        }
    }