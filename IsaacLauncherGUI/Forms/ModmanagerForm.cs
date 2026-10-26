// ModmanagerForm.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

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
            this.Icon = Properties.Resources.beast;
            //UIHelper.ApplyCustomFont(this, customFont.Families[0]);

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
            UIHelper.ApplyHoverEffects(this);
            UIHelper.ApplyCustomFont(this, customFont.Families[0]);
            _CachedMods = GetModItems();
            RefreshLists();
        }

        private void selectedMod(object sender, EventArgs e)
        {
            if (_isSelecting) return;

            var enabledMod = ((ListBox)sender).SelectedItem as ModItem;
            var disabledMod = ((ListBox)sender).SelectedItem as ModItem;
            _selectedMod = enabledMod ?? disabledMod;
            _isSelecting = true;
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
                if (!string.IsNullOrEmpty(_selectedMod.ImagePath) && File.Exists(_selectedMod.ImagePath))
                {
                    if (modImage.Image != null)
                    {
                        modImage.Image.Dispose();
                        modImage.Image = null;
                    }

                    using (var tempBmp = new Bitmap(_selectedMod.ImagePath))
                    {
                        modImage.Image = new Bitmap(tempBmp);
                    }
                }
                else
                {
                    if (modImage.Image != null)
                    {
                        modImage.Image.Dispose();
                        modImage.Image = null;
                    }
                }

            }
            _isSelecting = false;
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
