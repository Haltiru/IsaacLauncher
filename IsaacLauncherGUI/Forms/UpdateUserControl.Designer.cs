namespace IsaacLauncherGUI
{
    partial class UpdateUserControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateUserControl));
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.skipButton = new System.Windows.Forms.Button();
            this.laterButton = new System.Windows.Forms.Button();
            this.updateButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Upheaval TT (BRK)", 26.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Gold;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(520, 60);
            this.label3.TabIndex = 14;
            this.label3.Text = "    Update available   ";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.label1.Font = new System.Drawing.Font("Comic Sans MS", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(0, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(520, 165);
            this.label1.TabIndex = 15;
            this.label1.Text = "There is a new version ready to install!";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(58)))), ((int)(((byte)(48)))));
            this.panel1.Controls.Add(this.skipButton);
            this.panel1.Controls.Add(this.laterButton);
            this.panel1.Controls.Add(this.updateButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(520, 300);
            this.panel1.TabIndex = 16;
            // 
            // skipButton
            // 
            this.skipButton.BackColor = System.Drawing.Color.Transparent;
            this.skipButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("skipButton.BackgroundImage")));
            this.skipButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.skipButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.skipButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.skipButton.FlatAppearance.BorderSize = 0;
            this.skipButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.skipButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.skipButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.skipButton.Font = new System.Drawing.Font("Upheaval TT (BRK)", 20.25F);
            this.skipButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(76)))), ((int)(((byte)(74)))));
            this.skipButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.skipButton.Location = new System.Drawing.Point(350, 240);
            this.skipButton.Name = "skipButton";
            this.skipButton.Size = new System.Drawing.Size(170, 44);
            this.skipButton.TabIndex = 18;
            this.skipButton.Text = "skip";
            this.skipButton.UseVisualStyleBackColor = false;
            this.skipButton.Click += new System.EventHandler(this.skipButton_Click);
            // 
            // laterButton
            // 
            this.laterButton.BackColor = System.Drawing.Color.Transparent;
            this.laterButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("laterButton.BackgroundImage")));
            this.laterButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.laterButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.laterButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.laterButton.FlatAppearance.BorderSize = 0;
            this.laterButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.laterButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.laterButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.laterButton.Font = new System.Drawing.Font("Upheaval TT (BRK)", 20.25F);
            this.laterButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(76)))), ((int)(((byte)(74)))));
            this.laterButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.laterButton.Location = new System.Drawing.Point(175, 240);
            this.laterButton.Name = "laterButton";
            this.laterButton.Size = new System.Drawing.Size(170, 44);
            this.laterButton.TabIndex = 17;
            this.laterButton.Text = "later";
            this.laterButton.UseVisualStyleBackColor = false;
            this.laterButton.Click += new System.EventHandler(this.laterButton_Click);
            // 
            // updateButton
            // 
            this.updateButton.BackColor = System.Drawing.Color.Transparent;
            this.updateButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("updateButton.BackgroundImage")));
            this.updateButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.updateButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.updateButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.updateButton.FlatAppearance.BorderSize = 0;
            this.updateButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.updateButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.updateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.updateButton.Font = new System.Drawing.Font("Upheaval TT (BRK)", 20.25F);
            this.updateButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(76)))), ((int)(((byte)(74)))));
            this.updateButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.updateButton.Location = new System.Drawing.Point(0, 240);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(170, 44);
            this.updateButton.TabIndex = 16;
            this.updateButton.Text = "Update";
            this.updateButton.UseVisualStyleBackColor = false;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // UpdateUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.Controls.Add(this.panel1);
            this.Name = "UpdateUserControl";
            this.Size = new System.Drawing.Size(520, 300);
            this.Load += new System.EventHandler(this.onUpdateUserControlLoad);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button skipButton;
        private System.Windows.Forms.Button laterButton;
        private System.Windows.Forms.Button updateButton;
    }
}
