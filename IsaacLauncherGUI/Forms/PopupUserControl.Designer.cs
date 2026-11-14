namespace IsaacLauncherGUI
{
    partial class PopupUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopupUserControl));
            this.title = new System.Windows.Forms.Label();
            this.message = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.primaryButton = new System.Windows.Forms.Button();
            this.secondaryButton = new System.Windows.Forms.Button();
            this.tertiaryButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(58)))), ((int)(((byte)(48)))));
            this.title.Dock = System.Windows.Forms.DockStyle.Fill;
            this.title.Font = new System.Drawing.Font("Upheaval TT (BRK)", 26.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.ForeColor = System.Drawing.Color.Gold;
            this.title.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.title.Location = new System.Drawing.Point(0, 0);
            this.title.Margin = new System.Windows.Forms.Padding(0);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(520, 55);
            this.title.TabIndex = 14;
            this.title.Text = "    placeholder   ";
            this.title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // message
            // 
            this.message.AutoSize = true;
            this.message.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.message.Font = new System.Drawing.Font("Comic Sans MS", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.message.ForeColor = System.Drawing.Color.White;
            this.message.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.message.Location = new System.Drawing.Point(0, 55);
            this.message.Margin = new System.Windows.Forms.Padding(0);
            this.message.MaximumSize = new System.Drawing.Size(520, 1000);
            this.message.Name = "message";
            this.message.Padding = new System.Windows.Forms.Padding(20, 5, 20, 10);
            this.message.Size = new System.Drawing.Size(520, 44);
            this.message.TabIndex = 15;
            this.message.Text = "placeHolderText";
            this.message.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(58)))), ((int)(((byte)(48)))));
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(520, 300);
            this.panel1.TabIndex = 16;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.title, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.message, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(520, 169);
            this.tableLayoutPanel1.TabIndex = 20;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.flowLayoutPanel1.Controls.Add(this.primaryButton);
            this.flowLayoutPanel1.Controls.Add(this.secondaryButton);
            this.flowLayoutPanel1.Controls.Add(this.tertiaryButton);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 109);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(520, 50);
            this.flowLayoutPanel1.TabIndex = 19;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // primaryButton
            // 
            this.primaryButton.BackColor = System.Drawing.Color.Transparent;
            this.primaryButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("primaryButton.BackgroundImage")));
            this.primaryButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.primaryButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.primaryButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.primaryButton.FlatAppearance.BorderSize = 0;
            this.primaryButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.primaryButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.primaryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.primaryButton.Font = new System.Drawing.Font("Upheaval TT (BRK)", 20.25F);
            this.primaryButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(76)))), ((int)(((byte)(74)))));
            this.primaryButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.primaryButton.Location = new System.Drawing.Point(3, 3);
            this.primaryButton.Name = "primaryButton";
            this.primaryButton.Size = new System.Drawing.Size(170, 44);
            this.primaryButton.TabIndex = 16;
            this.primaryButton.Text = "Update";
            this.primaryButton.UseVisualStyleBackColor = false;
            this.primaryButton.Click += new System.EventHandler(this.PrimaryButton_click);
            // 
            // secondaryButton
            // 
            this.secondaryButton.BackColor = System.Drawing.Color.Transparent;
            this.secondaryButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("secondaryButton.BackgroundImage")));
            this.secondaryButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.secondaryButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.secondaryButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.secondaryButton.FlatAppearance.BorderSize = 0;
            this.secondaryButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.secondaryButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.secondaryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.secondaryButton.Font = new System.Drawing.Font("Upheaval TT (BRK)", 20.25F);
            this.secondaryButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(76)))), ((int)(((byte)(74)))));
            this.secondaryButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.secondaryButton.Location = new System.Drawing.Point(179, 3);
            this.secondaryButton.Name = "secondaryButton";
            this.secondaryButton.Size = new System.Drawing.Size(170, 44);
            this.secondaryButton.TabIndex = 17;
            this.secondaryButton.Text = "later";
            this.secondaryButton.UseVisualStyleBackColor = false;
            this.secondaryButton.Click += new System.EventHandler(this.SecondaryButton_click);
            // 
            // tertiaryButton
            // 
            this.tertiaryButton.BackColor = System.Drawing.Color.Transparent;
            this.tertiaryButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tertiaryButton.BackgroundImage")));
            this.tertiaryButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tertiaryButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.tertiaryButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.tertiaryButton.FlatAppearance.BorderSize = 0;
            this.tertiaryButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.tertiaryButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.tertiaryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tertiaryButton.Font = new System.Drawing.Font("Upheaval TT (BRK)", 20.25F);
            this.tertiaryButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(76)))), ((int)(((byte)(74)))));
            this.tertiaryButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tertiaryButton.Location = new System.Drawing.Point(355, 3);
            this.tertiaryButton.Name = "tertiaryButton";
            this.tertiaryButton.Size = new System.Drawing.Size(170, 44);
            this.tertiaryButton.TabIndex = 18;
            this.tertiaryButton.Text = "skip";
            this.tertiaryButton.UseVisualStyleBackColor = false;
            this.tertiaryButton.Click += new System.EventHandler(this.TertiaryButton_click);
            // 
            // PopupUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.Controls.Add(this.panel1);
            this.Name = "PopupUserControl";
            this.Size = new System.Drawing.Size(520, 300);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Label message;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button tertiaryButton;
        private System.Windows.Forms.Button secondaryButton;
        private System.Windows.Forms.Button primaryButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
