namespace IsaacLauncherGUI
{
    partial class ModmanagerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModmanagerForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.disabledModsListBox = new System.Windows.Forms.ListBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.enabledModsListBox = new System.Windows.Forms.ListBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.modDescription = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.searchbBox = new System.Windows.Forms.TextBox();
            this.panel2 = new ReaLTaiizor.Controls.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.enableAll = new System.Windows.Forms.Button();
            this.disableAll = new System.Windows.Forms.Button();
            this.disabledMods = new System.Windows.Forms.Label();
            this.enabledMods = new System.Windows.Forms.Label();
            this.playButton = new System.Windows.Forms.Button();
            this.modName = new System.Windows.Forms.Label();
            this.modImage = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modImage)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.panel6);
            this.panel1.Controls.Add(this.panel5);
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.enableAll);
            this.panel1.Controls.Add(this.disableAll);
            this.panel1.Controls.Add(this.disabledMods);
            this.panel1.Controls.Add(this.enabledMods);
            this.panel1.Controls.Add(this.playButton);
            this.panel1.Controls.Add(this.modName);
            this.panel1.Controls.Add(this.modImage);
            this.panel1.Name = "panel1";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.disabledModsListBox);
            resources.ApplyResources(this.panel6, "panel6");
            this.panel6.Name = "panel6";
            // 
            // disabledModsListBox
            // 
            this.disabledModsListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.disabledModsListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.disabledModsListBox, "disabledModsListBox");
            this.disabledModsListBox.ForeColor = System.Drawing.Color.White;
            this.disabledModsListBox.FormattingEnabled = true;
            this.disabledModsListBox.Name = "disabledModsListBox";
            this.disabledModsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.disabledModsListBox.Tag = "KeepComic";
            this.disabledModsListBox.SelectedIndexChanged += new System.EventHandler(this.selectedMod);
            this.disabledModsListBox.DoubleClick += new System.EventHandler(this.ChangeState);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.enabledModsListBox);
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Name = "panel5";
            // 
            // enabledModsListBox
            // 
            this.enabledModsListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.enabledModsListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.enabledModsListBox, "enabledModsListBox");
            this.enabledModsListBox.ForeColor = System.Drawing.Color.White;
            this.enabledModsListBox.FormattingEnabled = true;
            this.enabledModsListBox.Name = "enabledModsListBox";
            this.enabledModsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.enabledModsListBox.Tag = "KeepComic";
            this.enabledModsListBox.SelectedIndexChanged += new System.EventHandler(this.selectedMod);
            this.enabledModsListBox.DoubleClick += new System.EventHandler(this.ChangeState);
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.modDescription);
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // modDescription
            // 
            this.modDescription.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.modDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.modDescription, "modDescription");
            this.modDescription.ForeColor = System.Drawing.Color.White;
            this.modDescription.HideSelection = false;
            this.modDescription.Name = "modDescription";
            this.modDescription.ReadOnly = true;
            this.modDescription.TabStop = false;
            this.modDescription.Tag = "KeepComic";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.button3);
            this.panel3.Controls.Add(this.searchbBox);
            this.panel3.Controls.Add(this.panel2);
            this.panel3.Controls.Add(this.label1);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.button3, "button3");
            this.button3.Cursor = System.Windows.Forms.Cursors.Default;
            this.button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button3.FlatAppearance.BorderSize = 0;
            this.button3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.button3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.button3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(76)))), ((int)(((byte)(74)))));
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.Close_Click);
            // 
            // searchbBox
            // 
            this.searchbBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.searchbBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.searchbBox, "searchbBox");
            this.searchbBox.ForeColor = System.Drawing.Color.White;
            this.searchbBox.Name = "searchbBox";
            this.searchbBox.TextChanged += new System.EventHandler(this.SearchChanged);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(218)))), ((int)(((byte)(223)))));
            this.panel2.EdgeColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            this.panel2.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // enableAll
            // 
            this.enableAll.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.enableAll, "enableAll");
            this.enableAll.Cursor = System.Windows.Forms.Cursors.Default;
            this.enableAll.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.enableAll.FlatAppearance.BorderSize = 0;
            this.enableAll.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.enableAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.enableAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(76)))), ((int)(((byte)(74)))));
            this.enableAll.Name = "enableAll";
            this.enableAll.UseVisualStyleBackColor = false;
            this.enableAll.Click += new System.EventHandler(this.enableAll_Click);
            // 
            // disableAll
            // 
            this.disableAll.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.disableAll, "disableAll");
            this.disableAll.Cursor = System.Windows.Forms.Cursors.Default;
            this.disableAll.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.disableAll.FlatAppearance.BorderSize = 0;
            this.disableAll.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.disableAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.disableAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(76)))), ((int)(((byte)(74)))));
            this.disableAll.Name = "disableAll";
            this.disableAll.UseVisualStyleBackColor = false;
            this.disableAll.Click += new System.EventHandler(this.disableAll_Click);
            // 
            // disabledMods
            // 
            resources.ApplyResources(this.disabledMods, "disabledMods");
            this.disabledMods.ForeColor = System.Drawing.Color.Gold;
            this.disabledMods.Name = "disabledMods";
            // 
            // enabledMods
            // 
            resources.ApplyResources(this.enabledMods, "enabledMods");
            this.enabledMods.ForeColor = System.Drawing.Color.Gold;
            this.enabledMods.Name = "enabledMods";
            // 
            // playButton
            // 
            this.playButton.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.playButton, "playButton");
            this.playButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.playButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.playButton.FlatAppearance.BorderSize = 0;
            this.playButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.playButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.playButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(76)))), ((int)(((byte)(74)))));
            this.playButton.Name = "playButton";
            this.playButton.UseVisualStyleBackColor = false;
            this.playButton.Click += new System.EventHandler(this.WorkshopButton_Click);
            // 
            // modName
            // 
            resources.ApplyResources(this.modName, "modName");
            this.modName.Name = "modName";
            // 
            // modImage
            // 
            this.modImage.BackgroundImage = global::IsaacLauncherGUI.Properties.Resources.placeholder_200x200;
            resources.ApplyResources(this.modImage, "modImage");
            this.modImage.Name = "modImage";
            this.modImage.TabStop = false;
            // 
            // ModmanagerForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.Transparent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "ModmanagerForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Shown += new System.EventHandler(this.ModManagerShown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox modImage;
        private System.Windows.Forms.TextBox modDescription;
        private System.Windows.Forms.Label modName;
        private System.Windows.Forms.Label enabledMods;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Label disabledMods;
        private System.Windows.Forms.Button enableAll;
        private System.Windows.Forms.Button disableAll;
        private System.Windows.Forms.TextBox searchbBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button button3;
        private ReaLTaiizor.Controls.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.ListBox disabledModsListBox;
        private System.Windows.Forms.ListBox enabledModsListBox;
    }
}