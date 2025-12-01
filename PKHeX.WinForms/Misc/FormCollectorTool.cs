using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;

namespace PKHeX.WinForms;

public class FormCollectorTool : Form
{
    private readonly SaveFile SAV;
    private readonly Dictionary<int, HashSet<byte>> _ownedForms = new();

    private readonly ComboBox CB_Pokemon;
    private readonly FlowLayoutPanel FLP_Forms;
    private readonly Panel PNL_Stats;
    private readonly Label L_TotalForms;
    private readonly Label L_OwnedForms;
    private readonly Label L_MissingForms;
    private readonly ProgressBar PB_Forms;
    private readonly ListView LV_Summary;

    // Pokemon with collectible forms
    private static readonly Dictionary<int, (string name, int formCount, string[] formNames)> FormPokemon = new()
    {
        [201] = ("Unown", 28, new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "!", "?" }),
        [666] = ("Vivillon", 20, new[] { "Meadow", "Polar", "Tundra", "Continental", "Garden", "Elegant", "Icy Snow", "Modern", "Marine", "Archipelago", "High Plains", "Sandstorm", "River", "Monsoon", "Savanna", "Sun", "Ocean", "Jungle", "Fancy", "Pokeball" }),
        [869] = ("Alcremie", 63, GenerateAlcremieFormNames()),
        [585] = ("Deerling", 4, new[] { "Spring", "Summer", "Autumn", "Winter" }),
        [586] = ("Sawsbuck", 4, new[] { "Spring", "Summer", "Autumn", "Winter" }),
        [412] = ("Burmy", 3, new[] { "Plant", "Sandy", "Trash" }),
        [413] = ("Wormadam", 3, new[] { "Plant", "Sandy", "Trash" }),
        [422] = ("Shellos", 2, new[] { "West", "East" }),
        [423] = ("Gastrodon", 2, new[] { "West", "East" }),
        [550] = ("Basculin", 3, new[] { "Red", "Blue", "White" }),
        [669] = ("Flabebe", 5, new[] { "Red", "Yellow", "Orange", "Blue", "White" }),
        [670] = ("Floette", 6, new[] { "Red", "Yellow", "Orange", "Blue", "White", "Eternal" }),
        [671] = ("Florges", 5, new[] { "Red", "Yellow", "Orange", "Blue", "White" }),
        [676] = ("Furfrou", 10, new[] { "Natural", "Heart", "Star", "Diamond", "Debutante", "Matron", "Dandy", "La Reine", "Kabuki", "Pharaoh" }),
        [710] = ("Pumpkaboo", 4, new[] { "Small", "Average", "Large", "Super" }),
        [711] = ("Gourgeist", 4, new[] { "Small", "Average", "Large", "Super" }),
        [741] = ("Oricorio", 4, new[] { "Baile", "Pom-Pom", "Pa'u", "Sensu" }),
        [774] = ("Minior", 7, new[] { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet" }),
        [849] = ("Toxtricity", 2, new[] { "Amped", "Low Key" }),
        [875] = ("Eiscue", 2, new[] { "Ice Face", "Noice Face" }),
        [876] = ("Indeedee", 2, new[] { "Male", "Female" }),
        [877] = ("Morpeko", 2, new[] { "Full Belly", "Hangry" }),
        [892] = ("Urshifu", 2, new[] { "Single Strike", "Rapid Strike" }),
        [898] = ("Calyrex", 3, new[] { "Normal", "Ice Rider", "Shadow Rider" }),
        [902] = ("Basculegion", 2, new[] { "Male", "Female" }),
        [905] = ("Enamorus", 2, new[] { "Incarnate", "Therian" }),
        [925] = ("Maushold", 2, new[] { "Family of Three", "Family of Four" }),
        [931] = ("Squawkabilly", 4, new[] { "Green", "Blue", "Yellow", "White" }),
        [964] = ("Palafin", 2, new[] { "Zero", "Hero" }),
        [978] = ("Tatsugiri", 3, new[] { "Curly", "Droopy", "Stretchy" }),
        [999] = ("Gimmighoul", 2, new[] { "Chest", "Roaming" }),
        [1017] = ("Ogerpon", 4, new[] { "Teal Mask", "Wellspring Mask", "Hearthflame Mask", "Cornerstone Mask" }),
        [1024] = ("Terapagos", 3, new[] { "Normal", "Terastal", "Stellar" }),
    };

    public FormCollectorTool(SaveFile sav)
    {
        SAV = sav;
        Text = "Form Collector";
        Size = new Size(1000, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblTitle = new Label
        {
            Text = "Pokemon Form Collector",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        var lblSelect = new Label
        {
            Text = "Select Pokemon:",
            Location = new Point(350, 20),
            AutoSize = true,
            ForeColor = Color.White
        };

        CB_Pokemon = new ComboBox
        {
            Location = new Point(460, 17),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 60),
            ForeColor = Color.White
        };
        CB_Pokemon.Items.Add("-- All Pokemon --");
        foreach (var kvp in FormPokemon.OrderBy(x => x.Value.name))
            CB_Pokemon.Items.Add(kvp.Value.name);
        CB_Pokemon.SelectedIndex = 0;
        CB_Pokemon.SelectedIndexChanged += (s, e) => RefreshDisplay();

        // Stats panel
        PNL_Stats = new Panel
        {
            Location = new Point(20, 55),
            Size = new Size(960, 60),
            BackColor = Color.FromArgb(35, 35, 55),
            BorderStyle = BorderStyle.FixedSingle
        };

        L_TotalForms = new Label
        {
            Text = "Total Forms: 0",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11F)
        };

        L_OwnedForms = new Label
        {
            Text = "Owned: 0",
            Location = new Point(180, 10),
            AutoSize = true,
            ForeColor = Color.LightGreen,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        L_MissingForms = new Label
        {
            Text = "Missing: 0",
            Location = new Point(320, 10),
            AutoSize = true,
            ForeColor = Color.Salmon,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        PB_Forms = new ProgressBar
        {
            Location = new Point(20, 35),
            Size = new Size(920, 18),
            Style = ProgressBarStyle.Continuous
        };

        PNL_Stats.Controls.AddRange(new Control[] { L_TotalForms, L_OwnedForms, L_MissingForms, PB_Forms });

        // Summary list view
        LV_Summary = new ListView
        {
            Location = new Point(20, 125),
            Size = new Size(300, 520),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        LV_Summary.Columns.Add("Pokemon", 120);
        LV_Summary.Columns.Add("Owned", 80);
        LV_Summary.Columns.Add("Total", 80);
        LV_Summary.SelectedIndexChanged += (s, e) =>
        {
            if (LV_Summary.SelectedItems.Count > 0)
            {
                var name = LV_Summary.SelectedItems[0].Text;
                var idx = CB_Pokemon.Items.IndexOf(name);
                if (idx >= 0) CB_Pokemon.SelectedIndex = idx;
            }
        };

        // Forms display
        FLP_Forms = new FlowLayoutPanel
        {
            Location = new Point(330, 125),
            Size = new Size(650, 520),
            AutoScroll = true,
            BackColor = Color.FromArgb(30, 30, 50),
            BorderStyle = BorderStyle.FixedSingle
        };

        Controls.AddRange(new Control[] { lblTitle, lblSelect, CB_Pokemon, PNL_Stats, LV_Summary, FLP_Forms });

        ScanSaveFile();
        RefreshSummary();
        RefreshDisplay();
    }

    private static string[] GenerateAlcremieFormNames()
    {
        var creams = new[] { "Vanilla", "Ruby", "Matcha", "Mint", "Lemon", "Salted", "Ruby Swirl", "Caramel", "Rainbow" };
        var sweets = new[] { "Strawberry", "Berry", "Love", "Star", "Clover", "Flower", "Ribbon" };
        var names = new List<string>();

        foreach (var cream in creams)
            foreach (var sweet in sweets)
                names.Add($"{cream} {sweet}");

        return names.ToArray();
    }

    private void ScanSaveFile()
    {
        _ownedForms.Clear();

        for (int box = 0; box < SAV.BoxCount; box++)
        {
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                var pk = SAV.GetBoxSlotAtIndex(box, slot);
                if (pk.Species == 0) continue;

                if (!_ownedForms.ContainsKey(pk.Species))
                    _ownedForms[pk.Species] = new HashSet<byte>();
                _ownedForms[pk.Species].Add(pk.Form);
            }
        }

        if (SAV.HasParty)
        {
            for (int i = 0; i < 6; i++)
            {
                var pk = SAV.GetPartySlotAtIndex(i);
                if (pk.Species == 0) continue;

                if (!_ownedForms.ContainsKey(pk.Species))
                    _ownedForms[pk.Species] = new HashSet<byte>();
                _ownedForms[pk.Species].Add(pk.Form);
            }
        }
    }

    private void RefreshSummary()
    {
        LV_Summary.Items.Clear();

        foreach (var kvp in FormPokemon.OrderBy(x => x.Value.name))
        {
            int species = kvp.Key;
            var (name, formCount, _) = kvp.Value;

            int owned = _ownedForms.ContainsKey(species) ? _ownedForms[species].Count(f => f < formCount) : 0;

            var item = new ListViewItem(name);
            item.SubItems.Add(owned.ToString());
            item.SubItems.Add(formCount.ToString());

            if (owned == formCount)
                item.ForeColor = Color.Gold;
            else if (owned > 0)
                item.ForeColor = Color.LightGreen;
            else
                item.ForeColor = Color.Gray;

            LV_Summary.Items.Add(item);
        }
    }

    private void RefreshDisplay()
    {
        FLP_Forms.SuspendLayout();
        FLP_Forms.Controls.Clear();

        var selectedName = CB_Pokemon.SelectedItem?.ToString();

        if (selectedName == "-- All Pokemon --")
        {
            // Show all form pokemon
            int totalForms = 0;
            int ownedForms = 0;

            foreach (var kvp in FormPokemon.OrderBy(x => x.Value.name))
            {
                int species = kvp.Key;
                var (name, formCount, formNames) = kvp.Value;

                totalForms += formCount;
                int owned = _ownedForms.ContainsKey(species) ? _ownedForms[species].Count(f => f < formCount) : 0;
                ownedForms += owned;

                var panel = CreatePokemonSummaryPanel(species, name, formCount, formNames);
                FLP_Forms.Controls.Add(panel);
            }

            L_TotalForms.Text = $"Total Forms: {totalForms}";
            L_OwnedForms.Text = $"Owned: {ownedForms}";
            L_MissingForms.Text = $"Missing: {totalForms - ownedForms}";
            PB_Forms.Maximum = Math.Max(1, totalForms);
            PB_Forms.Value = ownedForms;
        }
        else
        {
            // Show specific pokemon forms
            var pokemonEntry = FormPokemon.FirstOrDefault(x => x.Value.name == selectedName);
            if (pokemonEntry.Key > 0)
            {
                int species = pokemonEntry.Key;
                var (name, formCount, formNames) = pokemonEntry.Value;

                int ownedCount = 0;

                for (int form = 0; form < formCount; form++)
                {
                    bool owned = _ownedForms.ContainsKey(species) && _ownedForms[species].Contains((byte)form);
                    if (owned) ownedCount++;

                    string formName = form < formNames.Length ? formNames[form] : $"Form {form}";
                    var panel = CreateFormPanel((ushort)species, (byte)form, formName, owned);
                    FLP_Forms.Controls.Add(panel);
                }

                L_TotalForms.Text = $"Total Forms: {formCount}";
                L_OwnedForms.Text = $"Owned: {ownedCount}";
                L_MissingForms.Text = $"Missing: {formCount - ownedCount}";
                PB_Forms.Maximum = Math.Max(1, formCount);
                PB_Forms.Value = ownedCount;
            }
        }

        FLP_Forms.ResumeLayout();
    }

    private Panel CreatePokemonSummaryPanel(int species, string name, int formCount, string[] formNames)
    {
        int owned = _ownedForms.ContainsKey(species) ? _ownedForms[species].Count(f => f < formCount) : 0;
        bool complete = owned == formCount;

        var panel = new Panel
        {
            Size = new Size(620, 35),
            BackColor = complete ? Color.FromArgb(40, 80, 40) : Color.FromArgb(45, 45, 65),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(2)
        };

        var pb = new PictureBox
        {
            Location = new Point(5, 2),
            Size = new Size(30, 30),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        try
        {
            pb.Image = SpriteUtil.GetSprite((ushort)species, 0, 0, 0, 0, false, Shiny.Never, EntityContext.Gen9);
        }
        catch { }

        var lblName = new Label
        {
            Text = name,
            Location = new Point(40, 8),
            Size = new Size(120, 20),
            ForeColor = complete ? Color.Gold : Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        var progress = new ProgressBar
        {
            Location = new Point(170, 8),
            Size = new Size(300, 18),
            Maximum = formCount,
            Value = owned,
            Style = ProgressBarStyle.Continuous
        };

        var lblProgress = new Label
        {
            Text = $"{owned}/{formCount}",
            Location = new Point(480, 8),
            Size = new Size(60, 20),
            ForeColor = complete ? Color.Gold : Color.White,
            Font = new Font("Segoe UI", 9F)
        };

        var lblPercent = new Label
        {
            Text = $"{(formCount > 0 ? owned * 100 / formCount : 0)}%",
            Location = new Point(550, 8),
            Size = new Size(50, 20),
            ForeColor = complete ? Color.Gold : Color.LightGray,
            Font = new Font("Segoe UI", 9F)
        };

        panel.Controls.AddRange(new Control[] { pb, lblName, progress, lblProgress, lblPercent });
        return panel;
    }

    private Panel CreateFormPanel(ushort species, byte form, string formName, bool owned)
    {
        var panel = new Panel
        {
            Size = new Size(150, 100),
            BackColor = owned ? Color.FromArgb(40, 80, 40) : Color.FromArgb(60, 30, 30),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(3)
        };

        var pb = new PictureBox
        {
            Location = new Point(50, 5),
            Size = new Size(50, 50),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        try
        {
            pb.Image = SpriteUtil.GetSprite(species, form, 0, 0, 0, false, Shiny.Never, EntityContext.Gen9);
            if (!owned)
                pb.Image = MakeGrayscale(pb.Image);
        }
        catch { }

        var lblForm = new Label
        {
            Text = formName,
            Location = new Point(0, 60),
            Size = new Size(150, 20),
            ForeColor = owned ? Color.LightGreen : Color.Gray,
            Font = new Font("Segoe UI", 8F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblStatus = new Label
        {
            Text = owned ? "Owned" : "Missing",
            Location = new Point(0, 80),
            Size = new Size(150, 15),
            ForeColor = owned ? Color.Gold : Color.Salmon,
            Font = new Font("Segoe UI", 7F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        panel.Controls.AddRange(new Control[] { pb, lblForm, lblStatus });
        return panel;
    }

    private Image MakeGrayscale(Image original)
    {
        var bmp = new Bitmap(original.Width, original.Height);
        using (var g = Graphics.FromImage(bmp))
        {
            var colorMatrix = new System.Drawing.Imaging.ColorMatrix(new float[][]
            {
                new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                new float[] { 0, 0, 0, 0.5f, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            });

            var attributes = new System.Drawing.Imaging.ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
        }
        return bmp;
    }
}
