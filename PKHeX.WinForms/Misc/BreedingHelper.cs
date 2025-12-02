using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class BreedingHelper : Form
{
    private readonly SaveFile SAV;
    private ComboBox cmbParent1Species = null!;
    private ComboBox cmbParent2Species = null!;
    private ComboBox cmbParent1Nature = null!;
    private ComboBox cmbParent2Nature = null!;
    private CheckBox chkParent1Everstone = null!;
    private CheckBox chkParent2Everstone = null!;
    private CheckBox chkParent1DestinyKnot = null!;
    private CheckBox chkParent2DestinyKnot = null!;
    private CheckBox chkMasudaMethod = null!;
    private CheckBox chkShinyCharm = null!;
    private CheckBox chkOvalCharm = null!;
    private Label lblOffspringInfo = null!;
    private Label lblIVPrediction = null!;
    private Label lblShinyOdds = null!;
    private Label lblEggMoves = null!;
    private ListView lstParent1IVs = null!;
    private ListView lstParent2IVs = null!;

    private static readonly string[] Natures = { "Hardy", "Lonely", "Brave", "Adamant", "Naughty", "Bold", "Docile", "Relaxed", "Impish", "Lax", "Timid", "Hasty", "Serious", "Jolly", "Naive", "Modest", "Mild", "Quiet", "Bashful", "Rash", "Calm", "Gentle", "Sassy", "Careful", "Quirky" };
    private static readonly string[] Stats = { "HP", "Attack", "Defense", "Sp. Atk", "Sp. Def", "Speed" };

    public BreedingHelper(SaveFile sav)
    {
        SAV = sav;
        Text = "Breeding Helper";
        Size = new Size(900, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        CalculateBreeding();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Breeding Helper",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(255, 180, 200),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Parent 1
        var grpParent1 = new GroupBox
        {
            Text = "Parent 1 (Female / Ditto)",
            Location = new Point(20, 50),
            Size = new Size(280, 280),
            ForeColor = Color.White
        };

        var lblSpecies1 = new Label { Text = "Species:", Location = new Point(10, 25), AutoSize = true, ForeColor = Color.White };
        cmbParent1Species = new ComboBox
        {
            Location = new Point(80, 22),
            Width = 150,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbParent1Species.Items.AddRange(new[] { "Ditto", "Pikachu", "Charizard", "Garchomp", "Eevee", "Tyranitar" });
        cmbParent1Species.SelectedIndex = 0;
        cmbParent1Species.SelectedIndexChanged += (s, e) => CalculateBreeding();

        var lblNature1 = new Label { Text = "Nature:", Location = new Point(10, 55), AutoSize = true, ForeColor = Color.White };
        cmbParent1Nature = new ComboBox
        {
            Location = new Point(80, 52),
            Width = 120,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbParent1Nature.Items.AddRange(Natures);
        cmbParent1Nature.SelectedIndex = 3; // Adamant
        cmbParent1Nature.SelectedIndexChanged += (s, e) => CalculateBreeding();

        chkParent1Everstone = new CheckBox { Text = "Everstone", Location = new Point(10, 85), AutoSize = true, ForeColor = Color.Cyan };
        chkParent1Everstone.CheckedChanged += (s, e) => CalculateBreeding();
        chkParent1DestinyKnot = new CheckBox { Text = "Destiny Knot", Location = new Point(120, 85), AutoSize = true, ForeColor = Color.Magenta };
        chkParent1DestinyKnot.CheckedChanged += (s, e) => CalculateBreeding();

        var lblIVs1 = new Label { Text = "IVs:", Location = new Point(10, 115), AutoSize = true, ForeColor = Color.White };
        lstParent1IVs = new ListView
        {
            Location = new Point(10, 135),
            Size = new Size(260, 135),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            HeaderStyle = ColumnHeaderStyle.Nonclickable
        };
        lstParent1IVs.Columns.Add("Stat", 80);
        lstParent1IVs.Columns.Add("IV", 50);
        lstParent1IVs.Columns.Add("Judge", 120);

        grpParent1.Controls.AddRange(new Control[] { lblSpecies1, cmbParent1Species, lblNature1, cmbParent1Nature, chkParent1Everstone, chkParent1DestinyKnot, lblIVs1, lstParent1IVs });

        // Parent 2
        var grpParent2 = new GroupBox
        {
            Text = "Parent 2 (Male)",
            Location = new Point(320, 50),
            Size = new Size(280, 280),
            ForeColor = Color.White
        };

        var lblSpecies2 = new Label { Text = "Species:", Location = new Point(10, 25), AutoSize = true, ForeColor = Color.White };
        cmbParent2Species = new ComboBox
        {
            Location = new Point(80, 22),
            Width = 150,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbParent2Species.Items.AddRange(new[] { "Pikachu", "Charizard", "Garchomp", "Eevee", "Tyranitar", "Dragonite" });
        cmbParent2Species.SelectedIndex = 2;
        cmbParent2Species.SelectedIndexChanged += (s, e) => CalculateBreeding();

        var lblNature2 = new Label { Text = "Nature:", Location = new Point(10, 55), AutoSize = true, ForeColor = Color.White };
        cmbParent2Nature = new ComboBox
        {
            Location = new Point(80, 52),
            Width = 120,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbParent2Nature.Items.AddRange(Natures);
        cmbParent2Nature.SelectedIndex = 10; // Timid
        cmbParent2Nature.SelectedIndexChanged += (s, e) => CalculateBreeding();

        chkParent2Everstone = new CheckBox { Text = "Everstone", Location = new Point(10, 85), AutoSize = true, ForeColor = Color.Cyan };
        chkParent2Everstone.CheckedChanged += (s, e) => CalculateBreeding();
        chkParent2DestinyKnot = new CheckBox { Text = "Destiny Knot", Location = new Point(120, 85), AutoSize = true, ForeColor = Color.Magenta };
        chkParent2DestinyKnot.Checked = true;
        chkParent2DestinyKnot.CheckedChanged += (s, e) => CalculateBreeding();

        var lblIVs2 = new Label { Text = "IVs:", Location = new Point(10, 115), AutoSize = true, ForeColor = Color.White };
        lstParent2IVs = new ListView
        {
            Location = new Point(10, 135),
            Size = new Size(260, 135),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            HeaderStyle = ColumnHeaderStyle.Nonclickable
        };
        lstParent2IVs.Columns.Add("Stat", 80);
        lstParent2IVs.Columns.Add("IV", 50);
        lstParent2IVs.Columns.Add("Judge", 120);

        grpParent2.Controls.AddRange(new Control[] { lblSpecies2, cmbParent2Species, lblNature2, cmbParent2Nature, chkParent2Everstone, chkParent2DestinyKnot, lblIVs2, lstParent2IVs });

        // Bonuses
        var grpBonuses = new GroupBox
        {
            Text = "Breeding Bonuses",
            Location = new Point(620, 50),
            Size = new Size(250, 130),
            ForeColor = Color.White
        };

        chkMasudaMethod = new CheckBox { Text = "Masuda Method (Foreign Parent)", Location = new Point(10, 25), AutoSize = true, ForeColor = Color.Gold };
        chkMasudaMethod.CheckedChanged += (s, e) => CalculateBreeding();
        chkShinyCharm = new CheckBox { Text = "Shiny Charm", Location = new Point(10, 55), AutoSize = true, ForeColor = Color.Gold };
        chkShinyCharm.CheckedChanged += (s, e) => CalculateBreeding();
        chkOvalCharm = new CheckBox { Text = "Oval Charm (Faster Eggs)", Location = new Point(10, 85), AutoSize = true, ForeColor = Color.Lime };

        grpBonuses.Controls.AddRange(new Control[] { chkMasudaMethod, chkShinyCharm, chkOvalCharm });

        // Offspring Prediction
        var grpOffspring = new GroupBox
        {
            Text = "Offspring Prediction",
            Location = new Point(20, 340),
            Size = new Size(580, 150),
            ForeColor = Color.White
        };

        lblOffspringInfo = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(280, 115),
            ForeColor = Color.Lime,
            Text = "Offspring: Gible\nNature: Adamant (100% from Parent 1)\nAbility: Sand Veil/Rough Skin\nBall: Inherited from Female"
        };

        lblIVPrediction = new Label
        {
            Location = new Point(300, 25),
            Size = new Size(270, 115),
            ForeColor = Color.Cyan,
            Text = "IV Inheritance:\n5 IVs passed (Destiny Knot)\n\nExpected Perfect IVs: 3-5\nChance of 6IV: ~1.56%"
        };

        grpOffspring.Controls.AddRange(new Control[] { lblOffspringInfo, lblIVPrediction });

        // Shiny Odds
        var grpShiny = new GroupBox
        {
            Text = "Shiny Odds",
            Location = new Point(620, 190),
            Size = new Size(250, 120),
            ForeColor = Color.White
        };

        lblShinyOdds = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(230, 85),
            ForeColor = Color.Gold,
            Text = "Current Odds: 1/4096\n\nWith Masuda: 1/683\nWith Charm: 1/512\nWith Both: 1/512"
        };

        grpShiny.Controls.Add(lblShinyOdds);

        // Egg Moves
        var grpEggMoves = new GroupBox
        {
            Text = "Egg Moves",
            Location = new Point(20, 500),
            Size = new Size(850, 110),
            ForeColor = Color.White
        };

        lblEggMoves = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(830, 75),
            ForeColor = Color.White,
            Text = "Available Egg Moves from Parent 2 (Garchomp):\n" +
                   "• Outrage (Dragon) - Physical, 120 Power\n" +
                   "• Iron Head (Steel) - Physical, 80 Power\n" +
                   "• Double-Edge (Normal) - Physical, 120 Power, Recoil"
        };

        grpEggMoves.Controls.Add(lblEggMoves);

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(770, 620),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, grpParent1, grpParent2, grpBonuses, grpOffspring, grpShiny, grpEggMoves, btnClose });

        // Load sample IVs
        LoadSampleIVs();
    }

    private void LoadSampleIVs()
    {
        int[] parent1IVs = { 31, 31, 31, 30, 31, 31 };
        int[] parent2IVs = { 31, 31, 31, 31, 31, 30 };

        for (int i = 0; i < Stats.Length; i++)
        {
            var item1 = new ListViewItem(Stats[i]);
            item1.SubItems.Add(parent1IVs[i].ToString());
            item1.SubItems.Add(parent1IVs[i] == 31 ? "Best" : parent1IVs[i] >= 30 ? "Fantastic" : "Good");
            if (parent1IVs[i] == 31) item1.ForeColor = Color.Lime;
            lstParent1IVs.Items.Add(item1);

            var item2 = new ListViewItem(Stats[i]);
            item2.SubItems.Add(parent2IVs[i].ToString());
            item2.SubItems.Add(parent2IVs[i] == 31 ? "Best" : parent2IVs[i] >= 30 ? "Fantastic" : "Good");
            if (parent2IVs[i] == 31) item2.ForeColor = Color.Lime;
            lstParent2IVs.Items.Add(item2);
        }
    }

    private void CalculateBreeding()
    {
        // Update offspring info
        string offspring = cmbParent1Species.Text == "Ditto" ? cmbParent2Species.Text : cmbParent1Species.Text;

        string nature = "Random";
        if (chkParent1Everstone.Checked && !chkParent2Everstone.Checked)
            nature = $"{cmbParent1Nature.Text} (100% from Parent 1)";
        else if (chkParent2Everstone.Checked && !chkParent1Everstone.Checked)
            nature = $"{cmbParent2Nature.Text} (100% from Parent 2)";
        else if (chkParent1Everstone.Checked && chkParent2Everstone.Checked)
            nature = $"{cmbParent1Nature.Text} or {cmbParent2Nature.Text} (50/50)";

        lblOffspringInfo.Text = $"Offspring: {offspring}\nNature: {nature}\nAbility: Standard/Hidden (if parent has HA)\nBall: Inherited from Female";

        // Update IV prediction
        int passedIVs = (chkParent1DestinyKnot.Checked || chkParent2DestinyKnot.Checked) ? 5 : 3;
        double chance6IV = Math.Pow(1.0/32, 6 - passedIVs) * 100;

        lblIVPrediction.Text = $"IV Inheritance:\n{passedIVs} IVs passed {(passedIVs == 5 ? "(Destiny Knot)" : "")}\n\n" +
                              $"Expected Perfect IVs: {passedIVs - 1}-{passedIVs}\n" +
                              $"Chance of 6IV: ~{chance6IV:F2}%";

        // Update shiny odds
        int baseOdds = 4096;
        int rolls = 1;
        if (chkMasudaMethod.Checked) rolls += 6;
        if (chkShinyCharm.Checked) rolls += 2;

        int finalOdds = baseOdds / rolls;
        lblShinyOdds.Text = $"Current Odds: 1/{finalOdds}\n\n" +
                           $"With Masuda: 1/{4096/7}\n" +
                           $"With Charm: 1/{4096/3}\n" +
                           $"With Both: 1/{4096/9}";
    }
}
