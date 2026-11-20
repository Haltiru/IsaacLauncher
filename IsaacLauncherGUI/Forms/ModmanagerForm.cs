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



        public ModmanagerForm(string gamePath)
        {
            InitializeComponent();

            _gamePath = gamePath;
            _modsFolderPath = Path.Combine(_gamePath, "mods");

            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            //this.UpdateStyles();

            customFont.AddFontFile("upheavtt.ttf");
            this.Icon = Properties.Resources.IsaacLauncher_Icon;

            UIHelper.ApplyHoverEffects(this);
            UIHelper.ApplyCustomFont(this, customFont.Families[0]);

        }

        public ModItem[] GetModItems()
        {
            ModManager modManager = new ModManager(_gamePath);
            var modItems = modManager.SortMods();
            return modItems;
        }

        public void RefreshLists()
        {
            enabledModsListBox.BeginUpdate();
            disabledModsListBox.BeginUpdate();

            enabledModsListBox.Items.Clear();
            disabledModsListBox.Items.Clear();
            string searchText = searchbBox.Text;

            try
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    enabledModsListBox.Items.AddRange(_CachedMods.Where(m => m.IsEnabled && (m.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 || m.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray());
                    disabledModsListBox.Items.AddRange(_CachedMods.Where(m => !m.IsEnabled && (m.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 || m.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray());
                }
                else
                {
                    enabledModsListBox.Items.AddRange(_CachedMods.Where(m => m.IsEnabled).ToArray());
                    disabledModsListBox.Items.AddRange(_CachedMods.Where(m => !m.IsEnabled).ToArray());
                }
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
        }

        private async void selectedMod(object sender, EventArgs e)
        {
            if (_isSelecting) return; 
            _isSelecting = true;

            try
            {
                var selectedMod = ((ListBox)sender).SelectedItem as ModItem;
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
                    modDescription.Text = _selectedMod.Description;

                    if (modImage.Image != null)
                    {
                        var oldImage = modImage.Image;
                        modImage.Image = null;
                        oldImage?.Dispose();
                    }

                    if (!string.IsNullOrEmpty(_selectedMod.ImagePath) && File.Exists(_selectedMod.ImagePath))
                    {
                        using (var tempBmp = new Bitmap(_selectedMod.ImagePath))
                        {
                            modImage.Image = new Bitmap(tempBmp);
                        }
                    }
                    
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
        private void ChangeState(object sender, EventArgs e)
        {
            var mod = ((ListBox)sender).SelectedItem as ModItem;
            _selectedMod = mod;
            if (mod != null)
            {
                ModManager modManager = new ModManager(_gamePath);
                modManager.ToggleModState(mod);
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
            ModManager modManager = new ModManager(_gamePath);
            ModItem[] search = search = _CachedMods.Where(m => m.IsEnabled && (m.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 || m.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();
            foreach (var mod in search) if (mod.IsEnabled) modManager.ToggleModState(mod);
            RefreshLists();
        }

        private void enableAll_Click(object sender, EventArgs e)
        {
            string searchText = searchbBox.Text;
            ModManager modManager = new ModManager(_gamePath);
            ModItem[] search = _CachedMods.Where(m => !m.IsEnabled && (m.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 || m.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();
            foreach (var mod in search) if (!mod.IsEnabled) modManager.ToggleModState(mod);
            RefreshLists();
        }

        private void SearchChanged(object sender, EventArgs e)
        {
            RefreshLists();
        }
    }
}
