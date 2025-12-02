using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public partial class TournamentBracketSimulator : Form
{
    private readonly SaveFile SAV;
    private TabControl tabControl = null!;
    private ComboBox cmbFormat = null!;
    private ComboBox cmbBracketType = null!;
    private NumericUpDown nudParticipants = null!;
    private ListView lstParticipants = null!;
    private Panel pnlBracket = null!;
    private RichTextBox rtbMatchLog = null!;
    private Button btnAddTeam = null!;
    private Button btnGenerateBracket = null!;
    private Button btnSimulateAll = null!;
    private Button btnNextMatch = null!;
    private Label lblCurrentMatch = null!;
    private Label lblTournamentStatus = null!;

    private List<TournamentTeam> teams = new();
    private List<TournamentMatch> matches = new();
    private int currentRound = 1;
    private int currentMatchIndex = 0;

    public TournamentBracketSimulator(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Tournament Bracket Simulator";
        Size = new Size(1300, 850);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 10F)
        };

        // Tab 1: Setup
        var tabSetup = new TabPage("Tournament Setup") { BackColor = Color.FromArgb(30, 30, 50) };
        CreateSetupTab(tabSetup);
        tabControl.TabPages.Add(tabSetup);

        // Tab 2: Bracket
        var tabBracket = new TabPage("Bracket View") { BackColor = Color.FromArgb(30, 30, 50) };
        CreateBracketTab(tabBracket);
        tabControl.TabPages.Add(tabBracket);

        // Tab 3: Match Log
        var tabLog = new TabPage("Match Results") { BackColor = Color.FromArgb(30, 30, 50) };
        CreateLogTab(tabLog);
        tabControl.TabPages.Add(tabLog);

        Controls.Add(tabControl);
    }

    private void CreateSetupTab(TabPage tab)
    {
        // Tournament Settings
        var grpSettings = new GroupBox
        {
            Text = "Tournament Settings",
            Location = new Point(20, 20),
            Size = new Size(400, 180),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        var lblFormat = new Label { Text = "Battle Format:", Location = new Point(15, 30), Size = new Size(100, 25), ForeColor = Color.White };
        cmbFormat = new ComboBox
        {
            Location = new Point(130, 27),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbFormat.Items.AddRange(new[] { "VGC Doubles", "Singles OU", "Battle Stadium", "Anything Goes" });
        cmbFormat.SelectedIndex = 0;

        var lblBracket = new Label { Text = "Bracket Type:", Location = new Point(15, 65), Size = new Size(100, 25), ForeColor = Color.White };
        cmbBracketType = new ComboBox
        {
            Location = new Point(130, 62),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbBracketType.Items.AddRange(new[] { "Single Elimination", "Double Elimination", "Swiss System", "Round Robin" });
        cmbBracketType.SelectedIndex = 0;

        var lblParticipants = new Label { Text = "Participants:", Location = new Point(15, 100), Size = new Size(100, 25), ForeColor = Color.White };
        nudParticipants = new NumericUpDown
        {
            Location = new Point(130, 97),
            Size = new Size(80, 25),
            Minimum = 4,
            Maximum = 64,
            Value = 8,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        var lblNote = new Label
        {
            Text = "Note: For elimination brackets, participant count should be a power of 2 (4, 8, 16, 32...)",
            Location = new Point(15, 135),
            Size = new Size(370, 30),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8F)
        };

        grpSettings.Controls.AddRange(new Control[] { lblFormat, cmbFormat, lblBracket, cmbBracketType, lblParticipants, nudParticipants, lblNote });

        // Participants List
        var grpParticipants = new GroupBox
        {
            Text = "Tournament Participants",
            Location = new Point(440, 20),
            Size = new Size(500, 400),
            ForeColor = Color.FromArgb(255, 200, 100)
        };

        lstParticipants = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(470, 320),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstParticipants.Columns.Add("Seed", 50);
        lstParticipants.Columns.Add("Team Name", 150);
        lstParticipants.Columns.Add("Pokemon", 250);

        btnAddTeam = new Button
        {
            Text = "Add Your Team",
            Location = new Point(15, 355),
            Size = new Size(120, 30),
            BackColor = Color.FromArgb(60, 120, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnAddTeam.Click += BtnAddTeam_Click;

        var btnAddRandom = new Button
        {
            Text = "Add Random Teams",
            Location = new Point(145, 355),
            Size = new Size(140, 30),
            BackColor = Color.FromArgb(120, 80, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnAddRandom.Click += BtnAddRandom_Click;

        var btnClear = new Button
        {
            Text = "Clear All",
            Location = new Point(295, 355),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(180, 60, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnClear.Click += (s, e) => { teams.Clear(); lstParticipants.Items.Clear(); };

        grpParticipants.Controls.AddRange(new Control[] { lstParticipants, btnAddTeam, btnAddRandom, btnClear });

        // Generate Button
        btnGenerateBracket = new Button
        {
            Text = "🏆 GENERATE TOURNAMENT BRACKET",
            Location = new Point(20, 220),
            Size = new Size(400, 50),
            BackColor = Color.FromArgb(60, 180, 80),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 12F),
            FlatStyle = FlatStyle.Flat
        };
        btnGenerateBracket.Click += BtnGenerateBracket_Click;

        // Quick Stats Panel
        var pnlStats = new Panel
        {
            Location = new Point(20, 290),
            Size = new Size(400, 130),
            BackColor = Color.FromArgb(35, 35, 55)
        };

        lblTournamentStatus = new Label
        {
            Text = "Tournament Status: Not Started\n\nAdd teams and generate bracket to begin!",
            Location = new Point(15, 15),
            Size = new Size(370, 100),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };

        pnlStats.Controls.Add(lblTournamentStatus);

        tab.Controls.AddRange(new Control[] { grpSettings, grpParticipants, btnGenerateBracket, pnlStats });
    }

    private void CreateBracketTab(TabPage tab)
    {
        // Bracket visualization panel
        pnlBracket = new Panel
        {
            Location = new Point(20, 20),
            Size = new Size(900, 550),
            BackColor = Color.FromArgb(35, 35, 55),
            AutoScroll = true
        };

        // Match controls
        var pnlControls = new Panel
        {
            Location = new Point(940, 20),
            Size = new Size(300, 550),
            BackColor = Color.FromArgb(35, 35, 55)
        };

        var lblControlsTitle = new Label
        {
            Text = "Match Controls",
            Location = new Point(15, 15),
            Size = new Size(270, 30),
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI Bold", 12F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        lblCurrentMatch = new Label
        {
            Text = "Current Match:\n---",
            Location = new Point(15, 60),
            Size = new Size(270, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        btnNextMatch = new Button
        {
            Text = "⚔️ Simulate Next Match",
            Location = new Point(15, 140),
            Size = new Size(270, 45),
            BackColor = Color.FromArgb(60, 120, 180),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 11F),
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnNextMatch.Click += BtnNextMatch_Click;

        btnSimulateAll = new Button
        {
            Text = "🏃 Simulate All Remaining",
            Location = new Point(15, 195),
            Size = new Size(270, 45),
            BackColor = Color.FromArgb(180, 120, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 11F),
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnSimulateAll.Click += BtnSimulateAll_Click;

        var lblLegend = new Label
        {
            Text = "Legend:\n🟢 Winner\n🔴 Eliminated\n⚪ Pending",
            Location = new Point(15, 280),
            Size = new Size(270, 100),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 10F)
        };

        pnlControls.Controls.AddRange(new Control[] { lblControlsTitle, lblCurrentMatch, btnNextMatch, btnSimulateAll, lblLegend });

        tab.Controls.AddRange(new Control[] { pnlBracket, pnlControls });
    }

    private void CreateLogTab(TabPage tab)
    {
        rtbMatchLog = new RichTextBox
        {
            Location = new Point(20, 20),
            Size = new Size(1220, 560),
            BackColor = Color.FromArgb(25, 25, 40),
            ForeColor = Color.White,
            Font = new Font("Consolas", 10F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };

        tab.Controls.Add(rtbMatchLog);
    }

    private void BtnAddTeam_Click(object? sender, EventArgs e)
    {
        var party = SAV.PartyData.Where(p => p.Species != 0).ToList();
        if (party.Count == 0)
        {
            MessageBox.Show("No Pokemon in your party!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var team = new TournamentTeam
        {
            Name = "Your Team",
            IsPlayer = true,
            Pokemon = party.Select(p => new TournamentPokemon
            {
                Name = SpeciesName.GetSpeciesName(p.Species, 2),
                Species = p.Species,
                BST = p.PersonalInfo.GetBaseStatTotal(),
                Type1 = (int)p.PersonalInfo.Type1,
                Type2 = (int)p.PersonalInfo.Type2
            }).ToList()
        };

        teams.Add(team);
        RefreshParticipantsList();
    }

    private void BtnAddRandom_Click(object? sender, EventArgs e)
    {
        var targetCount = (int)nudParticipants.Value;
        var random = new Random();

        // Sample meta Pokemon for random teams
        var metaPokemon = new (string Name, ushort Species, int BST)[]
        {
            ("Flutter Mane", 1006, 570), ("Koraidon", 1007, 670), ("Miraidon", 1008, 670),
            ("Dragapult", 887, 600), ("Gholdengo", 1000, 550), ("Great Tusk", 984, 570),
            ("Kingambit", 983, 550), ("Skeledirge", 911, 530), ("Garchomp", 445, 600),
            ("Dragonite", 149, 600), ("Tyranitar", 248, 600), ("Metagross", 376, 600),
            ("Salamence", 373, 600), ("Hydreigon", 635, 600), ("Volcarona", 637, 550),
            ("Excadrill", 530, 508), ("Ferrothorn", 598, 489), ("Toxapex", 748, 495),
            ("Clefable", 36, 483), ("Gliscor", 472, 510), ("Heatran", 485, 600),
            ("Landorus", 645, 600), ("Urshifu", 892, 550), ("Incineroar", 727, 530)
        };

        while (teams.Count < targetCount)
        {
            var teamPokemon = metaPokemon.OrderBy(x => random.Next()).Take(6).ToList();
            var team = new TournamentTeam
            {
                Name = $"Team {teams.Count + 1}",
                IsPlayer = false,
                Pokemon = teamPokemon.Select(p => new TournamentPokemon
                {
                    Name = p.Name,
                    Species = p.Species,
                    BST = p.BST
                }).ToList()
            };
            teams.Add(team);
        }

        RefreshParticipantsList();
    }

    private void RefreshParticipantsList()
    {
        lstParticipants.Items.Clear();

        for (int i = 0; i < teams.Count; i++)
        {
            var team = teams[i];
            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(team.Name + (team.IsPlayer ? " ★" : ""));
            item.SubItems.Add(string.Join(", ", team.Pokemon.Take(3).Select(p => p.Name)) + (team.Pokemon.Count > 3 ? "..." : ""));
            item.ForeColor = team.IsPlayer ? Color.FromArgb(100, 255, 150) : Color.White;
            lstParticipants.Items.Add(item);
        }

        lblTournamentStatus.Text = $"Tournament Status: Setup\n\nTeams Registered: {teams.Count}/{nudParticipants.Value}\n" +
            (teams.Count >= nudParticipants.Value ? "✓ Ready to generate bracket!" : "Add more teams to continue");
    }

    private void BtnGenerateBracket_Click(object? sender, EventArgs e)
    {
        if (teams.Count < nudParticipants.Value)
        {
            MessageBox.Show($"Need at least {nudParticipants.Value} teams to start!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Seed teams randomly
        var random = new Random();
        var seededTeams = teams.Take((int)nudParticipants.Value).OrderBy(x => x.IsPlayer ? 0 : random.Next()).ToList();

        for (int i = 0; i < seededTeams.Count; i++)
            seededTeams[i].Seed = i + 1;

        teams = seededTeams;
        GenerateBracket();

        btnNextMatch.Enabled = true;
        btnSimulateAll.Enabled = true;
        lblTournamentStatus.Text = "Tournament Status: In Progress\n\nBracket generated! Switch to Bracket View tab.";
        tabControl.SelectedIndex = 1;
    }

    private void GenerateBracket()
    {
        matches.Clear();
        currentRound = 1;
        currentMatchIndex = 0;

        var bracketType = cmbBracketType.SelectedItem?.ToString() ?? "Single Elimination";

        if (bracketType == "Single Elimination")
        {
            GenerateSingleEliminationBracket();
        }
        else if (bracketType == "Round Robin")
        {
            GenerateRoundRobinBracket();
        }
        else
        {
            GenerateSingleEliminationBracket(); // Default
        }

        DrawBracket();
        UpdateCurrentMatchDisplay();
        LogTournamentStart();
    }

    private void GenerateSingleEliminationBracket()
    {
        int numTeams = teams.Count;
        int matchId = 1;

        // Round 1 matches
        for (int i = 0; i < numTeams; i += 2)
        {
            var match = new TournamentMatch
            {
                MatchId = matchId++,
                Round = 1,
                Team1 = teams[i],
                Team2 = teams[i + 1]
            };
            matches.Add(match);
        }

        // Calculate subsequent rounds
        int matchesInRound = numTeams / 2;
        int round = 2;
        int prevRoundStart = 0;

        while (matchesInRound > 1)
        {
            matchesInRound /= 2;
            for (int i = 0; i < matchesInRound; i++)
            {
                var match = new TournamentMatch
                {
                    MatchId = matchId++,
                    Round = round,
                    Team1 = null, // TBD
                    Team2 = null  // TBD
                };
                matches.Add(match);
            }
            round++;
        }

        // Add finals
        matches.Add(new TournamentMatch { MatchId = matchId, Round = round, IsFinal = true });
    }

    private void GenerateRoundRobinBracket()
    {
        int matchId = 1;
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                matches.Add(new TournamentMatch
                {
                    MatchId = matchId++,
                    Round = 1,
                    Team1 = teams[i],
                    Team2 = teams[j]
                });
            }
        }
    }

    private void DrawBracket()
    {
        pnlBracket.Controls.Clear();

        int rounds = matches.Max(m => m.Round);
        int xSpacing = 200;
        int ySpacing = 80;

        for (int round = 1; round <= rounds; round++)
        {
            var roundMatches = matches.Where(m => m.Round == round).ToList();
            int x = 20 + (round - 1) * xSpacing;

            // Round label
            var lblRound = new Label
            {
                Text = round == rounds ? "Finals" : $"Round {round}",
                Location = new Point(x, 10),
                Size = new Size(180, 25),
                ForeColor = Color.FromArgb(255, 200, 100),
                Font = new Font("Segoe UI Bold", 10F),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlBracket.Controls.Add(lblRound);

            int yOffset = (int)Math.Pow(2, round - 1) * ySpacing / 2;
            int y = 50 + yOffset;

            foreach (var match in roundMatches)
            {
                var pnlMatch = CreateMatchPanel(match, x, y);
                pnlBracket.Controls.Add(pnlMatch);
                y += (int)Math.Pow(2, round) * ySpacing;
            }
        }
    }

    private Panel CreateMatchPanel(TournamentMatch match, int x, int y)
    {
        var panel = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(180, 70),
            BackColor = match.IsComplete ? Color.FromArgb(40, 60, 40) : Color.FromArgb(40, 40, 60),
            Tag = match
        };

        var lblTeam1 = new Label
        {
            Text = match.Team1?.Name ?? "TBD",
            Location = new Point(5, 5),
            Size = new Size(150, 25),
            ForeColor = match.Winner == match.Team1 ? Color.LightGreen : Color.White,
            Font = new Font("Segoe UI", 9F, match.Winner == match.Team1 ? FontStyle.Bold : FontStyle.Regular)
        };

        var lblVs = new Label
        {
            Text = "vs",
            Location = new Point(155, 25),
            Size = new Size(20, 20),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8F)
        };

        var lblTeam2 = new Label
        {
            Text = match.Team2?.Name ?? "TBD",
            Location = new Point(5, 40),
            Size = new Size(150, 25),
            ForeColor = match.Winner == match.Team2 ? Color.LightGreen : Color.White,
            Font = new Font("Segoe UI", 9F, match.Winner == match.Team2 ? FontStyle.Bold : FontStyle.Regular)
        };

        panel.Controls.AddRange(new Control[] { lblTeam1, lblVs, lblTeam2 });
        return panel;
    }

    private void BtnNextMatch_Click(object? sender, EventArgs e)
    {
        var pendingMatches = matches.Where(m => !m.IsComplete && m.Team1 != null && m.Team2 != null).ToList();
        if (pendingMatches.Count == 0)
        {
            CheckTournamentComplete();
            return;
        }

        var match = pendingMatches.First();
        SimulateMatch(match);
        AdvanceBracket(match);
        DrawBracket();
        UpdateCurrentMatchDisplay();
    }

    private void BtnSimulateAll_Click(object? sender, EventArgs e)
    {
        while (true)
        {
            var pendingMatches = matches.Where(m => !m.IsComplete && m.Team1 != null && m.Team2 != null).ToList();
            if (pendingMatches.Count == 0) break;

            var match = pendingMatches.First();
            SimulateMatch(match);
            AdvanceBracket(match);
        }

        DrawBracket();
        CheckTournamentComplete();
    }

    private void SimulateMatch(TournamentMatch match)
    {
        if (match.Team1 == null || match.Team2 == null) return;

        // Calculate win probability based on team strength
        double team1Strength = match.Team1.Pokemon.Sum(p => p.BST) + (match.Team1.IsPlayer ? 50 : 0);
        double team2Strength = match.Team2.Pokemon.Sum(p => p.BST);

        double total = team1Strength + team2Strength;
        double team1Chance = team1Strength / total;

        var random = new Random();
        match.Winner = random.NextDouble() < team1Chance ? match.Team1 : match.Team2;
        match.IsComplete = true;

        // Log match result
        LogMatchResult(match);
    }

    private void AdvanceBracket(TournamentMatch completedMatch)
    {
        // Find next match where winner should advance
        var nextRoundMatches = matches.Where(m => m.Round == completedMatch.Round + 1 && !m.IsComplete).ToList();

        foreach (var nextMatch in nextRoundMatches)
        {
            if (nextMatch.Team1 == null)
            {
                nextMatch.Team1 = completedMatch.Winner;
                break;
            }
            else if (nextMatch.Team2 == null)
            {
                nextMatch.Team2 = completedMatch.Winner;
                break;
            }
        }
    }

    private void CheckTournamentComplete()
    {
        var finalMatch = matches.FirstOrDefault(m => m.IsFinal);
        if (finalMatch?.IsComplete == true)
        {
            var winner = finalMatch.Winner;
            lblTournamentStatus.Text = $"🏆 TOURNAMENT COMPLETE!\n\nChampion: {winner?.Name}\n" +
                (winner?.IsPlayer == true ? "Congratulations! You won!" : "Better luck next time!");

            lblCurrentMatch.Text = $"🏆 Champion:\n{winner?.Name}";
            btnNextMatch.Enabled = false;
            btnSimulateAll.Enabled = false;

            LogTournamentEnd(winner);
        }
        else
        {
            UpdateCurrentMatchDisplay();
        }
    }

    private void UpdateCurrentMatchDisplay()
    {
        var nextMatch = matches.FirstOrDefault(m => !m.IsComplete && m.Team1 != null && m.Team2 != null);
        if (nextMatch != null)
        {
            lblCurrentMatch.Text = $"Next Match (Round {nextMatch.Round}):\n{nextMatch.Team1?.Name}\nvs\n{nextMatch.Team2?.Name}";
        }
        else
        {
            lblCurrentMatch.Text = "Waiting for\nprevious rounds...";
        }
    }

    private void LogTournamentStart()
    {
        rtbMatchLog.Clear();
        rtbMatchLog.SelectionColor = Color.Cyan;
        rtbMatchLog.AppendText("═══════════════════════════════════════════════════════════════\n");
        rtbMatchLog.AppendText($"          {cmbFormat.SelectedItem} TOURNAMENT - {cmbBracketType.SelectedItem}\n");
        rtbMatchLog.AppendText("═══════════════════════════════════════════════════════════════\n\n");
        rtbMatchLog.SelectionColor = Color.White;
        rtbMatchLog.AppendText($"Participants: {teams.Count}\n");
        rtbMatchLog.AppendText($"Total Matches: {matches.Count}\n");
        rtbMatchLog.AppendText($"Started: {DateTime.Now:yyyy-MM-dd HH:mm}\n\n");
        rtbMatchLog.AppendText("───────────────────────────────────────────────────────────────\n\n");
    }

    private void LogMatchResult(TournamentMatch match)
    {
        rtbMatchLog.SelectionColor = Color.Yellow;
        rtbMatchLog.AppendText($"Round {match.Round} - Match {match.MatchId}\n");
        rtbMatchLog.SelectionColor = Color.White;
        rtbMatchLog.AppendText($"  {match.Team1?.Name} vs {match.Team2?.Name}\n");
        rtbMatchLog.SelectionColor = Color.LightGreen;
        rtbMatchLog.AppendText($"  Winner: {match.Winner?.Name}\n\n");
    }

    private void LogTournamentEnd(TournamentTeam? champion)
    {
        rtbMatchLog.AppendText("───────────────────────────────────────────────────────────────\n\n");
        rtbMatchLog.SelectionColor = Color.Gold;
        rtbMatchLog.AppendText($"🏆 TOURNAMENT CHAMPION: {champion?.Name} 🏆\n");
        rtbMatchLog.SelectionColor = Color.White;
        rtbMatchLog.AppendText($"\nCompleted: {DateTime.Now:yyyy-MM-dd HH:mm}\n");
    }

    private class TournamentTeam
    {
        public string Name { get; set; } = "";
        public int Seed { get; set; }
        public bool IsPlayer { get; set; }
        public List<TournamentPokemon> Pokemon { get; set; } = new();
        public int Wins { get; set; }
        public int Losses { get; set; }
    }

    private class TournamentPokemon
    {
        public string Name { get; set; } = "";
        public ushort Species { get; set; }
        public int BST { get; set; }
        public int Type1 { get; set; }
        public int Type2 { get; set; }
    }

    private class TournamentMatch
    {
        public int MatchId { get; set; }
        public int Round { get; set; }
        public TournamentTeam? Team1 { get; set; }
        public TournamentTeam? Team2 { get; set; }
        public TournamentTeam? Winner { get; set; }
        public bool IsComplete { get; set; }
        public bool IsFinal { get; set; }
    }
}
