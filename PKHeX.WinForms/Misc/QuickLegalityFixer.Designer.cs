namespace PKHeX.WinForms
{
    partial class QuickLegalityFixer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            PNL_Header = new System.Windows.Forms.Panel();
            L_Title = new System.Windows.Forms.Label();
            L_Description = new System.Windows.Forms.Label();
            PNL_Content = new System.Windows.Forms.Panel();
            L_BoxLabel = new System.Windows.Forms.Label();
            CB_BoxSelect = new System.Windows.Forms.ComboBox();
            BTN_FixCurrentPokemon = new System.Windows.Forms.Button();
            BTN_FixCurrentBox = new System.Windows.Forms.Button();
            BTN_FixAllBoxes = new System.Windows.Forms.Button();
            PB_Progress = new System.Windows.Forms.ProgressBar();
            L_Status = new System.Windows.Forms.Label();
            RTB_Results = new System.Windows.Forms.RichTextBox();
            PNL_Footer = new System.Windows.Forms.Panel();
            L_Stats = new System.Windows.Forms.Label();
            BTN_Close = new System.Windows.Forms.Button();
            PNL_Header.SuspendLayout();
            PNL_Content.SuspendLayout();
            PNL_Footer.SuspendLayout();
            SuspendLayout();
            //
            // PNL_Header
            //
            PNL_Header.BackColor = System.Drawing.Color.Transparent;
            PNL_Header.Controls.Add(L_Title);
            PNL_Header.Controls.Add(L_Description);
            PNL_Header.Dock = System.Windows.Forms.DockStyle.Top;
            PNL_Header.Location = new System.Drawing.Point(0, 0);
            PNL_Header.Name = "PNL_Header";
            PNL_Header.Size = new System.Drawing.Size(500, 80);
            PNL_Header.TabIndex = 0;
            //
            // L_Title
            //
            L_Title.AutoSize = true;
            L_Title.BackColor = System.Drawing.Color.Transparent;
            L_Title.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            L_Title.ForeColor = System.Drawing.Color.White;
            L_Title.Location = new System.Drawing.Point(20, 15);
            L_Title.Name = "L_Title";
            L_Title.Size = new System.Drawing.Size(260, 37);
            L_Title.TabIndex = 0;
            L_Title.Text = "Quick Legality Fixer";
            //
            // L_Description
            //
            L_Description.AutoSize = true;
            L_Description.BackColor = System.Drawing.Color.Transparent;
            L_Description.Font = new System.Drawing.Font("Segoe UI", 10F);
            L_Description.ForeColor = System.Drawing.Color.FromArgb(200, 200, 220);
            L_Description.Location = new System.Drawing.Point(22, 52);
            L_Description.Name = "L_Description";
            L_Description.Size = new System.Drawing.Size(290, 19);
            L_Description.TabIndex = 1;
            L_Description.Text = "Batch fix legality issues in your Pokemon boxes";
            //
            // PNL_Content
            //
            PNL_Content.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            PNL_Content.Controls.Add(L_BoxLabel);
            PNL_Content.Controls.Add(CB_BoxSelect);
            PNL_Content.Controls.Add(BTN_FixCurrentPokemon);
            PNL_Content.Controls.Add(BTN_FixCurrentBox);
            PNL_Content.Controls.Add(BTN_FixAllBoxes);
            PNL_Content.Controls.Add(PB_Progress);
            PNL_Content.Controls.Add(L_Status);
            PNL_Content.Controls.Add(RTB_Results);
            PNL_Content.Location = new System.Drawing.Point(0, 80);
            PNL_Content.Name = "PNL_Content";
            PNL_Content.Padding = new System.Windows.Forms.Padding(20);
            PNL_Content.Size = new System.Drawing.Size(500, 330);
            PNL_Content.TabIndex = 1;
            //
            // L_BoxLabel
            //
            L_BoxLabel.AutoSize = true;
            L_BoxLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            L_BoxLabel.Location = new System.Drawing.Point(20, 20);
            L_BoxLabel.Name = "L_BoxLabel";
            L_BoxLabel.Size = new System.Drawing.Size(78, 19);
            L_BoxLabel.TabIndex = 0;
            L_BoxLabel.Text = "Select Box:";
            //
            // CB_BoxSelect
            //
            CB_BoxSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CB_BoxSelect.Font = new System.Drawing.Font("Segoe UI", 10F);
            CB_BoxSelect.Location = new System.Drawing.Point(110, 17);
            CB_BoxSelect.Name = "CB_BoxSelect";
            CB_BoxSelect.Size = new System.Drawing.Size(200, 25);
            CB_BoxSelect.TabIndex = 1;
            //
            // BTN_FixCurrentPokemon
            //
            BTN_FixCurrentPokemon.Font = new System.Drawing.Font("Segoe UI", 9F);
            BTN_FixCurrentPokemon.Location = new System.Drawing.Point(320, 16);
            BTN_FixCurrentPokemon.Name = "BTN_FixCurrentPokemon";
            BTN_FixCurrentPokemon.Size = new System.Drawing.Size(150, 28);
            BTN_FixCurrentPokemon.TabIndex = 2;
            BTN_FixCurrentPokemon.Text = "Fix Current Pokemon";
            BTN_FixCurrentPokemon.Click += BTN_FixCurrentPokemon_Click;
            //
            // BTN_FixCurrentBox
            //
            BTN_FixCurrentBox.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            BTN_FixCurrentBox.Location = new System.Drawing.Point(20, 55);
            BTN_FixCurrentBox.Name = "BTN_FixCurrentBox";
            BTN_FixCurrentBox.Size = new System.Drawing.Size(220, 40);
            BTN_FixCurrentBox.TabIndex = 3;
            BTN_FixCurrentBox.Text = "Fix Selected Box";
            BTN_FixCurrentBox.Click += BTN_FixCurrentBox_Click;
            //
            // BTN_FixAllBoxes
            //
            BTN_FixAllBoxes.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            BTN_FixAllBoxes.Location = new System.Drawing.Point(250, 55);
            BTN_FixAllBoxes.Name = "BTN_FixAllBoxes";
            BTN_FixAllBoxes.Size = new System.Drawing.Size(220, 40);
            BTN_FixAllBoxes.TabIndex = 4;
            BTN_FixAllBoxes.Text = "Fix ALL Boxes";
            BTN_FixAllBoxes.Click += BTN_FixAllBoxes_Click;
            //
            // PB_Progress
            //
            PB_Progress.Location = new System.Drawing.Point(20, 105);
            PB_Progress.Name = "PB_Progress";
            PB_Progress.Size = new System.Drawing.Size(450, 8);
            PB_Progress.TabIndex = 5;
            //
            // L_Status
            //
            L_Status.AutoSize = true;
            L_Status.Font = new System.Drawing.Font("Segoe UI", 9F);
            L_Status.Location = new System.Drawing.Point(20, 120);
            L_Status.Name = "L_Status";
            L_Status.Size = new System.Drawing.Size(39, 15);
            L_Status.TabIndex = 6;
            L_Status.Text = "Ready";
            //
            // RTB_Results
            //
            RTB_Results.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            RTB_Results.BorderStyle = System.Windows.Forms.BorderStyle.None;
            RTB_Results.Font = new System.Drawing.Font("Consolas", 10F);
            RTB_Results.Location = new System.Drawing.Point(20, 145);
            RTB_Results.Name = "RTB_Results";
            RTB_Results.ReadOnly = true;
            RTB_Results.Size = new System.Drawing.Size(450, 165);
            RTB_Results.TabIndex = 7;
            RTB_Results.Text = "Results will appear here...\n";
            //
            // PNL_Footer
            //
            PNL_Footer.Controls.Add(L_Stats);
            PNL_Footer.Controls.Add(BTN_Close);
            PNL_Footer.Dock = System.Windows.Forms.DockStyle.Bottom;
            PNL_Footer.Location = new System.Drawing.Point(0, 410);
            PNL_Footer.Name = "PNL_Footer";
            PNL_Footer.Size = new System.Drawing.Size(500, 50);
            PNL_Footer.TabIndex = 2;
            //
            // L_Stats
            //
            L_Stats.AutoSize = true;
            L_Stats.Font = new System.Drawing.Font("Segoe UI", 9F);
            L_Stats.Location = new System.Drawing.Point(20, 17);
            L_Stats.Name = "L_Stats";
            L_Stats.Size = new System.Drawing.Size(195, 15);
            L_Stats.TabIndex = 0;
            L_Stats.Text = "Processed: 0 | Legal: 0 | Issues: 0";
            //
            // BTN_Close
            //
            BTN_Close.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            BTN_Close.Font = new System.Drawing.Font("Segoe UI", 10F);
            BTN_Close.Location = new System.Drawing.Point(380, 10);
            BTN_Close.Name = "BTN_Close";
            BTN_Close.Size = new System.Drawing.Size(100, 30);
            BTN_Close.TabIndex = 1;
            BTN_Close.Text = "Close";
            BTN_Close.Click += BTN_Close_Click;
            //
            // QuickLegalityFixer
            //
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            ClientSize = new System.Drawing.Size(500, 460);
            Controls.Add(PNL_Header);
            Controls.Add(PNL_Content);
            Controls.Add(PNL_Footer);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = Properties.Resources.Icon;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "QuickLegalityFixer";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Quick Legality Fixer - PKM-Universe";
            PNL_Header.ResumeLayout(false);
            PNL_Header.PerformLayout();
            PNL_Content.ResumeLayout(false);
            PNL_Content.PerformLayout();
            PNL_Footer.ResumeLayout(false);
            PNL_Footer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel PNL_Header;
        private System.Windows.Forms.Label L_Title;
        private System.Windows.Forms.Label L_Description;
        private System.Windows.Forms.Panel PNL_Content;
        private System.Windows.Forms.Label L_BoxLabel;
        private System.Windows.Forms.ComboBox CB_BoxSelect;
        private System.Windows.Forms.Button BTN_FixCurrentPokemon;
        private System.Windows.Forms.Button BTN_FixCurrentBox;
        private System.Windows.Forms.Button BTN_FixAllBoxes;
        private System.Windows.Forms.ProgressBar PB_Progress;
        private System.Windows.Forms.Label L_Status;
        private System.Windows.Forms.RichTextBox RTB_Results;
        private System.Windows.Forms.Panel PNL_Footer;
        private System.Windows.Forms.Label L_Stats;
        private System.Windows.Forms.Button BTN_Close;
    }
}
