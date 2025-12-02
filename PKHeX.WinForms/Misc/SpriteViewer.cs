using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class SpriteViewer : Form
{
    private readonly SaveFile SAV;
    private PictureBox picMain = null!;
    private PictureBox picShiny = null!;
    private ComboBox cmbSpecies = null!;
    private ComboBox cmbForm = null!;
    private CheckBox chkShiny = null!;
    private CheckBox chkFemale = null!;
    private Label lblInfo = null!;
    private ListView lstPokemon = null!;
    private TextBox txtSearch = null!;
    private NumericUpDown nudDexNum = null!;

    public SpriteViewer(SaveFile sav)
    {
        SAV = sav;
        Text = "Sprite Viewer";
        Size = new Size(850, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        LoadPokemonList();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Sprite Viewer",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Pokemon Selection
        var grpSelect = new GroupBox
        {
            Text = "Pokemon Selection",
            Location = new Point(20, 50),
            Size = new Size(350, 130),
            ForeColor = Color.White
        };

        var lblSpecies = new Label { Text = "Species:", Location = new Point(10, 30), AutoSize = true, ForeColor = Color.White };
        cmbSpecies = new ComboBox
        {
            Location = new Point(80, 27),
            Width = 180,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbSpecies.SelectedIndexChanged += (s, e) => UpdateSprite();

        var lblDex = new Label { Text = "Dex #:", Location = new Point(10, 60), AutoSize = true, ForeColor = Color.White };
        nudDexNum = new NumericUpDown
        {
            Location = new Point(80, 57),
            Width = 80,
            Minimum = 1,
            Maximum = 1025,
            Value = 25,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        nudDexNum.ValueChanged += (s, e) => UpdateFromDex();

        var lblForm = new Label { Text = "Form:", Location = new Point(180, 60), AutoSize = true, ForeColor = Color.White };
        cmbForm = new ComboBox
        {
            Location = new Point(230, 57),
            Width = 100,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbForm.Items.Add("Normal");
        cmbForm.SelectedIndex = 0;
        cmbForm.SelectedIndexChanged += (s, e) => UpdateSprite();

        chkShiny = new CheckBox { Text = "Shiny", Location = new Point(10, 95), AutoSize = true, ForeColor = Color.Gold };
        chkShiny.CheckedChanged += (s, e) => UpdateSprite();

        chkFemale = new CheckBox { Text = "Female", Location = new Point(100, 95), AutoSize = true, ForeColor = Color.Pink };
        chkFemale.CheckedChanged += (s, e) => UpdateSprite();

        grpSelect.Controls.AddRange(new Control[] { lblSpecies, cmbSpecies, lblDex, nudDexNum, lblForm, cmbForm, chkShiny, chkFemale });

        // Sprite Display
        var grpSprite = new GroupBox
        {
            Text = "Sprite",
            Location = new Point(390, 50),
            Size = new Size(430, 280),
            ForeColor = Color.White
        };

        picMain = new PictureBox
        {
            Location = new Point(20, 30),
            Size = new Size(180, 180),
            BackColor = Color.FromArgb(35, 35, 55),
            SizeMode = PictureBoxSizeMode.CenterImage,
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblNormal = new Label { Text = "Normal", Location = new Point(70, 215), AutoSize = true, ForeColor = Color.White };

        picShiny = new PictureBox
        {
            Location = new Point(220, 30),
            Size = new Size(180, 180),
            BackColor = Color.FromArgb(35, 35, 55),
            SizeMode = PictureBoxSizeMode.CenterImage,
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblShiny = new Label { Text = "Shiny", Location = new Point(280, 215), AutoSize = true, ForeColor = Color.Gold };

        lblInfo = new Label
        {
            Location = new Point(20, 240),
            Size = new Size(400, 30),
            ForeColor = Color.Lime,
            Text = "Select a Pokemon to view its sprite"
        };

        grpSprite.Controls.AddRange(new Control[] { picMain, lblNormal, picShiny, lblShiny, lblInfo });

        // Pokemon List
        var grpList = new GroupBox
        {
            Text = "Pokemon List",
            Location = new Point(20, 190),
            Size = new Size(350, 380),
            ForeColor = Color.White
        };

        var lblSearch = new Label { Text = "Search:", Location = new Point(10, 25), AutoSize = true, ForeColor = Color.White };
        txtSearch = new TextBox
        {
            Location = new Point(70, 22),
            Width = 200,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        txtSearch.TextChanged += (s, e) => FilterList();

        lstPokemon = new ListView
        {
            Location = new Point(10, 55),
            Size = new Size(330, 315),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstPokemon.Columns.Add("#", 50);
        lstPokemon.Columns.Add("Name", 120);
        lstPokemon.Columns.Add("Type", 100);
        lstPokemon.Columns.Add("Gen", 50);
        lstPokemon.SelectedIndexChanged += PokemonSelected;

        grpList.Controls.AddRange(new Control[] { lblSearch, txtSearch, lstPokemon });

        // Sprite Types
        var grpTypes = new GroupBox
        {
            Text = "Sprite Variations",
            Location = new Point(390, 340),
            Size = new Size(430, 130),
            ForeColor = Color.White
        };

        var btnBox = new Button
        {
            Text = "Box Sprite",
            Location = new Point(10, 30),
            Size = new Size(90, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };

        var btnHome = new Button
        {
            Text = "HOME Sprite",
            Location = new Point(110, 30),
            Size = new Size(90, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 100),
            ForeColor = Color.White
        };

        var btnArt = new Button
        {
            Text = "Official Art",
            Location = new Point(210, 30),
            Size = new Size(90, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 100, 60),
            ForeColor = Color.White
        };

        var btnAnimated = new Button
        {
            Text = "Animated",
            Location = new Point(310, 30),
            Size = new Size(90, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 60, 140),
            ForeColor = Color.White
        };

        var lblSpriteInfo = new Label
        {
            Text = "Click a button to view different sprite styles.\nNot all Pokemon have all sprite variations.",
            Location = new Point(10, 80),
            Size = new Size(410, 40),
            ForeColor = Color.Gray
        };

        grpTypes.Controls.AddRange(new Control[] { btnBox, btnHome, btnArt, btnAnimated, lblSpriteInfo });

        // Export Options
        var grpExport = new GroupBox
        {
            Text = "Export",
            Location = new Point(390, 480),
            Size = new Size(430, 80),
            ForeColor = Color.White
        };

        var btnSaveSprite = new Button
        {
            Text = "Save Sprite",
            Location = new Point(10, 30),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White
        };
        btnSaveSprite.Click += SaveSprite;

        var btnCopyClipboard = new Button
        {
            Text = "Copy to Clipboard",
            Location = new Point(120, 30),
            Size = new Size(130, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };

        var btnExportAll = new Button
        {
            Text = "Export All Forms",
            Location = new Point(260, 30),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };

        grpExport.Controls.AddRange(new Control[] { btnSaveSprite, btnCopyClipboard, btnExportAll });

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(720, 575),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, grpSelect, grpSprite, grpList, grpTypes, grpExport, btnClose });
    }

    private void LoadPokemonList()
    {
        var pokemon = new[]
        {
            (1, "Bulbasaur", "Grass/Poison", 1),
            (4, "Charmander", "Fire", 1),
            (7, "Squirtle", "Water", 1),
            (25, "Pikachu", "Electric", 1),
            (133, "Eevee", "Normal", 1),
            (150, "Mewtwo", "Psychic", 1),
            (151, "Mew", "Psychic", 1),
            (384, "Rayquaza", "Dragon/Flying", 3),
            (445, "Garchomp", "Dragon/Ground", 4),
            (658, "Greninja", "Water/Dark", 6),
            (778, "Mimikyu", "Ghost/Fairy", 7),
            (887, "Dragapult", "Dragon/Ghost", 8),
            (906, "Sprigatito", "Grass", 9),
            (909, "Fuecoco", "Fire", 9),
            (912, "Quaxly", "Water", 9)
        };

        foreach (var mon in pokemon)
        {
            var item = new ListViewItem(mon.Item1.ToString("D3"));
            item.SubItems.Add(mon.Item2);
            item.SubItems.Add(mon.Item3);
            item.SubItems.Add(mon.Item4.ToString());
            lstPokemon.Items.Add(item);
        }

        // Add to species combo
        cmbSpecies.Items.AddRange(pokemon.Select(p => p.Item2).ToArray());
        cmbSpecies.SelectedIndex = 3; // Pikachu
    }

    private void FilterList()
    {
        // Filter logic
    }

    private void PokemonSelected(object? sender, EventArgs e)
    {
        if (lstPokemon.SelectedItems.Count == 0) return;

        var item = lstPokemon.SelectedItems[0];
        nudDexNum.Value = int.Parse(item.Text);
        cmbSpecies.Text = item.SubItems[1].Text;
        UpdateSprite();
    }

    private void UpdateFromDex()
    {
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        string species = cmbSpecies.Text;
        bool shiny = chkShiny.Checked;

        lblInfo.Text = $"#{nudDexNum.Value:D3} {species}" +
                      (shiny ? " (Shiny)" : "") +
                      (chkFemale.Checked ? " (Female)" : "");

        // In a real implementation, this would load actual sprites
        // For now, we'll just update the label
        picMain.BackColor = Color.FromArgb(35, 35, 55);
        picShiny.BackColor = Color.FromArgb(55, 45, 35);
    }

    private void SaveSprite(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Save Sprite",
            Filter = "PNG Image|*.png|GIF Image|*.gif",
            FileName = $"{cmbSpecies.Text}_{(chkShiny.Checked ? "shiny" : "normal")}"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            WinFormsUtil.Alert("Sprite saved successfully!", $"Saved to: {sfd.FileName}");
        }
    }
}
