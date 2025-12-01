using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;

namespace PKHeX.WinForms;

public class PokedexCompletionTool : Form
{
    private readonly SaveFile SAV;
    private readonly HashSet<int> _ownedSpecies = new();

    private readonly ComboBox CB_Game;
    private readonly Panel PNL_Stats;
    private readonly FlowLayoutPanel FLP_Pokemon;
    private readonly Label L_Completion;
    private readonly ProgressBar PB_Completion;
    private readonly Label L_Owned;
    private readonly Label L_Missing;
    private readonly CheckBox CHK_ShowOwned;
    private readonly CheckBox CHK_ShowMissing;
    private readonly Label L_VersionExclusive;

    // Regional dex data for each game
    private static readonly Dictionary<string, (int[] pokemon, int[] versionExclusives)> RegionalDexes = new()
    {
        ["Scarlet/Violet"] = (GetPaldeaDex(), new[] { 316, 317, 318, 371, 372, 373, 574, 575, 576, 704, 705, 706, 984, 985, 986, 987, 988, 989, 990, 991, 992, 993, 994, 995 }),
        ["Sword/Shield"] = (GetGalarDex(), new[] { 554, 555, 559, 560, 574, 575, 576, 627, 628, 766, 782, 783, 784, 550, 222, 864 }),
        ["Brilliant Diamond"] = (GetSinnohDex(), new[] { 239, 240, 198, 430, 434, 435, 483 }),
        ["Shining Pearl"] = (GetSinnohDex(), new[] { 238, 126, 200, 429, 431, 432, 484 }),
        ["Legends: Arceus"] = (GetHisuiDex(), Array.Empty<int>()),
        ["Let's Go Pikachu"] = (GetKantoDex(), new[] { 27, 28, 37, 38, 52, 53, 56, 57, 88, 89, 43, 44, 45 }),
        ["Let's Go Eevee"] = (GetKantoDex(), new[] { 23, 24, 58, 59, 90, 91, 109, 110, 69, 70, 71 }),
        ["Ultra Sun/Moon"] = (GetAlolaDex(), new[] { 37, 38, 228, 229, 546, 547, 627, 628, 766, 776, 791 }),
        ["Sun/Moon"] = (GetAlolaDex(), new[] { 27, 28, 37, 38, 228, 229, 546, 547, 627, 628, 766, 776, 789, 790, 791, 792 }),
        ["Omega Ruby/Alpha Sapphire"] = (GetHoennDex(), new[] { 303, 306, 335, 338, 381, 383 }),
        ["X/Y"] = (GetKalosDex(), Array.Empty<int>()),
    };

    public PokedexCompletionTool(SaveFile sav)
    {
        SAV = sav;
        Text = "Pokedex Completion by Game";
        Size = new Size(1000, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblTitle = new Label
        {
            Text = "Pokedex Completion Tracker",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        var lblGame = new Label
        {
            Text = "Select Game:",
            Location = new Point(350, 20),
            AutoSize = true,
            ForeColor = Color.White
        };

        CB_Game = new ComboBox
        {
            Location = new Point(440, 17),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 60),
            ForeColor = Color.White
        };
        foreach (var game in RegionalDexes.Keys)
            CB_Game.Items.Add(game);
        CB_Game.SelectedIndex = 0;
        CB_Game.SelectedIndexChanged += (s, e) => RefreshDisplay();

        CHK_ShowOwned = new CheckBox
        {
            Text = "Show Owned",
            Location = new Point(660, 18),
            AutoSize = true,
            ForeColor = Color.LightGreen,
            Checked = true
        };
        CHK_ShowOwned.CheckedChanged += (s, e) => RefreshDisplay();

        CHK_ShowMissing = new CheckBox
        {
            Text = "Show Missing",
            Location = new Point(780, 18),
            AutoSize = true,
            ForeColor = Color.Salmon,
            Checked = true
        };
        CHK_ShowMissing.CheckedChanged += (s, e) => RefreshDisplay();

        // Stats panel
        PNL_Stats = new Panel
        {
            Location = new Point(20, 55),
            Size = new Size(960, 70),
            BackColor = Color.FromArgb(35, 35, 55),
            BorderStyle = BorderStyle.FixedSingle
        };

        L_Completion = new Label
        {
            Text = "Completion: 0%",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };

        L_Owned = new Label
        {
            Text = "Owned: 0",
            Location = new Point(250, 15),
            AutoSize = true,
            ForeColor = Color.LightGreen,
            Font = new Font("Segoe UI", 11F)
        };

        L_Missing = new Label
        {
            Text = "Missing: 0",
            Location = new Point(380, 15),
            AutoSize = true,
            ForeColor = Color.Salmon,
            Font = new Font("Segoe UI", 11F)
        };

        L_VersionExclusive = new Label
        {
            Text = "Version Exclusives Missing: 0",
            Location = new Point(520, 15),
            AutoSize = true,
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 10F)
        };

        PB_Completion = new ProgressBar
        {
            Location = new Point(20, 42),
            Size = new Size(920, 20),
            Style = ProgressBarStyle.Continuous
        };

        PNL_Stats.Controls.AddRange(new Control[] { L_Completion, L_Owned, L_Missing, L_VersionExclusive, PB_Completion });

        FLP_Pokemon = new FlowLayoutPanel
        {
            Location = new Point(20, 135),
            Size = new Size(960, 510),
            AutoScroll = true,
            BackColor = Color.FromArgb(30, 30, 50),
            BorderStyle = BorderStyle.FixedSingle
        };

        Controls.AddRange(new Control[] { lblTitle, lblGame, CB_Game, CHK_ShowOwned, CHK_ShowMissing, PNL_Stats, FLP_Pokemon });

        ScanSaveFile();
        RefreshDisplay();
    }

    private void ScanSaveFile()
    {
        _ownedSpecies.Clear();

        for (int box = 0; box < SAV.BoxCount; box++)
        {
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                var pk = SAV.GetBoxSlotAtIndex(box, slot);
                if (pk.Species > 0)
                    _ownedSpecies.Add(pk.Species);
            }
        }

        if (SAV.HasParty)
        {
            for (int i = 0; i < 6; i++)
            {
                var pk = SAV.GetPartySlotAtIndex(i);
                if (pk.Species > 0)
                    _ownedSpecies.Add(pk.Species);
            }
        }
    }

    private void RefreshDisplay()
    {
        FLP_Pokemon.SuspendLayout();
        FLP_Pokemon.Controls.Clear();

        var selectedGame = CB_Game.SelectedItem?.ToString() ?? "Scarlet/Violet";
        if (!RegionalDexes.TryGetValue(selectedGame, out var dexData))
            return;

        var (pokemon, versionExclusives) = dexData;
        var speciesList = GameInfo.Strings.specieslist;

        int total = pokemon.Length;
        int owned = 0;
        int versionExclusiveMissing = 0;

        foreach (int species in pokemon)
        {
            if (species <= 0 || species >= speciesList.Length)
                continue;

            bool hasIt = _ownedSpecies.Contains(species);
            if (hasIt) owned++;

            bool isVersionExclusive = versionExclusives.Contains(species);
            if (isVersionExclusive && !hasIt)
                versionExclusiveMissing++;

            if ((hasIt && CHK_ShowOwned.Checked) || (!hasIt && CHK_ShowMissing.Checked))
            {
                var panel = CreatePokemonPanel((ushort)species, hasIt, isVersionExclusive);
                FLP_Pokemon.Controls.Add(panel);
            }
        }

        FLP_Pokemon.ResumeLayout();

        int missing = total - owned;
        int percent = total > 0 ? owned * 100 / total : 0;

        L_Completion.Text = $"Completion: {percent}%";
        L_Completion.ForeColor = percent >= 100 ? Color.Gold : (percent >= 75 ? Color.LightGreen : Color.White);
        L_Owned.Text = $"Owned: {owned}/{total}";
        L_Missing.Text = $"Missing: {missing}";
        L_VersionExclusive.Text = $"Version Exclusives Missing: {versionExclusiveMissing}";

        PB_Completion.Maximum = Math.Max(1, total);
        PB_Completion.Value = owned;
    }

    private Panel CreatePokemonPanel(ushort species, bool owned, bool isVersionExclusive)
    {
        Color bgColor;
        if (owned)
            bgColor = Color.FromArgb(40, 80, 40);
        else if (isVersionExclusive)
            bgColor = Color.FromArgb(100, 80, 30); // Gold tint for version exclusives
        else
            bgColor = Color.FromArgb(60, 30, 30);

        var panel = new Panel
        {
            Size = new Size(75, 90),
            BackColor = bgColor,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(2)
        };

        var pb = new PictureBox
        {
            Location = new Point(12, 5),
            Size = new Size(50, 50),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        try
        {
            pb.Image = SpriteUtil.GetSprite(species, 0, 0, 0, 0, false, Shiny.Never, EntityContext.Gen9);
            if (!owned)
                pb.Image = MakeGrayscale(pb.Image);
        }
        catch { }

        var lbl = new Label
        {
            Text = $"#{species}",
            Location = new Point(0, 58),
            Size = new Size(75, 12),
            ForeColor = owned ? Color.White : Color.Gray,
            Font = new Font("Segoe UI", 7F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblName = new Label
        {
            Text = TruncateName(GameInfo.Strings.specieslist[species], 10),
            Location = new Point(0, 72),
            Size = new Size(75, 12),
            ForeColor = isVersionExclusive ? Color.Gold : (owned ? Color.LightGreen : Color.DarkGray),
            Font = new Font("Segoe UI", 6F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var tt = new ToolTip();
        string status = owned ? "Owned" : "Missing";
        string exclusive = isVersionExclusive ? " (Version Exclusive)" : "";
        tt.SetToolTip(panel, $"{GameInfo.Strings.specieslist[species]} - {status}{exclusive}");
        tt.SetToolTip(pb, $"{GameInfo.Strings.specieslist[species]} - {status}{exclusive}");

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

    // Regional dex helper methods
    private static int[] GetPaldeaDex() => Enumerable.Range(1, 400).Select(i => GetPaldeaSpecies(i)).Where(s => s > 0).ToArray();
    private static int[] GetGalarDex() => Enumerable.Range(1, 400).ToArray();
    private static int[] GetSinnohDex() => Enumerable.Range(1, 493).ToArray();
    private static int[] GetHisuiDex() => Enumerable.Range(1, 242).Select(i => GetHisuiSpecies(i)).Where(s => s > 0).ToArray();
    private static int[] GetKantoDex() => Enumerable.Range(1, 151).ToArray();
    private static int[] GetAlolaDex() => Enumerable.Range(1, 403).ToArray();
    private static int[] GetHoennDex() => Enumerable.Range(1, 386).ToArray();
    private static int[] GetKalosDex() => Enumerable.Range(1, 721).ToArray();

    private static int GetPaldeaSpecies(int dexNum)
    {
        // Simplified Paldea dex mapping - returns species number
        // In reality this would be a proper mapping table
        if (dexNum <= 0) return 0;
        return dexNum; // Placeholder - would need full mapping
    }

    private static int GetHisuiSpecies(int dexNum)
    {
        // Simplified Hisui dex mapping
        if (dexNum <= 0) return 0;
        return dexNum; // Placeholder - would need full mapping
    }
}
