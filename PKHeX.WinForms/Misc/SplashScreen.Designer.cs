namespace PKHeX.WinForms
{
    partial class SplashScreen
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
            L_Status = new System.Windows.Forms.Label();
            L_Site = new System.Windows.Forms.Label();
            L_Title = new System.Windows.Forms.Label();
            L_Version = new System.Windows.Forms.Label();
            L_Tagline = new System.Windows.Forms.Label();
            PB_Icon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)PB_Icon).BeginInit();
            SuspendLayout();
            //
            // PB_Icon
            //
            PB_Icon.BackColor = System.Drawing.Color.Transparent;
            PB_Icon.BackgroundImage = global::PKHeX.WinForms.Properties.Resources.Icon.ToBitmap();
            PB_Icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            PB_Icon.Location = new System.Drawing.Point(30, 30);
            PB_Icon.Name = "PB_Icon";
            PB_Icon.Size = new System.Drawing.Size(64, 64);
            PB_Icon.TabIndex = 2;
            PB_Icon.TabStop = false;
            //
            // L_Title
            //
            L_Title.AutoSize = true;
            L_Title.BackColor = System.Drawing.Color.Transparent;
            L_Title.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            L_Title.ForeColor = System.Drawing.Color.FromArgb(180, 100, 255);
            L_Title.Location = new System.Drawing.Point(110, 25);
            L_Title.Name = "L_Title";
            L_Title.Size = new System.Drawing.Size(230, 45);
            L_Title.TabIndex = 3;
            L_Title.Text = "PKM-Universe";
            //
            // L_Tagline
            //
            L_Tagline.AutoSize = true;
            L_Tagline.BackColor = System.Drawing.Color.Transparent;
            L_Tagline.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Italic);
            L_Tagline.ForeColor = System.Drawing.Color.FromArgb(150, 140, 180);
            L_Tagline.Location = new System.Drawing.Point(112, 70);
            L_Tagline.Name = "L_Tagline";
            L_Tagline.Size = new System.Drawing.Size(195, 20);
            L_Tagline.TabIndex = 5;
            L_Tagline.Text = "The Ultimate Save Editor";
            //
            // L_Version
            //
            L_Version.AutoSize = true;
            L_Version.BackColor = System.Drawing.Color.Transparent;
            L_Version.Font = new System.Drawing.Font("Segoe UI", 9F);
            L_Version.ForeColor = System.Drawing.Color.FromArgb(120, 110, 150);
            L_Version.Location = new System.Drawing.Point(30, 110);
            L_Version.Name = "L_Version";
            L_Version.Size = new System.Drawing.Size(180, 15);
            L_Version.TabIndex = 4;
            L_Version.Text = "Version 25.01 | .NET 10 Preview";
            //
            // L_Status
            //
            L_Status.AutoSize = true;
            L_Status.BackColor = System.Drawing.Color.Transparent;
            L_Status.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            L_Status.ForeColor = System.Drawing.Color.FromArgb(200, 190, 220);
            L_Status.Location = new System.Drawing.Point(30, 140);
            L_Status.Name = "L_Status";
            L_Status.Size = new System.Drawing.Size(100, 19);
            L_Status.TabIndex = 0;
            L_Status.Text = "Initializing...";
            //
            // L_Site
            //
            L_Site.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            L_Site.AutoSize = true;
            L_Site.BackColor = System.Drawing.Color.Transparent;
            L_Site.Font = new System.Drawing.Font("Segoe UI", 8F);
            L_Site.ForeColor = System.Drawing.Color.FromArgb(100, 90, 130);
            L_Site.Location = new System.Drawing.Point(255, 110);
            L_Site.Name = "L_Site";
            L_Site.Size = new System.Drawing.Size(130, 13);
            L_Site.TabIndex = 1;
            L_Site.Text = "discord.gg/pkm-universe";
            //
            // SplashScreen
            //
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            BackColor = System.Drawing.Color.FromArgb(15, 12, 25);
            ClientSize = new System.Drawing.Size(420, 180);
            Controls.Add(L_Title);
            Controls.Add(L_Tagline);
            Controls.Add(L_Version);
            Controls.Add(PB_Icon);
            Controls.Add(L_Site);
            Controls.Add(L_Status);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = Properties.Resources.Icon;
            Name = "SplashScreen";
            Opacity = 0.97D;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            FormClosing += SplashScreen_FormClosing;
            ((System.ComponentModel.ISupportInitialize)PB_Icon).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label L_Site;
        private System.Windows.Forms.PictureBox PB_Icon;
        private System.Windows.Forms.Label L_Status;
        private System.Windows.Forms.Label L_Title;
        private System.Windows.Forms.Label L_Version;
        private System.Windows.Forms.Label L_Tagline;
    }
}
