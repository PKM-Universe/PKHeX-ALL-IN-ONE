using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class RibbonMasterTracker : Form
{
    private readonly SaveFile SAV;
    private ListView lstRibbons = null!;
    private ListView lstPokemon = null!;
    private Label lblProgress = null!;
    private ProgressBar prgRibbons = null!;
    private ComboBox cmbGeneration = null!;
    private TextBox txtPokemonName = null!;
    private Label lblGuide = null!;

    private static readonly (string Name, string Description, string Game)[] Ribbons = new[]
    {
        // Gen 3 Ribbons
        ("Champion Ribbon", "Beat the Champion", "RSE/FRLG/Colo/XD"),
        ("Winning Ribbon", "Win 100 battles at Tower", "RSE/Colo/XD"),
        ("Victory Ribbon", "Win 50 battles at Tower", "RSE/Colo/XD"),
        ("Artist Ribbon", "Win Master Rank Contest", "RSE/ORAS"),
        ("Effort Ribbon", "Max EVs", "Gen3+"),
        ("Cool/Beauty/Cute/Smart/Tough Ribbon", "Contest ribbons", "RSE/DPPt/ORAS"),
        // Gen 4 Ribbons
        ("Legend Ribbon", "Beat the Champion", "DPPt/HGSS"),
        ("Footprint Ribbon", "Max happiness", "DPPt/HGSS"),
        ("Record Ribbon", "Upload to Global Terminal", "DPPt/HGSS"),
        // Gen 5 Ribbons
        ("Event Ribbon", "Special events", "Gen5"),
        // Gen 6 Ribbons
        ("Kalos Champion", "Beat the Champion", "XY"),
        ("Hoenn Champion", "Beat the Champion", "ORAS"),
        ("Contest Memory Ribbon", "Contest memories", "ORAS"),
        ("Battle Memory Ribbon", "Battle memories", "ORAS"),
        // Gen 7 Ribbons
        ("Alola Champion", "Beat the Champion", "SM/USUM"),
        ("Battle Royal Master", "Win Battle Royal", "SM/USUM"),
        ("Battle Tree Great/Master", "Win at Battle Tree", "SM/USUM"),
        // Gen 8 Ribbons
        ("Galar Champion", "Beat the Champion", "SwSh"),
        ("Tower Master", "Beat Leon in Tower", "SwSh"),
        ("Master Rank Ribbon", "Reach Master Rank", "SwSh"),
        // Gen 9 Ribbons
        ("Paldea Champion", "Beat the Champion", "SV"),
        ("Once-in-a-Lifetime", "Special event", "SV"),
        ("Partner Ribbon", "Complete Pokedex", "SV"),
        // Mark Ribbons (Gen 8+)
        ("Lunchtime Mark", "Found during lunch", "SwSh/SV"),
        ("Sleepy-Time Mark", "Found while sleepy", "SwSh/SV"),
        ("Dusk Mark", "Found at dusk", "SwSh/SV"),
        ("Dawn Mark", "Found at dawn", "SwSh/SV"),
        ("Cloudy Mark", "Found in clouds", "SwSh/SV"),
        ("Rainy Mark", "Found in rain", "SwSh/SV"),
        ("Stormy Mark", "Found in storm", "SwSh/SV"),
        ("Snowy Mark", "Found in snow", "SwSh/SV"),
        ("Blizzard Mark", "Found in blizzard", "SwSh/SV"),
        ("Dry Mark", "Found in harsh sun", "SwSh/SV"),
        ("Sandstorm Mark", "Found in sandstorm", "SwSh/SV"),
        ("Misty Mark", "Found in mist", "SwSh/SV"),
        ("Rare Mark", "The Pokemon is rare", "SwSh/SV"),
        ("Uncommon Mark", "The Pokemon is uncommon", "SwSh/SV"),
        ("Rowdy Mark", "Rowdy Pokemon", "SwSh/SV"),
        ("Absent-Minded Mark", "Absent-minded Pokemon", "SwSh/SV"),
        ("Jittery Mark", "Jittery Pokemon", "SwSh/SV"),
        ("Excited Mark", "Excited Pokemon", "SwSh/SV"),
        ("Charismatic Mark", "Charismatic Pokemon", "SwSh/SV"),
        ("Calmness Mark", "Calm Pokemon", "SwSh/SV"),
        ("Intense Mark", "Intense Pokemon", "SwSh/SV"),
        ("Zoned-Out Mark", "Zoned out Pokemon", "SwSh/SV"),
        ("Joyful Mark", "Joyful Pokemon", "SwSh/SV"),
        ("Angry Mark", "Angry Pokemon", "SwSh/SV"),
        ("Smiley Mark", "Happy Pokemon", "SwSh/SV"),
        ("Teary Mark", "Teary Pokemon", "SwSh/SV"),
        ("Upbeat Mark", "Upbeat Pokemon", "SwSh/SV"),
        ("Peeved Mark", "Peeved Pokemon", "SwSh/SV"),
        ("Intellectual Mark", "Intellectual Pokemon", "SwSh/SV"),
        ("Ferocious Mark", "Ferocious Pokemon", "SwSh/SV"),
        ("Crafty Mark", "Crafty Pokemon", "SwSh/SV"),
        ("Scowling Mark", "Scowling Pokemon", "SwSh/SV"),
        ("Kindly Mark", "Kindly Pokemon", "SwSh/SV"),
        ("Flustered Mark", "Flustered Pokemon", "SwSh/SV"),
        ("Pumped-Up Mark", "Pumped up Pokemon", "SwSh/SV"),
        ("Zero Energy Mark", "Zero energy Pokemon", "SwSh/SV"),
        ("Prideful Mark", "Prideful Pokemon", "SwSh/SV"),
        ("Unsure Mark", "Unsure Pokemon", "SwSh/SV"),
        ("Humble Mark", "Humble Pokemon", "SwSh/SV"),
        ("Thorny Mark", "Thorny Pokemon", "SwSh/SV"),
        ("Vigor Mark", "Vigorous Pokemon", "SwSh/SV"),
        ("Slump Mark", "Pokemon in a slump", "SwSh/SV"),
        ("Destiny Mark", "Special Pokemon", "SwSh/SV"),
        ("Curry Mark", "Shared curry", "SwSh"),
        ("Sociable Mark", "Sociable Pokemon", "SwSh"),
        ("Mightiest Mark", "7-Star Raid", "SV"),
        ("Alpha Mark", "Alpha Pokemon", "PLA"),
        ("Gourmand Mark", "Picnic with Pokemon", "SV"),
        ("Jumbo Mark", "Large Pokemon", "SV"),
        ("Mini Mark", "Small Pokemon", "SV"),
        ("Itemfinder Mark", "Found item", "SV"),
        ("Partner Mark", "Walked 10000 steps", "SV"),
        ("Titan Mark", "Titan Pokemon", "SV")
    };

    public RibbonMasterTracker(SaveFile sav)
    {
        SAV = sav;
        Text = "Ribbon Master Tracker";
        Size = new Size(1000, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        LoadRibbons();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Ribbon Master Tracker",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Pokemon Selection
        var grpPokemon = new GroupBox
        {
            Text = "Pokemon",
            Location = new Point(20, 50),
            Size = new Size(300, 80),
            ForeColor = Color.White
        };

        var lblPokemon = new Label { Text = "Name:", Location = new Point(10, 30), AutoSize = true, ForeColor = Color.White };
        txtPokemonName = new TextBox
        {
            Location = new Point(60, 27),
            Width = 150,
            Text = "Mew",
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        var btnLoadPKM = new Button
        {
            Text = "Load",
            Location = new Point(220, 25),
            Size = new Size(60, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White
        };

        grpPokemon.Controls.AddRange(new Control[] { lblPokemon, txtPokemonName, btnLoadPKM });

        // Progress
        var grpProgress = new GroupBox
        {
            Text = "Ribbon Progress",
            Location = new Point(340, 50),
            Size = new Size(620, 80),
            ForeColor = Color.White
        };

        prgRibbons = new ProgressBar
        {
            Location = new Point(10, 25),
            Size = new Size(450, 25),
            Style = ProgressBarStyle.Continuous
        };

        lblProgress = new Label
        {
            Text = "0/80 Ribbons (0%)",
            Location = new Point(470, 28),
            AutoSize = true,
            ForeColor = Color.Lime,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        var lblGen = new Label { Text = "Filter:", Location = new Point(10, 55), AutoSize = true, ForeColor = Color.White };
        cmbGeneration = new ComboBox
        {
            Location = new Point(60, 52),
            Width = 120,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbGeneration.Items.AddRange(new[] { "All Ribbons", "Gen 3", "Gen 4", "Gen 5", "Gen 6", "Gen 7", "Gen 8", "Gen 9", "Marks Only" });
        cmbGeneration.SelectedIndex = 0;

        grpProgress.Controls.AddRange(new Control[] { prgRibbons, lblProgress, lblGen, cmbGeneration });

        // Ribbon List
        var grpRibbons = new GroupBox
        {
            Text = "Available Ribbons",
            Location = new Point(20, 140),
            Size = new Size(500, 450),
            ForeColor = Color.White
        };

        lstRibbons = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(480, 380),
            View = View.Details,
            FullRowSelect = true,
            CheckBoxes = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstRibbons.Columns.Add("Ribbon", 150);
        lstRibbons.Columns.Add("Description", 180);
        lstRibbons.Columns.Add("Origin", 130);
        lstRibbons.ItemChecked += RibbonChecked;

        var btnMarkOwned = new Button
        {
            Text = "Mark Selected",
            Location = new Point(10, 410),
            Size = new Size(110, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White
        };

        var btnUnmarkAll = new Button
        {
            Text = "Unmark All",
            Location = new Point(130, 410),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 60, 60),
            ForeColor = Color.White
        };

        grpRibbons.Controls.AddRange(new Control[] { lstRibbons, btnMarkOwned, btnUnmarkAll });

        // Guide
        var grpGuide = new GroupBox
        {
            Text = "Ribbon Master Guide",
            Location = new Point(540, 140),
            Size = new Size(420, 450),
            ForeColor = Color.White
        };

        lblGuide = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(400, 415),
            ForeColor = Color.White,
            Text = "RIBBON MASTER GUIDE\n\n" +
                   "A Ribbon Master is a Pokemon that has collected\n" +
                   "every possible ribbon across multiple games.\n\n" +
                   "REQUIREMENTS:\n" +
                   "• Start in Gen 3 (RSE/Colo/XD)\n" +
                   "• Transfer through each generation\n" +
                   "• Complete all available ribbons per game\n\n" +
                   "RECOMMENDED ORDER:\n" +
                   "1. RSE - Contest Ribbons, Tower Ribbons\n" +
                   "2. Colosseum/XD - Additional ribbons\n" +
                   "3. DPPt/HGSS - Footprint, Legend\n" +
                   "4. Gen 5 - Limited ribbons\n" +
                   "5. XY/ORAS - Champion, Contests\n" +
                   "6. SM/USUM - Alola Champion, Royal\n" +
                   "7. SwSh - Tower Master, Marks\n" +
                   "8. SV - Paldea Champion, Marks\n\n" +
                   "TIPS:\n" +
                   "• Some ribbons are mutually exclusive\n" +
                   "• Marks are random encounters\n" +
                   "• Event ribbons are time-limited"
        };

        grpGuide.Controls.Add(lblGuide);

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(860, 600),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, grpPokemon, grpProgress, grpRibbons, grpGuide, btnClose });
    }

    private void LoadRibbons()
    {
        foreach (var ribbon in Ribbons)
        {
            var item = new ListViewItem(ribbon.Name);
            item.SubItems.Add(ribbon.Description);
            item.SubItems.Add(ribbon.Game);
            lstRibbons.Items.Add(item);
        }

        // Simulate some owned ribbons
        lstRibbons.Items[0].Checked = true; // Champion
        lstRibbons.Items[4].Checked = true; // Effort

        UpdateProgress();
    }

    private void RibbonChecked(object? sender, ItemCheckedEventArgs e)
    {
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        int owned = lstRibbons.Items.Cast<ListViewItem>().Count(i => i.Checked);
        int total = lstRibbons.Items.Count;
        int percent = (int)((double)owned / total * 100);

        prgRibbons.Value = percent;
        lblProgress.Text = $"{owned}/{total} Ribbons ({percent}%)";
        lblProgress.ForeColor = percent == 100 ? Color.Gold : Color.Lime;
    }
}
