using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class TeamCoverageAnalyzer : Form
{
    private readonly SaveFile SAV;
    private readonly Panel PNL_Chart;
    private readonly Label L_Summary;
    private readonly string[] TypeNames = { "Normal", "Fighting", "Flying", "Poison", "Ground", "Rock", "Bug", "Ghost", "Steel", "Fire", "Water", "Grass", "Electric", "Psychic", "Ice", "Dragon", "Dark", "Fairy" };
    private readonly Color[] TypeColors;
    private List<PKM> _team = new();

    private static readonly float[,] TypeChart = new float[18, 18]
    {
        {1,1,1,1,1,.5f,1,0,0.5f,1,1,1,1,1,1,1,1,1}, // Normal
        {2,1,.5f,.5f,1,2,.5f,0,2,1,1,1,1,.5f,2,1,2,.5f}, // Fighting
        {1,2,1,1,1,.5f,2,1,.5f,1,1,2,.5f,1,1,1,1,1}, // Flying
        {1,1,1,.5f,.5f,.5f,1,.5f,0,1,1,2,1,1,1,1,1,2}, // Poison
        {1,1,0,2,1,2,.5f,1,2,2,1,.5f,2,1,1,1,1,1}, // Ground
        {1,.5f,2,1,.5f,1,2,1,.5f,2,1,1,1,1,2,1,1,1}, // Rock
        {1,.5f,.5f,.5f,1,1,1,.5f,.5f,.5f,1,2,1,2,1,1,2,.5f}, // Bug
        {0,1,1,1,1,1,1,2,1,1,1,1,1,2,1,1,.5f,1}, // Ghost
        {1,1,1,1,1,2,1,1,.5f,.5f,.5f,1,.5f,1,2,1,1,2}, // Steel
        {1,1,1,1,1,.5f,2,1,2,.5f,.5f,2,1,1,2,.5f,1,1}, // Fire
        {1,1,1,1,2,2,1,1,1,2,.5f,.5f,1,1,1,.5f,1,1}, // Water
        {1,1,.5f,.5f,2,2,.5f,1,.5f,.5f,2,.5f,1,1,1,.5f,1,1}, // Grass
        {1,1,2,1,0,1,1,1,1,1,2,.5f,.5f,1,1,.5f,1,1}, // Electric
        {1,2,1,2,1,1,1,1,.5f,1,1,1,1,.5f,1,1,0,1}, // Psychic
        {1,1,2,1,2,1,1,1,.5f,.5f,.5f,2,1,1,.5f,2,1,1}, // Ice
        {1,1,1,1,1,1,1,1,.5f,1,1,1,1,1,1,2,1,0}, // Dragon
        {1,.5f,1,1,1,1,1,2,1,1,1,1,1,2,1,1,.5f,.5f}, // Dark
        {1,2,1,.5f,1,1,1,1,.5f,.5f,1,1,1,1,1,2,2,1} // Fairy
    };

    public TeamCoverageAnalyzer(SaveFile sav)
    {
        SAV = sav;
        TypeColors = new Color[] { Color.Gray, Color.Brown, Color.SkyBlue, Color.Purple, Color.SandyBrown, Color.Goldenrod, Color.YellowGreen, Color.DarkSlateBlue, Color.Silver, Color.OrangeRed, Color.DodgerBlue, Color.LimeGreen, Color.Gold, Color.HotPink, Color.LightCyan, Color.Indigo, Color.DimGray, Color.Pink };

        Text = "Team Coverage Analyzer";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var btnLoadParty = new Button { Text = "Load Party", Location = new Point(20, 15), Size = new Size(100, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 40, 90), ForeColor = Color.White };
        btnLoadParty.Click += (s, e) => LoadParty();

        var btnLoadBox = new Button { Text = "Load Box (6)", Location = new Point(130, 15), Size = new Size(100, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 40, 90), ForeColor = Color.White };
        btnLoadBox.Click += (s, e) => LoadBox();

        L_Summary = new Label { Location = new Point(250, 20), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 10F) };

        PNL_Chart = new Panel { Location = new Point(20, 60), Size = new Size(740, 480), BackColor = Color.FromArgb(35, 35, 55) };
        PNL_Chart.Paint += PNL_Chart_Paint;

        Controls.AddRange(new Control[] { btnLoadParty, btnLoadBox, L_Summary, PNL_Chart });
    }

    private void LoadParty()
    {
        _team.Clear();
        if (!SAV.HasParty) { MessageBox.Show("No party available!"); return; }
        for (int i = 0; i < 6; i++)
        {
            var pk = SAV.GetPartySlotAtIndex(i);
            if (pk.Species != 0) _team.Add(pk);
        }
        UpdateAnalysis();
    }

    private void LoadBox()
    {
        _team.Clear();
        for (int i = 0; i < Math.Min(6, SAV.BoxSlotCount); i++)
        {
            var pk = SAV.GetBoxSlotAtIndex(SAV.CurrentBox, i);
            if (pk.Species != 0) _team.Add(pk);
        }
        UpdateAnalysis();
    }

    private void UpdateAnalysis()
    {
        L_Summary.Text = $"Team: {_team.Count} Pokemon loaded";
        PNL_Chart.Invalidate();
    }

    private void PNL_Chart_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        if (_team.Count == 0) { g.DrawString("Load a team to analyze coverage", new Font("Segoe UI", 14F), Brushes.Gray, 250, 200); return; }

        float[] offensiveCoverage = new float[18];
        float[] defensiveWeakness = new float[18];

        foreach (var pk in _team)
        {
            int type1 = pk.PersonalInfo.Type1;
            int type2 = pk.PersonalInfo.Type2;
            for (int t = 0; t < 18; t++)
            {
                offensiveCoverage[t] = Math.Max(offensiveCoverage[t], TypeChart[type1, t]);
                if (type1 != type2) offensiveCoverage[t] = Math.Max(offensiveCoverage[t], TypeChart[type2, t]);
                float def = TypeChart[t, type1] * (type1 != type2 ? TypeChart[t, type2] : 1);
                defensiveWeakness[t] = Math.Max(defensiveWeakness[t], def);
            }
        }

        using var font = new Font("Segoe UI", 9F);
        int barWidth = 35, startX = 50, startY = 30;

        g.DrawString("Offensive Coverage (Super Effective)", new Font("Segoe UI", 11F, FontStyle.Bold), Brushes.White, startX, 5);
        for (int i = 0; i < 18; i++)
        {
            int x = startX + i * barWidth;
            int barHeight = (int)(offensiveCoverage[i] * 50);
            using var brush = new SolidBrush(TypeColors[i]);
            g.FillRectangle(brush, x, startY + 100 - barHeight, barWidth - 4, barHeight);
            g.DrawString(TypeNames[i].Substring(0, 3), font, Brushes.White, x, startY + 105);
        }

        g.DrawString("Defensive Weaknesses", new Font("Segoe UI", 11F, FontStyle.Bold), Brushes.White, startX, 200);
        for (int i = 0; i < 18; i++)
        {
            int x = startX + i * barWidth;
            int barHeight = (int)(defensiveWeakness[i] * 30);
            Color barColor = defensiveWeakness[i] >= 2 ? Color.Red : defensiveWeakness[i] >= 1 ? Color.Yellow : Color.Green;
            using var brush = new SolidBrush(barColor);
            g.FillRectangle(brush, x, 330 - barHeight, barWidth - 4, barHeight);
            g.DrawString(TypeNames[i].Substring(0, 3), font, Brushes.White, x, 335);
        }

        g.DrawString("Team Members:", new Font("Segoe UI", 10F, FontStyle.Bold), Brushes.White, 50, 380);
        int ty = 405;
        foreach (var pk in _team)
        {
            var name = GameInfo.Strings.specieslist[pk.Species];
            var t1 = TypeNames[pk.PersonalInfo.Type1];
            var t2 = pk.PersonalInfo.Type1 != pk.PersonalInfo.Type2 ? "/" + TypeNames[pk.PersonalInfo.Type2] : "";
            g.DrawString($"{name} ({t1}{t2})", font, Brushes.LightGray, 50, ty);
            ty += 18;
        }
    }
}
