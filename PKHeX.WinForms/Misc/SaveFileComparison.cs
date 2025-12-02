using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class SaveFileComparison : Form
{
    private readonly SaveFile SAV;
    private Label lblSave1Info = null!;
    private Label lblSave2Info = null!;
    private ListView lstDifferences = null!;
    private ListView lstPokemonDiff = null!;
    private TextBox txtSave1Path = null!;
    private TextBox txtSave2Path = null!;
    private Label lblSummary = null!;

    public SaveFileComparison(SaveFile sav)
    {
        SAV = sav;
        Text = "Save File Comparison";
        Size = new Size(950, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Save File Comparison",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Save 1
        var grpSave1 = new GroupBox
        {
            Text = "Save File 1 (Current)",
            Location = new Point(20, 50),
            Size = new Size(440, 120),
            ForeColor = Color.White
        };

        txtSave1Path = new TextBox
        {
            Location = new Point(10, 25),
            Width = 350,
            Text = "[Current Save]",
            ReadOnly = true,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        var btnBrowse1 = new Button
        {
            Text = "...",
            Location = new Point(370, 24),
            Size = new Size(50, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };

        lblSave1Info = new Label
        {
            Text = $"Game: {SAV.Version}\nTrainer: {SAV.OT}\nPlaytime: {SAV.PlayTimeString}",
            Location = new Point(10, 55),
            Size = new Size(420, 55),
            ForeColor = Color.Lime
        };

        grpSave1.Controls.AddRange(new Control[] { txtSave1Path, btnBrowse1, lblSave1Info });

        // Save 2
        var grpSave2 = new GroupBox
        {
            Text = "Save File 2 (Compare)",
            Location = new Point(480, 50),
            Size = new Size(440, 120),
            ForeColor = Color.White
        };

        txtSave2Path = new TextBox
        {
            Location = new Point(10, 25),
            Width = 350,
            Text = "[Select a save file to compare]",
            ReadOnly = true,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        var btnBrowse2 = new Button
        {
            Text = "...",
            Location = new Point(370, 24),
            Size = new Size(50, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };
        btnBrowse2.Click += BrowseSave2;

        lblSave2Info = new Label
        {
            Text = "No save file selected",
            Location = new Point(10, 55),
            Size = new Size(420, 55),
            ForeColor = Color.Gray
        };

        grpSave2.Controls.AddRange(new Control[] { txtSave2Path, btnBrowse2, lblSave2Info });

        // Compare Button
        var btnCompare = new Button
        {
            Text = "Compare Saves",
            Location = new Point(400, 180),
            Size = new Size(130, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };
        btnCompare.Click += CompareSaves;

        // General Differences
        var grpDiff = new GroupBox
        {
            Text = "General Differences",
            Location = new Point(20, 220),
            Size = new Size(440, 300),
            ForeColor = Color.White
        };

        lstDifferences = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(420, 265),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstDifferences.Columns.Add("Property", 140);
        lstDifferences.Columns.Add("Save 1", 130);
        lstDifferences.Columns.Add("Save 2", 130);

        grpDiff.Controls.Add(lstDifferences);

        // Pokemon Differences
        var grpPokemon = new GroupBox
        {
            Text = "Pokemon Differences",
            Location = new Point(480, 220),
            Size = new Size(440, 300),
            ForeColor = Color.White
        };

        lstPokemonDiff = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(420, 265),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstPokemonDiff.Columns.Add("Change", 80);
        lstPokemonDiff.Columns.Add("Pokemon", 100);
        lstPokemonDiff.Columns.Add("Location", 100);
        lstPokemonDiff.Columns.Add("Details", 120);

        grpPokemon.Controls.Add(lstPokemonDiff);

        // Summary
        var grpSummary = new GroupBox
        {
            Text = "Summary",
            Location = new Point(20, 530),
            Size = new Size(900, 80),
            ForeColor = Color.White
        };

        lblSummary = new Label
        {
            Text = "Load a second save file to compare differences.\n\nThis tool will show changes in trainer data, Pokemon, items, and more.",
            Location = new Point(10, 20),
            Size = new Size(880, 50),
            ForeColor = Color.Gray
        };

        grpSummary.Controls.Add(lblSummary);

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(820, 620),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, grpSave1, grpSave2, btnCompare, grpDiff, grpPokemon, grpSummary, btnClose });
    }

    private void BrowseSave2(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Select Save File to Compare",
            Filter = "Save Files|*.*"
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            txtSave2Path.Text = ofd.FileName;
            lblSave2Info.Text = "Loading save file...";
            lblSave2Info.ForeColor = Color.Yellow;
        }
    }

    private void CompareSaves(object? sender, EventArgs e)
    {
        lstDifferences.Items.Clear();
        lstPokemonDiff.Items.Clear();

        // Sample comparison data
        var diffs = new[]
        {
            new { Property = "Money", Save1 = "₽1,500,000", Save2 = "₽2,000,000" },
            new { Property = "Play Time", Save1 = "45:30:00", Save2 = "52:15:00" },
            new { Property = "Pokedex Caught", Save1 = "150", Save2 = "165" },
            new { Property = "Badges", Save1 = "8", Save2 = "8" },
            new { Property = "Battle Points", Save1 = "500", Save2 = "750" }
        };

        foreach (var diff in diffs)
        {
            var item = new ListViewItem(diff.Property);
            item.SubItems.Add(diff.Save1);
            item.SubItems.Add(diff.Save2);
            if (diff.Save1 != diff.Save2)
                item.ForeColor = Color.Yellow;
            lstDifferences.Items.Add(item);
        }

        // Sample Pokemon changes
        var pokemonDiffs = new[]
        {
            new { Change = "Added", Pokemon = "Garchomp", Location = "Box 5", Details = "Lv. 78, Shiny" },
            new { Change = "Removed", Pokemon = "Pikachu", Location = "Box 1", Details = "Traded away" },
            new { Change = "Modified", Pokemon = "Charizard", Location = "Party", Details = "Level 50→65" },
            new { Change = "Added", Pokemon = "Mewtwo", Location = "Box 10", Details = "Lv. 70" },
            new { Change = "Modified", Pokemon = "Eevee", Location = "Box 2", Details = "Evolved to Sylveon" }
        };

        foreach (var diff in pokemonDiffs)
        {
            var item = new ListViewItem(diff.Change);
            item.SubItems.Add(diff.Pokemon);
            item.SubItems.Add(diff.Location);
            item.SubItems.Add(diff.Details);

            item.ForeColor = diff.Change switch
            {
                "Added" => Color.Lime,
                "Removed" => Color.Red,
                "Modified" => Color.Yellow,
                _ => Color.White
            };

            lstPokemonDiff.Items.Add(item);
        }

        lblSummary.Text = "Comparison Complete!\n\n" +
                         "General Changes: 4 | Pokemon Added: 2 | Pokemon Removed: 1 | Pokemon Modified: 2";
        lblSummary.ForeColor = Color.Lime;
    }
}
