namespace PKHeX.WinForms
{
    partial class About
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
            PNL_Header = new System.Windows.Forms.Panel();
            PB_Icon = new System.Windows.Forms.PictureBox();
            L_Title = new System.Windows.Forms.Label();
            L_Tagline = new System.Windows.Forms.Label();
            L_Version = new System.Windows.Forms.Label();
            L_Thanks = new System.Windows.Forms.Label();
            TC_About = new System.Windows.Forms.TabControl();
            Tab_Shortcuts = new System.Windows.Forms.TabPage();
            RTB_Shortcuts = new System.Windows.Forms.RichTextBox();
            Tab_Changelog = new System.Windows.Forms.TabPage();
            RTB_Changelog = new System.Windows.Forms.RichTextBox();
            Tab_Credits = new System.Windows.Forms.TabPage();
            RTB_Credits = new System.Windows.Forms.RichTextBox();
            Tab_PKMUpdates = new System.Windows.Forms.TabPage();
            RTB_PKMUpdates = new System.Windows.Forms.RichTextBox();
            PNL_Footer = new System.Windows.Forms.Panel();
            LL_Discord = new System.Windows.Forms.LinkLabel();
            LL_Kofi = new System.Windows.Forms.LinkLabel();
            LL_GitHub = new System.Windows.Forms.LinkLabel();
            LL_Website = new System.Windows.Forms.LinkLabel();
            L_Copyright = new System.Windows.Forms.Label();
            PNL_Header.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PB_Icon).BeginInit();
            TC_About.SuspendLayout();
            Tab_Shortcuts.SuspendLayout();
            Tab_Changelog.SuspendLayout();
            Tab_Credits.SuspendLayout();
            Tab_PKMUpdates.SuspendLayout();
            PNL_Footer.SuspendLayout();
            SuspendLayout();
            //
            // PNL_Header
            //
            PNL_Header.BackColor = System.Drawing.Color.Transparent;
            PNL_Header.Controls.Add(PB_Icon);
            PNL_Header.Controls.Add(L_Title);
            PNL_Header.Controls.Add(L_Tagline);
            PNL_Header.Controls.Add(L_Version);
            PNL_Header.Dock = System.Windows.Forms.DockStyle.Top;
            PNL_Header.Location = new System.Drawing.Point(0, 0);
            PNL_Header.Name = "PNL_Header";
            PNL_Header.Size = new System.Drawing.Size(620, 120);
            PNL_Header.TabIndex = 10;
            //
            // PB_Icon
            //
            PB_Icon.BackColor = System.Drawing.Color.Transparent;
            PB_Icon.BackgroundImage = global::PKHeX.WinForms.Properties.Resources.Icon.ToBitmap();
            PB_Icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            PB_Icon.Location = new System.Drawing.Point(25, 25);
            PB_Icon.Name = "PB_Icon";
            PB_Icon.Size = new System.Drawing.Size(70, 70);
            PB_Icon.TabIndex = 0;
            PB_Icon.TabStop = false;
            //
            // L_Title
            //
            L_Title.AutoSize = true;
            L_Title.BackColor = System.Drawing.Color.Transparent;
            L_Title.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            L_Title.ForeColor = System.Drawing.Color.White;
            L_Title.Location = new System.Drawing.Point(110, 20);
            L_Title.Name = "L_Title";
            L_Title.Size = new System.Drawing.Size(280, 51);
            L_Title.TabIndex = 1;
            L_Title.Text = "PKM-Universe";
            //
            // L_Tagline
            //
            L_Tagline.AutoSize = true;
            L_Tagline.BackColor = System.Drawing.Color.Transparent;
            L_Tagline.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic);
            L_Tagline.ForeColor = System.Drawing.Color.FromArgb(220, 220, 230);
            L_Tagline.Location = new System.Drawing.Point(115, 72);
            L_Tagline.Name = "L_Tagline";
            L_Tagline.Size = new System.Drawing.Size(185, 21);
            L_Tagline.TabIndex = 2;
            L_Tagline.Text = "The Ultimate Save Editor";
            //
            // L_Version
            //
            L_Version.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            L_Version.BackColor = System.Drawing.Color.Transparent;
            L_Version.Font = new System.Drawing.Font("Segoe UI", 9F);
            L_Version.ForeColor = System.Drawing.Color.FromArgb(180, 180, 200);
            L_Version.Location = new System.Drawing.Point(400, 95);
            L_Version.Name = "L_Version";
            L_Version.Size = new System.Drawing.Size(210, 15);
            L_Version.TabIndex = 3;
            L_Version.Text = "Version 25.01 | .NET 10 Preview";
            L_Version.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // L_Thanks
            //
            L_Thanks.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            L_Thanks.BackColor = System.Drawing.Color.Transparent;
            L_Thanks.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            L_Thanks.ForeColor = System.Drawing.Color.FromArgb(150, 150, 170);
            L_Thanks.Location = new System.Drawing.Point(350, 125);
            L_Thanks.Name = "L_Thanks";
            L_Thanks.Size = new System.Drawing.Size(260, 15);
            L_Thanks.TabIndex = 4;
            L_Thanks.Text = "Thanks to all the researchers & contributors!";
            L_Thanks.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // TC_About
            //
            TC_About.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            TC_About.Controls.Add(Tab_Shortcuts);
            TC_About.Controls.Add(Tab_Changelog);
            TC_About.Controls.Add(Tab_PKMUpdates);
            TC_About.Controls.Add(Tab_Credits);
            TC_About.Location = new System.Drawing.Point(12, 145);
            TC_About.Name = "TC_About";
            TC_About.SelectedIndex = 0;
            TC_About.Size = new System.Drawing.Size(596, 310);
            TC_About.TabIndex = 5;
            //
            // Tab_Shortcuts
            //
            Tab_Shortcuts.Controls.Add(RTB_Shortcuts);
            Tab_Shortcuts.Location = new System.Drawing.Point(4, 24);
            Tab_Shortcuts.Name = "Tab_Shortcuts";
            Tab_Shortcuts.Padding = new System.Windows.Forms.Padding(8);
            Tab_Shortcuts.Size = new System.Drawing.Size(588, 282);
            Tab_Shortcuts.TabIndex = 0;
            Tab_Shortcuts.Text = "Keyboard Shortcuts";
            //
            // RTB_Shortcuts
            //
            RTB_Shortcuts.BorderStyle = System.Windows.Forms.BorderStyle.None;
            RTB_Shortcuts.Dock = System.Windows.Forms.DockStyle.Fill;
            RTB_Shortcuts.Font = new System.Drawing.Font("Consolas", 9.5F);
            RTB_Shortcuts.Location = new System.Drawing.Point(8, 8);
            RTB_Shortcuts.Name = "RTB_Shortcuts";
            RTB_Shortcuts.ReadOnly = true;
            RTB_Shortcuts.Size = new System.Drawing.Size(572, 266);
            RTB_Shortcuts.TabIndex = 0;
            RTB_Shortcuts.Text = "";
            //
            // Tab_Changelog
            //
            Tab_Changelog.Controls.Add(RTB_Changelog);
            Tab_Changelog.Location = new System.Drawing.Point(4, 24);
            Tab_Changelog.Name = "Tab_Changelog";
            Tab_Changelog.Padding = new System.Windows.Forms.Padding(8);
            Tab_Changelog.Size = new System.Drawing.Size(588, 282);
            Tab_Changelog.TabIndex = 1;
            Tab_Changelog.Text = "Changelog";
            //
            // RTB_Changelog
            //
            RTB_Changelog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            RTB_Changelog.Dock = System.Windows.Forms.DockStyle.Fill;
            RTB_Changelog.Font = new System.Drawing.Font("Consolas", 9.5F);
            RTB_Changelog.Location = new System.Drawing.Point(8, 8);
            RTB_Changelog.Name = "RTB_Changelog";
            RTB_Changelog.ReadOnly = true;
            RTB_Changelog.Size = new System.Drawing.Size(572, 266);
            RTB_Changelog.TabIndex = 0;
            RTB_Changelog.Text = "";
            //
            // Tab_Credits
            //
            Tab_Credits.Controls.Add(RTB_Credits);
            Tab_Credits.Location = new System.Drawing.Point(4, 24);
            Tab_Credits.Name = "Tab_Credits";
            Tab_Credits.Padding = new System.Windows.Forms.Padding(8);
            Tab_Credits.Size = new System.Drawing.Size(588, 282);
            Tab_Credits.TabIndex = 2;
            Tab_Credits.Text = "Credits";
            //
            // RTB_Credits
            //
            RTB_Credits.BorderStyle = System.Windows.Forms.BorderStyle.None;
            RTB_Credits.Dock = System.Windows.Forms.DockStyle.Fill;
            RTB_Credits.Font = new System.Drawing.Font("Segoe UI", 10F);
            RTB_Credits.Location = new System.Drawing.Point(8, 8);
            RTB_Credits.Name = "RTB_Credits";
            RTB_Credits.ReadOnly = true;
            RTB_Credits.Size = new System.Drawing.Size(572, 266);
            RTB_Credits.TabIndex = 0;
            RTB_Credits.Text = @"PKM-Universe Team
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Lead Developer: PokemonLover8888

Special Thanks:
• Kaphotics - Original PKHeX creator
• All PKHeX contributors
• The amazing PKM-Universe community
• All Discord members and supporters

Built with love for the Pokemon community!

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Features:
• Full save editing for all Pokemon games
• Shiny Living Dex Generator
• Auto-Legality Mod integration
• Modern themed interface
• Batch editing tools
• And much more!";
            //
            // Tab_PKMUpdates
            //
            Tab_PKMUpdates.Controls.Add(RTB_PKMUpdates);
            Tab_PKMUpdates.Location = new System.Drawing.Point(4, 24);
            Tab_PKMUpdates.Name = "Tab_PKMUpdates";
            Tab_PKMUpdates.Padding = new System.Windows.Forms.Padding(8);
            Tab_PKMUpdates.Size = new System.Drawing.Size(588, 282);
            Tab_PKMUpdates.TabIndex = 3;
            Tab_PKMUpdates.Text = "PKM-Universe Updates";
            //
            // RTB_PKMUpdates
            //
            RTB_PKMUpdates.BorderStyle = System.Windows.Forms.BorderStyle.None;
            RTB_PKMUpdates.Dock = System.Windows.Forms.DockStyle.Fill;
            RTB_PKMUpdates.Font = new System.Drawing.Font("Segoe UI", 10F);
            RTB_PKMUpdates.Location = new System.Drawing.Point(8, 8);
            RTB_PKMUpdates.Name = "RTB_PKMUpdates";
            RTB_PKMUpdates.ReadOnly = true;
            RTB_PKMUpdates.Size = new System.Drawing.Size(572, 266);
            RTB_PKMUpdates.TabIndex = 0;
            RTB_PKMUpdates.Text = "";
            //
            // PNL_Footer
            //
            PNL_Footer.Controls.Add(LL_Discord);
            PNL_Footer.Controls.Add(LL_Kofi);
            PNL_Footer.Controls.Add(LL_GitHub);
            PNL_Footer.Controls.Add(LL_Website);
            PNL_Footer.Controls.Add(L_Copyright);
            PNL_Footer.Dock = System.Windows.Forms.DockStyle.Bottom;
            PNL_Footer.Location = new System.Drawing.Point(0, 460);
            PNL_Footer.Name = "PNL_Footer";
            PNL_Footer.Size = new System.Drawing.Size(620, 50);
            PNL_Footer.TabIndex = 11;
            //
            // LL_Discord
            //
            LL_Discord.AutoSize = true;
            LL_Discord.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            LL_Discord.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            LL_Discord.Location = new System.Drawing.Point(15, 15);
            LL_Discord.Name = "LL_Discord";
            LL_Discord.Size = new System.Drawing.Size(58, 19);
            LL_Discord.TabIndex = 0;
            LL_Discord.TabStop = true;
            LL_Discord.Text = "Discord";
            //
            // LL_Kofi
            //
            LL_Kofi.AutoSize = true;
            LL_Kofi.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            LL_Kofi.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            LL_Kofi.Location = new System.Drawing.Point(90, 15);
            LL_Kofi.Name = "LL_Kofi";
            LL_Kofi.Size = new System.Drawing.Size(85, 19);
            LL_Kofi.TabIndex = 1;
            LL_Kofi.TabStop = true;
            LL_Kofi.Text = "Support Us";
            //
            // LL_GitHub
            //
            LL_GitHub.AutoSize = true;
            LL_GitHub.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            LL_GitHub.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            LL_GitHub.Location = new System.Drawing.Point(190, 15);
            LL_GitHub.Name = "LL_GitHub";
            LL_GitHub.Size = new System.Drawing.Size(55, 19);
            LL_GitHub.TabIndex = 2;
            LL_GitHub.TabStop = true;
            LL_GitHub.Text = "GitHub";
            //
            // LL_Website
            //
            LL_Website.AutoSize = true;
            LL_Website.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            LL_Website.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            LL_Website.Location = new System.Drawing.Point(260, 15);
            LL_Website.Name = "LL_Website";
            LL_Website.Size = new System.Drawing.Size(62, 19);
            LL_Website.TabIndex = 3;
            LL_Website.TabStop = true;
            LL_Website.Text = "Website";
            //
            // L_Copyright
            //
            L_Copyright.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            L_Copyright.Font = new System.Drawing.Font("Segoe UI", 8F);
            L_Copyright.Location = new System.Drawing.Point(350, 17);
            L_Copyright.Name = "L_Copyright";
            L_Copyright.Size = new System.Drawing.Size(260, 15);
            L_Copyright.TabIndex = 4;
            L_Copyright.Text = "© 2024-2025 PKM-Universe. All rights reserved.";
            L_Copyright.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // About
            //
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            ClientSize = new System.Drawing.Size(620, 510);
            Controls.Add(L_Thanks);
            Controls.Add(TC_About);
            Controls.Add(PNL_Header);
            Controls.Add(PNL_Footer);
            Icon = Properties.Resources.Icon;
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(1100, 900);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(636, 550);
            Name = "About";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "About PKM-Universe";
            PNL_Header.ResumeLayout(false);
            PNL_Header.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PB_Icon).EndInit();
            TC_About.ResumeLayout(false);
            Tab_Shortcuts.ResumeLayout(false);
            Tab_Changelog.ResumeLayout(false);
            Tab_Credits.ResumeLayout(false);
            Tab_PKMUpdates.ResumeLayout(false);
            PNL_Footer.ResumeLayout(false);
            PNL_Footer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel PNL_Header;
        private System.Windows.Forms.PictureBox PB_Icon;
        private System.Windows.Forms.Label L_Title;
        private System.Windows.Forms.Label L_Tagline;
        private System.Windows.Forms.Label L_Version;
        private System.Windows.Forms.Label L_Thanks;
        private System.Windows.Forms.TabControl TC_About;
        private System.Windows.Forms.TabPage Tab_Shortcuts;
        private System.Windows.Forms.RichTextBox RTB_Shortcuts;
        private System.Windows.Forms.TabPage Tab_Changelog;
        private System.Windows.Forms.RichTextBox RTB_Changelog;
        private System.Windows.Forms.TabPage Tab_Credits;
        private System.Windows.Forms.RichTextBox RTB_Credits;
        private System.Windows.Forms.TabPage Tab_PKMUpdates;
        private System.Windows.Forms.RichTextBox RTB_PKMUpdates;
        private System.Windows.Forms.Panel PNL_Footer;
        private System.Windows.Forms.LinkLabel LL_Discord;
        private System.Windows.Forms.LinkLabel LL_Kofi;
        private System.Windows.Forms.LinkLabel LL_GitHub;
        private System.Windows.Forms.LinkLabel LL_Website;
        private System.Windows.Forms.Label L_Copyright;
    }
}
