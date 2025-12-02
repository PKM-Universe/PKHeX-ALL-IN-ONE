using System;
using System.Drawing;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class ShinyOddsCalculator : Form
{
    private readonly SaveFile SAV;
    private ComboBox cmbGame = null!;
    private ComboBox cmbMethod = null!;
    private NumericUpDown nudChainLength = null!;
    private NumericUpDown nudResearchLevel = null!;
    private CheckBox chkShinyCharm = null!;
    private CheckBox chkMasudaMethod = null!;
    private CheckBox chkLure = null!;
    private CheckBox chkSparklingPower = null!;
    private Label lblOdds = null!;
    private Label lblOddsPercent = null!;
    private Label lblExpectedEncounters = null!;
    private ProgressBar prgOdds = null!;

    private static readonly string[] Games = { "Scarlet/Violet", "Legends Arceus", "BDSP", "Sword/Shield", "Let's Go P/E", "USUM", "SM", "ORAS", "XY", "B2W2", "BW", "HGSS", "DPPt", "Emerald", "FRLG", "RSE", "Gen 1-2" };
    private static readonly string[] Methods = { "Wild Encounter", "Breeding (Masuda)", "Soft Reset", "Chain Fishing", "PokeRadar Chain", "DexNav Chain", "SOS Chain", "Catch Combo", "Mass Outbreak", "Massive Mass Outbreak", "Tera Raid", "Dynamax Adventure" };

    public ShinyOddsCalculator(SaveFile sav)
    {
        SAV = sav;
        Text = "Shiny Odds Calculator";
        Size = new Size(600, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        CalculateOdds();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "✨ Shiny Odds Calculator",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Game Selection
        var grpGame = new GroupBox
        {
            Text = "Game & Method",
            Location = new Point(20, 50),
            Size = new Size(540, 100),
            ForeColor = Color.White
        };

        var lblGame = new Label { Text = "Game:", Location = new Point(10, 30), AutoSize = true, ForeColor = Color.White };
        cmbGame = new ComboBox
        {
            Location = new Point(80, 27),
            Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbGame.Items.AddRange(Games);
        cmbGame.SelectedIndex = 0;
        cmbGame.SelectedIndexChanged += (s, e) => CalculateOdds();

        var lblMethod = new Label { Text = "Method:", Location = new Point(280, 30), AutoSize = true, ForeColor = Color.White };
        cmbMethod = new ComboBox
        {
            Location = new Point(350, 27),
            Width = 170,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbMethod.Items.AddRange(Methods);
        cmbMethod.SelectedIndex = 0;
        cmbMethod.SelectedIndexChanged += (s, e) => CalculateOdds();

        var lblChain = new Label { Text = "Chain Length:", Location = new Point(10, 65), AutoSize = true, ForeColor = Color.White };
        nudChainLength = new NumericUpDown
        {
            Location = new Point(110, 62),
            Width = 80,
            Maximum = 500,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        nudChainLength.ValueChanged += (s, e) => CalculateOdds();

        var lblResearch = new Label { Text = "Research Lv:", Location = new Point(210, 65), AutoSize = true, ForeColor = Color.White };
        nudResearchLevel = new NumericUpDown
        {
            Location = new Point(310, 62),
            Width = 80,
            Maximum = 10,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        nudResearchLevel.ValueChanged += (s, e) => CalculateOdds();

        grpGame.Controls.AddRange(new Control[] { lblGame, cmbGame, lblMethod, cmbMethod, lblChain, nudChainLength, lblResearch, nudResearchLevel });

        // Bonuses
        var grpBonuses = new GroupBox
        {
            Text = "Shiny Bonuses",
            Location = new Point(20, 160),
            Size = new Size(540, 100),
            ForeColor = Color.White
        };

        chkShinyCharm = new CheckBox
        {
            Text = "Shiny Charm (+2 rolls)",
            Location = new Point(20, 30),
            AutoSize = true,
            ForeColor = Color.Magenta
        };
        chkShinyCharm.CheckedChanged += (s, e) => CalculateOdds();

        chkMasudaMethod = new CheckBox
        {
            Text = "Masuda Method (+5/+6 rolls)",
            Location = new Point(200, 30),
            AutoSize = true,
            ForeColor = Color.Cyan
        };
        chkMasudaMethod.CheckedChanged += (s, e) => CalculateOdds();

        chkLure = new CheckBox
        {
            Text = "Lure Active",
            Location = new Point(20, 60),
            AutoSize = true,
            ForeColor = Color.Yellow
        };
        chkLure.CheckedChanged += (s, e) => CalculateOdds();

        chkSparklingPower = new CheckBox
        {
            Text = "Sparkling Power Lv3",
            Location = new Point(200, 60),
            AutoSize = true,
            ForeColor = Color.Gold
        };
        chkSparklingPower.CheckedChanged += (s, e) => CalculateOdds();

        grpBonuses.Controls.AddRange(new Control[] { chkShinyCharm, chkMasudaMethod, chkLure, chkSparklingPower });

        // Results
        var grpResults = new GroupBox
        {
            Text = "Shiny Odds Results",
            Location = new Point(20, 270),
            Size = new Size(540, 200),
            ForeColor = Color.White
        };

        lblOdds = new Label
        {
            Text = "1 in 4096",
            Location = new Point(20, 40),
            AutoSize = true,
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 24F, FontStyle.Bold)
        };

        lblOddsPercent = new Label
        {
            Text = "(0.0244%)",
            Location = new Point(200, 50),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F)
        };

        prgOdds = new ProgressBar
        {
            Location = new Point(20, 90),
            Size = new Size(500, 25),
            Style = ProgressBarStyle.Continuous
        };

        lblExpectedEncounters = new Label
        {
            Text = "Expected encounters for 50% chance: ~2,839\nExpected encounters for 90% chance: ~9,430",
            Location = new Point(20, 130),
            Size = new Size(500, 60),
            ForeColor = Color.Lime
        };

        grpResults.Controls.AddRange(new Control[] { lblOdds, lblOddsPercent, prgOdds, lblExpectedEncounters });

        // Quick Reference
        var grpReference = new GroupBox
        {
            Text = "Quick Reference",
            Location = new Point(20, 480),
            Size = new Size(540, 120),
            ForeColor = Color.White
        };

        var lblRef = new Label
        {
            Text = "Base Odds by Generation:\n" +
                   "• Gen 1-5: 1/8192 (0.0122%)\n" +
                   "• Gen 6+: 1/4096 (0.0244%)\n" +
                   "• With Shiny Charm: 1/1365 (0.0732%)\n" +
                   "• Masuda + Charm: 1/512 (0.1953%)",
            Location = new Point(20, 25),
            Size = new Size(500, 85),
            ForeColor = Color.White
        };

        grpReference.Controls.Add(lblRef);

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(460, 560),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, grpGame, grpBonuses, grpResults, grpReference, btnClose });
    }

    private void CalculateOdds()
    {
        int baseOdds = 4096; // Gen 6+ base
        int rolls = 1;

        // Gen 1-5 had different base odds
        if (cmbGame.SelectedIndex >= 10) // BW and earlier
            baseOdds = 8192;

        // Shiny Charm adds 2 rolls
        if (chkShinyCharm.Checked)
            rolls += 2;

        // Masuda Method
        if (chkMasudaMethod.Checked)
        {
            rolls += cmbGame.SelectedIndex <= 6 ? 6 : 5; // Gen 6+ = 6, Gen 4-5 = 5
        }

        // Sparkling Power (SV)
        if (chkSparklingPower.Checked && cmbGame.SelectedIndex == 0)
            rolls += 3;

        // Chain bonuses
        int chain = (int)nudChainLength.Value;
        if (chain > 0)
        {
            switch (cmbMethod.SelectedItem?.ToString())
            {
                case "Chain Fishing":
                    rolls += Math.Min(chain, 20) * 2;
                    break;
                case "PokeRadar Chain":
                    if (chain >= 40) rolls += 39;
                    else rolls += chain;
                    break;
                case "SOS Chain":
                    if (chain >= 31) rolls += 12;
                    else if (chain >= 21) rolls += 8;
                    else if (chain >= 11) rolls += 4;
                    break;
                case "Catch Combo":
                    if (chain >= 31) rolls += 11;
                    else if (chain >= 21) rolls += 7;
                    else if (chain >= 11) rolls += 3;
                    break;
                case "Mass Outbreak":
                    rolls += Math.Min(chain, 25);
                    break;
            }
        }

        // Research Level (PLA)
        int research = (int)nudResearchLevel.Value;
        if (research > 0 && cmbGame.SelectedIndex == 1)
            rolls += research;

        // Calculate final odds
        double effectiveOdds = (double)baseOdds / rolls;
        double percent = (1.0 / effectiveOdds) * 100;

        lblOdds.Text = $"1 in {effectiveOdds:N0}";
        lblOddsPercent.Text = $"({percent:F4}%)";

        // Progress bar (max out at 1/100)
        prgOdds.Value = Math.Min((int)(percent * 100), 100);

        // Expected encounters
        double expected50 = effectiveOdds * Math.Log(2);
        double expected90 = effectiveOdds * Math.Log(10);
        lblExpectedEncounters.Text = $"Expected encounters for 50% chance: ~{expected50:N0}\n" +
                                     $"Expected encounters for 90% chance: ~{expected90:N0}";
    }
}
