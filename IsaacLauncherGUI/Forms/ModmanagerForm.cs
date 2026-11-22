// ModmanagerForm.cs
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Media;

namespace IsaacLauncherGUI
{
    public partial class ModmanagerForm : Form
    {
        private string _gamePath;
        private string _modsFolderPath;
        private PrivateFontCollection customFont = new PrivateFontCollection();
        private ModItem _selectedMod;
        private ModItem[] _CachedMods;
        private bool _isSelecting = false;
        private MemoryStream _gifStream;
        private readonly BBCodeRenderer _bbCodeRenderer = new BBCodeRenderer();
        private ModManager _modManager;

        public ModmanagerForm(string gamePath)
        {
            InitializeComponent();

            _gamePath = gamePath;
            _modsFolderPath = Path.Combine(_gamePath, "mods");

            _modManager = new ModManager(_gamePath);
            _modManager.ThumbnailDownloaded += OnThumbnailDownloaded;

            customFont.AddFontFile("upheavtt.ttf");
            this.Icon = Properties.Resources.IsaacLauncher_Icon;

            UIHelper.ApplyHoverEffects(this);
            UIHelper.ApplyCustomFont(this, customFont.Families[0]);

        }

        private void OnThumbnailDownloaded(object sender, ModManager.ThumbnailDownloadedEventArgs e)
        {
            Debug.WriteLine($"Downloaded: {e.WorkshopId} - {e.ImagePath}");
            if (IsDisposed || Disposing || !IsHandleCreated) return;
            BeginInvoke (new Action(() =>
            {
                if (IsDisposed || _CachedMods == null) return;
                var mod = _CachedMods.FirstOrDefault(m => m.WorkShopId == e.WorkshopId);
                if (mod != null)
                {
                    mod.ImagePath = e.ImagePath;
                    if (_selectedMod == mod) UpdateModImage();
                }
            }));
        }

        public ModItem[] GetModItems()
        {
            var modItems = _modManager.SortMods();
            return modItems;
        }

        public void RefreshLists()
        {
            int enabledTop = enabledModsListBox.TopIndex;
            int disabledTop = disabledModsListBox.TopIndex;

            enabledModsListBox.BeginUpdate();
            disabledModsListBox.BeginUpdate();


            try
            {
                enabledModsListBox.Items.Clear();
                disabledModsListBox.Items.Clear();
                string searchText = searchbBox.Text;

                var filteredMods = _CachedMods.Where(m => string.IsNullOrEmpty(searchText) || m.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 || m.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

                enabledModsListBox.Items.AddRange(filteredMods.Where(m => m.IsEnabled).ToArray());
                disabledModsListBox.Items.AddRange(filteredMods.Where(m => !m.IsEnabled).ToArray());

                if (enabledModsListBox.Items.Count > 0) enabledModsListBox.TopIndex = Math.Min(enabledTop,enabledModsListBox.Items.Count - 1);
                if (disabledModsListBox.Items.Count > 0)disabledModsListBox.TopIndex = Math.Min(disabledTop,disabledModsListBox.Items.Count - 1);
            }
            finally
            {
                enabledModsListBox.EndUpdate();
                disabledModsListBox.EndUpdate();
            }
     
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ModManagerShown(object sender, EventArgs e)
        {
            _CachedMods = GetModItems();
            RefreshLists();
            var firstMod = _CachedMods.FirstOrDefault(m => m.IsEnabled);

            if (firstMod != null) enabledModsListBox.SelectedItem = firstMod;
        }

        private async void selectedMod(object sender, EventArgs e)
        {
            
            if (_isSelecting) return; 
            _isSelecting = true;

            try
            {
                var selectedMod = ((ListBox)sender).SelectedItem as ModItem;
                if (selectedMod == _selectedMod) return;
                _selectedMod = selectedMod;
                switch (_selectedMod?.IsEnabled)
                {
                    case true:
                        enabledModsListBox.SelectedItem = _selectedMod;
                        disabledModsListBox.SelectedItem = null;
                        break;
                    case false:
                        disabledModsListBox.SelectedItem = _selectedMod;
                        enabledModsListBox.SelectedItem = null;
                        break;
                    default:
                        enabledModsListBox.SelectedItem = null;
                        disabledModsListBox.SelectedItem = null;
                        break;
                }
                if (_selectedMod != null)
                {
                    modName.Text = _selectedMod.DisplayName;
                    _bbCodeRenderer.Render(modDescription, _selectedMod.Description);
                    UpdateModImage();

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                await PopupManager.ShowAsync(
                    this,
                    "Error",
                    $"An error occurred:\n{ex}",
                    "OK",
                    null,
                    null,
                    SystemSounds.Hand
                );
            }
            finally
            {
                _isSelecting = false;
            }
        }

        private void UpdateModImage()
        {
            if (_selectedMod == null) return;

            var oldImage = modImage.Image;
            modImage.Image = new Bitmap(Properties.Resources.placeholder_200x200);
            oldImage?.Dispose();

            _gifStream?.Dispose();
            _gifStream = null;

            if (string.IsNullOrEmpty(_selectedMod.ImagePath) ||!File.Exists(_selectedMod.ImagePath)) return;

            try
            {
                string extension = Path.GetExtension(_selectedMod.ImagePath);
                Image newImage;

                if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    _gifStream = new MemoryStream(File.ReadAllBytes(_selectedMod.ImagePath));
                    newImage = Image.FromStream(_gifStream);
                }
                else using (var tempBmp = new Bitmap(_selectedMod.ImagePath)) newImage = new Bitmap(tempBmp);
                oldImage = modImage.Image;
                modImage.Image = newImage;
                oldImage?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image: {_selectedMod.ImagePath} - {ex}");

                _gifStream?.Dispose();
                _gifStream = null;
            }
        }
        private void ChangeState(object sender, EventArgs e)
        {
            var mod = ((ListBox)sender).SelectedItem as ModItem;
            _selectedMod = mod;
            if (mod != null) 
            {
                _modManager.ToggleModState(mod);
                RefreshLists();
            }
        }

        private void WorkshopButton_Click(object sender, EventArgs e)
        { 
            if (!string.IsNullOrEmpty(_selectedMod.WorkShopLink))
            {
                Process.Start(_selectedMod.WorkShopLink);
            }
        }

        private void disableAll_Click(object sender, EventArgs e)
        {
            string searchText = searchbBox.Text;
            ModItem[] search = _CachedMods.Where(m => m.IsEnabled && (m.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 || m.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();
            foreach (var mod in search) if (mod.IsEnabled) _modManager.ToggleModState(mod);
            RefreshLists();
        }

        private void enableAll_Click(object sender, EventArgs e)
        {
            string searchText = searchbBox.Text;
            ModItem[] search = _CachedMods.Where(m => !m.IsEnabled && (m.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 || m.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();
            foreach (var mod in search) if (!mod.IsEnabled) _modManager.ToggleModState(mod);
            RefreshLists();
        }

        private void SearchChanged(object sender, EventArgs e)
        {
            RefreshLists();
        }

        private void ModManagerClosing(object sender, FormClosingEventArgs e)
        {
            _modManager.ThumbnailDownloaded -= OnThumbnailDownloaded;
            modImage.Image?.Dispose();
            modImage.Image = null;
            _gifStream?.Dispose();
            _gifStream = null;
        }
    }
}
