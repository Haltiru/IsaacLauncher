namespace IsaacLauncherGUI
{
    partial class UpdateProgressUserControl
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.statusLabel = new System.Windows.Forms.Label();
            this.updaterTitle = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.percentageLabel = new System.Windows.Forms.Label();
            this.backPanel = new System.Windows.Forms.Panel();
            this.progressBar = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.backPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(58)))), ((int)(((byte)(48)))));
            this.panel1.Controls.Add(this.statusLabel);
            this.panel1.Controls.Add(this.updaterTitle);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(520, 300);
            this.panel1.TabIndex = 17;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoEllipsis = true;
            this.statusLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.statusLabel.Font = new System.Drawing.Font("Comic Sans MS", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.statusLabel.ForeColor = System.Drawing.Color.White;
            this.statusLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.statusLabel.Location = new System.Drawing.Point(0, 78);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(520, 58);
            this.statusLabel.TabIndex = 16;
            this.statusLabel.Text = "downloading package...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.updaterTitle.BackColor = System.Drawing.Color.Transparent;
            this.updaterTitle.Font = new System.Drawing.Font("Upheaval TT (BRK)", 26.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updaterTitle.ForeColor = System.Drawing.Color.Gold;
            this.updaterTitle.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.updaterTitle.Location = new System.Drawing.Point(0, 0);
            this.updaterTitle.Name = "label3";
            this.updaterTitle.Size = new System.Drawing.Size(520, 60);
            this.updaterTitle.TabIndex = 14;
            this.updaterTitle.Text = "   downloading update   ";
            this.updaterTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.panel2.Controls.Add(this.backPanel);
            this.panel2.Controls.Add(this.percentageLabel);
            this.panel2.Location = new System.Drawing.Point(0, 78);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(520, 219);
            this.panel2.TabIndex = 18;
            // 
            // percentageLabel
            // 
            this.percentageLabel.AutoEllipsis = true;
            this.percentageLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.percentageLabel.Font = new System.Drawing.Font("Comic Sans MS", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.percentageLabel.ForeColor = System.Drawing.Color.White;
            this.percentageLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.percentageLabel.Location = new System.Drawing.Point(3, 124);
            this.percentageLabel.Name = "percentageLabel";
            this.percentageLabel.Size = new System.Drawing.Size(520, 49);
            this.percentageLabel.TabIndex = 19;
            this.percentageLabel.Text = "0%";
            this.percentageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // backPanel
            // 
            this.backPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(58)))), ((int)(((byte)(48)))));
            this.backPanel.Controls.Add(this.progressBar);
            this.backPanel.Location = new System.Drawing.Point(50, 70);
            this.backPanel.Name = "backPanel";
            this.backPanel.Size = new System.Drawing.Size(420, 30);
            this.backPanel.TabIndex = 20;
            // 
            // progressBar
            // 
            this.progressBar.BackColor = System.Drawing.Color.Gold;
            this.progressBar.Location = new System.Drawing.Point(0, 0);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(193, 30);
            this.progressBar.TabIndex = 19;
            // 
            // UpdateProgressUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(67)))), ((int)(((byte)(56)))));
            this.Controls.Add(this.panel1);
            this.Name = "UpdateProgressUserControl";
            this.Size = new System.Drawing.Size(520, 300);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.backPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label updaterTitle;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label percentageLabel;
        private System.Windows.Forms.Panel backPanel;
        private System.Windows.Forms.FlowLayoutPanel progressBar;
    }
}
