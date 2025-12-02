using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Visual Type Coverage Chart - Displays team coverage with colored type grid
/// </summary>
public class VisualCoverageChart : Form
{
    private readonly SaveFile SAV;
    private readonly List<PKM> Team;
    private Panel chartPanel = null!;
    private Label lblSummary = null!;

    // Type colors (official Pokemon type colors)
    private static readonly Dictionary<PokemonType, Color> TypeColors = new()
    {
        [PokemonType.Normal] = Color.FromArgb(168, 168, 120),
        [PokemonType.Fire] = Color.FromArgb(240, 128, 48),
        [PokemonType.Water] = Color.FromArgb(104, 144, 240),
        [PokemonType.Electric] = Color.FromArgb(248, 208, 48),
        [PokemonType.Grass] = Color.FromArgb(120, 200, 80),
        [PokemonType.Ice] = Color.FromArgb(152, 216, 216),
        [PokemonType.Fighting] = Color.FromArgb(192, 48, 40),
        [PokemonType.Poison] = Color.FromArgb(160, 64, 160),
        [PokemonType.Ground] = Color.FromArgb(224, 192, 104),
        [PokemonType.Flying] = Color.FromArgb(168, 144, 240),
        [PokemonType.Psychic] = Color.FromArgb(248, 88, 136),
        [PokemonType.Bug] = Color.FromArgb(168, 184, 32),
        [PokemonType.Rock] = Color.FromArgb(184, 160, 56),
        [PokemonType.Ghost] = Color.FromArgb(112, 88, 152),
        [PokemonType.Dragon] = Color.FromArgb(112, 56, 248),
        [PokemonType.Dark] = Color.FromArgb(112, 88, 72),
        [PokemonType.Steel] = Color.FromArgb(184, 184, 208),
        [PokemonType.Fairy] = Color.FromArgb(238, 153, 172)
    };

    public VisualCoverageChart(SaveFile sav, List<PKM>? team = null)
    {
        SAV = sav;
        Team = team ?? sav.PartyData.Where(p => p.Species > 0).ToList();

        InitializeComponents();
        DrawChart();
    }

    private void InitializeComponents()
    {
        Text = "Team Type Coverage Chart";
        Size = new Size(900, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 30, 35);
        FormBorderStyle = FormBorderStyle.Sizable;

        // Title
        var lblTitle = new Label
        {
            Text = "TYPE COVERAGE CHART",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 15)
        };

        // Summary label
        lblSummary = new Label
        {
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.LightGray,
            AutoSize = true,
            Location = new Point(20, 50)
        };

        // Chart panel
        chartPanel = new Panel
        {
            Location = new Point(20, 90),
            Size = new Size(850, 500),
            BackColor = Color.FromArgb(45, 45, 50),
            BorderStyle = BorderStyle.FixedSingle
        };
        chartPanel.Paint += ChartPanel_Paint;

        // Legend panel
        var legendPanel = CreateLegendPanel();
        legendPanel.Location = new Point(20, 600);

        Controls.AddRange(new Control[] { lblTitle, lblSummary, chartPanel, legendPanel });
    }

    private Panel CreateLegendPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Size = new Size(850, 40),
            BackColor = Color.Transparent
        };

        var items = new[]
        {
            ("Super Effective (2x+)", Color.FromArgb(76, 175, 80)),
            ("Neutral (1x)", Color.FromArgb(158, 158, 158)),
            ("Resisted (0.5x)", Color.FromArgb(255, 152, 0)),
            ("Immune (0x)", Color.FromArgb(244, 67, 54))
        };

        foreach (var (text, color) in items)
        {
            var box = new Panel
            {
                Size = new Size(15, 15),
                BackColor = color,
                Margin = new Padding(5, 3, 3, 3)
            };
            var lbl = new Label
            {
                Text = text,
                ForeColor = Color.LightGray,
                AutoSize = true,
                Margin = new Padding(0, 3, 15, 3)
            };
            panel.Controls.Add(box);
            panel.Controls.Add(lbl);
        }

        return panel;
    }

    private void DrawChart()
    {
        var analyzer = new TeamAnalyzer(SAV);
        var analysis = analyzer.AnalyzeTeam(Team);

        // Update summary
        var weakCount = analysis.CriticalWeaknesses.Count;
        var gapCount = analysis.CoverageGaps.Count;
        lblSummary.Text = $"Team: {Team.Count} Pokemon | Critical Weaknesses: {weakCount} | Coverage Gaps: {gapCount}";

        if (weakCount == 0 && gapCount == 0)
            lblSummary.ForeColor = Color.LightGreen;
        else if (weakCount > 2 || gapCount > 3)
            lblSummary.ForeColor = Color.Salmon;
        else
            lblSummary.ForeColor = Color.Orange;

        chartPanel.Invalidate();
    }

    private void ChartPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var types = Enum.GetValues(typeof(PokemonType)).Cast<PokemonType>()
            .Where(t => t != PokemonType.None).ToList();

        int headerHeight = 30;
        int rowHeight = 25;
        int colWidth = 45;
        int leftMargin = 100;
        int topMargin = 40;

        // Draw header (attacking types)
        g.DrawString("Defending Type", new Font("Segoe UI", 8, FontStyle.Bold), Brushes.White, 5, 10);

        for (int i = 0; i < types.Count; i++)
        {
            var type = types[i];
            var x = leftMargin + (i * colWidth);
            var typeColor = TypeColors.GetValueOrDefault(type, Color.Gray);

            // Draw type header
            using var brush = new SolidBrush(typeColor);
            g.FillRectangle(brush, x, topMargin, colWidth - 2, headerHeight - 2);

            // Draw type abbreviation
            var abbrev = GetTypeAbbreviation(type);
            using var textBrush = new SolidBrush(GetContrastColor(typeColor));
            var textSize = g.MeasureString(abbrev, Font);
            g.DrawString(abbrev, Font, textBrush, x + (colWidth - textSize.Width) / 2, topMargin + 7);
        }

        // Draw team Pokemon rows
        for (int pkIndex = 0; pkIndex < Team.Count; pkIndex++)
        {
            var pk = Team[pkIndex];
            var y = topMargin + headerHeight + (pkIndex * rowHeight);
            var pkTypes = GetPokemonTypes(pk);

            // Draw Pokemon name
            var name = pk.Nickname ?? SpeciesName.GetSpeciesName(pk.Species, 2);
            if (name.Length > 12) name = name.Substring(0, 10) + "..";
            g.DrawString(name, Font, Brushes.White, 5, y + 5);

            // Draw effectiveness for each attacking type
            for (int typeIndex = 0; typeIndex < types.Count; typeIndex++)
            {
                var attackType = types[typeIndex];
                var x = leftMargin + (typeIndex * colWidth);

                var effectiveness = GetTypeEffectiveness(attackType, pkTypes.Type1, pkTypes.Type2);
                var cellColor = GetEffectivenessColor(effectiveness);

                using var brush = new SolidBrush(cellColor);
                g.FillRectangle(brush, x, y, colWidth - 2, rowHeight - 2);

                // Draw effectiveness value
                var effText = FormatEffectiveness(effectiveness);
                using var textBrush = new SolidBrush(GetContrastColor(cellColor));
                var textSize = g.MeasureString(effText, Font);
                g.DrawString(effText, Font, textBrush, x + (colWidth - textSize.Width) / 2, y + 5);
            }
        }

        // Draw team total row
        int totalY = topMargin + headerHeight + (Team.Count * rowHeight) + 10;
        g.DrawString("TEAM TOTAL", new Font("Segoe UI", 8, FontStyle.Bold), Brushes.Cyan, 5, totalY + 5);

        for (int typeIndex = 0; typeIndex < types.Count; typeIndex++)
        {
            var attackType = types[typeIndex];
            var x = leftMargin + (typeIndex * colWidth);

            int weakCount = 0;
            int resistCount = 0;
            int immuneCount = 0;

            foreach (var pk in Team)
            {
                var pkTypes = GetPokemonTypes(pk);
                var effectiveness = GetTypeEffectiveness(attackType, pkTypes.Type1, pkTypes.Type2);

                if (effectiveness > 1) weakCount++;
                else if (effectiveness == 0) immuneCount++;
                else if (effectiveness < 1) resistCount++;
            }

            // Color based on team vulnerability
            Color cellColor;
            if (immuneCount > 0)
                cellColor = Color.FromArgb(33, 150, 243); // Blue - have immunity
            else if (weakCount >= 3)
                cellColor = Color.FromArgb(244, 67, 54); // Red - critical weakness
            else if (weakCount >= 2)
                cellColor = Color.FromArgb(255, 152, 0); // Orange - moderate weakness
            else if (resistCount >= Team.Count / 2)
                cellColor = Color.FromArgb(76, 175, 80); // Green - good coverage
            else
                cellColor = Color.FromArgb(158, 158, 158); // Gray - neutral

            using var brush = new SolidBrush(cellColor);
            g.FillRectangle(brush, x, totalY, colWidth - 2, rowHeight - 2);

            var summary = $"{weakCount}W";
            using var textBrush = new SolidBrush(GetContrastColor(cellColor));
            var textSize = g.MeasureString(summary, Font);
            g.DrawString(summary, Font, textBrush, x + (colWidth - textSize.Width) / 2, totalY + 5);
        }
    }

    private (PokemonType Type1, PokemonType Type2) GetPokemonTypes(PKM pk)
    {
        var pi = pk.PersonalInfo;
        return ((PokemonType)pi.Type1, (PokemonType)pi.Type2);
    }

    private double GetTypeEffectiveness(PokemonType attackType, PokemonType defType1, PokemonType defType2)
    {
        double effectiveness = 1.0;

        effectiveness *= GetSingleTypeEffectiveness(attackType, defType1);
        if (defType2 != PokemonType.None && defType2 != defType1)
            effectiveness *= GetSingleTypeEffectiveness(attackType, defType2);

        return effectiveness;
    }

    private double GetSingleTypeEffectiveness(PokemonType attack, PokemonType defend)
    {
        // Type chart lookup - simplified version
        return (attack, defend) switch
        {
            // Normal
            (PokemonType.Normal, PokemonType.Rock) => 0.5,
            (PokemonType.Normal, PokemonType.Ghost) => 0,
            (PokemonType.Normal, PokemonType.Steel) => 0.5,

            // Fire
            (PokemonType.Fire, PokemonType.Fire) => 0.5,
            (PokemonType.Fire, PokemonType.Water) => 0.5,
            (PokemonType.Fire, PokemonType.Grass) => 2,
            (PokemonType.Fire, PokemonType.Ice) => 2,
            (PokemonType.Fire, PokemonType.Bug) => 2,
            (PokemonType.Fire, PokemonType.Rock) => 0.5,
            (PokemonType.Fire, PokemonType.Dragon) => 0.5,
            (PokemonType.Fire, PokemonType.Steel) => 2,

            // Water
            (PokemonType.Water, PokemonType.Fire) => 2,
            (PokemonType.Water, PokemonType.Water) => 0.5,
            (PokemonType.Water, PokemonType.Grass) => 0.5,
            (PokemonType.Water, PokemonType.Ground) => 2,
            (PokemonType.Water, PokemonType.Rock) => 2,
            (PokemonType.Water, PokemonType.Dragon) => 0.5,

            // Electric
            (PokemonType.Electric, PokemonType.Water) => 2,
            (PokemonType.Electric, PokemonType.Electric) => 0.5,
            (PokemonType.Electric, PokemonType.Grass) => 0.5,
            (PokemonType.Electric, PokemonType.Ground) => 0,
            (PokemonType.Electric, PokemonType.Flying) => 2,
            (PokemonType.Electric, PokemonType.Dragon) => 0.5,

            // Grass
            (PokemonType.Grass, PokemonType.Fire) => 0.5,
            (PokemonType.Grass, PokemonType.Water) => 2,
            (PokemonType.Grass, PokemonType.Grass) => 0.5,
            (PokemonType.Grass, PokemonType.Poison) => 0.5,
            (PokemonType.Grass, PokemonType.Ground) => 2,
            (PokemonType.Grass, PokemonType.Flying) => 0.5,
            (PokemonType.Grass, PokemonType.Bug) => 0.5,
            (PokemonType.Grass, PokemonType.Rock) => 2,
            (PokemonType.Grass, PokemonType.Dragon) => 0.5,
            (PokemonType.Grass, PokemonType.Steel) => 0.5,

            // Ice
            (PokemonType.Ice, PokemonType.Fire) => 0.5,
            (PokemonType.Ice, PokemonType.Water) => 0.5,
            (PokemonType.Ice, PokemonType.Grass) => 2,
            (PokemonType.Ice, PokemonType.Ice) => 0.5,
            (PokemonType.Ice, PokemonType.Ground) => 2,
            (PokemonType.Ice, PokemonType.Flying) => 2,
            (PokemonType.Ice, PokemonType.Dragon) => 2,
            (PokemonType.Ice, PokemonType.Steel) => 0.5,

            // Fighting
            (PokemonType.Fighting, PokemonType.Normal) => 2,
            (PokemonType.Fighting, PokemonType.Ice) => 2,
            (PokemonType.Fighting, PokemonType.Poison) => 0.5,
            (PokemonType.Fighting, PokemonType.Flying) => 0.5,
            (PokemonType.Fighting, PokemonType.Psychic) => 0.5,
            (PokemonType.Fighting, PokemonType.Bug) => 0.5,
            (PokemonType.Fighting, PokemonType.Rock) => 2,
            (PokemonType.Fighting, PokemonType.Ghost) => 0,
            (PokemonType.Fighting, PokemonType.Dark) => 2,
            (PokemonType.Fighting, PokemonType.Steel) => 2,
            (PokemonType.Fighting, PokemonType.Fairy) => 0.5,

            // Poison
            (PokemonType.Poison, PokemonType.Grass) => 2,
            (PokemonType.Poison, PokemonType.Poison) => 0.5,
            (PokemonType.Poison, PokemonType.Ground) => 0.5,
            (PokemonType.Poison, PokemonType.Rock) => 0.5,
            (PokemonType.Poison, PokemonType.Ghost) => 0.5,
            (PokemonType.Poison, PokemonType.Steel) => 0,
            (PokemonType.Poison, PokemonType.Fairy) => 2,

            // Ground
            (PokemonType.Ground, PokemonType.Fire) => 2,
            (PokemonType.Ground, PokemonType.Electric) => 2,
            (PokemonType.Ground, PokemonType.Grass) => 0.5,
            (PokemonType.Ground, PokemonType.Poison) => 2,
            (PokemonType.Ground, PokemonType.Flying) => 0,
            (PokemonType.Ground, PokemonType.Bug) => 0.5,
            (PokemonType.Ground, PokemonType.Rock) => 2,
            (PokemonType.Ground, PokemonType.Steel) => 2,

            // Flying
            (PokemonType.Flying, PokemonType.Electric) => 0.5,
            (PokemonType.Flying, PokemonType.Grass) => 2,
            (PokemonType.Flying, PokemonType.Fighting) => 2,
            (PokemonType.Flying, PokemonType.Bug) => 2,
            (PokemonType.Flying, PokemonType.Rock) => 0.5,
            (PokemonType.Flying, PokemonType.Steel) => 0.5,

            // Psychic
            (PokemonType.Psychic, PokemonType.Fighting) => 2,
            (PokemonType.Psychic, PokemonType.Poison) => 2,
            (PokemonType.Psychic, PokemonType.Psychic) => 0.5,
            (PokemonType.Psychic, PokemonType.Dark) => 0,
            (PokemonType.Psychic, PokemonType.Steel) => 0.5,

            // Bug
            (PokemonType.Bug, PokemonType.Fire) => 0.5,
            (PokemonType.Bug, PokemonType.Grass) => 2,
            (PokemonType.Bug, PokemonType.Fighting) => 0.5,
            (PokemonType.Bug, PokemonType.Poison) => 0.5,
            (PokemonType.Bug, PokemonType.Flying) => 0.5,
            (PokemonType.Bug, PokemonType.Psychic) => 2,
            (PokemonType.Bug, PokemonType.Ghost) => 0.5,
            (PokemonType.Bug, PokemonType.Dark) => 2,
            (PokemonType.Bug, PokemonType.Steel) => 0.5,
            (PokemonType.Bug, PokemonType.Fairy) => 0.5,

            // Rock
            (PokemonType.Rock, PokemonType.Fire) => 2,
            (PokemonType.Rock, PokemonType.Ice) => 2,
            (PokemonType.Rock, PokemonType.Fighting) => 0.5,
            (PokemonType.Rock, PokemonType.Ground) => 0.5,
            (PokemonType.Rock, PokemonType.Flying) => 2,
            (PokemonType.Rock, PokemonType.Bug) => 2,
            (PokemonType.Rock, PokemonType.Steel) => 0.5,

            // Ghost
            (PokemonType.Ghost, PokemonType.Normal) => 0,
            (PokemonType.Ghost, PokemonType.Psychic) => 2,
            (PokemonType.Ghost, PokemonType.Ghost) => 2,
            (PokemonType.Ghost, PokemonType.Dark) => 0.5,

            // Dragon
            (PokemonType.Dragon, PokemonType.Dragon) => 2,
            (PokemonType.Dragon, PokemonType.Steel) => 0.5,
            (PokemonType.Dragon, PokemonType.Fairy) => 0,

            // Dark
            (PokemonType.Dark, PokemonType.Fighting) => 0.5,
            (PokemonType.Dark, PokemonType.Psychic) => 2,
            (PokemonType.Dark, PokemonType.Ghost) => 2,
            (PokemonType.Dark, PokemonType.Dark) => 0.5,
            (PokemonType.Dark, PokemonType.Fairy) => 0.5,

            // Steel
            (PokemonType.Steel, PokemonType.Fire) => 0.5,
            (PokemonType.Steel, PokemonType.Water) => 0.5,
            (PokemonType.Steel, PokemonType.Electric) => 0.5,
            (PokemonType.Steel, PokemonType.Ice) => 2,
            (PokemonType.Steel, PokemonType.Rock) => 2,
            (PokemonType.Steel, PokemonType.Steel) => 0.5,
            (PokemonType.Steel, PokemonType.Fairy) => 2,

            // Fairy
            (PokemonType.Fairy, PokemonType.Fire) => 0.5,
            (PokemonType.Fairy, PokemonType.Fighting) => 2,
            (PokemonType.Fairy, PokemonType.Poison) => 0.5,
            (PokemonType.Fairy, PokemonType.Dragon) => 2,
            (PokemonType.Fairy, PokemonType.Dark) => 2,
            (PokemonType.Fairy, PokemonType.Steel) => 0.5,

            _ => 1.0
        };
    }

    private string GetTypeAbbreviation(PokemonType type)
    {
        return type switch
        {
            PokemonType.Normal => "NRM",
            PokemonType.Fire => "FIR",
            PokemonType.Water => "WTR",
            PokemonType.Electric => "ELC",
            PokemonType.Grass => "GRS",
            PokemonType.Ice => "ICE",
            PokemonType.Fighting => "FGT",
            PokemonType.Poison => "PSN",
            PokemonType.Ground => "GRD",
            PokemonType.Flying => "FLY",
            PokemonType.Psychic => "PSY",
            PokemonType.Bug => "BUG",
            PokemonType.Rock => "RCK",
            PokemonType.Ghost => "GHT",
            PokemonType.Dragon => "DRG",
            PokemonType.Dark => "DRK",
            PokemonType.Steel => "STL",
            PokemonType.Fairy => "FRY",
            _ => "???"
        };
    }

    private Color GetEffectivenessColor(double effectiveness)
    {
        return effectiveness switch
        {
            0 => Color.FromArgb(244, 67, 54),      // Red - Immune
            0.25 => Color.FromArgb(255, 152, 0),   // Orange - Double resist
            0.5 => Color.FromArgb(255, 193, 7),    // Yellow - Resist
            1 => Color.FromArgb(158, 158, 158),    // Gray - Neutral
            2 => Color.FromArgb(76, 175, 80),      // Green - Super effective
            4 => Color.FromArgb(46, 125, 50),      // Dark Green - Double SE
            _ => Color.FromArgb(158, 158, 158)
        };
    }

    private string FormatEffectiveness(double effectiveness)
    {
        return effectiveness switch
        {
            0 => "0x",
            0.25 => "1/4",
            0.5 => "1/2",
            1 => "1x",
            2 => "2x",
            4 => "4x",
            _ => $"{effectiveness}x"
        };
    }

    private Color GetContrastColor(Color color)
    {
        // Calculate luminance
        double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
        return luminance > 0.5 ? Color.Black : Color.White;
    }
}
