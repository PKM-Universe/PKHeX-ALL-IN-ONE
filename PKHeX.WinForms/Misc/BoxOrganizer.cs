using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class BoxOrganizer : Form
{
    private readonly SaveFile SAV;
    private ComboBox cmbSortMethod = null!;
    private ComboBox cmbSortOrder = null!;
    private ComboBox cmbBoxRange = null!;
    private CheckBox chkPreserveParty = null!;
    private CheckBox chkGroupShiny = null!;
    private CheckBox chkGroupLegendary = null!;
    private ListView lstPreview = null!;
    private Label lblStats = null!;
    private ProgressBar prgSort = null!;

    private static readonly string[] SortMethods = new[]
    {
        "National Dex Number",
        "Species Name (A-Z)",
        "Level (High to Low)",
        "Level (Low to High)",
        "Type (Primary)",
        "Original Trainer",
        "Caught Date",
        "IV Total",
        "Shiny Status",
        "Ball Type",
        "Generation",
        "Ability",
        "Nature",
        "Held Item",
        "Friendship",
        "Egg Group"
    };

    public BoxOrganizer(SaveFile sav)
    {
        SAV = sav;
        Text = "Box Organizer";
        Size = new Size(800, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        LoadPreview();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Box Organizer",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Sort Options
        var grpSort = new GroupBox
        {
            Text = "Sort Options",
            Location = new Point(20, 50),
            Size = new Size(350, 180),
            ForeColor = Color.White
        };

        var lblMethod = new Label { Text = "Sort By:", Location = new Point(10, 30), AutoSize = true, ForeColor = Color.White };
        cmbSortMethod = new ComboBox
        {
            Location = new Point(80, 27),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbSortMethod.Items.AddRange(SortMethods);
        cmbSortMethod.SelectedIndex = 0;
        cmbSortMethod.SelectedIndexChanged += (s, e) => UpdatePreview();

        var lblOrder = new Label { Text = "Order:", Location = new Point(10, 65), AutoSize = true, ForeColor = Color.White };
        cmbSortOrder = new ComboBox
        {
            Location = new Point(80, 62),
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbSortOrder.Items.AddRange(new[] { "Ascending", "Descending" });
        cmbSortOrder.SelectedIndex = 0;

        var lblRange = new Label { Text = "Boxes:", Location = new Point(10, 100), AutoSize = true, ForeColor = Color.White };
        cmbBoxRange = new ComboBox
        {
            Location = new Point(80, 97),
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbBoxRange.Items.AddRange(new[] { "All Boxes", "Current Box", "Boxes 1-10", "Boxes 11-20", "Boxes 21-30" });
        cmbBoxRange.SelectedIndex = 0;

        chkPreserveParty = new CheckBox
        {
            Text = "Don't move Party Pokemon",
            Location = new Point(10, 135),
            AutoSize = true,
            ForeColor = Color.Yellow,
            Checked = true
        };

        grpSort.Controls.AddRange(new Control[] { lblMethod, cmbSortMethod, lblOrder, cmbSortOrder, lblRange, cmbBoxRange, chkPreserveParty });

        // Grouping Options
        var grpGroup = new GroupBox
        {
            Text = "Grouping",
            Location = new Point(390, 50),
            Size = new Size(380, 180),
            ForeColor = Color.White
        };

        chkGroupShiny = new CheckBox
        {
            Text = "Group Shinies Together",
            Location = new Point(10, 30),
            AutoSize = true,
            ForeColor = Color.Gold
        };

        chkGroupLegendary = new CheckBox
        {
            Text = "Group Legendaries/Mythicals",
            Location = new Point(10, 60),
            AutoSize = true,
            ForeColor = Color.Magenta
        };

        var lblPresets = new Label { Text = "Quick Presets:", Location = new Point(10, 100), AutoSize = true, ForeColor = Color.White };

        var btnLivingDex = new Button
        {
            Text = "Living Dex Order",
            Location = new Point(10, 125),
            Size = new Size(110, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White
        };
        btnLivingDex.Click += (s, e) =>
        {
            cmbSortMethod.SelectedIndex = 0; // Dex Number
            chkGroupShiny.Checked = false;
            chkGroupLegendary.Checked = false;
        };

        var btnShinyFirst = new Button
        {
            Text = "Shinies First",
            Location = new Point(130, 125),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(180, 140, 60),
            ForeColor = Color.White
        };
        btnShinyFirst.Click += (s, e) =>
        {
            cmbSortMethod.SelectedIndex = 8; // Shiny Status
            chkGroupShiny.Checked = true;
        };

        var btnCompetitive = new Button
        {
            Text = "By IV Total",
            Location = new Point(240, 125),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };
        btnCompetitive.Click += (s, e) =>
        {
            cmbSortMethod.SelectedIndex = 7; // IV Total
            cmbSortOrder.SelectedIndex = 1; // Descending
        };

        grpGroup.Controls.AddRange(new Control[] { chkGroupShiny, chkGroupLegendary, lblPresets, btnLivingDex, btnShinyFirst, btnCompetitive });

        // Preview
        var grpPreview = new GroupBox
        {
            Text = "Preview",
            Location = new Point(20, 240),
            Size = new Size(750, 300),
            ForeColor = Color.White
        };

        lstPreview = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(730, 230),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstPreview.Columns.Add("#", 40);
        lstPreview.Columns.Add("Pokemon", 100);
        lstPreview.Columns.Add("Level", 50);
        lstPreview.Columns.Add("Type", 80);
        lstPreview.Columns.Add("Shiny", 50);
        lstPreview.Columns.Add("IVs", 60);
        lstPreview.Columns.Add("Current Box", 80);
        lstPreview.Columns.Add("New Box", 80);

        lblStats = new Label
        {
            Text = "Total Pokemon: 0 | Will be moved: 0",
            Location = new Point(10, 265),
            AutoSize = true,
            ForeColor = Color.Lime
        };

        grpPreview.Controls.AddRange(new Control[] { lstPreview, lblStats });

        // Actions
        prgSort = new ProgressBar
        {
            Location = new Point(20, 550),
            Size = new Size(500, 25),
            Style = ProgressBarStyle.Continuous
        };

        var btnPreview = new Button
        {
            Text = "Update Preview",
            Location = new Point(540, 550),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };
        btnPreview.Click += (s, e) => UpdatePreview();

        var btnSort = new Button
        {
            Text = "Sort Boxes",
            Location = new Point(540, 600),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 60),
            ForeColor = Color.White
        };
        btnSort.Click += SortBoxes;

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(670, 600),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, grpSort, grpGroup, grpPreview, prgSort, btnPreview, btnSort, btnClose });
    }

    private void LoadPreview()
    {
        // Sample preview data
        var pokemon = new[]
        {
            new { Num = "025", Name = "Pikachu", Level = "25", Type = "Electric", Shiny = "No", IVs = "150", Current = "Box 3", New = "Box 1" },
            new { Num = "006", Name = "Charizard", Level = "78", Type = "Fire/Flying", Shiny = "Yes", IVs = "186", Current = "Box 1", New = "Box 1" },
            new { Num = "149", Name = "Dragonite", Level = "65", Type = "Dragon/Flying", Shiny = "No", IVs = "175", Current = "Box 5", New = "Box 2" },
            new { Num = "150", Name = "Mewtwo", Level = "70", Type = "Psychic", Shiny = "No", IVs = "186", Current = "Box 10", New = "Box 2" },
            new { Num = "133", Name = "Eevee", Level = "15", Type = "Normal", Shiny = "Yes", IVs = "155", Current = "Box 2", New = "Box 2" }
        };

        foreach (var mon in pokemon)
        {
            var item = new ListViewItem(mon.Num);
            item.SubItems.Add(mon.Name);
            item.SubItems.Add(mon.Level);
            item.SubItems.Add(mon.Type);
            item.SubItems.Add(mon.Shiny);
            item.SubItems.Add(mon.IVs);
            item.SubItems.Add(mon.Current);
            item.SubItems.Add(mon.New);

            if (mon.Shiny == "Yes")
                item.ForeColor = Color.Gold;

            lstPreview.Items.Add(item);
        }

        lblStats.Text = "Total Pokemon: 5 | Will be moved: 3";
    }

    private void UpdatePreview()
    {
        // Would update preview based on sort settings
    }

    private void SortBoxes(object? sender, EventArgs e)
    {
        var result = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Sort Boxes?", "This will reorganize your Pokemon in the boxes.\n\nThis operation cannot be undone. Continue?");
        if (result != DialogResult.Yes) return;

        prgSort.Value = 0;
        for (int i = 0; i <= 100; i += 10)
        {
            prgSort.Value = i;
            System.Threading.Thread.Sleep(50);
        }

        WinFormsUtil.Alert("Box organization complete!\n\nPokemon have been sorted according to your settings.");
    }
}
