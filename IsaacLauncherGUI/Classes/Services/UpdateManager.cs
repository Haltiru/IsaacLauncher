//UpdateManager.cs

namespace IsaacLauncherGUI
{
    public class UpdateManager
    {
        private readonly AppSettings _settings;
        private readonly Form _form;

        public UpdateManager(AppSettings settings, Form form)
        {
            _settings = settings;
            _form = form;
        }

        public async Task<bool> CheckForUpdateAsync(bool autoCheck)
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
                    if (!autoCheck) await PopupManager.ShowAsync(_form, "UPDATE CHECK FAILED", "Failed to check for updates. Please check your internet connection and try again.", "OK", null, null, SystemSounds.Hand);
                    return false;
                }

                // If the user has chosen to skip this version, we don't prompt them again unless they manually check for updates.
                bool isUpdateAvailable = updater.IsUpdateAvailable();
                if (isUpdateAvailable && !_settings.Reminder && _settings.SkippedUpdateVersion == updater.RemoteVersion.ToString() && autoCheck) return false;

                if (isUpdateAvailable) return await HandleUpdateDialogAsync(updater);
                else
                {
                    if (!autoCheck) await PopupManager.ShowAsync(_form, "NO UPDATE AVAILABLE", $"You are using the latest version of the launcher: {updater.LocalVersion}.", "OK", null, null, SystemSounds.Asterisk);
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (!autoCheck) await PopupManager.ShowAsync(_form, "UPDATE CHECK ERROR", $"An error occurred while checking for updates: {ex.Message}", "OK", null, null, SystemSounds.Hand);
                Debug.WriteLine($"CheckForUpdate error: {ex}");
                return false;
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

            DialogResult choiceSelected = await PopupManager.ShowUpdateDialogAsync(_form, updater.RemoteVersion.ToString());
            switch (choiceSelected)
            {
                case DialogResult.Yes:
                    _settings.Reminder = false;
                    AppConfig.Save(_settings);
                    try { await PopupManager.ShowUpdateProgressDialogAsync(_form, (progressReporter, statusReporter) => updater.StartUpdateProcess(progressReporter, statusReporter)); }
                    catch (Exception ex)
                    {
                        await PopupManager.ShowAsync(_form, "UPDATE FAILED", $"The update failed: {ex.Message}", "OK", null, null, SystemSounds.Hand);
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
    }
}
