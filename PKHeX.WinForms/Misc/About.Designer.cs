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
            L_Thanks = new System.Windows.Forms.Label();
            TC_About = new System.Windows.Forms.TabControl();
            Tab_Shortcuts = new System.Windows.Forms.TabPage();
            RTB_Shortcuts = new System.Windows.Forms.RichTextBox();
            Tab_Changelog = new System.Windows.Forms.TabPage();
            RTB_Changelog = new System.Windows.Forms.RichTextBox();
            LL_Discord = new System.Windows.Forms.LinkLabel();
            LL_Kofi = new System.Windows.Forms.LinkLabel();
            LL_GitHub = new System.Windows.Forms.LinkLabel();
            L_PKMUniverse = new System.Windows.Forms.Label();
            TC_About.SuspendLayout();
            Tab_Shortcuts.SuspendLayout();
            Tab_Changelog.SuspendLayout();
            SuspendLayout();
            // 
            // L_Thanks
            // 
            L_Thanks.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            L_Thanks.Location = new System.Drawing.Point(309, 5);
            L_Thanks.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            L_Thanks.Name = "L_Thanks";
            L_Thanks.Size = new System.Drawing.Size(262, 15);
            L_Thanks.TabIndex = 2;
            L_Thanks.Text = "Thanks to all the researchers!";
            L_Thanks.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TC_About
            // 
            TC_About.Controls.Add(Tab_Shortcuts);
            TC_About.Controls.Add(Tab_Changelog);
            TC_About.Dock = System.Windows.Forms.DockStyle.Fill;
            TC_About.Location = new System.Drawing.Point(0, 0);
            TC_About.Margin = new System.Windows.Forms.Padding(0);
            TC_About.Name = "TC_About";
            TC_About.SelectedIndex = 0;
            TC_About.Size = new System.Drawing.Size(576, 429);
            TC_About.TabIndex = 5;
            // 
            // Tab_Shortcuts
            // 
            Tab_Shortcuts.Controls.Add(RTB_Shortcuts);
            Tab_Shortcuts.Location = new System.Drawing.Point(4, 24);
            Tab_Shortcuts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Tab_Shortcuts.Name = "Tab_Shortcuts";
            Tab_Shortcuts.Size = new System.Drawing.Size(568, 401);
            Tab_Shortcuts.TabIndex = 0;
            Tab_Shortcuts.Text = "Shortcuts";
            Tab_Shortcuts.UseVisualStyleBackColor = true;
            // 
            // RTB_Shortcuts
            // 
            RTB_Shortcuts.Dock = System.Windows.Forms.DockStyle.Fill;
            RTB_Shortcuts.Location = new System.Drawing.Point(0, 0);
            RTB_Shortcuts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            RTB_Shortcuts.Name = "RTB_Shortcuts";
            RTB_Shortcuts.ReadOnly = true;
            RTB_Shortcuts.Size = new System.Drawing.Size(568, 401);
            RTB_Shortcuts.TabIndex = 3;
            RTB_Shortcuts.Text = "";
            RTB_Shortcuts.WordWrap = false;
            // 
            // Tab_Changelog
            // 
            Tab_Changelog.Controls.Add(RTB_Changelog);
            Tab_Changelog.Location = new System.Drawing.Point(4, 24);
            Tab_Changelog.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Tab_Changelog.Name = "Tab_Changelog";
            Tab_Changelog.Size = new System.Drawing.Size(568, 401);
            Tab_Changelog.TabIndex = 1;
            Tab_Changelog.Text = "Changelog";
            Tab_Changelog.UseVisualStyleBackColor = true;
            // 
            // RTB_Changelog
            // 
            RTB_Changelog.Dock = System.Windows.Forms.DockStyle.Fill;
            RTB_Changelog.Location = new System.Drawing.Point(0, 0);
            RTB_Changelog.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            RTB_Changelog.Name = "RTB_Changelog";
            RTB_Changelog.ReadOnly = true;
            RTB_Changelog.Size = new System.Drawing.Size(568, 401);
            RTB_Changelog.TabIndex = 2;
            RTB_Changelog.Text = "";
            RTB_Changelog.WordWrap = false;
            //
            // L_PKMUniverse
            //
            L_PKMUniverse.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            L_PKMUniverse.AutoSize = true;
            L_PKMUniverse.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            L_PKMUniverse.ForeColor = System.Drawing.Color.FromArgb(139, 0, 0);
            L_PKMUniverse.Location = new System.Drawing.Point(12, 435);
            L_PKMUniverse.Name = "L_PKMUniverse";
            L_PKMUniverse.Size = new System.Drawing.Size(90, 15);
            L_PKMUniverse.TabIndex = 6;
            L_PKMUniverse.Text = "PKM-Universe";
            //
            // LL_Discord
            //
            LL_Discord.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            LL_Discord.AutoSize = true;
            LL_Discord.Location = new System.Drawing.Point(110, 435);
            LL_Discord.Name = "LL_Discord";
            LL_Discord.Size = new System.Drawing.Size(46, 15);
            LL_Discord.TabIndex = 7;
            LL_Discord.TabStop = true;
            LL_Discord.Text = "Discord";
            //
            // LL_Kofi
            //
            LL_Kofi.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            LL_Kofi.AutoSize = true;
            LL_Kofi.Location = new System.Drawing.Point(165, 435);
            LL_Kofi.Name = "LL_Kofi";
            LL_Kofi.Size = new System.Drawing.Size(35, 15);
            LL_Kofi.TabIndex = 8;
            LL_Kofi.TabStop = true;
            LL_Kofi.Text = "Ko-fi";
            //
            // LL_GitHub
            //
            LL_GitHub.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            LL_GitHub.AutoSize = true;
            LL_GitHub.Location = new System.Drawing.Point(210, 435);
            LL_GitHub.Name = "LL_GitHub";
            LL_GitHub.Size = new System.Drawing.Size(44, 15);
            LL_GitHub.TabIndex = 9;
            LL_GitHub.TabStop = true;
            LL_GitHub.Text = "GitHub";
            //
            // About
            //
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            ClientSize = new System.Drawing.Size(576, 460);
            Controls.Add(L_PKMUniverse);
            Controls.Add(LL_Discord);
            Controls.Add(LL_Kofi);
            Controls.Add(LL_GitHub);
            Controls.Add(L_Thanks);
            Controls.Add(TC_About);
            Icon = Properties.Resources.Icon;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(1059, 850);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(592, 500);
            Name = "About";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "About PKM-Universe";
            TC_About.ResumeLayout(false);
            Tab_Shortcuts.ResumeLayout(false);
            Tab_Changelog.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label L_Thanks;
        private System.Windows.Forms.TabControl TC_About;
        private System.Windows.Forms.TabPage Tab_Shortcuts;
        private System.Windows.Forms.RichTextBox RTB_Shortcuts;
        private System.Windows.Forms.TabPage Tab_Changelog;
        private System.Windows.Forms.RichTextBox RTB_Changelog;
        private System.Windows.Forms.Label L_PKMUniverse;
        private System.Windows.Forms.LinkLabel LL_Discord;
        private System.Windows.Forms.LinkLabel LL_Kofi;
        private System.Windows.Forms.LinkLabel LL_GitHub;
    }
}
