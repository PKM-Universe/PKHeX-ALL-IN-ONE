using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;

namespace PKHeX.WinForms;

public class LivingDexTracker : Form
{
    private readonly SaveFile SAV;
    private readonly HashSet<int> _ownedSpecies = new();
    private readonly HashSet<int> _ownedShiny = new();
    private readonly Dictionary<int, HashSet<byte>> _ownedForms = new();

    private readonly FlowLayoutPanel FLP_Pokemon;
    private readonly Panel PNL_Stats;
    private readonly Label L_Total;
    private readonly Label L_Owned;
    private readonly Label L_Missing;
    private readonly Label L_ShinyOwned;
    private readonly ProgressBar PB_Total;
    private readonly ComboBox CB_Generation;
    private readonly CheckBox CHK_ShinyMode;
    private readonly Button BTN_Export;

    private bool _shinyMode;
    private int _selectedGen;

    public LivingDexTracker(SaveFile sav)
    {
        SAV = sav;
        Text = "Living Dex Tracker";
        Size = new Size(1000, 750);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        // Title
        var lblTitle = new Label
        {
            Text = "Living Dex Tracker",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Generation filter
        var lblGen = new Label
        {
            Text = "Generation:",
            Location = new Point(300, 20),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };

        CB_Generation = new ComboBox
        {
            Location = new Point(390, 17),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 60),
            ForeColor = Color.White
        };
        CB_Generation.Items.AddRange(new object[] { "All Generations", "Gen 1 (1-151)", "Gen 2 (152-251)",
            "Gen 3 (252-386)", "Gen 4 (387-493)", "Gen 5 (494-649)", "Gen 6 (650-721)",
            "Gen 7 (722-809)", "Gen 8 (810-905)", "Gen 9 (906-1025)" });
        CB_Generation.SelectedIndex = 0;
        CB_Generation.SelectedIndexChanged += (s, e) => { _selectedGen = CB_Generation.SelectedIndex; RefreshDisplay(); };

        // Shiny mode toggle
        CHK_ShinyMode = new CheckBox
        {
            Text = "Shiny Living Dex",
            Location = new Point(560, 18),
            AutoSize = true,
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        CHK_ShinyMode.CheckedChanged += (s, e) => { _shinyMode = CHK_ShinyMode.Checked; RefreshDisplay(); };

        // Export button
        BTN_Export = new Button
        {
            Text = "Export Missing List",
            Location = new Point(850, 15),
            Size = new Size(130, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 60),
            ForeColor = Color.White
        };
        BTN_Export.Click += (s, e) => ExportMissingList();

        // Stats panel
        PNL_Stats = new Panel
        {
            Location = new Point(20, 55),
            Size = new Size(960, 80),
            BackColor = Color.FromArgb(35, 35, 55),
            BorderStyle = BorderStyle.FixedSingle
        };

        L_Total = new Label
        {
            Text = "Total Pokemon: 0",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11F)
        };

        L_Owned = new Label
        {
            Text = "Owned: 0",
            Location = new Point(200, 10),
            AutoSize = true,
            ForeColor = Color.LightGreen,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        L_Missing = new Label
        {
            Text = "Missing: 0",
            Location = new Point(350, 10),
            AutoSize = true,
            ForeColor = Color.Salmon,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        L_ShinyOwned = new Label
        {
            Text = "Shiny: 0",
            Location = new Point(500, 10),
            AutoSize = true,
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        PB_Total = new ProgressBar
        {
            Location = new Point(20, 45),
            Size = new Size(920, 25),
            Style = ProgressBarStyle.Continuous
        };

        PNL_Stats.Controls.AddRange(new Control[] { L_Total, L_Owned, L_Missing, L_ShinyOwned, PB_Total });

        // Pokemon grid
        FLP_Pokemon = new FlowLayoutPanel
        {
            Location = new Point(20, 145),
            Size = new Size(960, 550),
            AutoScroll = true,
            BackColor = Color.FromArgb(30, 30, 50),
            BorderStyle = BorderStyle.FixedSingle
        };

        // Generation progress panels
        var pnlGenProgress = CreateGenerationProgressPanel();
        pnlGenProgress.Location = new Point(650, 10);
        PNL_Stats.Controls.Add(pnlGenProgress);

        Controls.AddRange(new Control[] { lblTitle, lblGen, CB_Generation, CHK_ShinyMode, BTN_Export, PNL_Stats, FLP_Pokemon });

        ScanSaveFile();
        RefreshDisplay();
    }

    private Panel CreateGenerationProgressPanel()
    {
        var panel = new Panel
        {
            Size = new Size(300, 60),
            BackColor = Color.Transparent
        };

        var genRanges = new[] { (1, 151), (152, 251), (252, 386), (387, 493), (494, 649), (650, 721), (722, 809), (810, 905), (906, 1025) };

        for (int i = 0; i < 9; i++)
        {
            var (start, end) = genRanges[i];
            int total = end - start + 1;
            int owned = _ownedSpecies.Count(s => s >= start && s <= end);

            var lbl = new Label
            {
                Text = $"G{i + 1}",
                Location = new Point(i * 33, 0),
                Size = new Size(30, 15),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 7F),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var pb = new ProgressBar
            {
                Location = new Point(i * 33, 18),
                Size = new Size(28, 10),
                Maximum = total,
                Value = Math.Min(owned, total),
                Style = ProgressBarStyle.Continuous
            };

            var pct = new Label
            {
                Text = $"{(total > 0 ? owned * 100 / total : 0)}%",
                Location = new Point(i * 33, 30),
                Size = new Size(30, 12),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 6F),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.AddRange(new Control[] { lbl, pb, pct });
        }

        return panel;
    }

    private void ScanSaveFile()
    {
        _ownedSpecies.Clear();
        _ownedShiny.Clear();
        _ownedForms.Clear();

        // Scan all boxes
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                var pk = SAV.GetBoxSlotAtIndex(box, slot);
                if (pk.Species == 0) continue;

                _ownedSpecies.Add(pk.Species);

                if (pk.IsShiny)
                    _ownedShiny.Add(pk.Species);

                if (!_ownedForms.ContainsKey(pk.Species))
                    _ownedForms[pk.Species] = new HashSet<byte>();
                _ownedForms[pk.Species].Add(pk.Form);
            }
        }

        // Scan party
        if (SAV.HasParty)
        {
            for (int i = 0; i < 6; i++)
            {
                var pk = SAV.GetPartySlotAtIndex(i);
                if (pk.Species == 0) continue;

                _ownedSpecies.Add(pk.Species);

                if (pk.IsShiny)
                    _ownedShiny.Add(pk.Species);

                if (!_ownedForms.ContainsKey(pk.Species))
                    _ownedForms[pk.Species] = new HashSet<byte>();
                _ownedForms[pk.Species].Add(pk.Form);
            }
        }
    }

    private void RefreshDisplay()
    {
        FLP_Pokemon.SuspendLayout();
        FLP_Pokemon.Controls.Clear();

        var (minSpecies, maxSpecies) = GetGenerationRange(_selectedGen);
        var speciesList = GameInfo.Strings.specieslist;

        int total = 0;
        int owned = 0;

        for (int species = minSpecies; species <= maxSpecies; species++)
        {
            if (species >= speciesList.Length || string.IsNullOrEmpty(speciesList[species]))
                continue;

            total++;
            bool hasIt = _shinyMode ? _ownedShiny.Contains(species) : _ownedSpecies.Contains(species);
            if (hasIt) owned++;

            var panel = CreatePokemonPanel((ushort)species, hasIt);
            FLP_Pokemon.Controls.Add(panel);
        }

        FLP_Pokemon.ResumeLayout();

        // Update stats
        L_Total.Text = $"Total Pokemon: {total}";
        L_Owned.Text = $"Owned: {owned}";
        L_Missing.Text = $"Missing: {total - owned}";
        L_ShinyOwned.Text = $"Shiny: {_ownedShiny.Count(s => s >= minSpecies && s <= maxSpecies)}";

        PB_Total.Maximum = Math.Max(1, total);
        PB_Total.Value = owned;
    }

    private (int min, int max) GetGenerationRange(int gen)
    {
        return gen switch
        {
            1 => (1, 151),
            2 => (152, 251),
            3 => (252, 386),
            4 => (387, 493),
            5 => (494, 649),
            6 => (650, 721),
            7 => (722, 809),
            8 => (810, 905),
            9 => (906, 1025),
            _ => (1, 1025)
        };
    }

    private Panel CreatePokemonPanel(ushort species, bool owned)
    {
        var panel = new Panel
        {
            Size = new Size(70, 85),
            BackColor = owned ? Color.FromArgb(40, 80, 40) : Color.FromArgb(60, 30, 30),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(2)
        };

        var pb = new PictureBox
        {
            Location = new Point(10, 5),
            Size = new Size(50, 50),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        try
        {
            var shiny = _shinyMode ? Shiny.AlwaysStar : Shiny.Never;
            pb.Image = SpriteUtil.GetSprite(species, 0, 0, 0, 0, false, shiny, EntityContext.Gen9);

            if (!owned)
            {
                // Grayscale the image for missing Pokemon
                pb.Image = MakeGrayscale(pb.Image);
            }
        }
        catch { }

        var lbl = new Label
        {
            Text = $"#{species}",
            Location = new Point(0, 58),
            Size = new Size(70, 12),
            ForeColor = owned ? Color.White : Color.Gray,
            Font = new Font("Segoe UI", 7F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblName = new Label
        {
            Text = TruncateName(GameInfo.Strings.specieslist[species], 10),
            Location = new Point(0, 70),
            Size = new Size(70, 12),
            ForeColor = owned ? Color.LightGreen : Color.DarkGray,
            Font = new Font("Segoe UI", 6F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Tooltip with full name
        var tt = new ToolTip();
        tt.SetToolTip(panel, $"{GameInfo.Strings.specieslist[species]} - {(owned ? "Owned" : "Missing")}");
        tt.SetToolTip(pb, $"{GameInfo.Strings.specieslist[species]} - {(owned ? "Owned" : "Missing")}");

        panel.Controls.AddRange(new Control[] { pb, lbl, lblName });
        return panel;
    }

    private string TruncateName(string name, int maxLength)
    {
        if (name.Length <= maxLength) return name;
        return name.Substring(0, maxLength - 2) + "..";
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

    private void ExportMissingList()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "Text File|*.txt",
            FileName = _shinyMode ? "MissingShinyLivingDex.txt" : "MissingLivingDex.txt"
        };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        var (minSpecies, maxSpecies) = GetGenerationRange(_selectedGen);
        var speciesList = GameInfo.Strings.specieslist;
        var missing = new List<string>();

        for (int species = minSpecies; species <= maxSpecies; species++)
        {
            if (species >= speciesList.Length || string.IsNullOrEmpty(speciesList[species]))
                continue;

            bool hasIt = _shinyMode ? _ownedShiny.Contains(species) : _ownedSpecies.Contains(species);
            if (!hasIt)
                missing.Add($"#{species} - {speciesList[species]}");
        }

        System.IO.File.WriteAllLines(sfd.FileName, missing);
        WinFormsUtil.Alert($"Exported {missing.Count} missing Pokemon to file!");
    }
}
