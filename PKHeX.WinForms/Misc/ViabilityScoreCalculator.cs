using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public partial class ViabilityScoreCalculator : Form
{
    private readonly SaveFile SAV;
    private ListView lstPokemon = null!;
    private Panel pnlScoreDisplay = null!;
    private Label lblOverallScore = null!;
    private Label lblTierRating = null!;
    private ProgressBar pbViability = null!;
    private RichTextBox rtbBreakdown = null!;
    private ComboBox cmbFormat = null!;
    private ComboBox cmbGeneration = null!;
    private Button btnAnalyze = null!;
    private Button btnAnalyzeTeam = null!;
    private Panel pnlRadar = null!;

    // Smogon-style tier data (simplified)
    private static readonly Dictionary<string, Dictionary<ushort, string>> TierData = new()
    {
        ["Gen 9 OU"] = new()
        {
            [1006] = "OU", // Iron Valiant
            [1007] = "OU", // Koraidon
            [1008] = "Uber", // Miraidon
            [887] = "OU", // Dragapult
            [1000] = "OU", // Gholdengo
            [984] = "OU", // Great Tusk
            [983] = "OU", // Kingambit
            [911] = "OU", // Skeledirge
            [889] = "OU", // Zamazenta
            [445] = "OU", // Garchomp
            [149] = "OU", // Dragonite
            [6] = "OU", // Charizard
            [94] = "OU", // Gengar
            [248] = "OU", // Tyranitar
            [373] = "UU", // Salamence
            [376] = "OU", // Metagross
            [635] = "UU", // Hydreigon
        },
        ["VGC 2024"] = new()
        {
            [1006] = "S", [1007] = "S", [1008] = "S",
            [887] = "A", [1000] = "A", [984] = "A",
            [727] = "S", // Incineroar
            [812] = "A", // Rillaboom
            [892] = "S", // Urshifu
        }
    };

    // Base viability scores by stat distribution
    private static readonly Dictionary<string, double> RoleMultipliers = new()
    {
        ["Sweeper"] = 1.2,
        ["Wall"] = 1.1,
        ["Support"] = 1.0,
        ["Pivot"] = 1.15,
        ["Setup Sweeper"] = 1.25,
        ["Choice User"] = 1.1,
        ["Hazard Setter"] = 1.05
    };

    public ViabilityScoreCalculator(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
        LoadPokemonList();
    }

    private void InitializeComponent()
    {
        Text = "Competitive Viability Score Calculator";
        Size = new Size(1100, 750);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        // Format/Gen selectors
        var lblFormat = new Label
        {
            Text = "Format:",
            Location = new Point(20, 20),
            Size = new Size(60, 25),
            ForeColor = Color.White
        };

        cmbFormat = new ComboBox
        {
            Location = new Point(85, 17),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbFormat.Items.AddRange(new[] { "Gen 9 OU", "VGC 2024", "Battle Stadium", "Ubers", "UU", "RU", "NU" });
        cmbFormat.SelectedIndex = 0;

        var lblGen = new Label
        {
            Text = "Generation:",
            Location = new Point(260, 20),
            Size = new Size(80, 25),
            ForeColor = Color.White
        };

        cmbGeneration = new ComboBox
        {
            Location = new Point(345, 17),
            Size = new Size(100, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbGeneration.Items.AddRange(new[] { "Gen 9", "Gen 8", "Gen 7", "Gen 6" });
        cmbGeneration.SelectedIndex = 0;

        // Pokemon List
        var grpPokemon = new GroupBox
        {
            Text = "Your Pokemon",
            Location = new Point(20, 60),
            Size = new Size(400, 400),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        lstPokemon = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(370, 330),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstPokemon.Columns.Add("Pokemon", 150);
        lstPokemon.Columns.Add("BST", 60);
        lstPokemon.Columns.Add("Type", 90);
        lstPokemon.Columns.Add("Score", 60);
        lstPokemon.SelectedIndexChanged += LstPokemon_SelectedIndexChanged;

        btnAnalyze = new Button
        {
            Text = "Analyze Selected",
            Location = new Point(15, 360),
            Size = new Size(120, 30),
            BackColor = Color.FromArgb(60, 120, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnAnalyze.Click += BtnAnalyze_Click;

        btnAnalyzeTeam = new Button
        {
            Text = "Analyze Full Team",
            Location = new Point(145, 360),
            Size = new Size(120, 30),
            BackColor = Color.FromArgb(60, 180, 120),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnAnalyzeTeam.Click += BtnAnalyzeTeam_Click;

        grpPokemon.Controls.AddRange(new Control[] { lstPokemon, btnAnalyze, btnAnalyzeTeam });

        // Score Display Panel
        pnlScoreDisplay = new Panel
        {
            Location = new Point(440, 60),
            Size = new Size(300, 200),
            BackColor = Color.FromArgb(35, 35, 55)
        };

        lblOverallScore = new Label
        {
            Text = "VIABILITY SCORE",
            Location = new Point(20, 15),
            Size = new Size(260, 30),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 12F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblScoreValue = new Label
        {
            Name = "lblScoreValue",
            Text = "---",
            Location = new Point(20, 45),
            Size = new Size(260, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 36F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        lblTierRating = new Label
        {
            Text = "Tier: ---",
            Location = new Point(20, 110),
            Size = new Size(260, 30),
            ForeColor = Color.FromArgb(255, 200, 100),
            Font = new Font("Segoe UI Semibold", 14F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        pbViability = new ProgressBar
        {
            Location = new Point(20, 150),
            Size = new Size(260, 25),
            Maximum = 100,
            Style = ProgressBarStyle.Continuous
        };

        pnlScoreDisplay.Controls.AddRange(new Control[] { lblOverallScore, lblScoreValue, lblTierRating, pbViability });

        // Radar Chart Panel (visual stat representation)
        pnlRadar = new Panel
        {
            Location = new Point(760, 60),
            Size = new Size(300, 200),
            BackColor = Color.FromArgb(35, 35, 55)
        };
        pnlRadar.Paint += PnlRadar_Paint;

        // Breakdown
        var grpBreakdown = new GroupBox
        {
            Text = "Viability Breakdown",
            Location = new Point(440, 270),
            Size = new Size(620, 190),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        rtbBreakdown = new RichTextBox
        {
            Location = new Point(15, 25),
            Size = new Size(590, 150),
            BackColor = Color.FromArgb(30, 30, 45),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };

        grpBreakdown.Controls.Add(rtbBreakdown);

        // Team Summary Panel
        var grpTeam = new GroupBox
        {
            Text = "Team Viability Summary",
            Location = new Point(20, 470),
            Size = new Size(1040, 200),
            ForeColor = Color.FromArgb(100, 255, 150)
        };

        var pnlTeamStats = new FlowLayoutPanel
        {
            Name = "pnlTeamStats",
            Location = new Point(15, 25),
            Size = new Size(1010, 160),
            BackColor = Color.Transparent,
            AutoScroll = true
        };

        grpTeam.Controls.Add(pnlTeamStats);

        Controls.AddRange(new Control[] { lblFormat, cmbFormat, lblGen, cmbGeneration, grpPokemon, pnlScoreDisplay, pnlRadar, grpBreakdown, grpTeam });
    }

    private PKM? currentPokemon;
    private double[] currentStats = new double[6];

    private void LoadPokemonList()
    {
        lstPokemon.Items.Clear();

        var allPokemon = SAV.PartyData.Concat(SAV.BoxData).Where(p => p.Species != 0).ToList();

        foreach (var pk in allPokemon)
        {
            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            var bst = pk.PersonalInfo.GetBaseStatTotal();
            var type1 = GetTypeName((int)pk.PersonalInfo.Type1);
            var type2 = pk.PersonalInfo.Type2 != pk.PersonalInfo.Type1 ? "/" + GetTypeName((int)pk.PersonalInfo.Type2) : "";
            var quickScore = CalculateQuickViability(pk);

            var item = new ListViewItem(name);
            item.SubItems.Add(bst.ToString());
            item.SubItems.Add(type1 + type2);
            item.SubItems.Add(quickScore.ToString("F0"));
            item.Tag = pk;

            // Color code by score
            if (quickScore >= 80) item.ForeColor = Color.LightGreen;
            else if (quickScore >= 60) item.ForeColor = Color.Yellow;
            else if (quickScore >= 40) item.ForeColor = Color.Orange;
            else item.ForeColor = Color.Salmon;

            lstPokemon.Items.Add(item);
        }
    }

    private void LstPokemon_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstPokemon.SelectedItems.Count > 0 && lstPokemon.SelectedItems[0].Tag is PKM pk)
        {
            currentPokemon = pk;
            UpdateQuickPreview(pk);
        }
    }

    private void UpdateQuickPreview(PKM pk)
    {
        var score = CalculateQuickViability(pk);
        var scoreLabel = pnlScoreDisplay.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblScoreValue");
        if (scoreLabel != null)
        {
            scoreLabel.Text = score.ToString("F0");
            scoreLabel.ForeColor = score >= 70 ? Color.LightGreen : score >= 50 ? Color.Yellow : Color.Salmon;
        }

        pbViability.Value = Math.Min(100, (int)score);
        lblTierRating.Text = $"Tier: {GetTierRating(pk)}";

        // Update radar
        currentStats = new double[]
        {
            pk.PersonalInfo.HP / 255.0 * 100,
            pk.PersonalInfo.ATK / 190.0 * 100,
            pk.PersonalInfo.DEF / 230.0 * 100,
            pk.PersonalInfo.SPA / 194.0 * 100,
            pk.PersonalInfo.SPD / 230.0 * 100,
            pk.PersonalInfo.SPE / 200.0 * 100
        };
        pnlRadar.Invalidate();
    }

    private void BtnAnalyze_Click(object? sender, EventArgs e)
    {
        if (currentPokemon == null)
        {
            MessageBox.Show("Please select a Pokemon first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        PerformDetailedAnalysis(currentPokemon);
    }

    private void BtnAnalyzeTeam_Click(object? sender, EventArgs e)
    {
        var party = SAV.PartyData.Where(p => p.Species != 0).ToList();
        if (party.Count == 0)
        {
            MessageBox.Show("No Pokemon in party!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        AnalyzeTeamViability(party);
    }

    private void PerformDetailedAnalysis(PKM pk)
    {
        rtbBreakdown.Clear();
        var format = cmbFormat.SelectedItem?.ToString() ?? "Gen 9 OU";

        rtbBreakdown.SelectionColor = Color.Cyan;
        rtbBreakdown.AppendText($"═══ VIABILITY ANALYSIS: {SpeciesName.GetSpeciesName(pk.Species, 2)} ═══\n\n");

        // Base Stats Analysis
        rtbBreakdown.SelectionColor = Color.Yellow;
        rtbBreakdown.AppendText("BASE STAT ANALYSIS:\n");
        rtbBreakdown.SelectionColor = Color.White;

        var bst = pk.PersonalInfo.GetBaseStatTotal();
        var bstScore = Math.Min(30, bst / 20.0);
        rtbBreakdown.AppendText($"  BST: {bst} → Score: +{bstScore:F1}\n");

        // Stat Distribution Score
        int atk = pk.PersonalInfo.ATK;
        int spa = pk.PersonalInfo.SPA;
        int spe = pk.PersonalInfo.SPE;
        var offenseScore = (Math.Max(atk, spa) / 150.0) * 15;
        var speedScore = (spe / 150.0) * 15;
        rtbBreakdown.AppendText($"  Offense: {Math.Max(atk, spa)} → Score: +{offenseScore:F1}\n");
        rtbBreakdown.AppendText($"  Speed: {spe} → Score: +{speedScore:F1}\n");

        // Type Analysis
        rtbBreakdown.SelectionColor = Color.Yellow;
        rtbBreakdown.AppendText("\nTYPE VIABILITY:\n");
        rtbBreakdown.SelectionColor = Color.White;

        var type1 = GetTypeName((int)pk.PersonalInfo.Type1);
        var type2 = pk.PersonalInfo.Type2 != pk.PersonalInfo.Type1 ? GetTypeName((int)pk.PersonalInfo.Type2) : null;
        var typeScore = CalculateTypeViability(type1, type2);
        rtbBreakdown.AppendText($"  Types: {type1}{(type2 != null ? "/" + type2 : "")}\n");
        rtbBreakdown.AppendText($"  Type Score: +{typeScore:F1}\n");

        var (weaknesses, resistances) = CalculateTypeMatchups(type1, type2);
        rtbBreakdown.AppendText($"  Weaknesses: {weaknesses} | Resistances: {resistances}\n");

        // Meta Relevance
        rtbBreakdown.SelectionColor = Color.Yellow;
        rtbBreakdown.AppendText("\nMETA RELEVANCE:\n");
        rtbBreakdown.SelectionColor = Color.White;

        var tier = GetTierRating(pk);
        var tierScore = tier switch
        {
            "S" or "Uber" => 15,
            "A" or "OU" => 12,
            "B" or "UU" => 9,
            "C" or "RU" => 6,
            _ => 3
        };
        rtbBreakdown.AppendText($"  Tier: {tier} → Score: +{tierScore}\n");

        // Role Potential
        var role = DetermineOptimalRole(pk);
        var roleScore = RoleMultipliers.GetValueOrDefault(role, 1.0) * 10;
        rtbBreakdown.AppendText($"  Optimal Role: {role} → Score: +{roleScore:F1}\n");

        // Total Score
        var totalScore = bstScore + offenseScore + speedScore + typeScore + tierScore + roleScore;
        totalScore = Math.Min(100, totalScore);

        rtbBreakdown.SelectionColor = Color.LightGreen;
        rtbBreakdown.AppendText($"\n═══ TOTAL VIABILITY SCORE: {totalScore:F0}/100 ═══\n");

        // Update display
        var scoreLabel = pnlScoreDisplay.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblScoreValue");
        if (scoreLabel != null)
        {
            scoreLabel.Text = totalScore.ToString("F0");
            scoreLabel.ForeColor = totalScore >= 70 ? Color.LightGreen : totalScore >= 50 ? Color.Yellow : Color.Salmon;
        }
        pbViability.Value = (int)totalScore;
    }

    private void AnalyzeTeamViability(List<PKM> team)
    {
        var pnlTeamStats = Controls.Find("pnlTeamStats", true).FirstOrDefault() as FlowLayoutPanel;
        if (pnlTeamStats == null) return;

        pnlTeamStats.Controls.Clear();

        double teamTotal = 0;

        foreach (var pk in team)
        {
            var score = CalculateQuickViability(pk);
            teamTotal += score;

            var card = new Panel
            {
                Size = new Size(155, 140),
                BackColor = Color.FromArgb(40, 40, 60),
                Margin = new Padding(5)
            };

            var lblName = new Label
            {
                Text = SpeciesName.GetSpeciesName(pk.Species, 2),
                Location = new Point(5, 5),
                Size = new Size(145, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblScore = new Label
            {
                Text = score.ToString("F0"),
                Location = new Point(5, 35),
                Size = new Size(145, 40),
                ForeColor = score >= 70 ? Color.LightGreen : score >= 50 ? Color.Yellow : Color.Salmon,
                Font = new Font("Segoe UI Bold", 22F),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblTier = new Label
            {
                Text = $"Tier: {GetTierRating(pk)}",
                Location = new Point(5, 80),
                Size = new Size(145, 20),
                ForeColor = Color.FromArgb(255, 200, 100),
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var pbScore = new ProgressBar
            {
                Location = new Point(10, 105),
                Size = new Size(135, 15),
                Maximum = 100,
                Value = Math.Min(100, (int)score)
            };

            card.Controls.AddRange(new Control[] { lblName, lblScore, lblTier, pbScore });
            pnlTeamStats.Controls.Add(card);
        }

        // Add team summary card
        var avgScore = teamTotal / team.Count;
        var summaryCard = new Panel
        {
            Size = new Size(155, 140),
            BackColor = Color.FromArgb(50, 80, 50),
            Margin = new Padding(5)
        };

        var lblSummaryTitle = new Label
        {
            Text = "TEAM AVERAGE",
            Location = new Point(5, 5),
            Size = new Size(145, 25),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 9F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblAvgScore = new Label
        {
            Text = avgScore.ToString("F0"),
            Location = new Point(5, 35),
            Size = new Size(145, 50),
            ForeColor = avgScore >= 70 ? Color.LightGreen : avgScore >= 50 ? Color.Yellow : Color.Salmon,
            Font = new Font("Segoe UI Bold", 28F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblRating = new Label
        {
            Text = avgScore >= 80 ? "S-Tier Team" : avgScore >= 65 ? "A-Tier Team" : avgScore >= 50 ? "B-Tier Team" : "C-Tier Team",
            Location = new Point(5, 95),
            Size = new Size(145, 30),
            ForeColor = Color.FromArgb(100, 255, 150),
            Font = new Font("Segoe UI Bold", 10F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        summaryCard.Controls.AddRange(new Control[] { lblSummaryTitle, lblAvgScore, lblRating });
        pnlTeamStats.Controls.Add(summaryCard);

        rtbBreakdown.Clear();
        rtbBreakdown.SelectionColor = Color.Cyan;
        rtbBreakdown.AppendText("═══ TEAM VIABILITY ANALYSIS ═══\n\n");
        rtbBreakdown.SelectionColor = Color.White;
        rtbBreakdown.AppendText($"Team Size: {team.Count} Pokemon\n");
        rtbBreakdown.AppendText($"Average Viability: {avgScore:F1}/100\n");
        rtbBreakdown.AppendText($"Team Rating: {(avgScore >= 80 ? "S-Tier" : avgScore >= 65 ? "A-Tier" : avgScore >= 50 ? "B-Tier" : "C-Tier")}\n\n");

        // Type coverage analysis
        var types = team.SelectMany(p => new[] {
            GetTypeName((int)p.PersonalInfo.Type1),
            p.PersonalInfo.Type2 != p.PersonalInfo.Type1 ? GetTypeName((int)p.PersonalInfo.Type2) : null
        }).Where(t => t != null).Distinct().ToList();

        rtbBreakdown.SelectionColor = Color.Yellow;
        rtbBreakdown.AppendText($"Type Coverage: {types.Count}/18 types represented\n");
        rtbBreakdown.SelectionColor = Color.White;
        rtbBreakdown.AppendText($"Types: {string.Join(", ", types)}\n");
    }

    private void PnlRadar_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int cx = pnlRadar.Width / 2;
        int cy = pnlRadar.Height / 2;
        int radius = Math.Min(cx, cy) - 20;

        // Draw hexagon guides
        for (int r = 20; r <= radius; r += 20)
        {
            var points = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 2 + i * Math.PI / 3;
                points[i] = new PointF(cx + (float)(r * Math.Cos(angle)), cy - (float)(r * Math.Sin(angle)));
            }
            g.DrawPolygon(new Pen(Color.FromArgb(60, 60, 80)), points);
        }

        // Draw stat labels
        string[] labels = { "HP", "ATK", "DEF", "SpA", "SpD", "SPE" };
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 2 + i * Math.PI / 3;
            float lx = cx + (float)((radius + 15) * Math.Cos(angle));
            float ly = cy - (float)((radius + 15) * Math.Sin(angle));
            g.DrawString(labels[i], Font, Brushes.White, lx - 12, ly - 7);
        }

        // Draw stat polygon
        if (currentStats.Sum() > 0)
        {
            var statPoints = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 2 + i * Math.PI / 3;
                float r = (float)(currentStats[i] / 100.0 * radius);
                statPoints[i] = new PointF(cx + (float)(r * Math.Cos(angle)), cy - (float)(r * Math.Sin(angle)));
            }

            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Point(0, 0), new Point(pnlRadar.Width, pnlRadar.Height),
                Color.FromArgb(100, 100, 200, 255), Color.FromArgb(100, 100, 255, 150));
            g.FillPolygon(brush, statPoints);
            g.DrawPolygon(new Pen(Color.FromArgb(100, 200, 255), 2), statPoints);
        }
    }

    private double CalculateQuickViability(PKM pk)
    {
        var bst = pk.PersonalInfo.GetBaseStatTotal();
        var bstScore = Math.Min(30, bst / 20.0);

        var atk = Math.Max(pk.PersonalInfo.ATK, pk.PersonalInfo.SPA);
        var offenseScore = (atk / 150.0) * 20;

        var spe = pk.PersonalInfo.SPE;
        var speedScore = (spe / 150.0) * 15;

        var type1 = GetTypeName((int)pk.PersonalInfo.Type1);
        var type2 = pk.PersonalInfo.Type2 != pk.PersonalInfo.Type1 ? GetTypeName((int)pk.PersonalInfo.Type2) : null;
        var typeScore = CalculateTypeViability(type1, type2);

        var tierScore = GetTierRating(pk) switch
        {
            "S" or "Uber" => 15,
            "A" or "OU" => 12,
            "B" or "UU" => 9,
            "C" or "RU" => 6,
            _ => 3
        };

        return Math.Min(100, bstScore + offenseScore + speedScore + typeScore + tierScore);
    }

    private double CalculateTypeViability(string type1, string? type2)
    {
        // High-tier types in current meta
        var goodTypes = new[] { "Steel", "Fairy", "Dragon", "Ghost", "Ground", "Water" };
        var badTypes = new[] { "Bug", "Ice", "Rock", "Grass" };

        double score = 5;
        if (goodTypes.Contains(type1)) score += 5;
        if (type2 != null && goodTypes.Contains(type2)) score += 5;
        if (badTypes.Contains(type1)) score -= 3;
        if (type2 != null && badTypes.Contains(type2)) score -= 2;

        return Math.Max(0, Math.Min(15, score));
    }

    private (int weaknesses, int resistances) CalculateTypeMatchups(string type1, string? type2)
    {
        int weak = 0, resist = 0;
        // Simplified calculation
        var types = new[] { type1, type2 }.Where(t => t != null).ToList();

        foreach (var defType in types)
        {
            if (defType == "Steel") { resist += 5; weak += 1; }
            else if (defType == "Fairy") { resist += 2; weak += 2; }
            else if (defType == "Ghost") { resist += 2; weak += 2; }
            else if (defType == "Ice") { weak += 4; resist += 1; }
            else { weak += 2; resist += 2; }
        }

        return (weak, resist);
    }

    private string GetTierRating(PKM pk)
    {
        var format = cmbFormat.SelectedItem?.ToString() ?? "Gen 9 OU";
        if (TierData.TryGetValue(format, out var tiers) && tiers.TryGetValue(pk.Species, out var tier))
            return tier;

        // Estimate tier by BST
        var bst = pk.PersonalInfo.GetBaseStatTotal();
        return bst switch
        {
            >= 600 => "OU",
            >= 520 => "UU",
            >= 450 => "RU",
            >= 400 => "NU",
            _ => "PU"
        };
    }

    private string DetermineOptimalRole(PKM pk)
    {
        int atk = pk.PersonalInfo.ATK;
        int spa = pk.PersonalInfo.SPA;
        int def = pk.PersonalInfo.DEF;
        int spd = pk.PersonalInfo.SPD;
        int spe = pk.PersonalInfo.SPE;

        if (spe >= 100 && (atk >= 100 || spa >= 100)) return "Sweeper";
        if (spe >= 80 && (atk >= 110 || spa >= 110)) return "Setup Sweeper";
        if (def >= 100 && spd >= 100) return "Wall";
        if (spe >= 90 && def >= 80) return "Pivot";
        if (atk >= 120 || spa >= 120) return "Choice User";
        return "Support";
    }

    private string GetTypeName(int typeId) => typeId switch
    {
        0 => "Normal", 1 => "Fighting", 2 => "Flying", 3 => "Poison", 4 => "Ground",
        5 => "Rock", 6 => "Bug", 7 => "Ghost", 8 => "Steel", 9 => "Fire",
        10 => "Water", 11 => "Grass", 12 => "Electric", 13 => "Psychic", 14 => "Ice",
        15 => "Dragon", 16 => "Dark", 17 => "Fairy", _ => "Normal"
    };
}
