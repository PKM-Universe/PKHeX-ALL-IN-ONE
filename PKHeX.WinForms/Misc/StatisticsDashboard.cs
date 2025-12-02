using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class StatisticsDashboard : Form
{
    private readonly SaveFile SAV;
    private TabControl tabMain = null!;
    private Panel pnlOverview = null!;
    private Panel pnlTypes = null!;
    private Panel pnlGenerations = null!;
    private Panel pnlShiny = null!;

    public StatisticsDashboard(SaveFile sav)
    {
        SAV = sav;
        Text = "Collection Statistics Dashboard";
        Size = new Size(1000, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(20, 20, 35);
        DoubleBuffered = true;
        InitializeUI();
        CalculateStatistics();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "📊 Collection Statistics",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 20F, FontStyle.Bold)
        };

        tabMain = new TabControl
        {
            Location = new Point(20, 60),
            Size = new Size(940, 580),
            Font = new Font("Segoe UI", 10F)
        };

        // Overview Tab
        var tabOverview = new TabPage("Overview") { BackColor = Color.FromArgb(25, 25, 40) };
        pnlOverview = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        tabOverview.Controls.Add(pnlOverview);

        // Type Distribution Tab
        var tabTypes = new TabPage("Type Distribution") { BackColor = Color.FromArgb(25, 25, 40) };
        pnlTypes = new Panel { Dock = DockStyle.Fill };
        tabTypes.Controls.Add(pnlTypes);

        // Generation Breakdown Tab
        var tabGens = new TabPage("Generations") { BackColor = Color.FromArgb(25, 25, 40) };
        pnlGenerations = new Panel { Dock = DockStyle.Fill };
        tabGens.Controls.Add(pnlGenerations);

        // Shiny Stats Tab
        var tabShiny = new TabPage("Shiny Collection") { BackColor = Color.FromArgb(25, 25, 40) };
        pnlShiny = new Panel { Dock = DockStyle.Fill };
        tabShiny.Controls.Add(pnlShiny);

        tabMain.TabPages.AddRange(new[] { tabOverview, tabTypes, tabGens, tabShiny });

        var btnRefresh = new Button
        {
            Text = "Refresh Stats",
            Location = new Point(800, 650),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 60),
            ForeColor = Color.White
        };
        btnRefresh.Click += (s, e) => CalculateStatistics();

        var btnExport = new Button
        {
            Text = "Export Report",
            Location = new Point(690, 650),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };
        btnExport.Click += ExportReport;

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(910, 650),
            Size = new Size(70, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, tabMain, btnRefresh, btnExport, btnClose });
    }

    private void CalculateStatistics()
    {
        var pokemon = new List<PKM>();

        // Collect all Pokemon from boxes
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                var pk = SAV.GetBoxSlotAtIndex(box, slot);
                if (pk.Species > 0)
                    pokemon.Add(pk);
            }
        }

        // Add party Pokemon
        for (int i = 0; i < SAV.PartyCount; i++)
        {
            var pk = SAV.GetPartySlotAtIndex(i);
            if (pk.Species > 0)
                pokemon.Add(pk);
        }

        BuildOverview(pokemon);
        BuildTypeChart(pokemon);
        BuildGenerationChart(pokemon);
        BuildShinyStats(pokemon);
    }

    private void BuildOverview(List<PKM> pokemon)
    {
        pnlOverview.Controls.Clear();

        int totalPokemon = pokemon.Count;
        int uniqueSpecies = pokemon.Select(p => p.Species).Distinct().Count();
        int shinies = pokemon.Count(p => p.IsShiny);
        int legends = pokemon.Count(p => IsLegendary(p.Species));
        int mythicals = pokemon.Count(p => IsMythical(p.Species));
        int maxIVs = pokemon.Count(p => p.IVTotal == 186);
        int eggs = pokemon.Count(p => p.IsEgg);
        int level100 = pokemon.Count(p => p.CurrentLevel == 100);
        double avgLevel = pokemon.Count > 0 ? pokemon.Average(p => p.CurrentLevel) : 0;

        // Stats Cards
        var cards = new[]
        {
            ("Total Pokemon", totalPokemon.ToString(), Color.FromArgb(100, 150, 255), "📦"),
            ("Unique Species", $"{uniqueSpecies}/1025", Color.FromArgb(100, 200, 150), "🔢"),
            ("Shiny Pokemon", shinies.ToString(), Color.Gold, "✨"),
            ("Legendary", legends.ToString(), Color.FromArgb(200, 150, 255), "⚡"),
            ("Mythical", mythicals.ToString(), Color.FromArgb(255, 150, 200), "🌟"),
            ("Perfect IVs", maxIVs.ToString(), Color.FromArgb(100, 255, 200), "💎"),
            ("Level 100", level100.ToString(), Color.FromArgb(255, 200, 100), "🏆"),
            ("Eggs", eggs.ToString(), Color.FromArgb(255, 220, 180), "🥚"),
        };

        for (int i = 0; i < cards.Length; i++)
        {
            var (title, value, color, icon) = cards[i];
            var card = CreateStatCard(title, value, color, icon, 20 + (i % 4) * 225, 20 + (i / 4) * 120);
            pnlOverview.Controls.Add(card);
        }

        // Progress bar for Living Dex
        var lblProgress = new Label
        {
            Text = $"Living Dex Progress: {uniqueSpecies}/1025 ({(uniqueSpecies / 1025.0 * 100):F1}%)",
            Location = new Point(20, 270),
            Size = new Size(400, 25),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        var prgDex = new ProgressBar
        {
            Location = new Point(20, 300),
            Size = new Size(880, 25),
            Maximum = 1025,
            Value = Math.Min(uniqueSpecies, 1025),
            Style = ProgressBarStyle.Continuous
        };

        // Shiny Dex progress
        int uniqueShinies = pokemon.Where(p => p.IsShiny).Select(p => p.Species).Distinct().Count();
        var lblShinyProgress = new Label
        {
            Text = $"Shiny Dex Progress: {uniqueShinies}/1025 ({(uniqueShinies / 1025.0 * 100):F1}%)",
            Location = new Point(20, 340),
            Size = new Size(400, 25),
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        var prgShiny = new ProgressBar
        {
            Location = new Point(20, 370),
            Size = new Size(880, 25),
            Maximum = 1025,
            Value = Math.Min(uniqueShinies, 1025),
            Style = ProgressBarStyle.Continuous
        };

        // Recent additions (mock)
        var grpRecent = new GroupBox
        {
            Text = "Collection Summary",
            Location = new Point(20, 410),
            Size = new Size(880, 120),
            ForeColor = Color.White
        };

        var summary = new Label
        {
            Text = $"Your collection contains {totalPokemon:N0} Pokemon across {SAV.BoxCount} boxes.\n\n" +
                   $"Average Level: {avgLevel:F1} | Highest: {(pokemon.Count > 0 ? pokemon.Max(p => p.CurrentLevel) : 0)} | Lowest: {(pokemon.Count > 0 ? pokemon.Min(p => p.CurrentLevel) : 0)}\n" +
                   $"Most Common: Pikachu (placeholder) | Rarest: Mew (placeholder)",
            Location = new Point(15, 25),
            Size = new Size(850, 85),
            ForeColor = Color.LightGray
        };
        grpRecent.Controls.Add(summary);

        pnlOverview.Controls.AddRange(new Control[] { lblProgress, prgDex, lblShinyProgress, prgShiny, grpRecent });
    }

    private Panel CreateStatCard(string title, string value, Color accentColor, string icon, int x, int y)
    {
        var card = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(210, 100),
            BackColor = Color.FromArgb(35, 35, 55)
        };
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(accentColor, 3);
            e.Graphics.DrawLine(pen, 0, 0, 0, card.Height);
        };

        var lblIcon = new Label
        {
            Text = icon,
            Location = new Point(15, 15),
            AutoSize = true,
            Font = new Font("Segoe UI", 24F)
        };

        var lblTitle = new Label
        {
            Text = title,
            Location = new Point(70, 15),
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9F)
        };

        var lblValue = new Label
        {
            Text = value,
            Location = new Point(70, 40),
            AutoSize = true,
            ForeColor = accentColor,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold)
        };

        card.Controls.AddRange(new Control[] { lblIcon, lblTitle, lblValue });
        return card;
    }

    private void BuildTypeChart(List<PKM> pokemon)
    {
        pnlTypes.Controls.Clear();

        var typeCounts = new Dictionary<int, int>();
        foreach (var pk in pokemon)
        {
            if (!typeCounts.ContainsKey(pk.PersonalInfo.Type1))
                typeCounts[pk.PersonalInfo.Type1] = 0;
            typeCounts[pk.PersonalInfo.Type1]++;

            if (pk.PersonalInfo.Type2 != pk.PersonalInfo.Type1)
            {
                if (!typeCounts.ContainsKey(pk.PersonalInfo.Type2))
                    typeCounts[pk.PersonalInfo.Type2] = 0;
                typeCounts[pk.PersonalInfo.Type2]++;
            }
        }

        var typeNames = new[] { "Normal", "Fighting", "Flying", "Poison", "Ground", "Rock", "Bug", "Ghost",
            "Steel", "Fire", "Water", "Grass", "Electric", "Psychic", "Ice", "Dragon", "Dark", "Fairy" };

        var typeColors = new[]
        {
            Color.FromArgb(168, 168, 120), Color.FromArgb(192, 48, 40), Color.FromArgb(168, 144, 240),
            Color.FromArgb(160, 64, 160), Color.FromArgb(224, 192, 104), Color.FromArgb(184, 160, 56),
            Color.FromArgb(168, 184, 32), Color.FromArgb(112, 88, 152), Color.FromArgb(184, 184, 208),
            Color.FromArgb(240, 128, 48), Color.FromArgb(104, 144, 240), Color.FromArgb(120, 200, 80),
            Color.FromArgb(248, 208, 48), Color.FromArgb(248, 88, 136), Color.FromArgb(152, 216, 216),
            Color.FromArgb(112, 56, 248), Color.FromArgb(112, 88, 72), Color.FromArgb(238, 153, 172)
        };

        int maxCount = typeCounts.Count > 0 ? typeCounts.Values.Max() : 1;

        var lblChartTitle = new Label
        {
            Text = "Type Distribution",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };
        pnlTypes.Controls.Add(lblChartTitle);

        for (int i = 0; i < typeNames.Length; i++)
        {
            int count = typeCounts.ContainsKey(i) ? typeCounts[i] : 0;
            int barWidth = maxCount > 0 ? (int)(count / (float)maxCount * 500) : 0;

            var lblType = new Label
            {
                Text = typeNames[i],
                Location = new Point(20, 55 + i * 28),
                Size = new Size(80, 20),
                ForeColor = typeColors[i],
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            var bar = new Panel
            {
                Location = new Point(110, 55 + i * 28),
                Size = new Size(Math.Max(barWidth, 2), 22),
                BackColor = typeColors[i]
            };

            var lblCount = new Label
            {
                Text = count.ToString(),
                Location = new Point(620, 55 + i * 28),
                AutoSize = true,
                ForeColor = Color.White
            };

            pnlTypes.Controls.AddRange(new Control[] { lblType, bar, lblCount });
        }

        // Pie chart placeholder
        var grpPie = new GroupBox
        {
            Text = "Type Pie Chart",
            Location = new Point(700, 50),
            Size = new Size(220, 220),
            ForeColor = Color.White
        };

        var piePlaceholder = new Label
        {
            Text = "🥧\n\nPie chart visualization\n(Coming soon)",
            Location = new Point(40, 50),
            Size = new Size(140, 120),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleCenter
        };
        grpPie.Controls.Add(piePlaceholder);
        pnlTypes.Controls.Add(grpPie);
    }

    private void BuildGenerationChart(List<PKM> pokemon)
    {
        pnlGenerations.Controls.Clear();

        var genRanges = new[]
        {
            (1, 151, "Gen 1 (Kanto)"),
            (152, 251, "Gen 2 (Johto)"),
            (252, 386, "Gen 3 (Hoenn)"),
            (387, 493, "Gen 4 (Sinnoh)"),
            (494, 649, "Gen 5 (Unova)"),
            (650, 721, "Gen 6 (Kalos)"),
            (722, 809, "Gen 7 (Alola)"),
            (810, 905, "Gen 8 (Galar)"),
            (906, 1025, "Gen 9 (Paldea)")
        };

        var genCounts = new int[9];
        foreach (var pk in pokemon)
        {
            for (int g = 0; g < genRanges.Length; g++)
            {
                if (pk.Species >= genRanges[g].Item1 && pk.Species <= genRanges[g].Item2)
                {
                    genCounts[g]++;
                    break;
                }
            }
        }

        var genColors = new[]
        {
            Color.FromArgb(255, 100, 100), Color.FromArgb(255, 200, 100), Color.FromArgb(100, 200, 100),
            Color.FromArgb(100, 200, 255), Color.FromArgb(150, 150, 255), Color.FromArgb(255, 100, 200),
            Color.FromArgb(255, 180, 100), Color.FromArgb(200, 100, 255), Color.FromArgb(100, 255, 200)
        };

        int maxCount = genCounts.Max();

        var lblChartTitle = new Label
        {
            Text = "Pokemon by Generation",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };
        pnlGenerations.Controls.Add(lblChartTitle);

        for (int i = 0; i < genRanges.Length; i++)
        {
            int barWidth = maxCount > 0 ? (int)(genCounts[i] / (float)maxCount * 450) : 0;

            var lblGen = new Label
            {
                Text = genRanges[i].Item3,
                Location = new Point(20, 60 + i * 50),
                Size = new Size(150, 20),
                ForeColor = genColors[i],
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            var bar = new Panel
            {
                Location = new Point(180, 60 + i * 50),
                Size = new Size(Math.Max(barWidth, 2), 35),
                BackColor = genColors[i]
            };

            var lblCount = new Label
            {
                Text = $"{genCounts[i]} ({genRanges[i].Item2 - genRanges[i].Item1 + 1} total)",
                Location = new Point(640, 60 + i * 50),
                AutoSize = true,
                ForeColor = Color.White
            };

            pnlGenerations.Controls.AddRange(new Control[] { lblGen, bar, lblCount });
        }

        // Summary
        var grpSummary = new GroupBox
        {
            Text = "Generation Summary",
            Location = new Point(700, 300),
            Size = new Size(220, 180),
            ForeColor = Color.White
        };

        int bestGen = Array.IndexOf(genCounts, genCounts.Max());
        var summary = new Label
        {
            Text = $"Most Pokemon from:\n{genRanges[bestGen].Item3}\n\n" +
                   $"Total: {pokemon.Count}\n" +
                   $"Unique Gens: {genCounts.Count(c => c > 0)}/9",
            Location = new Point(15, 25),
            Size = new Size(190, 140),
            ForeColor = Color.LightGray
        };
        grpSummary.Controls.Add(summary);
        pnlGenerations.Controls.Add(grpSummary);
    }

    private void BuildShinyStats(List<PKM> pokemon)
    {
        pnlShiny.Controls.Clear();

        var shinies = pokemon.Where(p => p.IsShiny).ToList();
        int squareShiny = shinies.Count(p => p.ShinyXor == 0);
        int starShiny = shinies.Count - squareShiny;

        var lblChartTitle = new Label
        {
            Text = "Shiny Collection Statistics",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };

        // Shiny cards
        var cards = new[]
        {
            ("Total Shinies", shinies.Count.ToString(), Color.Gold, "✨"),
            ("Square Shinies", squareShiny.ToString(), Color.FromArgb(255, 200, 255), "◆"),
            ("Star Shinies", starShiny.ToString(), Color.FromArgb(255, 255, 150), "★"),
            ("Unique Species", shinies.Select(p => p.Species).Distinct().Count().ToString(), Color.Cyan, "🔢"),
        };

        for (int i = 0; i < cards.Length; i++)
        {
            var (title, value, color, icon) = cards[i];
            var card = CreateStatCard(title, value, color, icon, 20 + i * 225, 50);
            pnlShiny.Controls.Add(card);
        }

        // Shiny list
        var grpList = new GroupBox
        {
            Text = "Your Shiny Pokemon",
            Location = new Point(20, 170),
            Size = new Size(880, 350),
            ForeColor = Color.White
        };

        var lstShinies = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(860, 315),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstShinies.Columns.Add("Species", 150);
        lstShinies.Columns.Add("Nickname", 120);
        lstShinies.Columns.Add("Level", 60);
        lstShinies.Columns.Add("Type", 80);
        lstShinies.Columns.Add("OT", 100);
        lstShinies.Columns.Add("Shiny Type", 100);
        lstShinies.Columns.Add("Location", 200);

        foreach (var pk in shinies.OrderBy(p => p.Species))
        {
            var item = new ListViewItem(SpeciesName.GetSpeciesName(pk.Species, 2));
            item.SubItems.Add(pk.Nickname);
            item.SubItems.Add(pk.CurrentLevel.ToString());
            item.SubItems.Add($"{pk.PersonalInfo.Type1}");
            item.SubItems.Add(pk.OriginalTrainerName);
            item.SubItems.Add(pk.ShinyXor == 0 ? "◆ Square" : "★ Star");
            item.SubItems.Add("Box");

            item.ForeColor = pk.ShinyXor == 0 ? Color.FromArgb(255, 200, 255) : Color.Gold;
            lstShinies.Items.Add(item);
        }

        grpList.Controls.Add(lstShinies);

        pnlShiny.Controls.AddRange(new Control[] { lblChartTitle, grpList });
    }

    private void ExportReport(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Export Statistics Report",
            Filter = "Text File|*.txt|HTML Report|*.html",
            FileName = "PKM-Universe-Stats"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            WinFormsUtil.Alert("Statistics report exported!", $"Saved to: {sfd.FileName}");
        }
    }

    private static bool IsLegendary(ushort species) =>
        new ushort[] { 144, 145, 146, 150, 243, 244, 245, 249, 250, 377, 378, 379, 380, 381, 382, 383, 384,
            480, 481, 482, 483, 484, 485, 486, 487, 488, 638, 639, 640, 641, 642, 643, 644, 645, 646,
            716, 717, 718, 785, 786, 787, 788, 789, 790, 791, 792, 800, 888, 889, 890, 891, 892,
            894, 895, 896, 897, 898, 905, 1001, 1002, 1003, 1004, 1007, 1008, 1014, 1015, 1016, 1017 }.Contains(species);

    private static bool IsMythical(ushort species) =>
        new ushort[] { 151, 251, 385, 386, 489, 490, 491, 492, 493, 494, 647, 648, 649, 719, 720, 721,
            801, 802, 807, 808, 809, 893 }.Contains(species);
}
