using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class EncounterDatabaseBrowser : Form
{
    private readonly SaveFile SAV;
    private ComboBox cmbGame = null!;
    private ComboBox cmbLocation = null!;
    private ComboBox cmbEncounterType = null!;
    private TextBox txtSearch = null!;
    private ListView lstEncounters = null!;
    private Label lblDetails = null!;
    private CheckBox chkShinyOnly = null!;
    private CheckBox chkLegendaryOnly = null!;

    private static readonly string[] GameVersions = { "All Games", "Scarlet/Violet", "Legends Arceus", "BDSP", "Sword/Shield", "Let's Go", "USUM", "SM", "ORAS", "XY", "B2W2", "BW", "HGSS", "Platinum", "DP", "Emerald", "FRLG", "RSE" };
    private static readonly string[] EncounterTypes = { "All Types", "Wild Grass", "Wild Cave", "Wild Water", "Fishing", "Static", "Gift", "Trade", "Egg", "Raid", "Tera Raid", "Mass Outbreak", "SOS Chain" };

    public EncounterDatabaseBrowser(SaveFile sav)
    {
        SAV = sav;
        Text = "Encounter Database Browser";
        Size = new Size(1000, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Encounter Database Browser",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Filters Panel
        var grpFilters = new GroupBox
        {
            Text = "Filters",
            Location = new Point(20, 50),
            Size = new Size(950, 100),
            ForeColor = Color.White
        };

        var lblGame = new Label { Text = "Game:", Location = new Point(10, 30), AutoSize = true, ForeColor = Color.White };
        cmbGame = new ComboBox
        {
            Location = new Point(60, 27),
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbGame.Items.AddRange(GameVersions);
        cmbGame.SelectedIndex = 0;
        cmbGame.SelectedIndexChanged += (s, e) => FilterEncounters();

        var lblType = new Label { Text = "Type:", Location = new Point(230, 30), AutoSize = true, ForeColor = Color.White };
        cmbEncounterType = new ComboBox
        {
            Location = new Point(280, 27),
            Width = 120,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbEncounterType.Items.AddRange(EncounterTypes);
        cmbEncounterType.SelectedIndex = 0;
        cmbEncounterType.SelectedIndexChanged += (s, e) => FilterEncounters();

        var lblSearch = new Label { Text = "Search:", Location = new Point(420, 30), AutoSize = true, ForeColor = Color.White };
        txtSearch = new TextBox
        {
            Location = new Point(480, 27),
            Width = 200,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        txtSearch.TextChanged += (s, e) => FilterEncounters();

        chkShinyOnly = new CheckBox
        {
            Text = "Shiny Available",
            Location = new Point(10, 60),
            AutoSize = true,
            ForeColor = Color.Gold
        };
        chkShinyOnly.CheckedChanged += (s, e) => FilterEncounters();

        chkLegendaryOnly = new CheckBox
        {
            Text = "Legendaries Only",
            Location = new Point(150, 60),
            AutoSize = true,
            ForeColor = Color.Magenta
        };
        chkLegendaryOnly.CheckedChanged += (s, e) => FilterEncounters();

        var lblLocation = new Label { Text = "Location:", Location = new Point(320, 60), AutoSize = true, ForeColor = Color.White };
        cmbLocation = new ComboBox
        {
            Location = new Point(390, 57),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbLocation.Items.Add("All Locations");
        cmbLocation.SelectedIndex = 0;
        cmbLocation.SelectedIndexChanged += (s, e) => FilterEncounters();

        grpFilters.Controls.AddRange(new Control[] { lblGame, cmbGame, lblType, cmbEncounterType, lblSearch, txtSearch, chkShinyOnly, chkLegendaryOnly, lblLocation, cmbLocation });

        // Encounters List
        lstEncounters = new ListView
        {
            Location = new Point(20, 160),
            Size = new Size(600, 450),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstEncounters.Columns.Add("Species", 120);
        lstEncounters.Columns.Add("Location", 150);
        lstEncounters.Columns.Add("Type", 80);
        lstEncounters.Columns.Add("Level", 60);
        lstEncounters.Columns.Add("Rarity", 80);
        lstEncounters.Columns.Add("Game", 100);
        lstEncounters.SelectedIndexChanged += EncounterSelected;

        // Details Panel
        var grpDetails = new GroupBox
        {
            Text = "Encounter Details",
            Location = new Point(640, 160),
            Size = new Size(330, 450),
            ForeColor = Color.White
        };

        lblDetails = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(310, 380),
            ForeColor = Color.White,
            Text = "Select an encounter to view details..."
        };

        var btnGeneratePKM = new Button
        {
            Text = "Generate Pokemon",
            Location = new Point(10, 410),
            Size = new Size(140, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White
        };
        btnGeneratePKM.Click += GeneratePokemon;

        var btnViewSprite = new Button
        {
            Text = "View Sprite",
            Location = new Point(160, 410),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };

        grpDetails.Controls.AddRange(new Control[] { lblDetails, btnGeneratePKM, btnViewSprite });

        // Statistics Panel
        var grpStats = new GroupBox
        {
            Text = "Database Statistics",
            Location = new Point(20, 620),
            Size = new Size(600, 50),
            ForeColor = Color.White
        };

        var lblStats = new Label
        {
            Text = "Total Encounters: 0 | Filtered: 0 | Unique Species: 0",
            Location = new Point(10, 20),
            AutoSize = true,
            ForeColor = Color.Lime
        };
        grpStats.Controls.Add(lblStats);

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(870, 625),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, grpFilters, lstEncounters, grpDetails, grpStats, btnClose });

        LoadSampleEncounters();
    }

    private void LoadSampleEncounters()
    {
        // Sample encounters - in real implementation, this would load from PKHeX encounter data
        var encounters = new[]
        {
            new { Species = "Pikachu", Location = "Viridian Forest", Type = "Wild Grass", Level = "3-5", Rarity = "Common", Game = "FRLG" },
            new { Species = "Mewtwo", Location = "Cerulean Cave", Type = "Static", Level = "70", Rarity = "Legendary", Game = "FRLG" },
            new { Species = "Garchomp", Location = "Area Zero", Type = "Tera Raid", Level = "75", Rarity = "Rare", Game = "SV" },
            new { Species = "Charizard", Location = "Tera Raids", Type = "Tera Raid", Level = "100", Rarity = "Event", Game = "SV" },
            new { Species = "Eevee", Location = "Route 4", Type = "Wild Grass", Level = "10-13", Rarity = "Uncommon", Game = "SwSh" },
            new { Species = "Zamazenta", Location = "Energy Plant", Type = "Static", Level = "70", Rarity = "Legendary", Game = "Shield" },
            new { Species = "Arceus", Location = "Temple of Sinnoh", Type = "Static", Level = "75", Rarity = "Mythical", Game = "PLA" },
            new { Species = "Koraidon", Location = "Area Zero", Type = "Static", Level = "68", Rarity = "Legendary", Game = "Scarlet" },
            new { Species = "Miraidon", Location = "Area Zero", Type = "Static", Level = "68", Rarity = "Legendary", Game = "Violet" },
            new { Species = "Dialga", Location = "Spear Pillar", Type = "Static", Level = "47", Rarity = "Legendary", Game = "BDSP" },
            new { Species = "Palkia", Location = "Spear Pillar", Type = "Static", Level = "47", Rarity = "Legendary", Game = "BDSP" },
            new { Species = "Rayquaza", Location = "Sky Pillar", Type = "Static", Level = "70", Rarity = "Legendary", Game = "ORAS" },
            new { Species = "Deoxys", Location = "Sky Pillar", Type = "Static", Level = "80", Rarity = "Mythical", Game = "ORAS" },
            new { Species = "Zygarde", Location = "Terminus Cave", Type = "Static", Level = "70", Rarity = "Legendary", Game = "XY" },
            new { Species = "Greninja", Location = "Gift", Type = "Gift", Level = "5", Rarity = "Starter", Game = "XY" }
        };

        foreach (var enc in encounters)
        {
            var item = new ListViewItem(enc.Species);
            item.SubItems.Add(enc.Location);
            item.SubItems.Add(enc.Type);
            item.SubItems.Add(enc.Level);
            item.SubItems.Add(enc.Rarity);
            item.SubItems.Add(enc.Game);
            lstEncounters.Items.Add(item);
        }
    }

    private void FilterEncounters()
    {
        // Filter logic would go here
    }

    private void EncounterSelected(object? sender, EventArgs e)
    {
        if (lstEncounters.SelectedItems.Count == 0) return;

        var item = lstEncounters.SelectedItems[0];
        lblDetails.Text = $"Species: {item.Text}\n\n" +
                         $"Location: {item.SubItems[1].Text}\n\n" +
                         $"Encounter Type: {item.SubItems[2].Text}\n\n" +
                         $"Level Range: {item.SubItems[3].Text}\n\n" +
                         $"Rarity: {item.SubItems[4].Text}\n\n" +
                         $"Game Version: {item.SubItems[5].Text}\n\n" +
                         $"─────────────────\n" +
                         $"Additional Info:\n" +
                         $"• Held Items: None\n" +
                         $"• Abilities: Standard\n" +
                         $"• Special Moves: None\n" +
                         $"• Shiny Locked: No";
    }

    private void GeneratePokemon(object? sender, EventArgs e)
    {
        if (lstEncounters.SelectedItems.Count == 0)
        {
            WinFormsUtil.Alert("Please select an encounter first.");
            return;
        }

        WinFormsUtil.Alert("Pokemon generation would create a legal PKM based on this encounter.");
    }
}
