using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public partial class BattleMatchupPredictor : Form
{
    private readonly SaveFile SAV;
    private TabControl tabControl = null!;
    private ListView lstYourTeam = null!;
    private ListView lstOpponentTeam = null!;
    private Panel pnlMatchupMatrix = null!;
    private Label lblWinRate = null!;
    private Label lblThreatAnalysis = null!;
    private RichTextBox rtbDetailedAnalysis = null!;
    private ComboBox cmbFormat = null!;
    private Button btnAnalyze = null!;
    private Button btnLoadFromBox = null!;
    private Button btnLoadMetaTeam = null!;
    private ProgressBar pbOverallScore = null!;

    // Type effectiveness chart
    private static readonly Dictionary<string, Dictionary<string, double>> TypeChart = new()
    {
        ["Normal"] = new() { ["Rock"] = 0.5, ["Ghost"] = 0, ["Steel"] = 0.5 },
        ["Fire"] = new() { ["Fire"] = 0.5, ["Water"] = 0.5, ["Grass"] = 2, ["Ice"] = 2, ["Bug"] = 2, ["Rock"] = 0.5, ["Dragon"] = 0.5, ["Steel"] = 2 },
        ["Water"] = new() { ["Fire"] = 2, ["Water"] = 0.5, ["Grass"] = 0.5, ["Ground"] = 2, ["Rock"] = 2, ["Dragon"] = 0.5 },
        ["Electric"] = new() { ["Water"] = 2, ["Electric"] = 0.5, ["Grass"] = 0.5, ["Ground"] = 0, ["Flying"] = 2, ["Dragon"] = 0.5 },
        ["Grass"] = new() { ["Fire"] = 0.5, ["Water"] = 2, ["Grass"] = 0.5, ["Poison"] = 0.5, ["Ground"] = 2, ["Flying"] = 0.5, ["Bug"] = 0.5, ["Rock"] = 2, ["Dragon"] = 0.5, ["Steel"] = 0.5 },
        ["Ice"] = new() { ["Fire"] = 0.5, ["Water"] = 0.5, ["Grass"] = 2, ["Ice"] = 0.5, ["Ground"] = 2, ["Flying"] = 2, ["Dragon"] = 2, ["Steel"] = 0.5 },
        ["Fighting"] = new() { ["Normal"] = 2, ["Ice"] = 2, ["Poison"] = 0.5, ["Flying"] = 0.5, ["Psychic"] = 0.5, ["Bug"] = 0.5, ["Rock"] = 2, ["Ghost"] = 0, ["Dark"] = 2, ["Steel"] = 2, ["Fairy"] = 0.5 },
        ["Poison"] = new() { ["Grass"] = 2, ["Poison"] = 0.5, ["Ground"] = 0.5, ["Rock"] = 0.5, ["Ghost"] = 0.5, ["Steel"] = 0, ["Fairy"] = 2 },
        ["Ground"] = new() { ["Fire"] = 2, ["Electric"] = 2, ["Grass"] = 0.5, ["Poison"] = 2, ["Flying"] = 0, ["Bug"] = 0.5, ["Rock"] = 2, ["Steel"] = 2 },
        ["Flying"] = new() { ["Electric"] = 0.5, ["Grass"] = 2, ["Fighting"] = 2, ["Bug"] = 2, ["Rock"] = 0.5, ["Steel"] = 0.5 },
        ["Psychic"] = new() { ["Fighting"] = 2, ["Poison"] = 2, ["Psychic"] = 0.5, ["Dark"] = 0, ["Steel"] = 0.5 },
        ["Bug"] = new() { ["Fire"] = 0.5, ["Grass"] = 2, ["Fighting"] = 0.5, ["Poison"] = 0.5, ["Flying"] = 0.5, ["Psychic"] = 2, ["Ghost"] = 0.5, ["Dark"] = 2, ["Steel"] = 0.5, ["Fairy"] = 0.5 },
        ["Rock"] = new() { ["Fire"] = 2, ["Ice"] = 2, ["Fighting"] = 0.5, ["Ground"] = 0.5, ["Flying"] = 2, ["Bug"] = 2, ["Steel"] = 0.5 },
        ["Ghost"] = new() { ["Normal"] = 0, ["Psychic"] = 2, ["Ghost"] = 2, ["Dark"] = 0.5 },
        ["Dragon"] = new() { ["Dragon"] = 2, ["Steel"] = 0.5, ["Fairy"] = 0 },
        ["Dark"] = new() { ["Fighting"] = 0.5, ["Psychic"] = 2, ["Ghost"] = 2, ["Dark"] = 0.5, ["Fairy"] = 0.5 },
        ["Steel"] = new() { ["Fire"] = 0.5, ["Water"] = 0.5, ["Electric"] = 0.5, ["Ice"] = 2, ["Rock"] = 2, ["Steel"] = 0.5, ["Fairy"] = 2 },
        ["Fairy"] = new() { ["Fire"] = 0.5, ["Fighting"] = 2, ["Poison"] = 0.5, ["Dragon"] = 2, ["Dark"] = 2, ["Steel"] = 0.5 }
    };

    private static readonly string[] AllTypes = { "Normal", "Fire", "Water", "Electric", "Grass", "Ice", "Fighting", "Poison", "Ground", "Flying", "Psychic", "Bug", "Rock", "Ghost", "Dragon", "Dark", "Steel", "Fairy" };

    // Meta teams for different formats
    private static readonly Dictionary<string, List<string[]>> MetaTeams = new()
    {
        ["VGC 2024"] = new()
        {
            new[] { "Flutter Mane", "Landorus", "Rillaboom", "Incineroar", "Urshifu", "Chien-Pao" },
            new[] { "Koraidon", "Miraidon", "Calyrex-Shadow", "Zacian", "Incineroar", "Rillaboom" },
            new[] { "Ogerpon", "Ursaluna", "Farigiraf", "Annihilape", "Pelipper", "Archaludon" }
        },
        ["OU Singles"] = new()
        {
            new[] { "Dragapult", "Gholdengo", "Great Tusk", "Kingambit", "Skeledirge", "Zamazenta" },
            new[] { "Iron Valiant", "Gliscor", "Heatran", "Slowking-Galar", "Weavile", "Corviknight" },
            new[] { "Darkrai", "Kyurem", "Landorus-Therian", "Toxapex", "Clefable", "Volcarona" }
        },
        ["Battle Stadium"] = new()
        {
            new[] { "Flutter Mane", "Garchomp", "Gholdengo", "Dragonite", "Annihilape", "Grimmsnarl" },
            new[] { "Iron Hands", "Palafin", "Dondozo", "Tatsugiri", "Amoonguss", "Arcanine" }
        }
    };

    private List<PokemonMatchupData> yourTeam = new();
    private List<PokemonMatchupData> opponentTeam = new();

    public BattleMatchupPredictor(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
        LoadYourTeamFromSave();
    }

    private void InitializeComponent()
    {
        Text = "Battle Matchup Predictor - AI Analysis";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 10F)
        };

        // Tab 1: Team Setup
        var tabSetup = new TabPage("Team Setup") { BackColor = Color.FromArgb(30, 30, 50) };
        CreateSetupTab(tabSetup);
        tabControl.TabPages.Add(tabSetup);

        // Tab 2: Matchup Matrix
        var tabMatrix = new TabPage("Matchup Matrix") { BackColor = Color.FromArgb(30, 30, 50) };
        CreateMatrixTab(tabMatrix);
        tabControl.TabPages.Add(tabMatrix);

        // Tab 3: Detailed Analysis
        var tabAnalysis = new TabPage("AI Analysis") { BackColor = Color.FromArgb(30, 30, 50) };
        CreateAnalysisTab(tabAnalysis);
        tabControl.TabPages.Add(tabAnalysis);

        Controls.Add(tabControl);
    }

    private void CreateSetupTab(TabPage tab)
    {
        // Format selector
        var lblFormat = new Label
        {
            Text = "Battle Format:",
            Location = new Point(20, 20),
            Size = new Size(100, 25),
            ForeColor = Color.White
        };

        cmbFormat = new ComboBox
        {
            Location = new Point(130, 17),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbFormat.Items.AddRange(new[] { "VGC 2024", "OU Singles", "Battle Stadium", "Anything Goes", "Custom" });
        cmbFormat.SelectedIndex = 0;

        // Your Team
        var grpYourTeam = new GroupBox
        {
            Text = "Your Team",
            Location = new Point(20, 60),
            Size = new Size(540, 300),
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI Semibold", 11F)
        };

        lstYourTeam = new ListView
        {
            Location = new Point(15, 30),
            Size = new Size(510, 220),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F)
        };
        lstYourTeam.Columns.Add("Pokemon", 120);
        lstYourTeam.Columns.Add("Type 1", 70);
        lstYourTeam.Columns.Add("Type 2", 70);
        lstYourTeam.Columns.Add("Role", 100);
        lstYourTeam.Columns.Add("Threat Level", 80);

        btnLoadFromBox = new Button
        {
            Text = "Load from Party/Box",
            Location = new Point(15, 255),
            Size = new Size(150, 30),
            BackColor = Color.FromArgb(60, 60, 90),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnLoadFromBox.Click += BtnLoadFromBox_Click;

        grpYourTeam.Controls.AddRange(new Control[] { lstYourTeam, btnLoadFromBox });

        // Opponent Team
        var grpOpponent = new GroupBox
        {
            Text = "Opponent Team (Predicted/Entered)",
            Location = new Point(580, 60),
            Size = new Size(540, 300),
            ForeColor = Color.FromArgb(255, 150, 100),
            Font = new Font("Segoe UI Semibold", 11F)
        };

        lstOpponentTeam = new ListView
        {
            Location = new Point(15, 30),
            Size = new Size(510, 220),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F)
        };
        lstOpponentTeam.Columns.Add("Pokemon", 120);
        lstOpponentTeam.Columns.Add("Type 1", 70);
        lstOpponentTeam.Columns.Add("Type 2", 70);
        lstOpponentTeam.Columns.Add("Role", 100);
        lstOpponentTeam.Columns.Add("Threat Level", 80);

        btnLoadMetaTeam = new Button
        {
            Text = "Load Meta Team",
            Location = new Point(15, 255),
            Size = new Size(150, 30),
            BackColor = Color.FromArgb(90, 60, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnLoadMetaTeam.Click += BtnLoadMetaTeam_Click;

        grpOpponent.Controls.AddRange(new Control[] { lstOpponentTeam, btnLoadMetaTeam });

        // Analyze Button
        btnAnalyze = new Button
        {
            Text = "🔍 ANALYZE MATCHUP",
            Location = new Point(450, 380),
            Size = new Size(250, 50),
            BackColor = Color.FromArgb(80, 180, 80),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 12F),
            FlatStyle = FlatStyle.Flat
        };
        btnAnalyze.Click += BtnAnalyze_Click;

        // Win Rate Display
        var pnlWinRate = new Panel
        {
            Location = new Point(20, 450),
            Size = new Size(1100, 120),
            BackColor = Color.FromArgb(35, 35, 55)
        };

        lblWinRate = new Label
        {
            Text = "Predicted Win Rate: ---%",
            Location = new Point(20, 15),
            Size = new Size(400, 40),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 18F)
        };

        pbOverallScore = new ProgressBar
        {
            Location = new Point(20, 60),
            Size = new Size(500, 30),
            Style = ProgressBarStyle.Continuous,
            Maximum = 100
        };

        lblThreatAnalysis = new Label
        {
            Text = "Key Threats: Analyze to see results",
            Location = new Point(550, 15),
            Size = new Size(530, 90),
            ForeColor = Color.FromArgb(255, 200, 100),
            Font = new Font("Segoe UI", 10F)
        };

        pnlWinRate.Controls.AddRange(new Control[] { lblWinRate, pbOverallScore, lblThreatAnalysis });

        tab.Controls.AddRange(new Control[] { lblFormat, cmbFormat, grpYourTeam, grpOpponent, btnAnalyze, pnlWinRate });
    }

    private void CreateMatrixTab(TabPage tab)
    {
        pnlMatchupMatrix = new Panel
        {
            Location = new Point(20, 20),
            Size = new Size(1100, 550),
            BackColor = Color.FromArgb(35, 35, 55),
            AutoScroll = true
        };

        var lblInfo = new Label
        {
            Text = "Matchup matrix will be generated after analysis. Green = Favorable, Red = Unfavorable, Yellow = Neutral",
            Location = new Point(20, 580),
            Size = new Size(800, 25),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9F)
        };

        tab.Controls.AddRange(new Control[] { pnlMatchupMatrix, lblInfo });
    }

    private void CreateAnalysisTab(TabPage tab)
    {
        rtbDetailedAnalysis = new RichTextBox
        {
            Location = new Point(20, 20),
            Size = new Size(1100, 550),
            BackColor = Color.FromArgb(30, 30, 45),
            ForeColor = Color.White,
            Font = new Font("Consolas", 10F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };

        tab.Controls.Add(rtbDetailedAnalysis);
    }

    private void LoadYourTeamFromSave()
    {
        yourTeam.Clear();
        lstYourTeam.Items.Clear();

        var pokemon = SAV.PartyData.Where(p => p.Species != 0).ToList();

        foreach (var pk in pokemon)
        {
            var data = new PokemonMatchupData
            {
                Name = SpeciesName.GetSpeciesName(pk.Species, 2),
                Type1 = GetTypeName((int)pk.PersonalInfo.Type1),
                Type2 = pk.PersonalInfo.Type2 != pk.PersonalInfo.Type1 ? GetTypeName((int)pk.PersonalInfo.Type2) : "",
                BaseStatTotal = pk.PersonalInfo.GetBaseStatTotal(),
                Role = DetermineRole(pk)
            };
            data.ThreatLevel = CalculateThreatLevel(data);
            yourTeam.Add(data);

            var item = new ListViewItem(data.Name);
            item.SubItems.Add(data.Type1);
            item.SubItems.Add(data.Type2);
            item.SubItems.Add(data.Role);
            item.SubItems.Add(data.ThreatLevel.ToString("F1"));
            lstYourTeam.Items.Add(item);
        }
    }

    private void BtnLoadFromBox_Click(object? sender, EventArgs e)
    {
        LoadYourTeamFromSave();
        MessageBox.Show($"Loaded {yourTeam.Count} Pokemon from your party!", "Team Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnLoadMetaTeam_Click(object? sender, EventArgs e)
    {
        var format = cmbFormat.SelectedItem?.ToString() ?? "VGC 2024";

        if (!MetaTeams.ContainsKey(format))
        {
            MessageBox.Show("No meta teams available for this format.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var teams = MetaTeams[format];
        var random = new Random();
        var selectedTeam = teams[random.Next(teams.Count)];

        opponentTeam.Clear();
        lstOpponentTeam.Items.Clear();

        foreach (var name in selectedTeam)
        {
            var data = new PokemonMatchupData
            {
                Name = name,
                Type1 = GetMetaPokemonType1(name),
                Type2 = GetMetaPokemonType2(name),
                BaseStatTotal = GetMetaPokemonBST(name),
                Role = GetMetaPokemonRole(name)
            };
            data.ThreatLevel = CalculateThreatLevel(data);
            opponentTeam.Add(data);

            var item = new ListViewItem(data.Name);
            item.SubItems.Add(data.Type1);
            item.SubItems.Add(data.Type2);
            item.SubItems.Add(data.Role);
            item.SubItems.Add(data.ThreatLevel.ToString("F1"));
            lstOpponentTeam.Items.Add(item);
        }

        MessageBox.Show($"Loaded meta team: {string.Join(", ", selectedTeam)}", "Meta Team Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnAnalyze_Click(object? sender, EventArgs e)
    {
        if (yourTeam.Count == 0)
        {
            MessageBox.Show("Please load your team first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (opponentTeam.Count == 0)
        {
            MessageBox.Show("Please load an opponent team to analyze against!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        PerformMatchupAnalysis();
    }

    private void PerformMatchupAnalysis()
    {
        // Calculate overall matchup scores
        double totalScore = 0;
        var matchupDetails = new List<string>();
        var threats = new List<string>();
        var advantages = new List<string>();

        // Build matchup matrix
        pnlMatchupMatrix.Controls.Clear();
        int cellWidth = 100;
        int cellHeight = 50;
        int startX = 120;
        int startY = 60;

        // Header - Opponent Pokemon
        for (int j = 0; j < opponentTeam.Count; j++)
        {
            var lbl = new Label
            {
                Text = opponentTeam[j].Name.Length > 10 ? opponentTeam[j].Name[..10] : opponentTeam[j].Name,
                Location = new Point(startX + j * cellWidth, 20),
                Size = new Size(cellWidth - 5, 35),
                ForeColor = Color.FromArgb(255, 150, 100),
                Font = new Font("Segoe UI", 8F),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlMatchupMatrix.Controls.Add(lbl);
        }

        // Rows - Your Pokemon
        for (int i = 0; i < yourTeam.Count; i++)
        {
            var lblRow = new Label
            {
                Text = yourTeam[i].Name.Length > 12 ? yourTeam[i].Name[..12] : yourTeam[i].Name,
                Location = new Point(10, startY + i * cellHeight),
                Size = new Size(105, cellHeight - 5),
                ForeColor = Color.FromArgb(100, 200, 255),
                Font = new Font("Segoe UI", 8F),
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlMatchupMatrix.Controls.Add(lblRow);

            double rowTotal = 0;

            for (int j = 0; j < opponentTeam.Count; j++)
            {
                double matchup = CalculateTypeMatchup(yourTeam[i], opponentTeam[j]);
                rowTotal += matchup;

                Color cellColor;
                if (matchup > 1.2) cellColor = Color.FromArgb(60, 180, 60);
                else if (matchup < 0.8) cellColor = Color.FromArgb(180, 60, 60);
                else cellColor = Color.FromArgb(180, 180, 60);

                var cell = new Panel
                {
                    Location = new Point(startX + j * cellWidth, startY + i * cellHeight),
                    Size = new Size(cellWidth - 5, cellHeight - 5),
                    BackColor = cellColor
                };

                var lblScore = new Label
                {
                    Text = matchup.ToString("F2"),
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Bold", 10F),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                cell.Controls.Add(lblScore);
                pnlMatchupMatrix.Controls.Add(cell);

                if (matchup < 0.5)
                    threats.Add($"{opponentTeam[j].Name} threatens {yourTeam[i].Name}");
                else if (matchup > 1.5)
                    advantages.Add($"{yourTeam[i].Name} counters {opponentTeam[j].Name}");
            }

            totalScore += rowTotal / opponentTeam.Count;
        }

        double avgScore = totalScore / yourTeam.Count;
        double winRate = Math.Min(95, Math.Max(5, 50 + (avgScore - 1) * 40));

        lblWinRate.Text = $"Predicted Win Rate: {winRate:F1}%";
        lblWinRate.ForeColor = winRate >= 50 ? Color.FromArgb(100, 255, 100) : Color.FromArgb(255, 100, 100);
        pbOverallScore.Value = (int)winRate;

        // Threat analysis
        var threatText = "KEY THREATS:\n";
        foreach (var threat in threats.Take(3))
            threatText += $"⚠️ {threat}\n";
        threatText += "\nADVANTAGES:\n";
        foreach (var adv in advantages.Take(3))
            threatText += $"✓ {adv}\n";
        lblThreatAnalysis.Text = threatText;

        // Detailed AI Analysis
        GenerateDetailedAnalysis(winRate, threats, advantages);
    }

    private void GenerateDetailedAnalysis(double winRate, List<string> threats, List<string> advantages)
    {
        rtbDetailedAnalysis.Clear();

        rtbDetailedAnalysis.SelectionColor = Color.FromArgb(100, 200, 255);
        rtbDetailedAnalysis.AppendText("═══════════════════════════════════════════════════════════════\n");
        rtbDetailedAnalysis.AppendText("                    AI MATCHUP ANALYSIS REPORT                   \n");
        rtbDetailedAnalysis.AppendText("═══════════════════════════════════════════════════════════════\n\n");

        rtbDetailedAnalysis.SelectionColor = Color.White;
        rtbDetailedAnalysis.AppendText($"Format: {cmbFormat.SelectedItem}\n");
        rtbDetailedAnalysis.AppendText($"Analysis Date: {DateTime.Now:yyyy-MM-dd HH:mm}\n\n");

        rtbDetailedAnalysis.SelectionColor = winRate >= 50 ? Color.LightGreen : Color.Salmon;
        rtbDetailedAnalysis.AppendText($"OVERALL WIN PROBABILITY: {winRate:F1}%\n\n");

        rtbDetailedAnalysis.SelectionColor = Color.Yellow;
        rtbDetailedAnalysis.AppendText("YOUR TEAM ANALYSIS:\n");
        rtbDetailedAnalysis.AppendText("───────────────────────────────────────────────────────────────\n");
        rtbDetailedAnalysis.SelectionColor = Color.White;

        foreach (var pk in yourTeam)
        {
            rtbDetailedAnalysis.AppendText($"  • {pk.Name} ({pk.Type1}{(string.IsNullOrEmpty(pk.Type2) ? "" : "/" + pk.Type2)})\n");
            rtbDetailedAnalysis.AppendText($"    Role: {pk.Role} | Threat Level: {pk.ThreatLevel:F1}\n");
        }

        rtbDetailedAnalysis.AppendText("\n");
        rtbDetailedAnalysis.SelectionColor = Color.FromArgb(255, 150, 100);
        rtbDetailedAnalysis.AppendText("OPPONENT TEAM ANALYSIS:\n");
        rtbDetailedAnalysis.AppendText("───────────────────────────────────────────────────────────────\n");
        rtbDetailedAnalysis.SelectionColor = Color.White;

        foreach (var pk in opponentTeam)
        {
            rtbDetailedAnalysis.AppendText($"  • {pk.Name} ({pk.Type1}{(string.IsNullOrEmpty(pk.Type2) ? "" : "/" + pk.Type2)})\n");
            rtbDetailedAnalysis.AppendText($"    Role: {pk.Role} | Threat Level: {pk.ThreatLevel:F1}\n");
        }

        rtbDetailedAnalysis.AppendText("\n");
        rtbDetailedAnalysis.SelectionColor = Color.Red;
        rtbDetailedAnalysis.AppendText("⚠️ KEY THREATS TO WATCH:\n");
        rtbDetailedAnalysis.AppendText("───────────────────────────────────────────────────────────────\n");
        rtbDetailedAnalysis.SelectionColor = Color.White;

        foreach (var threat in threats.Distinct().Take(5))
            rtbDetailedAnalysis.AppendText($"  {threat}\n");

        rtbDetailedAnalysis.AppendText("\n");
        rtbDetailedAnalysis.SelectionColor = Color.LightGreen;
        rtbDetailedAnalysis.AppendText("✓ YOUR ADVANTAGES:\n");
        rtbDetailedAnalysis.AppendText("───────────────────────────────────────────────────────────────\n");
        rtbDetailedAnalysis.SelectionColor = Color.White;

        foreach (var adv in advantages.Distinct().Take(5))
            rtbDetailedAnalysis.AppendText($"  {adv}\n");

        rtbDetailedAnalysis.AppendText("\n");
        rtbDetailedAnalysis.SelectionColor = Color.Cyan;
        rtbDetailedAnalysis.AppendText("📋 STRATEGIC RECOMMENDATIONS:\n");
        rtbDetailedAnalysis.AppendText("───────────────────────────────────────────────────────────────\n");
        rtbDetailedAnalysis.SelectionColor = Color.White;

        GenerateStrategicRecommendations();
    }

    private void GenerateStrategicRecommendations()
    {
        var recommendations = new List<string>();

        // Analyze type coverage gaps
        var yourTypes = yourTeam.SelectMany(p => new[] { p.Type1, p.Type2 }).Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();
        var opponentTypes = opponentTeam.SelectMany(p => new[] { p.Type1, p.Type2 }).Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();

        // Check for missing coverage
        foreach (var oppType in opponentTypes)
        {
            bool hasSuperEffective = false;
            foreach (var yourType in yourTypes)
            {
                if (TypeChart.ContainsKey(yourType) && TypeChart[yourType].ContainsKey(oppType) && TypeChart[yourType][oppType] > 1)
                {
                    hasSuperEffective = true;
                    break;
                }
            }
            if (!hasSuperEffective)
                recommendations.Add($"Consider adding coverage for {oppType} types");
        }

        // Lead recommendations
        var bestLead = yourTeam.OrderByDescending(p =>
            opponentTeam.Sum(o => CalculateTypeMatchup(p, o))).FirstOrDefault();
        if (bestLead != null)
            recommendations.Add($"Recommended Lead: {bestLead.Name} has the best overall matchup");

        // Role balance check
        var roles = yourTeam.Select(p => p.Role).ToList();
        if (!roles.Contains("Support"))
            recommendations.Add("Your team lacks dedicated support - consider adding utility Pokemon");
        if (roles.Count(r => r == "Physical Attacker") > 4)
            recommendations.Add("Team is very physically offensive - opponents may stack physical walls");

        foreach (var rec in recommendations.Take(5))
            rtbDetailedAnalysis.AppendText($"  → {rec}\n");
    }

    private double CalculateTypeMatchup(PokemonMatchupData attacker, PokemonMatchupData defender)
    {
        double score = 1.0;

        // Attacker's types vs defender's types
        var attackTypes = new[] { attacker.Type1, attacker.Type2 }.Where(t => !string.IsNullOrEmpty(t));
        var defenseTypes = new[] { defender.Type1, defender.Type2 }.Where(t => !string.IsNullOrEmpty(t));

        double bestOffense = 1.0;
        foreach (var atkType in attackTypes)
        {
            double mult = 1.0;
            foreach (var defType in defenseTypes)
            {
                if (TypeChart.ContainsKey(atkType) && TypeChart[atkType].ContainsKey(defType))
                    mult *= TypeChart[atkType][defType];
            }
            if (mult > bestOffense) bestOffense = mult;
        }

        double worstDefense = 1.0;
        foreach (var oppAtkType in defenseTypes)
        {
            double mult = 1.0;
            foreach (var myDefType in attackTypes)
            {
                if (TypeChart.ContainsKey(oppAtkType) && TypeChart[oppAtkType].ContainsKey(myDefType))
                    mult *= TypeChart[oppAtkType][myDefType];
            }
            if (mult > worstDefense) worstDefense = mult;
        }

        score = (bestOffense / worstDefense);

        // Factor in BST difference
        double bstFactor = (double)attacker.BaseStatTotal / Math.Max(1, defender.BaseStatTotal);
        score *= (0.8 + bstFactor * 0.2);

        return Math.Max(0.1, Math.Min(4.0, score));
    }

    private string DetermineRole(PKM pk)
    {
        int atk = pk.PersonalInfo.ATK;
        int spa = pk.PersonalInfo.SPA;
        int def = pk.PersonalInfo.DEF;
        int spd = pk.PersonalInfo.SPD;
        int spe = pk.PersonalInfo.SPE;

        if (atk > spa && atk > def && atk > spd) return "Physical Attacker";
        if (spa > atk && spa > def && spa > spd) return "Special Attacker";
        if (def > atk && def > spa && def > spe) return "Physical Wall";
        if (spd > atk && spd > spa && spd > spe) return "Special Wall";
        if (spe > atk && spe > spa) return "Speed Control";
        return "Balanced";
    }

    private double CalculateThreatLevel(PokemonMatchupData data)
    {
        double threat = data.BaseStatTotal / 100.0;

        // Boost for offensive types
        if (data.Type1 == "Dragon" || data.Type1 == "Fairy" || data.Type1 == "Steel") threat += 0.5;
        if (data.Role.Contains("Attacker")) threat += 0.3;

        return Math.Min(10.0, threat);
    }

    private string GetTypeName(int typeId) => typeId switch
    {
        0 => "Normal", 1 => "Fighting", 2 => "Flying", 3 => "Poison", 4 => "Ground",
        5 => "Rock", 6 => "Bug", 7 => "Ghost", 8 => "Steel", 9 => "Fire",
        10 => "Water", 11 => "Grass", 12 => "Electric", 13 => "Psychic", 14 => "Ice",
        15 => "Dragon", 16 => "Dark", 17 => "Fairy", _ => ""
    };

    // Meta Pokemon data helpers
    private string GetMetaPokemonType1(string name) => name switch
    {
        "Flutter Mane" => "Ghost", "Landorus" => "Ground", "Rillaboom" => "Grass",
        "Incineroar" => "Fire", "Urshifu" => "Fighting", "Chien-Pao" => "Dark",
        "Koraidon" => "Fighting", "Miraidon" => "Electric", "Calyrex-Shadow" => "Psychic",
        "Zacian" => "Fairy", "Ogerpon" => "Grass", "Ursaluna" => "Ground",
        "Dragapult" => "Dragon", "Gholdengo" => "Steel", "Great Tusk" => "Ground",
        "Kingambit" => "Dark", "Skeledirge" => "Fire", "Zamazenta" => "Fighting",
        "Iron Valiant" => "Fairy", "Gliscor" => "Ground", "Heatran" => "Fire",
        "Iron Hands" => "Fighting", "Palafin" => "Water", "Dondozo" => "Water",
        "Garchomp" => "Dragon", "Dragonite" => "Dragon", "Annihilape" => "Fighting",
        _ => "Normal"
    };

    private string GetMetaPokemonType2(string name) => name switch
    {
        "Flutter Mane" => "Fairy", "Landorus" => "Flying", "Incineroar" => "Dark",
        "Urshifu" => "Dark", "Koraidon" => "Dragon", "Miraidon" => "Dragon",
        "Calyrex-Shadow" => "Ghost", "Zacian" => "Steel", "Ursaluna" => "Normal",
        "Dragapult" => "Ghost", "Gholdengo" => "Ghost", "Great Tusk" => "Fighting",
        "Kingambit" => "Steel", "Skeledirge" => "Ghost", "Zamazenta" => "Steel",
        "Iron Valiant" => "Fighting", "Gliscor" => "Flying", "Heatran" => "Steel",
        "Iron Hands" => "Electric", "Dondozo" => "", "Garchomp" => "Ground",
        "Dragonite" => "Flying", "Annihilape" => "Ghost",
        _ => ""
    };

    private int GetMetaPokemonBST(string name) => name switch
    {
        "Flutter Mane" => 570, "Koraidon" => 670, "Miraidon" => 670,
        "Calyrex-Shadow" => 680, "Zacian" => 670, "Dragapult" => 600,
        "Gholdengo" => 550, "Garchomp" => 600, "Dragonite" => 600,
        _ => 520
    };

    private string GetMetaPokemonRole(string name) => name switch
    {
        "Flutter Mane" => "Special Attacker", "Incineroar" => "Support",
        "Rillaboom" => "Physical Attacker", "Landorus" => "Physical Attacker",
        "Koraidon" => "Physical Attacker", "Miraidon" => "Special Attacker",
        "Dondozo" => "Physical Wall", "Toxapex" => "Special Wall",
        _ => "Balanced"
    };

    private class PokemonMatchupData
    {
        public string Name { get; set; } = "";
        public string Type1 { get; set; } = "";
        public string Type2 { get; set; } = "";
        public int BaseStatTotal { get; set; }
        public string Role { get; set; } = "";
        public double ThreatLevel { get; set; }
    }
}
