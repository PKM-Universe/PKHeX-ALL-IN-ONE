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
            PB_Icon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)PB_Icon).BeginInit();
            SuspendLayout();
            //
            // L_Title
            //
            L_Title.AutoSize = true;
            L_Title.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            L_Title.ForeColor = System.Drawing.Color.FromArgb(139, 0, 0);
            L_Title.Location = new System.Drawing.Point(70, 12);
            L_Title.Name = "L_Title";
            L_Title.Size = new System.Drawing.Size(160, 30);
            L_Title.TabIndex = 3;
            L_Title.Text = "PKM-Universe";
            //
            // L_Version
            //
            L_Version.AutoSize = true;
            L_Version.Font = new System.Drawing.Font("Segoe UI", 9F);
            L_Version.ForeColor = System.Drawing.Color.Gray;
            L_Version.Location = new System.Drawing.Point(70, 42);
            L_Version.Name = "L_Version";
            L_Version.Size = new System.Drawing.Size(140, 15);
            L_Version.TabIndex = 4;
            L_Version.Text = "Pokemon Save Editor";
            //
            // L_Status
            //
            L_Status.AutoSize = true;
            L_Status.Font = new System.Drawing.Font("Segoe UI", 9F);
            L_Status.ForeColor = System.Drawing.Color.DimGray;
            L_Status.Location = new System.Drawing.Point(70, 70);
            L_Status.Name = "L_Status";
            L_Status.Size = new System.Drawing.Size(113, 15);
            L_Status.TabIndex = 0;
            L_Status.Text = "Starting up...";
            //
            // L_Site
            //
            L_Site.AutoSize = true;
            L_Site.Font = new System.Drawing.Font("Segoe UI", 8F);
            L_Site.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
            L_Site.Location = new System.Drawing.Point(70, 90);
            L_Site.Name = "L_Site";
            L_Site.Size = new System.Drawing.Size(116, 13);
            L_Site.TabIndex = 1;
            L_Site.Text = "discord.gg/pkm-universe";
            //
            // PB_Icon
            //
            PB_Icon.BackgroundImage = global::PKHeX.WinForms.Properties.Resources.Icon.ToBitmap();
            PB_Icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            PB_Icon.Location = new System.Drawing.Point(15, 25);
            PB_Icon.Name = "PB_Icon";
            PB_Icon.Size = new System.Drawing.Size(48, 48);
            PB_Icon.TabIndex = 2;
            PB_Icon.TabStop = false;
            //
            // SplashScreen
            //
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            ClientSize = new System.Drawing.Size(280, 115);
            Controls.Add(L_Title);
            Controls.Add(L_Version);
            Controls.Add(PB_Icon);
            Controls.Add(L_Site);
            Controls.Add(L_Status);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = Properties.Resources.Icon;
            Name = "SplashScreen";
            Opacity = 0.95D;
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
    }
}
