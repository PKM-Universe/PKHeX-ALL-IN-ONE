using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public partial class TeamSynergyAnalyzer : Form
{
    private readonly SaveFile SAV;
    private Panel pnlSynergyMatrix = null!;
    private Panel pnlTypeChart = null!;
    private ListView lstTeam = null!;
    private RichTextBox rtbAnalysis = null!;
    private Label lblOverallSynergy = null!;
    private ProgressBar pbSynergyScore = null!;
    private ComboBox cmbFormat = null!;
    private Button btnAnalyze = null!;
    private Button btnSuggestReplacements = null!;

    // Type effectiveness data
    private static readonly string[] AllTypes = { "Normal", "Fire", "Water", "Electric", "Grass", "Ice", "Fighting", "Poison", "Ground", "Flying", "Psychic", "Bug", "Rock", "Ghost", "Dragon", "Dark", "Steel", "Fairy" };

    private static readonly Dictionary<string, HashSet<string>> TypeResistances = new()
    {
        ["Normal"] = new() { },
        ["Fire"] = new() { "Fire", "Grass", "Ice", "Bug", "Steel", "Fairy" },
        ["Water"] = new() { "Fire", "Water", "Ice", "Steel" },
        ["Electric"] = new() { "Electric", "Flying", "Steel" },
        ["Grass"] = new() { "Water", "Electric", "Grass", "Ground" },
        ["Ice"] = new() { "Ice" },
        ["Fighting"] = new() { "Bug", "Rock", "Dark" },
        ["Poison"] = new() { "Grass", "Fighting", "Poison", "Bug", "Fairy" },
        ["Ground"] = new() { "Poison", "Rock" },
        ["Flying"] = new() { "Grass", "Fighting", "Bug" },
        ["Psychic"] = new() { "Fighting", "Psychic" },
        ["Bug"] = new() { "Grass", "Fighting", "Ground" },
        ["Rock"] = new() { "Normal", "Fire", "Poison", "Flying" },
        ["Ghost"] = new() { "Poison", "Bug" },
        ["Dragon"] = new() { "Fire", "Water", "Electric", "Grass" },
        ["Dark"] = new() { "Ghost", "Dark" },
        ["Steel"] = new() { "Normal", "Grass", "Ice", "Flying", "Psychic", "Bug", "Rock", "Dragon", "Steel", "Fairy" },
        ["Fairy"] = new() { "Fighting", "Bug", "Dark" }
    };

    private static readonly Dictionary<string, HashSet<string>> TypeImmunities = new()
    {
        ["Normal"] = new() { "Ghost" },
        ["Ground"] = new() { "Electric" },
        ["Flying"] = new() { "Ground" },
        ["Ghost"] = new() { "Normal", "Fighting" },
        ["Dark"] = new() { "Psychic" },
        ["Steel"] = new() { "Poison" },
        ["Fairy"] = new() { "Dragon" }
    };

    private List<TeamMember> currentTeam = new();

    public TeamSynergyAnalyzer(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
        LoadTeamFromSave();
    }

    private void InitializeComponent()
    {
        Text = "Team Synergy Analyzer - Matrix Builder";
        Size = new Size(1300, 850);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        // Team List
        var grpTeam = new GroupBox
        {
            Text = "Current Team",
            Location = new Point(20, 20),
            Size = new Size(350, 300),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        lstTeam = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(320, 220),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstTeam.Columns.Add("Pokemon", 120);
        lstTeam.Columns.Add("Type 1", 70);
        lstTeam.Columns.Add("Type 2", 70);
        lstTeam.Columns.Add("Role", 50);

        var btnReload = new Button
        {
            Text = "Reload Team",
            Location = new Point(15, 255),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(60, 60, 90),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnReload.Click += (s, e) => LoadTeamFromSave();

        var lblFormat = new Label
        {
            Text = "Format:",
            Location = new Point(130, 260),
            Size = new Size(50, 25),
            ForeColor = Color.White
        };

        cmbFormat = new ComboBox
        {
            Location = new Point(185, 257),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbFormat.Items.AddRange(new[] { "VGC Doubles", "Singles OU", "Battle Stadium", "Anything Goes" });
        cmbFormat.SelectedIndex = 0;

        grpTeam.Controls.AddRange(new Control[] { lstTeam, btnReload, lblFormat, cmbFormat });

        // Synergy Score Display
        var pnlScore = new Panel
        {
            Location = new Point(390, 20),
            Size = new Size(400, 120),
            BackColor = Color.FromArgb(35, 35, 55)
        };

        var lblScoreTitle = new Label
        {
            Text = "TEAM SYNERGY SCORE",
            Location = new Point(15, 10),
            Size = new Size(370, 25),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        lblOverallSynergy = new Label
        {
            Text = "---",
            Location = new Point(15, 35),
            Size = new Size(370, 45),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 28F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        pbSynergyScore = new ProgressBar
        {
            Location = new Point(15, 85),
            Size = new Size(370, 20),
            Maximum = 100,
            Style = ProgressBarStyle.Continuous
        };

        pnlScore.Controls.AddRange(new Control[] { lblScoreTitle, lblOverallSynergy, pbSynergyScore });

        // Action Buttons
        btnAnalyze = new Button
        {
            Text = "🔍 Analyze Synergy",
            Location = new Point(810, 20),
            Size = new Size(180, 50),
            BackColor = Color.FromArgb(60, 120, 180),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 11F),
            FlatStyle = FlatStyle.Flat
        };
        btnAnalyze.Click += BtnAnalyze_Click;

        btnSuggestReplacements = new Button
        {
            Text = "💡 Suggest Improvements",
            Location = new Point(1010, 20),
            Size = new Size(180, 50),
            BackColor = Color.FromArgb(180, 120, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 11F),
            FlatStyle = FlatStyle.Flat
        };
        btnSuggestReplacements.Click += BtnSuggestReplacements_Click;

        // Type Coverage Chart
        var grpTypeCoverage = new GroupBox
        {
            Text = "Defensive Type Coverage",
            Location = new Point(390, 150),
            Size = new Size(500, 170),
            ForeColor = Color.FromArgb(255, 200, 100)
        };

        pnlTypeChart = new Panel
        {
            Location = new Point(15, 25),
            Size = new Size(470, 130),
            BackColor = Color.FromArgb(30, 30, 45)
        };
        pnlTypeChart.Paint += PnlTypeChart_Paint;

        grpTypeCoverage.Controls.Add(pnlTypeChart);

        // Synergy Matrix
        var grpMatrix = new GroupBox
        {
            Text = "Pokemon Synergy Matrix",
            Location = new Point(20, 330),
            Size = new Size(870, 460),
            ForeColor = Color.FromArgb(100, 255, 150)
        };

        pnlSynergyMatrix = new Panel
        {
            Location = new Point(15, 25),
            Size = new Size(840, 420),
            BackColor = Color.FromArgb(30, 30, 45),
            AutoScroll = true
        };

        grpMatrix.Controls.Add(pnlSynergyMatrix);

        // Analysis Panel
        var grpAnalysis = new GroupBox
        {
            Text = "Synergy Analysis",
            Location = new Point(910, 150),
            Size = new Size(350, 640),
            ForeColor = Color.FromArgb(200, 200, 200)
        };

        rtbAnalysis = new RichTextBox
        {
            Location = new Point(15, 25),
            Size = new Size(320, 600),
            BackColor = Color.FromArgb(25, 25, 40),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };

        grpAnalysis.Controls.Add(rtbAnalysis);

        Controls.AddRange(new Control[] { grpTeam, pnlScore, btnAnalyze, btnSuggestReplacements, grpTypeCoverage, grpMatrix, grpAnalysis });
    }

    private void LoadTeamFromSave()
    {
        currentTeam.Clear();
        lstTeam.Items.Clear();

        var party = SAV.PartyData.Where(p => p.Species != 0).ToList();

        foreach (var pk in party)
        {
            var member = new TeamMember
            {
                Name = SpeciesName.GetSpeciesName(pk.Species, 2),
                Species = pk.Species,
                Type1 = GetTypeName((int)pk.PersonalInfo.Type1),
                Type2 = pk.PersonalInfo.Type2 != pk.PersonalInfo.Type1 ? GetTypeName((int)pk.PersonalInfo.Type2) : "",
                BST = pk.PersonalInfo.GetBaseStatTotal(),
                Role = DetermineRole(pk)
            };
            currentTeam.Add(member);

            var item = new ListViewItem(member.Name);
            item.SubItems.Add(member.Type1);
            item.SubItems.Add(member.Type2);
            item.SubItems.Add(member.Role[0].ToString());
            lstTeam.Items.Add(item);
        }

        pnlTypeChart.Invalidate();
    }

    private void BtnAnalyze_Click(object? sender, EventArgs e)
    {
        if (currentTeam.Count == 0)
        {
            MessageBox.Show("No team loaded!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        PerformSynergyAnalysis();
        DrawSynergyMatrix();
    }

    private void PerformSynergyAnalysis()
    {
        rtbAnalysis.Clear();
        double totalScore = 0;

        // Header
        rtbAnalysis.SelectionColor = Color.Cyan;
        rtbAnalysis.AppendText("═══ TEAM SYNERGY ANALYSIS ═══\n\n");

        // 1. Defensive Coverage Analysis
        rtbAnalysis.SelectionColor = Color.Yellow;
        rtbAnalysis.AppendText("📊 DEFENSIVE COVERAGE:\n");
        rtbAnalysis.SelectionColor = Color.White;

        var defensiveCoverage = CalculateDefensiveCoverage();
        var weaknesses = defensiveCoverage.Where(kv => kv.Value < 0).OrderBy(kv => kv.Value).ToList();
        var resistances = defensiveCoverage.Where(kv => kv.Value > 0).OrderByDescending(kv => kv.Value).ToList();

        rtbAnalysis.SelectionColor = Color.Red;
        rtbAnalysis.AppendText($"  Weak to: {weaknesses.Count} types\n");
        foreach (var w in weaknesses.Take(3))
            rtbAnalysis.AppendText($"    ⚠ {w.Key} ({w.Value:+0;-0})\n");

        rtbAnalysis.SelectionColor = Color.LightGreen;
        rtbAnalysis.AppendText($"  Resistant to: {resistances.Count} types\n");
        foreach (var r in resistances.Take(3))
            rtbAnalysis.AppendText($"    ✓ {r.Key} ({r.Value:+0;-0})\n");

        double coverageScore = (18 - weaknesses.Count) / 18.0 * 30;
        totalScore += coverageScore;

        // 2. Role Balance Analysis
        rtbAnalysis.AppendText("\n");
        rtbAnalysis.SelectionColor = Color.Yellow;
        rtbAnalysis.AppendText("⚔️ ROLE BALANCE:\n");
        rtbAnalysis.SelectionColor = Color.White;

        var roleCounts = currentTeam.GroupBy(m => m.Role).ToDictionary(g => g.Key, g => g.Count());
        foreach (var role in roleCounts)
            rtbAnalysis.AppendText($"  {role.Key}: {role.Value}\n");

        // Check for role diversity
        double roleScore = Math.Min(25, roleCounts.Count * 5);
        if (!roleCounts.ContainsKey("Support")) roleScore -= 5;
        if (roleCounts.GetValueOrDefault("Physical Attacker", 0) > 3) roleScore -= 5;
        totalScore += Math.Max(0, roleScore);

        // 3. Type Synergy (switch-in compatibility)
        rtbAnalysis.AppendText("\n");
        rtbAnalysis.SelectionColor = Color.Yellow;
        rtbAnalysis.AppendText("🔄 SWITCH-IN SYNERGY:\n");
        rtbAnalysis.SelectionColor = Color.White;

        var synergyPairs = CalculateSwitchInSynergy();
        foreach (var pair in synergyPairs.Take(3))
        {
            rtbAnalysis.SelectionColor = Color.LightGreen;
            rtbAnalysis.AppendText($"  ✓ {pair.Pokemon1} ↔ {pair.Pokemon2}\n");
            rtbAnalysis.SelectionColor = Color.Gray;
            rtbAnalysis.AppendText($"    {pair.Reason}\n");
        }

        double synergyScore = Math.Min(25, synergyPairs.Count * 5);
        totalScore += synergyScore;

        // 4. Ability Synergy Check
        rtbAnalysis.AppendText("\n");
        rtbAnalysis.SelectionColor = Color.Yellow;
        rtbAnalysis.AppendText("✨ ABILITY SYNERGY:\n");
        rtbAnalysis.SelectionColor = Color.White;

        var abilitySynergies = CheckAbilitySynergy();
        if (abilitySynergies.Count > 0)
        {
            foreach (var syn in abilitySynergies)
                rtbAnalysis.AppendText($"  ✓ {syn}\n");
            totalScore += abilitySynergies.Count * 5;
        }
        else
        {
            rtbAnalysis.SelectionColor = Color.Gray;
            rtbAnalysis.AppendText("  No notable ability synergies detected\n");
        }

        // 5. Speed Tier Analysis
        rtbAnalysis.AppendText("\n");
        rtbAnalysis.SelectionColor = Color.Yellow;
        rtbAnalysis.AppendText("⚡ SPEED CONTROL:\n");
        rtbAnalysis.SelectionColor = Color.White;

        var fastPokemon = currentTeam.Where(m => m.BST > 500).Count();
        rtbAnalysis.AppendText($"  Fast threats: {fastPokemon}/6\n");

        if (fastPokemon >= 2 && fastPokemon <= 4)
        {
            rtbAnalysis.AppendText("  ✓ Good speed balance\n");
            totalScore += 10;
        }
        else
        {
            rtbAnalysis.AppendText("  ⚠ Consider adjusting speed balance\n");
            totalScore += 5;
        }

        // Final Score
        totalScore = Math.Min(100, totalScore);
        lblOverallSynergy.Text = $"{totalScore:F0}/100";
        lblOverallSynergy.ForeColor = totalScore >= 70 ? Color.LightGreen : totalScore >= 50 ? Color.Yellow : Color.Salmon;
        pbSynergyScore.Value = (int)totalScore;

        rtbAnalysis.AppendText("\n");
        rtbAnalysis.SelectionColor = Color.Cyan;
        rtbAnalysis.AppendText($"═══ OVERALL SCORE: {totalScore:F0}/100 ═══\n");

        // Rating
        string rating = totalScore switch
        {
            >= 85 => "S-Tier: Exceptional Synergy!",
            >= 70 => "A-Tier: Great Synergy",
            >= 55 => "B-Tier: Good Synergy",
            >= 40 => "C-Tier: Needs Work",
            _ => "D-Tier: Major Gaps"
        };
        rtbAnalysis.SelectionColor = Color.Gold;
        rtbAnalysis.AppendText($"\nRating: {rating}\n");
    }

    private Dictionary<string, int> CalculateDefensiveCoverage()
    {
        var coverage = AllTypes.ToDictionary(t => t, t => 0);

        foreach (var member in currentTeam)
        {
            var types = new[] { member.Type1, member.Type2 }.Where(t => !string.IsNullOrEmpty(t));

            foreach (var defType in types)
            {
                // Add resistances
                if (TypeResistances.ContainsKey(defType))
                {
                    foreach (var resist in TypeResistances[defType])
                        coverage[resist]++;
                }

                // Add immunities (count as 2 resistances)
                if (TypeImmunities.ContainsKey(defType))
                {
                    foreach (var immune in TypeImmunities[defType])
                        coverage[immune] += 2;
                }
            }

            // Subtract weaknesses (simplified)
            foreach (var attackType in AllTypes)
            {
                foreach (var defType in types)
                {
                    if (IsWeakTo(defType, attackType))
                        coverage[attackType]--;
                }
            }
        }

        return coverage;
    }

    private bool IsWeakTo(string defType, string atkType)
    {
        // Simplified weakness chart
        return (defType, atkType) switch
        {
            ("Normal", "Fighting") => true,
            ("Fire", "Water") or ("Fire", "Ground") or ("Fire", "Rock") => true,
            ("Water", "Electric") or ("Water", "Grass") => true,
            ("Grass", "Fire") or ("Grass", "Ice") or ("Grass", "Poison") or ("Grass", "Flying") or ("Grass", "Bug") => true,
            ("Electric", "Ground") => true,
            ("Ice", "Fire") or ("Ice", "Fighting") or ("Ice", "Rock") or ("Ice", "Steel") => true,
            ("Fighting", "Flying") or ("Fighting", "Psychic") or ("Fighting", "Fairy") => true,
            ("Poison", "Ground") or ("Poison", "Psychic") => true,
            ("Ground", "Water") or ("Ground", "Grass") or ("Ground", "Ice") => true,
            ("Flying", "Electric") or ("Flying", "Ice") or ("Flying", "Rock") => true,
            ("Psychic", "Bug") or ("Psychic", "Ghost") or ("Psychic", "Dark") => true,
            ("Bug", "Fire") or ("Bug", "Flying") or ("Bug", "Rock") => true,
            ("Rock", "Water") or ("Rock", "Grass") or ("Rock", "Fighting") or ("Rock", "Ground") or ("Rock", "Steel") => true,
            ("Ghost", "Ghost") or ("Ghost", "Dark") => true,
            ("Dragon", "Ice") or ("Dragon", "Dragon") or ("Dragon", "Fairy") => true,
            ("Dark", "Fighting") or ("Dark", "Bug") or ("Dark", "Fairy") => true,
            ("Steel", "Fire") or ("Steel", "Fighting") or ("Steel", "Ground") => true,
            ("Fairy", "Poison") or ("Fairy", "Steel") => true,
            _ => false
        };
    }

    private List<SynergyPair> CalculateSwitchInSynergy()
    {
        var pairs = new List<SynergyPair>();

        for (int i = 0; i < currentTeam.Count; i++)
        {
            for (int j = i + 1; j < currentTeam.Count; j++)
            {
                var pk1 = currentTeam[i];
                var pk2 = currentTeam[j];

                // Check if one resists the other's weaknesses
                var types1 = new[] { pk1.Type1, pk1.Type2 }.Where(t => !string.IsNullOrEmpty(t)).ToList();
                var types2 = new[] { pk2.Type1, pk2.Type2 }.Where(t => !string.IsNullOrEmpty(t)).ToList();

                foreach (var type in AllTypes)
                {
                    bool pk1WeakTo = types1.Any(t => IsWeakTo(t, type));
                    bool pk2ResistsOrImmune = types2.Any(t =>
                        (TypeResistances.ContainsKey(t) && TypeResistances[t].Contains(type)) ||
                        (TypeImmunities.ContainsKey(t) && TypeImmunities[t].Contains(type)));

                    if (pk1WeakTo && pk2ResistsOrImmune)
                    {
                        pairs.Add(new SynergyPair
                        {
                            Pokemon1 = pk1.Name,
                            Pokemon2 = pk2.Name,
                            Reason = $"{pk2.Name} covers {pk1.Name}'s {type} weakness"
                        });
                        break;
                    }
                }
            }
        }

        return pairs.DistinctBy(p => $"{p.Pokemon1}-{p.Pokemon2}").ToList();
    }

    private List<string> CheckAbilitySynergy()
    {
        var synergies = new List<string>();

        // Check for common VGC synergies
        var types = currentTeam.SelectMany(m => new[] { m.Type1, m.Type2 }).Where(t => !string.IsNullOrEmpty(t)).Distinct();

        if (types.Contains("Fire") && types.Contains("Grass"))
            synergies.Add("Sun Team Potential (Fire + Grass)");
        if (types.Contains("Water") && types.Contains("Electric"))
            synergies.Add("Rain Team Potential (Water + Electric)");
        if (types.Contains("Ice") && types.Contains("Steel"))
            synergies.Add("Hail Team Potential (Ice + Steel)");
        if (currentTeam.Any(m => m.Role == "Support") && currentTeam.Any(m => m.Role.Contains("Attacker")))
            synergies.Add("Support + Attacker Balance");

        return synergies;
    }

    private void DrawSynergyMatrix()
    {
        pnlSynergyMatrix.Controls.Clear();

        int cellSize = 100;
        int padding = 10;

        // Headers
        for (int i = 0; i < currentTeam.Count; i++)
        {
            var lblCol = new Label
            {
                Text = currentTeam[i].Name.Length > 10 ? currentTeam[i].Name[..10] : currentTeam[i].Name,
                Location = new Point(120 + i * cellSize, 10),
                Size = new Size(cellSize - 5, 30),
                ForeColor = Color.FromArgb(100, 200, 255),
                Font = new Font("Segoe UI", 8F),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlSynergyMatrix.Controls.Add(lblCol);
        }

        // Matrix cells
        for (int i = 0; i < currentTeam.Count; i++)
        {
            var lblRow = new Label
            {
                Text = currentTeam[i].Name.Length > 12 ? currentTeam[i].Name[..12] : currentTeam[i].Name,
                Location = new Point(10, 50 + i * cellSize),
                Size = new Size(105, cellSize - 5),
                ForeColor = Color.FromArgb(100, 200, 255),
                Font = new Font("Segoe UI", 8F),
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlSynergyMatrix.Controls.Add(lblRow);

            for (int j = 0; j < currentTeam.Count; j++)
            {
                double synergy = CalculatePairSynergy(currentTeam[i], currentTeam[j]);

                Color cellColor;
                if (i == j) cellColor = Color.FromArgb(50, 50, 70);
                else if (synergy >= 0.7) cellColor = Color.FromArgb(60, 150, 60);
                else if (synergy >= 0.4) cellColor = Color.FromArgb(150, 150, 60);
                else cellColor = Color.FromArgb(150, 60, 60);

                var cell = new Panel
                {
                    Location = new Point(120 + j * cellSize, 50 + i * cellSize),
                    Size = new Size(cellSize - 5, cellSize - 5),
                    BackColor = cellColor
                };

                if (i != j)
                {
                    var lblScore = new Label
                    {
                        Text = $"{synergy * 100:F0}%",
                        Dock = DockStyle.Fill,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI Bold", 12F),
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    var lblDesc = new Label
                    {
                        Text = GetSynergyDescription(synergy),
                        Location = new Point(5, 60),
                        Size = new Size(cellSize - 15, 25),
                        ForeColor = Color.Gray,
                        Font = new Font("Segoe UI", 7F),
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    cell.Controls.Add(lblScore);
                    cell.Controls.Add(lblDesc);
                }

                pnlSynergyMatrix.Controls.Add(cell);
            }
        }
    }

    private double CalculatePairSynergy(TeamMember pk1, TeamMember pk2)
    {
        double synergy = 0.5;

        var types1 = new[] { pk1.Type1, pk1.Type2 }.Where(t => !string.IsNullOrEmpty(t)).ToList();
        var types2 = new[] { pk2.Type1, pk2.Type2 }.Where(t => !string.IsNullOrEmpty(t)).ToList();

        // Check if pk2 resists pk1's weaknesses
        foreach (var type in AllTypes)
        {
            bool pk1WeakTo = types1.Any(t => IsWeakTo(t, type));
            bool pk2Resists = types2.Any(t => TypeResistances.ContainsKey(t) && TypeResistances[t].Contains(type));
            bool pk2Immune = types2.Any(t => TypeImmunities.ContainsKey(t) && TypeImmunities[t].Contains(type));

            if (pk1WeakTo && (pk2Resists || pk2Immune))
                synergy += 0.1;
        }

        // Role synergy bonus
        if ((pk1.Role.Contains("Attacker") && pk2.Role == "Support") ||
            (pk2.Role.Contains("Attacker") && pk1.Role == "Support"))
            synergy += 0.15;

        return Math.Min(1.0, synergy);
    }

    private string GetSynergyDescription(double synergy) => synergy switch
    {
        >= 0.8 => "Excellent",
        >= 0.6 => "Good",
        >= 0.4 => "Fair",
        _ => "Poor"
    };

    private void BtnSuggestReplacements_Click(object? sender, EventArgs e)
    {
        if (currentTeam.Count == 0)
        {
            MessageBox.Show("Load a team first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        rtbAnalysis.Clear();
        rtbAnalysis.SelectionColor = Color.Gold;
        rtbAnalysis.AppendText("═══ IMPROVEMENT SUGGESTIONS ═══\n\n");

        var coverage = CalculateDefensiveCoverage();
        var weakTypes = coverage.Where(kv => kv.Value < -1).Select(kv => kv.Key).ToList();

        if (weakTypes.Count > 0)
        {
            rtbAnalysis.SelectionColor = Color.Yellow;
            rtbAnalysis.AppendText("RECOMMENDED ADDITIONS:\n");
            rtbAnalysis.SelectionColor = Color.White;

            foreach (var weakType in weakTypes.Take(3))
            {
                var counters = GetCountersForType(weakType);
                rtbAnalysis.AppendText($"\n  To cover {weakType} weakness:\n");
                foreach (var counter in counters.Take(3))
                    rtbAnalysis.AppendText($"    → {counter}\n");
            }
        }

        // Suggest based on role balance
        var roles = currentTeam.Select(m => m.Role).ToList();
        if (!roles.Any(r => r == "Support"))
        {
            rtbAnalysis.AppendText("\n  ⚠ Consider adding support Pokemon:\n");
            rtbAnalysis.AppendText("    → Incineroar (Intimidate)\n");
            rtbAnalysis.AppendText("    → Amoonguss (Rage Powder)\n");
            rtbAnalysis.AppendText("    → Grimmsnarl (Screens)\n");
        }
    }

    private List<string> GetCountersForType(string attackType)
    {
        // Return Pokemon that resist/are immune to the type
        var counters = new Dictionary<string, List<string>>
        {
            ["Fighting"] = new() { "Dragapult (Ghost)", "Corviknight (Flying)", "Toxapex (Poison)" },
            ["Ground"] = new() { "Landorus (Flying)", "Corviknight (Flying)", "Hydreigon (Levitate)" },
            ["Fire"] = new() { "Heatran (Flash Fire)", "Volcanion (Water)", "Dragonite (Multiscale)" },
            ["Ice"] = new() { "Heatran (Steel/Fire)", "Volcarona (Fire)", "Skeledirge (Fire)" },
            ["Electric"] = new() { "Garchomp (Ground)", "Great Tusk (Ground)", "Landorus (Ground)" },
            ["Fairy"] = new() { "Gholdengo (Steel)", "Heatran (Steel)", "Excadrill (Steel)" },
            ["Dark"] = new() { "Kingambit (Dark/Steel)", "Grimmsnarl (Fairy)", "Iron Valiant (Fairy)" }
        };

        return counters.GetValueOrDefault(attackType, new() { "Steel-type Pokemon", "Type-resistant wall", "Immunity Pokemon" });
    }

    private void PnlTypeChart_Paint(object? sender, PaintEventArgs e)
    {
        if (currentTeam.Count == 0) return;

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var coverage = CalculateDefensiveCoverage();
        int barWidth = 24;
        int maxHeight = 100;
        int x = 10;

        foreach (var type in AllTypes)
        {
            int value = coverage[type];
            int height = Math.Min(maxHeight, Math.Abs(value) * 15 + 5);

            Color barColor = value > 0 ? Color.FromArgb(100, 200, 100) : value < 0 ? Color.FromArgb(200, 100, 100) : Color.Gray;

            int y = value >= 0 ? 65 - height : 65;
            g.FillRectangle(new SolidBrush(barColor), x, y, barWidth - 2, height);

            // Type label
            g.DrawString(type[..2], new Font("Segoe UI", 7F), Brushes.White, x, 110);

            x += barWidth;
        }

        // Draw baseline
        g.DrawLine(Pens.Gray, 10, 65, pnlTypeChart.Width - 10, 65);
    }

    private string DetermineRole(PKM pk)
    {
        int atk = pk.PersonalInfo.ATK;
        int spa = pk.PersonalInfo.SPA;
        int def = pk.PersonalInfo.DEF;
        int spd = pk.PersonalInfo.SPD;

        if (atk >= 100 && atk > spa) return "Physical Attacker";
        if (spa >= 100 && spa > atk) return "Special Attacker";
        if (def >= 100 || spd >= 100) return "Wall";
        return "Support";
    }

    private string GetTypeName(int typeId) => typeId switch
    {
        0 => "Normal", 1 => "Fighting", 2 => "Flying", 3 => "Poison", 4 => "Ground",
        5 => "Rock", 6 => "Bug", 7 => "Ghost", 8 => "Steel", 9 => "Fire",
        10 => "Water", 11 => "Grass", 12 => "Electric", 13 => "Psychic", 14 => "Ice",
        15 => "Dragon", 16 => "Dark", 17 => "Fairy", _ => "Normal"
    };

    private class TeamMember
    {
        public string Name { get; set; } = "";
        public ushort Species { get; set; }
        public string Type1 { get; set; } = "";
        public string Type2 { get; set; } = "";
        public int BST { get; set; }
        public string Role { get; set; } = "";
    }

    private class SynergyPair
    {
        public string Pokemon1 { get; set; } = "";
        public string Pokemon2 { get; set; } = "";
        public string Reason { get; set; } = "";
    }
}
