using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class BreedingPreview : Form
{
    private readonly SaveFile SAV;
    private ComboBox cmbParent1Species = null!;
    private ComboBox cmbParent2Species = null!;
    private ComboBox cmbParent1Nature = null!;
    private ComboBox cmbParent2Nature = null!;
    private CheckBox chkParent1Everstone = null!;
    private CheckBox chkParent2Everstone = null!;
    private CheckBox chkDestinyKnot = null!;
    private NumericUpDown[] nudParent1IVs = new NumericUpDown[6];
    private NumericUpDown[] nudParent2IVs = new NumericUpDown[6];
    private Panel pnlPreview = null!;
    private Label lblCompatibility = null!;
    private Label lblEggSpecies = null!;
    private Label lblInheritance = null!;
    private Label lblShinyOdds = null!;
    private CheckBox chkMasuda = null!;
    private CheckBox chkShinyCharm = null!;
    private ProgressBar prgCompatibility = null!;

    private static readonly string[] StatNames = { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };
    private static readonly string[] Natures = { "Hardy", "Lonely", "Brave", "Adamant", "Naughty", "Bold", "Docile", "Relaxed", "Impish", "Lax", "Timid", "Hasty", "Serious", "Jolly", "Naive", "Modest", "Mild", "Quiet", "Bashful", "Rash", "Calm", "Gentle", "Sassy", "Careful", "Quirky" };

    public BreedingPreview(SaveFile sav)
    {
        SAV = sav;
        Text = "Breeding Preview";
        Size = new Size(950, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(20, 20, 35);
        InitializeUI();
        UpdatePreview();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "🥚 Breeding Preview",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.FromArgb(255, 200, 150),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold)
        };

        var lblSubtitle = new Label
        {
            Text = "Preview potential offspring from breeding pairs",
            Location = new Point(22, 50),
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9F)
        };

        // Parent 1
        var grpParent1 = new GroupBox
        {
            Text = "Parent 1 (Female/Ditto)",
            Location = new Point(20, 80),
            Size = new Size(300, 280),
            ForeColor = Color.FromArgb(255, 150, 200)
        };

        var lblSpecies1 = new Label { Text = "Species:", Location = new Point(15, 30), AutoSize = true, ForeColor = Color.White };
        cmbParent1Species = new ComboBox
        {
            Location = new Point(80, 27),
            Width = 150,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbParent1Species.Items.AddRange(new[] { "Ditto", "Pikachu", "Eevee", "Charmander", "Bulbasaur", "Squirtle", "Ralts", "Gastly", "Gible", "Larvitar" });
        cmbParent1Species.SelectedIndex = 0;
        cmbParent1Species.SelectedIndexChanged += (s, e) => UpdatePreview();

        var lblNature1 = new Label { Text = "Nature:", Location = new Point(15, 60), AutoSize = true, ForeColor = Color.White };
        cmbParent1Nature = new ComboBox
        {
            Location = new Point(80, 57),
            Width = 120,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbParent1Nature.Items.AddRange(Natures);
        cmbParent1Nature.SelectedIndex = 3; // Adamant
        cmbParent1Nature.SelectedIndexChanged += (s, e) => UpdatePreview();

        chkParent1Everstone = new CheckBox { Text = "Everstone", Location = new Point(210, 60), AutoSize = true, ForeColor = Color.Cyan };
        chkParent1Everstone.CheckedChanged += (s, e) => UpdatePreview();

        var lblIVs1 = new Label { Text = "IVs:", Location = new Point(15, 95), AutoSize = true, ForeColor = Color.White };

        for (int i = 0; i < 6; i++)
        {
            var lbl = new Label
            {
                Text = StatNames[i],
                Location = new Point(15 + (i % 3) * 95, 115 + (i / 3) * 50),
                AutoSize = true,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8F)
            };

            nudParent1IVs[i] = new NumericUpDown
            {
                Location = new Point(15 + (i % 3) * 95, 133 + (i / 3) * 50),
                Width = 50,
                Maximum = 31,
                Value = 31,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White
            };
            nudParent1IVs[i].ValueChanged += (s, e) => UpdatePreview();

            grpParent1.Controls.AddRange(new Control[] { lbl, nudParent1IVs[i] });
        }

        grpParent1.Controls.AddRange(new Control[] { lblSpecies1, cmbParent1Species, lblNature1, cmbParent1Nature, chkParent1Everstone, lblIVs1 });

        // Parent 2
        var grpParent2 = new GroupBox
        {
            Text = "Parent 2 (Male)",
            Location = new Point(340, 80),
            Size = new Size(300, 280),
            ForeColor = Color.FromArgb(150, 200, 255)
        };

        var lblSpecies2 = new Label { Text = "Species:", Location = new Point(15, 30), AutoSize = true, ForeColor = Color.White };
        cmbParent2Species = new ComboBox
        {
            Location = new Point(80, 27),
            Width = 150,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbParent2Species.Items.AddRange(new[] { "Pikachu", "Eevee", "Charmander", "Bulbasaur", "Squirtle", "Ralts", "Gastly", "Gible", "Larvitar", "Riolu" });
        cmbParent2Species.SelectedIndex = 0;
        cmbParent2Species.SelectedIndexChanged += (s, e) => UpdatePreview();

        var lblNature2 = new Label { Text = "Nature:", Location = new Point(15, 60), AutoSize = true, ForeColor = Color.White };
        cmbParent2Nature = new ComboBox
        {
            Location = new Point(80, 57),
            Width = 120,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbParent2Nature.Items.AddRange(Natures);
        cmbParent2Nature.SelectedIndex = 10; // Timid
        cmbParent2Nature.SelectedIndexChanged += (s, e) => UpdatePreview();

        chkParent2Everstone = new CheckBox { Text = "Everstone", Location = new Point(210, 60), AutoSize = true, ForeColor = Color.Cyan };
        chkParent2Everstone.CheckedChanged += (s, e) => UpdatePreview();

        var lblIVs2 = new Label { Text = "IVs:", Location = new Point(15, 95), AutoSize = true, ForeColor = Color.White };

        for (int i = 0; i < 6; i++)
        {
            var lbl = new Label
            {
                Text = StatNames[i],
                Location = new Point(15 + (i % 3) * 95, 115 + (i / 3) * 50),
                AutoSize = true,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8F)
            };

            nudParent2IVs[i] = new NumericUpDown
            {
                Location = new Point(15 + (i % 3) * 95, 133 + (i / 3) * 50),
                Width = 50,
                Maximum = 31,
                Value = 31,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White
            };
            nudParent2IVs[i].ValueChanged += (s, e) => UpdatePreview();

            grpParent2.Controls.AddRange(new Control[] { lbl, nudParent2IVs[i] });
        }

        grpParent2.Controls.AddRange(new Control[] { lblSpecies2, cmbParent2Species, lblNature2, cmbParent2Nature, chkParent2Everstone, lblIVs2 });

        // Breeding Items
        var grpItems = new GroupBox
        {
            Text = "Breeding Settings",
            Location = new Point(660, 80),
            Size = new Size(260, 150),
            ForeColor = Color.White
        };

        chkDestinyKnot = new CheckBox
        {
            Text = "Destiny Knot (5 IVs)",
            Location = new Point(15, 30),
            AutoSize = true,
            ForeColor = Color.FromArgb(255, 100, 150),
            Checked = true
        };
        chkDestinyKnot.CheckedChanged += (s, e) => UpdatePreview();

        chkMasuda = new CheckBox
        {
            Text = "Masuda Method",
            Location = new Point(15, 55),
            AutoSize = true,
            ForeColor = Color.Gold
        };
        chkMasuda.CheckedChanged += (s, e) => UpdatePreview();

        chkShinyCharm = new CheckBox
        {
            Text = "Shiny Charm",
            Location = new Point(15, 80),
            AutoSize = true,
            ForeColor = Color.Gold
        };
        chkShinyCharm.CheckedChanged += (s, e) => UpdatePreview();

        lblShinyOdds = new Label
        {
            Text = "Shiny Odds: 1/4096",
            Location = new Point(15, 110),
            AutoSize = true,
            ForeColor = Color.White
        };

        grpItems.Controls.AddRange(new Control[] { chkDestinyKnot, chkMasuda, chkShinyCharm, lblShinyOdds });

        // Compatibility
        var grpCompat = new GroupBox
        {
            Text = "Compatibility",
            Location = new Point(660, 240),
            Size = new Size(260, 120),
            ForeColor = Color.White
        };

        lblCompatibility = new Label
        {
            Text = "The two seem to get along",
            Location = new Point(15, 30),
            Size = new Size(230, 40),
            ForeColor = Color.Lime
        };

        prgCompatibility = new ProgressBar
        {
            Location = new Point(15, 75),
            Size = new Size(230, 20),
            Maximum = 100,
            Value = 70
        };

        grpCompat.Controls.AddRange(new Control[] { lblCompatibility, prgCompatibility });

        // Offspring Preview
        var grpOffspring = new GroupBox
        {
            Text = "Potential Offspring",
            Location = new Point(20, 370),
            Size = new Size(900, 230),
            ForeColor = Color.White
        };

        pnlPreview = new Panel
        {
            Location = new Point(10, 25),
            Size = new Size(880, 195),
            AutoScroll = true,
            BackColor = Color.FromArgb(30, 30, 50)
        };

        lblEggSpecies = new Label
        {
            Text = "Egg Species: Pichu",
            Location = new Point(15, 25),
            AutoSize = true,
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        lblInheritance = new Label
        {
            Text = "IV Inheritance: 5 IVs from parents (Destiny Knot)",
            Location = new Point(15, 50),
            Size = new Size(400, 20),
            ForeColor = Color.LightGray
        };

        grpOffspring.Controls.AddRange(new Control[] { pnlPreview });

        // Simulate Button
        var btnSimulate = new Button
        {
            Text = "Simulate 100 Eggs",
            Location = new Point(660, 610),
            Size = new Size(140, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        btnSimulate.Click += SimulateEggs;

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(820, 610),
            Size = new Size(100, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, lblSubtitle, grpParent1, grpParent2, grpItems, grpCompat, grpOffspring, btnSimulate, btnClose });
    }

    private void UpdatePreview()
    {
        // Update egg species
        string parent1 = cmbParent1Species.SelectedItem?.ToString() ?? "";
        string parent2 = cmbParent2Species.SelectedItem?.ToString() ?? "";

        string eggSpecies;
        if (parent1 == "Ditto")
            eggSpecies = GetBabySpecies(parent2);
        else
            eggSpecies = GetBabySpecies(parent1);

        lblEggSpecies.Text = $"Egg Species: {eggSpecies}";

        // Update shiny odds
        int odds = 4096;
        if (chkMasuda.Checked && chkShinyCharm.Checked)
            odds = 512;
        else if (chkMasuda.Checked)
            odds = 683;
        else if (chkShinyCharm.Checked)
            odds = 1365;

        lblShinyOdds.Text = $"Shiny Odds: 1/{odds}";
        lblShinyOdds.ForeColor = odds < 1000 ? Color.Gold : Color.White;

        // Update compatibility
        if (parent1 == "Ditto" || parent2 == "Ditto")
        {
            lblCompatibility.Text = "The two don't seem to like\neach other much (w/ Ditto)";
            lblCompatibility.ForeColor = Color.Orange;
            prgCompatibility.Value = 20;
        }
        else if (parent1 == parent2)
        {
            lblCompatibility.Text = "The two seem to get along\nvery well! (Same species)";
            lblCompatibility.ForeColor = Color.Lime;
            prgCompatibility.Value = 70;
        }
        else
        {
            lblCompatibility.Text = "The two seem to get along";
            lblCompatibility.ForeColor = Color.LightGreen;
            prgCompatibility.Value = 50;
        }

        // Update IV inheritance text
        int ivsInherited = chkDestinyKnot.Checked ? 5 : 3;
        lblInheritance.Text = $"IV Inheritance: {ivsInherited} IVs from parents" +
            (chkDestinyKnot.Checked ? " (Destiny Knot)" : "");

        // Generate sample offspring
        GenerateOffspringPreviews();
    }

    private void GenerateOffspringPreviews()
    {
        pnlPreview.Controls.Clear();
        var rng = new Random();

        int[] parent1IVs = nudParent1IVs.Select(n => (int)n.Value).ToArray();
        int[] parent2IVs = nudParent2IVs.Select(n => (int)n.Value).ToArray();
        int ivsToInherit = chkDestinyKnot.Checked ? 5 : 3;

        // Generate 5 sample offspring
        for (int i = 0; i < 5; i++)
        {
            var card = new Panel
            {
                Location = new Point(10 + i * 175, 10),
                Size = new Size(165, 175),
                BackColor = Color.FromArgb(40, 40, 60),
                BorderStyle = BorderStyle.FixedSingle
            };

            int[] offspringIVs = new int[6];

            // Determine which IVs to inherit
            var inheritIndices = Enumerable.Range(0, 6).OrderBy(x => rng.Next()).Take(ivsToInherit).ToList();

            for (int j = 0; j < 6; j++)
            {
                if (inheritIndices.Contains(j))
                {
                    // Inherit from random parent
                    offspringIVs[j] = rng.Next(2) == 0 ? parent1IVs[j] : parent2IVs[j];
                }
                else
                {
                    // Random IV
                    offspringIVs[j] = rng.Next(32);
                }
            }

            // Determine nature
            string nature;
            if (chkParent1Everstone.Checked && chkParent2Everstone.Checked)
                nature = rng.Next(2) == 0 ? cmbParent1Nature.SelectedItem?.ToString()! : cmbParent2Nature.SelectedItem?.ToString()!;
            else if (chkParent1Everstone.Checked)
                nature = cmbParent1Nature.SelectedItem?.ToString()!;
            else if (chkParent2Everstone.Checked)
                nature = cmbParent2Nature.SelectedItem?.ToString()!;
            else
                nature = Natures[rng.Next(Natures.Length)];

            var lblNum = new Label
            {
                Text = $"Offspring #{i + 1}",
                Location = new Point(5, 5),
                AutoSize = true,
                ForeColor = Color.Cyan,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var lblNature = new Label
            {
                Text = nature,
                Location = new Point(5, 28),
                AutoSize = true,
                ForeColor = Color.Gold
            };

            // Show IVs
            for (int j = 0; j < 6; j++)
            {
                var lbl = new Label
                {
                    Text = $"{StatNames[j]}: {offspringIVs[j]}",
                    Location = new Point(5 + (j % 2) * 80, 50 + (j / 2) * 22),
                    AutoSize = true,
                    ForeColor = offspringIVs[j] == 31 ? Color.Lime :
                               (offspringIVs[j] >= 25 ? Color.LightGreen :
                               (offspringIVs[j] < 10 ? Color.Gray : Color.White)),
                    Font = new Font("Segoe UI", 8F)
                };
                card.Controls.Add(lbl);
            }

            // Total IVs
            int total = offspringIVs.Sum();
            int perfect = offspringIVs.Count(iv => iv == 31);
            var lblTotal = new Label
            {
                Text = $"Total: {total}/186 ({perfect}x31)",
                Location = new Point(5, 120),
                AutoSize = true,
                ForeColor = perfect >= 5 ? Color.Gold : (perfect >= 3 ? Color.Lime : Color.White),
                Font = new Font("Segoe UI", 8F)
            };

            // Quality rating
            string quality = perfect switch
            {
                6 => "★★★★★ PERFECT!",
                5 => "★★★★☆ Excellent!",
                4 => "★★★☆☆ Great",
                3 => "★★☆☆☆ Good",
                _ => "★☆☆☆☆ Keep trying"
            };
            var lblQuality = new Label
            {
                Text = quality,
                Location = new Point(5, 145),
                AutoSize = true,
                ForeColor = perfect >= 5 ? Color.Gold : (perfect >= 3 ? Color.Lime : Color.Gray),
                Font = new Font("Segoe UI", 8F)
            };

            card.Controls.AddRange(new Control[] { lblNum, lblNature, lblTotal, lblQuality });
            pnlPreview.Controls.Add(card);
        }
    }

    private void SimulateEggs(object? sender, EventArgs e)
    {
        var rng = new Random();
        int[] parent1IVs = nudParent1IVs.Select(n => (int)n.Value).ToArray();
        int[] parent2IVs = nudParent2IVs.Select(n => (int)n.Value).ToArray();
        int ivsToInherit = chkDestinyKnot.Checked ? 5 : 3;

        int perfectCount = 0;
        int fiveIVCount = 0;
        int fourIVCount = 0;
        int shinies = 0;

        int shinyOdds = 4096;
        if (chkMasuda.Checked && chkShinyCharm.Checked)
            shinyOdds = 512;
        else if (chkMasuda.Checked)
            shinyOdds = 683;
        else if (chkShinyCharm.Checked)
            shinyOdds = 1365;

        for (int i = 0; i < 100; i++)
        {
            int[] ivs = new int[6];
            var inheritIndices = Enumerable.Range(0, 6).OrderBy(x => rng.Next()).Take(ivsToInherit).ToList();

            for (int j = 0; j < 6; j++)
            {
                if (inheritIndices.Contains(j))
                    ivs[j] = rng.Next(2) == 0 ? parent1IVs[j] : parent2IVs[j];
                else
                    ivs[j] = rng.Next(32);
            }

            int perfect = ivs.Count(iv => iv == 31);
            if (perfect == 6) perfectCount++;
            else if (perfect == 5) fiveIVCount++;
            else if (perfect == 4) fourIVCount++;

            if (rng.Next(shinyOdds) == 0)
                shinies++;
        }

        WinFormsUtil.Alert("Simulation Results (100 Eggs)",
            $"6 IV (Perfect): {perfectCount}\n" +
            $"5 IV: {fiveIVCount}\n" +
            $"4 IV: {fourIVCount}\n" +
            $"Shinies: {shinies}\n\n" +
            $"With your current setup, you have a good chance\n" +
            $"of getting a competitive Pokemon!");
    }

    private static string GetBabySpecies(string parent) => parent switch
    {
        "Pikachu" => "Pichu",
        "Raichu" => "Pichu",
        "Eevee" => "Eevee",
        "Charmander" => "Charmander",
        "Charmeleon" => "Charmander",
        "Charizard" => "Charmander",
        "Bulbasaur" => "Bulbasaur",
        "Squirtle" => "Squirtle",
        "Ralts" => "Ralts",
        "Gastly" => "Gastly",
        "Gible" => "Gible",
        "Larvitar" => "Larvitar",
        "Riolu" => "Riolu",
        _ => parent
    };
}
