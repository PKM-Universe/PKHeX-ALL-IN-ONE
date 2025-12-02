using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class MarkHunter : Form
{
    private readonly SaveFile SAV;
    private ListView lstMarks = null!;
    private ListView lstFound = null!;
    private Label lblOdds = null!;
    private Label lblTips = null!;
    private ComboBox cmbWeather = null!;
    private ComboBox cmbTime = null!;
    private CheckBox chkShinyCharm = null!;
    private CheckBox chkMarkCharm = null!;

    private static readonly (string Name, string Title, string Condition, double Odds)[] Marks = new[]
    {
        // Time Marks
        ("Lunchtime Mark", "the Peckish", "11:00-11:59", 0.02),
        ("Sleepy-Time Mark", "the Sleepy", "20:00-23:59", 0.02),
        ("Dusk Mark", "the Pokemon of the Dusk", "17:00-17:59", 0.02),
        ("Dawn Mark", "the Pokemon of the Dawn", "6:00-6:59", 0.02),
        // Weather Marks
        ("Cloudy Mark", "the Pokemon on Cloud Nine", "Overcast weather", 0.02),
        ("Rainy Mark", "the Pokemon thatستقsoak", "Rain", 0.02),
        ("Stormy Mark", "the Pokemon of a Storm", "Thunderstorm", 0.02),
        ("Snowy Mark", "the Snow Frolicker", "Snow", 0.02),
        ("Blizzard Mark", "the Pokemon of a Blizzard", "Blizzard", 0.02),
        ("Dry Mark", "the Pokemon of Drought", "Harsh sun", 0.02),
        ("Sandstorm Mark", "the Pokemon of Sandstorm", "Sandstorm", 0.02),
        ("Misty Mark", "the Pokemon of the Mist", "Fog", 0.02),
        // Personality Marks
        ("Rowdy Mark", "the Rowdy", "Random wild", 0.0033),
        ("Absent-Minded Mark", "the Spacey", "Random wild", 0.0033),
        ("Jittery Mark", "the Pokemon of Jitters", "Random wild", 0.0033),
        ("Excited Mark", "the Pokemon of Excitement", "Random wild", 0.0033),
        ("Charismatic Mark", "the Charisma Pokemon", "Random wild", 0.0033),
        ("Calmness Mark", "the Pokemon of Calm", "Random wild", 0.0033),
        ("Intense Mark", "the Pokemon of Intensity", "Random wild", 0.0033),
        ("Zoned-Out Mark", "the Pokemon in a Daze", "Random wild", 0.0033),
        ("Joyful Mark", "the Joy Pokemon", "Random wild", 0.0033),
        ("Angry Mark", "the Pokemon of Anger", "Random wild", 0.0033),
        ("Smiley Mark", "the Pokemon with a Grin", "Random wild", 0.0033),
        ("Teary Mark", "the Pokemon of Tears", "Random wild", 0.0033),
        ("Upbeat Mark", "the Pokemon in High Spirits", "Random wild", 0.0033),
        ("Peeved Mark", "the Pokemon in a Bad Mood", "Random wild", 0.0033),
        ("Intellectual Mark", "the Pokemon of Brilliance", "Random wild", 0.0033),
        ("Ferocious Mark", "the Pokemon of Ferocity", "Random wild", 0.0033),
        ("Crafty Mark", "the Pokemon of Craftiness", "Random wild", 0.0033),
        ("Scowling Mark", "the Pokemon of a Scowl", "Random wild", 0.0033),
        ("Kindly Mark", "the Pokemon of Kindness", "Random wild", 0.0033),
        ("Flustered Mark", "the Pokemon of Distress", "Random wild", 0.0033),
        ("Pumped-Up Mark", "the Pokemon of Energy", "Random wild", 0.0033),
        ("Zero Energy Mark", "the Pokemon of Lethargy", "Random wild", 0.0033),
        ("Prideful Mark", "the Pokemon of Pride", "Random wild", 0.0033),
        ("Unsure Mark", "the Pokemon of Uncertainty", "Random wild", 0.0033),
        ("Humble Mark", "the Pokemon of Humility", "Random wild", 0.0033),
        ("Thorny Mark", "the Pokemonof Sharp Tongue", "Random wild", 0.0033),
        ("Vigor Mark", "the Pokemon of Vigor", "Random wild", 0.0033),
        ("Slump Mark", "the Pokemon in a Slump", "Random wild", 0.0033),
        // Rare Marks
        ("Rare Mark", "the Recluse", "Very rare wild", 0.001),
        ("Uncommon Mark", "the Pokemon", "Uncommon wild", 0.01),
        ("Destiny Mark", "the Pokemon of Destiny", "Extremely rare", 0.0001),
        // Special Marks (SV)
        ("Mightiest Mark", "the Unrivaled", "7-Star Raid", 1.0),
        ("Gourmand Mark", "the Pokemon of Taste", "Picnic item", 0.01),
        ("Partner Mark", "the Pokemon of Partnership", "10000 steps Let's Go", 1.0),
        ("Jumbo Mark", "the Pokemon of Jumbo", "XXL size", 0.02),
        ("Mini Mark", "the Pokemon of Mini", "XXXS size", 0.02),
        ("Itemfinder Mark", "the Pokemon with Item", "Found hidden item", 0.01),
        ("Titan Mark", "the Former Titan", "Titan Pokemon", 1.0)
    };

    public MarkHunter(SaveFile sav)
    {
        SAV = sav;
        Text = "Mark Hunter";
        Size = new Size(950, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        LoadMarks();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Mark Hunter",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(255, 180, 100),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Conditions Panel
        var grpConditions = new GroupBox
        {
            Text = "Current Conditions",
            Location = new Point(20, 50),
            Size = new Size(550, 80),
            ForeColor = Color.White
        };

        var lblWeather = new Label { Text = "Weather:", Location = new Point(10, 30), AutoSize = true, ForeColor = Color.White };
        cmbWeather = new ComboBox
        {
            Location = new Point(80, 27),
            Width = 120,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbWeather.Items.AddRange(new[] { "Clear", "Overcast", "Rain", "Thunderstorm", "Snow", "Blizzard", "Harsh Sun", "Sandstorm", "Fog" });
        cmbWeather.SelectedIndex = 0;
        cmbWeather.SelectedIndexChanged += (s, e) => UpdateOdds();

        var lblTime = new Label { Text = "Time:", Location = new Point(220, 30), AutoSize = true, ForeColor = Color.White };
        cmbTime = new ComboBox
        {
            Location = new Point(270, 27),
            Width = 100,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbTime.Items.AddRange(new[] { "Morning", "Day", "Dusk", "Night", "Lunchtime", "Dawn" });
        cmbTime.SelectedIndex = 1;
        cmbTime.SelectedIndexChanged += (s, e) => UpdateOdds();

        chkMarkCharm = new CheckBox
        {
            Text = "Mark Charm",
            Location = new Point(400, 30),
            AutoSize = true,
            ForeColor = Color.Magenta
        };
        chkMarkCharm.CheckedChanged += (s, e) => UpdateOdds();

        grpConditions.Controls.AddRange(new Control[] { lblWeather, cmbWeather, lblTime, cmbTime, chkMarkCharm });

        // Current Odds
        var grpOdds = new GroupBox
        {
            Text = "Mark Odds",
            Location = new Point(590, 50),
            Size = new Size(330, 80),
            ForeColor = Color.White
        };

        lblOdds = new Label
        {
            Text = "Base: 1/50 (2%)\nWith Conditions: ~1/20 (5%)",
            Location = new Point(10, 25),
            Size = new Size(310, 45),
            ForeColor = Color.Lime,
            Font = new Font("Segoe UI", 10F)
        };

        grpOdds.Controls.Add(lblOdds);

        // Marks List
        var grpMarks = new GroupBox
        {
            Text = "Available Marks",
            Location = new Point(20, 140),
            Size = new Size(500, 400),
            ForeColor = Color.White
        };

        lstMarks = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(480, 365),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstMarks.Columns.Add("Mark", 140);
        lstMarks.Columns.Add("Title", 140);
        lstMarks.Columns.Add("Condition", 110);
        lstMarks.Columns.Add("Odds", 70);

        grpMarks.Controls.Add(lstMarks);

        // Found Pokemon
        var grpFound = new GroupBox
        {
            Text = "Pokemon with Marks (in Save)",
            Location = new Point(540, 140),
            Size = new Size(380, 250),
            ForeColor = Color.White
        };

        lstFound = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(360, 215),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstFound.Columns.Add("Pokemon", 100);
        lstFound.Columns.Add("Mark", 140);
        lstFound.Columns.Add("Shiny", 50);
        lstFound.Columns.Add("Box", 60);

        var btnScanSave = new Button
        {
            Text = "Scan Save",
            Location = new Point(540, 395),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };
        btnScanSave.Click += ScanForMarks;

        grpFound.Controls.Add(lstFound);

        // Tips
        var grpTips = new GroupBox
        {
            Text = "Mark Hunting Tips",
            Location = new Point(540, 430),
            Size = new Size(380, 110),
            ForeColor = Color.White
        };

        lblTips = new Label
        {
            Text = "• Weather marks only appear in that weather\n" +
                   "• Time marks require specific in-game times\n" +
                   "• Personality marks are completely random\n" +
                   "• Mark Charm (SV) increases odds by 3x\n" +
                   "• Destiny Mark is extremely rare (~1/10000)",
            Location = new Point(10, 20),
            Size = new Size(360, 80),
            ForeColor = Color.Gray
        };

        grpTips.Controls.Add(lblTips);

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(820, 600),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, grpConditions, grpOdds, grpMarks, grpFound, btnScanSave, grpTips, btnClose });
    }

    private void LoadMarks()
    {
        foreach (var mark in Marks)
        {
            var item = new ListViewItem(mark.Name);
            item.SubItems.Add(mark.Title);
            item.SubItems.Add(mark.Condition);
            item.SubItems.Add($"1/{(int)(1/mark.Odds)}");

            // Color code by rarity
            if (mark.Odds >= 0.02)
                item.ForeColor = Color.White;
            else if (mark.Odds >= 0.005)
                item.ForeColor = Color.Yellow;
            else if (mark.Odds >= 0.001)
                item.ForeColor = Color.Orange;
            else
                item.ForeColor = Color.Red;

            lstMarks.Items.Add(item);
        }

        // Sample found Pokemon
        var found = new[]
        {
            new { Pokemon = "Pikachu", Mark = "Rowdy Mark", Shiny = "No", Box = "Box 1" },
            new { Pokemon = "Eevee", Mark = "Joyful Mark", Shiny = "Yes", Box = "Box 2" },
            new { Pokemon = "Garchomp", Mark = "Rare Mark", Shiny = "No", Box = "Box 5" }
        };

        foreach (var mon in found)
        {
            var item = new ListViewItem(mon.Pokemon);
            item.SubItems.Add(mon.Mark);
            item.SubItems.Add(mon.Shiny);
            item.SubItems.Add(mon.Box);
            if (mon.Shiny == "Yes")
                item.ForeColor = Color.Gold;
            lstFound.Items.Add(item);
        }
    }

    private void UpdateOdds()
    {
        double baseOdds = 0.02;
        int activeMarks = 1;

        // Weather bonus
        if (cmbWeather.SelectedIndex > 0)
            activeMarks++;

        // Time bonus
        if (cmbTime.SelectedIndex >= 2)
            activeMarks++;

        // Mark Charm
        if (chkMarkCharm.Checked)
            baseOdds *= 3;

        double combinedOdds = baseOdds * activeMarks;
        lblOdds.Text = $"Base: 1/{(int)(1/baseOdds)} ({baseOdds*100:F1}%)\n" +
                       $"With Conditions: ~1/{(int)(1/combinedOdds)} ({combinedOdds*100:F1}%)";
    }

    private void ScanForMarks(object? sender, EventArgs e)
    {
        WinFormsUtil.Alert("Scanning save file for Pokemon with marks...\n\nThis will find all Pokemon that have any mark attached.");
    }
}
