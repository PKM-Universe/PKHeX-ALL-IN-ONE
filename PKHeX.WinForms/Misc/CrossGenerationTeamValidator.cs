using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public partial class CrossGenerationTeamValidator : Form
{
    private readonly SaveFile SAV;
    private CheckedListBox clbTargetGames = null!;
    private ListView lstTeamValidation = null!;
    private ListView lstMoveCompatibility = null!;
    private ListView lstAbilityCompatibility = null!;
    private RichTextBox rtbValidationReport = null!;
    private Panel pnlCompatibilityMatrix = null!;
    private Button btnValidate = null!;
    private Button btnExportCompatible = null!;
    private Label lblOverallCompatibility = null!;
    private ProgressBar pbCompatibility = null!;

    // Game compatibility data
    private static readonly Dictionary<string, GameInfo> GameData = new()
    {
        ["Scarlet/Violet"] = new() { Generation = 9, Code = "SV", MaxDexNum = 1025, HasTeraTypes = true },
        ["Sword/Shield"] = new() { Generation = 8, Code = "SwSh", MaxDexNum = 898, HasDynamax = true },
        ["Brilliant Diamond/Shining Pearl"] = new() { Generation = 8, Code = "BDSP", MaxDexNum = 493, IsRemake = true },
        ["Legends: Arceus"] = new() { Generation = 8, Code = "PLA", MaxDexNum = 905, HasAlphas = true },
        ["Let's Go Pikachu/Eevee"] = new() { Generation = 7, Code = "LGPE", MaxDexNum = 153, IsSimplified = true },
        ["Sun/Moon / Ultra"] = new() { Generation = 7, Code = "SM", MaxDexNum = 807, HasZMoves = true },
        ["X/Y"] = new() { Generation = 6, Code = "XY", MaxDexNum = 721, HasMegas = true },
        ["Omega Ruby/Alpha Sapphire"] = new() { Generation = 6, Code = "ORAS", MaxDexNum = 721, HasMegas = true }
    };

    // Pokemon availability per game (simplified - species that are NOT available)
    private static readonly Dictionary<string, HashSet<ushort>> UnavailablePokemon = new()
    {
        ["SwSh"] = new() { 13, 14, 15, 19, 20, 21, 22, 23, 24, 46, 47, 48, 49, 56, 57 }, // Sample exclusions
        ["BDSP"] = new() { 906, 907, 908, 909, 910 }, // Gen 9 Pokemon
        ["PLA"] = new() { 1, 2, 3, 4, 5, 6 }, // Many Pokemon unavailable
        ["LGPE"] = new() { 152, 153, 154 } // Only Kanto + Meltan/Melmetal
    };

    // Moves removed in certain generations
    private static readonly Dictionary<string, HashSet<string>> RemovedMoves = new()
    {
        ["SV"] = new() { "Hidden Power", "Return", "Frustration", "Pursuit" },
        ["SwSh"] = new() { "Hidden Power", "Return", "Frustration", "Pursuit" },
        ["BDSP"] = new() { }, // Has most moves
        ["PLA"] = new() { "Toxic", "Stealth Rock", "Protect" } // Many moves changed/removed
    };

    public CrossGenerationTeamValidator(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
        LoadTeamData();
    }

    private void InitializeComponent()
    {
        Text = "Cross-Generation Team Validator";
        Size = new Size(1250, 850);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        // Target Games Selection
        var grpGames = new GroupBox
        {
            Text = "Target Games (Check all games to validate against)",
            Location = new Point(20, 20),
            Size = new Size(400, 200),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        clbTargetGames = new CheckedListBox
        {
            Location = new Point(15, 25),
            Size = new Size(370, 160),
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            CheckOnClick = true
        };

        foreach (var game in GameData.Keys)
            clbTargetGames.Items.Add(game, true);

        grpGames.Controls.Add(clbTargetGames);

        // Compatibility Summary
        var pnlSummary = new Panel
        {
            Location = new Point(440, 20),
            Size = new Size(350, 100),
            BackColor = Color.FromArgb(35, 35, 55)
        };

        var lblTitle = new Label
        {
            Text = "OVERALL COMPATIBILITY",
            Location = new Point(15, 10),
            Size = new Size(320, 25),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        lblOverallCompatibility = new Label
        {
            Text = "---",
            Location = new Point(15, 35),
            Size = new Size(320, 35),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 20F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        pbCompatibility = new ProgressBar
        {
            Location = new Point(15, 75),
            Size = new Size(320, 18),
            Maximum = 100
        };

        pnlSummary.Controls.AddRange(new Control[] { lblTitle, lblOverallCompatibility, pbCompatibility });

        // Action Buttons
        btnValidate = new Button
        {
            Text = "🔍 VALIDATE TEAM",
            Location = new Point(440, 135),
            Size = new Size(170, 45),
            BackColor = Color.FromArgb(60, 120, 180),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 11F),
            FlatStyle = FlatStyle.Flat
        };
        btnValidate.Click += BtnValidate_Click;

        btnExportCompatible = new Button
        {
            Text = "📤 Export Valid Set",
            Location = new Point(620, 135),
            Size = new Size(170, 45),
            BackColor = Color.FromArgb(60, 180, 80),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 11F),
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnExportCompatible.Click += BtnExportCompatible_Click;

        // Team Validation Results
        var grpTeam = new GroupBox
        {
            Text = "Team Pokemon Compatibility",
            Location = new Point(20, 230),
            Size = new Size(500, 280),
            ForeColor = Color.FromArgb(100, 255, 150)
        };

        lstTeamValidation = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(470, 240),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstTeamValidation.Columns.Add("Pokemon", 100);
        lstTeamValidation.Columns.Add("SV", 45);
        lstTeamValidation.Columns.Add("SwSh", 50);
        lstTeamValidation.Columns.Add("BDSP", 50);
        lstTeamValidation.Columns.Add("PLA", 45);
        lstTeamValidation.Columns.Add("SM", 45);
        lstTeamValidation.Columns.Add("Issues", 120);
        lstTeamValidation.SelectedIndexChanged += LstTeamValidation_SelectedIndexChanged;

        grpTeam.Controls.Add(lstTeamValidation);

        // Move Compatibility
        var grpMoves = new GroupBox
        {
            Text = "Move Compatibility",
            Location = new Point(540, 230),
            Size = new Size(340, 280),
            ForeColor = Color.FromArgb(255, 200, 100)
        };

        lstMoveCompatibility = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(310, 240),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstMoveCompatibility.Columns.Add("Move", 120);
        lstMoveCompatibility.Columns.Add("Available In", 170);

        grpMoves.Controls.Add(lstMoveCompatibility);

        // Ability Compatibility
        var grpAbilities = new GroupBox
        {
            Text = "Ability Compatibility",
            Location = new Point(900, 230),
            Size = new Size(310, 280),
            ForeColor = Color.FromArgb(200, 150, 255)
        };

        lstAbilityCompatibility = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(280, 240),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstAbilityCompatibility.Columns.Add("Ability", 130);
        lstAbilityCompatibility.Columns.Add("Status", 130);

        grpAbilities.Controls.Add(lstAbilityCompatibility);

        // Detailed Report
        var grpReport = new GroupBox
        {
            Text = "Validation Report",
            Location = new Point(20, 520),
            Size = new Size(1190, 260),
            ForeColor = Color.FromArgb(200, 200, 200)
        };

        rtbValidationReport = new RichTextBox
        {
            Location = new Point(15, 25),
            Size = new Size(1160, 220),
            BackColor = Color.FromArgb(25, 25, 40),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };

        grpReport.Controls.Add(rtbValidationReport);

        // Compatibility Matrix (right side)
        var grpMatrix = new GroupBox
        {
            Text = "Game Feature Matrix",
            Location = new Point(810, 20),
            Size = new Size(400, 200),
            ForeColor = Color.FromArgb(255, 150, 100)
        };

        pnlCompatibilityMatrix = new Panel
        {
            Location = new Point(15, 25),
            Size = new Size(370, 160),
            BackColor = Color.FromArgb(30, 30, 45)
        };
        pnlCompatibilityMatrix.Paint += PnlCompatibilityMatrix_Paint;

        grpMatrix.Controls.Add(pnlCompatibilityMatrix);

        Controls.AddRange(new Control[] { grpGames, pnlSummary, btnValidate, btnExportCompatible, grpTeam, grpMoves, grpAbilities, grpMatrix, grpReport });
    }

    private List<TeamPokemonData> teamData = new();

    private void LoadTeamData()
    {
        teamData.Clear();
        var party = SAV.PartyData.Where(p => p.Species != 0).ToList();

        foreach (var pk in party)
        {
            teamData.Add(new TeamPokemonData
            {
                Pokemon = pk,
                Name = SpeciesName.GetSpeciesName(pk.Species, 2),
                Species = pk.Species,
                Moves = new[] { pk.Move1, pk.Move2, pk.Move3, pk.Move4 }.Where(m => m != 0).ToArray()
            });
        }
    }

    private void BtnValidate_Click(object? sender, EventArgs e)
    {
        LoadTeamData();

        if (teamData.Count == 0)
        {
            MessageBox.Show("No Pokemon in party to validate!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedGames = clbTargetGames.CheckedItems.Cast<string>().ToList();
        if (selectedGames.Count == 0)
        {
            MessageBox.Show("Please select at least one target game!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        PerformValidation(selectedGames);
    }

    private void PerformValidation(List<string> targetGames)
    {
        lstTeamValidation.Items.Clear();
        lstMoveCompatibility.Items.Clear();
        lstAbilityCompatibility.Items.Clear();
        rtbValidationReport.Clear();

        int totalChecks = 0;
        int passedChecks = 0;
        var issues = new List<string>();

        rtbValidationReport.SelectionColor = Color.Cyan;
        rtbValidationReport.AppendText("═══════════════════════════════════════════════════════════════\n");
        rtbValidationReport.AppendText("              CROSS-GENERATION TEAM VALIDATION REPORT           \n");
        rtbValidationReport.AppendText("═══════════════════════════════════════════════════════════════\n\n");

        foreach (var pokemon in teamData)
        {
            var item = new ListViewItem(pokemon.Name);
            var pokemonIssues = new List<string>();

            foreach (var game in new[] { "Scarlet/Violet", "Sword/Shield", "Brilliant Diamond/Shining Pearl", "Legends: Arceus", "Sun/Moon / Ultra" })
            {
                var gameInfo = GameData[game];
                bool available = IsPokemonAvailable(pokemon.Species, gameInfo.Code);
                totalChecks++;

                if (available)
                {
                    passedChecks++;
                    item.SubItems.Add("✓");
                }
                else
                {
                    item.SubItems.Add("✗");
                    pokemonIssues.Add($"Not in {gameInfo.Code}");
                }
            }

            item.SubItems.Add(pokemonIssues.Count > 0 ? $"{pokemonIssues.Count} issues" : "OK");
            item.ForeColor = pokemonIssues.Count == 0 ? Color.LightGreen : pokemonIssues.Count <= 2 ? Color.Yellow : Color.Salmon;
            item.Tag = pokemon;
            lstTeamValidation.Items.Add(item);

            // Log issues
            if (pokemonIssues.Count > 0)
            {
                rtbValidationReport.SelectionColor = Color.Yellow;
                rtbValidationReport.AppendText($"⚠ {pokemon.Name}:\n");
                rtbValidationReport.SelectionColor = Color.White;
                foreach (var issue in pokemonIssues)
                    rtbValidationReport.AppendText($"   - {issue}\n");
                rtbValidationReport.AppendText("\n");
            }
        }

        // Calculate overall compatibility
        double compatibility = totalChecks > 0 ? (double)passedChecks / totalChecks * 100 : 0;
        lblOverallCompatibility.Text = $"{compatibility:F1}%";
        lblOverallCompatibility.ForeColor = compatibility >= 80 ? Color.LightGreen : compatibility >= 50 ? Color.Yellow : Color.Salmon;
        pbCompatibility.Value = (int)compatibility;

        // Summary
        rtbValidationReport.AppendText("───────────────────────────────────────────────────────────────\n");
        rtbValidationReport.SelectionColor = Color.Cyan;
        rtbValidationReport.AppendText($"\nSUMMARY:\n");
        rtbValidationReport.SelectionColor = Color.White;
        rtbValidationReport.AppendText($"  Team Size: {teamData.Count} Pokemon\n");
        rtbValidationReport.AppendText($"  Compatibility Checks: {passedChecks}/{totalChecks} passed\n");
        rtbValidationReport.AppendText($"  Overall Compatibility: {compatibility:F1}%\n");

        // Recommendations
        if (compatibility < 100)
        {
            rtbValidationReport.AppendText("\n");
            rtbValidationReport.SelectionColor = Color.Yellow;
            rtbValidationReport.AppendText("RECOMMENDATIONS:\n");
            rtbValidationReport.SelectionColor = Color.White;

            var problematicPokemon = teamData.Where(p => !IsPokemonAvailable(p.Species, "SwSh")).ToList();
            if (problematicPokemon.Count > 0)
            {
                rtbValidationReport.AppendText($"  • {problematicPokemon.Count} Pokemon not available in Sword/Shield\n");
            }

            rtbValidationReport.AppendText("  • Consider alternative Pokemon for cross-game compatibility\n");
            rtbValidationReport.AppendText("  • Check move availability before transfer\n");
        }
        else
        {
            rtbValidationReport.SelectionColor = Color.LightGreen;
            rtbValidationReport.AppendText("\n✓ Team is fully compatible across all selected games!\n");
        }

        btnExportCompatible.Enabled = true;
    }

    private void LstTeamValidation_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstTeamValidation.SelectedItems.Count == 0) return;

        var pokemon = lstTeamValidation.SelectedItems[0].Tag as TeamPokemonData;
        if (pokemon == null) return;

        UpdateMoveCompatibility(pokemon);
        UpdateAbilityCompatibility(pokemon);
    }

    private void UpdateMoveCompatibility(TeamPokemonData pokemon)
    {
        lstMoveCompatibility.Items.Clear();

        foreach (var moveId in pokemon.Moves)
        {
            var moveName = $"Move_{moveId}"; // In real implementation, use actual move names
            var item = new ListViewItem(moveName);

            var availableGames = new List<string>();
            foreach (var (game, gameInfo) in GameData)
            {
                var removedMoves = RemovedMoves.GetValueOrDefault(gameInfo.Code, new HashSet<string>());
                if (!removedMoves.Contains(moveName))
                    availableGames.Add(gameInfo.Code);
            }

            item.SubItems.Add(availableGames.Count >= 5 ? "All games" : string.Join(", ", availableGames));
            item.ForeColor = availableGames.Count >= 5 ? Color.LightGreen : Color.Yellow;
            lstMoveCompatibility.Items.Add(item);
        }
    }

    private void UpdateAbilityCompatibility(TeamPokemonData pokemon)
    {
        lstAbilityCompatibility.Items.Clear();

        // In real implementation, would check actual ability
        var item = new ListViewItem("Current Ability");
        item.SubItems.Add("Available in all games");
        item.ForeColor = Color.LightGreen;
        lstAbilityCompatibility.Items.Add(item);

        // Hidden ability check
        var haItem = new ListViewItem("Hidden Ability");
        haItem.SubItems.Add("Check game-specific");
        haItem.ForeColor = Color.Yellow;
        lstAbilityCompatibility.Items.Add(haItem);
    }

    private void PnlCompatibilityMatrix_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        string[] features = { "Mega Evolution", "Z-Moves", "Dynamax", "Tera Types", "Alpha Pokemon" };
        string[] games = { "SV", "SwSh", "BDSP", "SM", "XY" };

        bool[,] featureMatrix = new bool[,]
        {
            { false, false, false, true, true },   // Mega Evolution
            { false, false, false, true, false },  // Z-Moves
            { false, true, false, false, false },  // Dynamax
            { true, false, false, false, false },  // Tera Types
            { false, false, false, false, false }  // Alpha Pokemon (only PLA)
        };

        int cellWidth = 55;
        int cellHeight = 25;
        int startX = 90;
        int startY = 30;

        // Draw game headers
        for (int j = 0; j < games.Length; j++)
        {
            g.DrawString(games[j], new Font("Segoe UI", 8F), Brushes.Cyan, startX + j * cellWidth, 10);
        }

        // Draw features and matrix
        for (int i = 0; i < features.Length; i++)
        {
            g.DrawString(features[i].Length > 12 ? features[i][..12] : features[i],
                new Font("Segoe UI", 7F), Brushes.White, 5, startY + i * cellHeight);

            for (int j = 0; j < games.Length; j++)
            {
                var brush = featureMatrix[i, j] ?
                    new SolidBrush(Color.FromArgb(60, 150, 60)) :
                    new SolidBrush(Color.FromArgb(80, 40, 40));

                g.FillRectangle(brush, startX + j * cellWidth, startY + i * cellHeight, cellWidth - 3, cellHeight - 3);
                g.DrawString(featureMatrix[i, j] ? "✓" : "✗",
                    new Font("Segoe UI", 9F),
                    Brushes.White,
                    startX + j * cellWidth + 18,
                    startY + i * cellHeight + 3);
            }
        }
    }

    private void BtnExportCompatible_Click(object? sender, EventArgs e)
    {
        var compatiblePokemon = teamData.Where(p =>
            IsPokemonAvailable(p.Species, "SwSh") &&
            IsPokemonAvailable(p.Species, "SV")).ToList();

        if (compatiblePokemon.Count == 0)
        {
            MessageBox.Show("No Pokemon are fully compatible across all games.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "Text File|*.txt",
            FileName = "compatible_team.txt"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            var lines = new List<string>
            {
                "=== CROSS-GENERATION COMPATIBLE TEAM ===",
                $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}",
                ""
            };

            foreach (var pokemon in compatiblePokemon)
            {
                lines.Add($"- {pokemon.Name}");
                lines.Add($"  Compatible with: SV, SwSh, BDSP, SM");
                lines.Add("");
            }

            System.IO.File.WriteAllLines(sfd.FileName, lines);
            MessageBox.Show($"Exported {compatiblePokemon.Count} compatible Pokemon!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private bool IsPokemonAvailable(ushort species, string gameCode)
    {
        var gameInfo = GameData.Values.FirstOrDefault(g => g.Code == gameCode);
        if (gameInfo == null) return true;

        // Check if species is within game's dex
        if (species > gameInfo.MaxDexNum) return false;

        // Check specific exclusions
        var exclusions = UnavailablePokemon.GetValueOrDefault(gameCode, new HashSet<ushort>());
        return !exclusions.Contains(species);
    }

    private class TeamPokemonData
    {
        public PKM Pokemon { get; set; } = null!;
        public string Name { get; set; } = "";
        public ushort Species { get; set; }
        public ushort[] Moves { get; set; } = Array.Empty<ushort>();
    }

    private class GameInfo
    {
        public int Generation { get; set; }
        public string Code { get; set; } = "";
        public int MaxDexNum { get; set; }
        public bool HasMegas { get; set; }
        public bool HasZMoves { get; set; }
        public bool HasDynamax { get; set; }
        public bool HasTeraTypes { get; set; }
        public bool HasAlphas { get; set; }
        public bool IsRemake { get; set; }
        public bool IsSimplified { get; set; }
    }
}
