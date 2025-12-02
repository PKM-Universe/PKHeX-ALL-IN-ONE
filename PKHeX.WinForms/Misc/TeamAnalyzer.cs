using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class TeamAnalyzer : Form
{
    private readonly SaveFile SAV;
    private ListView lstTeam = null!;
    private Label lblTypeChart = null!;
    private Label lblWeaknesses = null!;
    private Label lblResistances = null!;
    private Label lblCoverage = null!;
    private Label lblSuggestions = null!;
    private Panel pnlTypeChart = null!;

    private static readonly string[] Types = { "Normal", "Fire", "Water", "Electric", "Grass", "Ice", "Fighting", "Poison", "Ground", "Flying", "Psychic", "Bug", "Rock", "Ghost", "Dragon", "Dark", "Steel", "Fairy" };
    private static readonly Color[] TypeColors = {
        Color.FromArgb(168, 168, 120), // Normal
        Color.FromArgb(240, 128, 48),  // Fire
        Color.FromArgb(104, 144, 240), // Water
        Color.FromArgb(248, 208, 48),  // Electric
        Color.FromArgb(120, 200, 80),  // Grass
        Color.FromArgb(152, 216, 216), // Ice
        Color.FromArgb(192, 48, 40),   // Fighting
        Color.FromArgb(160, 64, 160),  // Poison
        Color.FromArgb(224, 192, 104), // Ground
        Color.FromArgb(168, 144, 240), // Flying
        Color.FromArgb(248, 88, 136),  // Psychic
        Color.FromArgb(168, 184, 32),  // Bug
        Color.FromArgb(184, 160, 56),  // Rock
        Color.FromArgb(112, 88, 152),  // Ghost
        Color.FromArgb(112, 56, 248),  // Dragon
        Color.FromArgb(112, 88, 72),   // Dark
        Color.FromArgb(184, 184, 208), // Steel
        Color.FromArgb(238, 153, 172)  // Fairy
    };

    public TeamAnalyzer(SaveFile sav)
    {
        SAV = sav;
        Text = "Team Analyzer";
        Size = new Size(1000, 750);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        LoadTeam();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Team Analyzer",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Team List
        var grpTeam = new GroupBox
        {
            Text = "Your Team",
            Location = new Point(20, 50),
            Size = new Size(400, 250),
            ForeColor = Color.White
        };

        lstTeam = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(380, 180),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstTeam.Columns.Add("Pokemon", 100);
        lstTeam.Columns.Add("Type 1", 70);
        lstTeam.Columns.Add("Type 2", 70);
        lstTeam.Columns.Add("Role", 100);

        var btnLoadParty = new Button
        {
            Text = "Load Party",
            Location = new Point(10, 210),
            Size = new Size(100, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White
        };
        btnLoadParty.Click += LoadParty;

        var btnLoadBox = new Button
        {
            Text = "Load Box",
            Location = new Point(120, 210),
            Size = new Size(100, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };

        var btnAnalyze = new Button
        {
            Text = "Analyze",
            Location = new Point(280, 210),
            Size = new Size(100, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 80, 140),
            ForeColor = Color.White
        };
        btnAnalyze.Click += (s, e) => AnalyzeTeam();

        grpTeam.Controls.AddRange(new Control[] { lstTeam, btnLoadParty, btnLoadBox, btnAnalyze });

        // Type Coverage
        var grpCoverage = new GroupBox
        {
            Text = "Type Coverage",
            Location = new Point(440, 50),
            Size = new Size(530, 250),
            ForeColor = Color.White
        };

        pnlTypeChart = new Panel
        {
            Location = new Point(10, 25),
            Size = new Size(510, 215),
            BackColor = Color.FromArgb(30, 30, 45)
        };
        CreateTypeChart();

        grpCoverage.Controls.Add(pnlTypeChart);

        // Weaknesses & Resistances
        var grpWeakness = new GroupBox
        {
            Text = "Team Weaknesses",
            Location = new Point(20, 310),
            Size = new Size(300, 200),
            ForeColor = Color.White
        };

        lblWeaknesses = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(280, 165),
            ForeColor = Color.OrangeRed,
            Text = "Load a team to analyze weaknesses..."
        };
        grpWeakness.Controls.Add(lblWeaknesses);

        var grpResist = new GroupBox
        {
            Text = "Team Resistances",
            Location = new Point(340, 310),
            Size = new Size(300, 200),
            ForeColor = Color.White
        };

        lblResistances = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(280, 165),
            ForeColor = Color.LimeGreen,
            Text = "Load a team to analyze resistances..."
        };
        grpResist.Controls.Add(lblResistances);

        var grpSuggest = new GroupBox
        {
            Text = "Suggestions",
            Location = new Point(660, 310),
            Size = new Size(310, 200),
            ForeColor = Color.White
        };

        lblSuggestions = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(290, 165),
            ForeColor = Color.Cyan,
            Text = "Suggestions will appear here after analysis..."
        };
        grpSuggest.Controls.Add(lblSuggestions);

        // Offensive Coverage
        var grpOffense = new GroupBox
        {
            Text = "Offensive Coverage",
            Location = new Point(20, 520),
            Size = new Size(620, 150),
            ForeColor = Color.White
        };

        lblCoverage = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(600, 115),
            ForeColor = Color.White,
            Text = "Move type coverage will be displayed here..."
        };
        grpOffense.Controls.Add(lblCoverage);

        // Role Analysis
        var grpRoles = new GroupBox
        {
            Text = "Team Composition",
            Location = new Point(660, 520),
            Size = new Size(310, 150),
            ForeColor = Color.White
        };

        var lblRoles = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(290, 115),
            ForeColor = Color.White,
            Text = "Sweepers: 0\nWalls: 0\nSupport: 0\nSetup: 0\nUtility: 0"
        };
        grpRoles.Controls.Add(lblRoles);

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(870, 680),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, grpTeam, grpCoverage, grpWeakness, grpResist, grpSuggest, grpOffense, grpRoles, btnClose });
    }

    private void CreateTypeChart()
    {
        int cellWidth = 28;
        int cellHeight = 11;
        int startX = 50;
        int startY = 15;

        // Column headers (attacking types)
        for (int i = 0; i < Types.Length; i++)
        {
            var lbl = new Label
            {
                Text = Types[i].Substring(0, 3),
                Location = new Point(startX + i * cellWidth, 0),
                Size = new Size(cellWidth, 14),
                ForeColor = TypeColors[i],
                Font = new Font("Segoe UI", 6F),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlTypeChart.Controls.Add(lbl);
        }

        // Row headers (defending types)
        for (int i = 0; i < Types.Length; i++)
        {
            var lbl = new Label
            {
                Text = Types[i].Substring(0, 3),
                Location = new Point(5, startY + i * cellHeight),
                Size = new Size(40, cellHeight),
                ForeColor = TypeColors[i],
                Font = new Font("Segoe UI", 6F),
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlTypeChart.Controls.Add(lbl);
        }
    }

    private void LoadTeam()
    {
        // Sample team
        var team = new[]
        {
            new { Name = "Garchomp", Type1 = "Dragon", Type2 = "Ground", Role = "Sweeper" },
            new { Name = "Toxapex", Type1 = "Poison", Type2 = "Water", Role = "Wall" },
            new { Name = "Corviknight", Type1 = "Flying", Type2 = "Steel", Role = "Wall" },
            new { Name = "Dragapult", Type1 = "Dragon", Type2 = "Ghost", Role = "Sweeper" },
            new { Name = "Clefable", Type1 = "Fairy", Type2 = "", Role = "Support" },
            new { Name = "Ferrothorn", Type1 = "Grass", Type2 = "Steel", Role = "Wall" }
        };

        foreach (var mon in team)
        {
            var item = new ListViewItem(mon.Name);
            item.SubItems.Add(mon.Type1);
            item.SubItems.Add(mon.Type2);
            item.SubItems.Add(mon.Role);
            lstTeam.Items.Add(item);
        }

        AnalyzeTeam();
    }

    private void LoadParty(object? sender, EventArgs e)
    {
        lstTeam.Items.Clear();
        // Would load from SAV.PartyData
        WinFormsUtil.Alert("Loading party from save file...");
        LoadTeam();
    }

    private void AnalyzeTeam()
    {
        // Sample analysis results
        lblWeaknesses.Text = "Major Weaknesses:\n" +
                            "• Ice (4x on Garchomp)\n" +
                            "• Fire (2x on Ferrothorn)\n" +
                            "• Ground (2x on Toxapex)\n\n" +
                            "Coverage Gaps:\n" +
                            "• No Fire coverage\n" +
                            "• Weak to Fairy";

        lblResistances.Text = "Strong Against:\n" +
                             "• Dragon (immune via Clefable)\n" +
                             "• Poison (immune via Steel)\n" +
                             "• Ground (immune via Flying)\n" +
                             "• Electric (immune via Ground)\n\n" +
                             "Many resists to:\n" +
                             "• Grass, Bug, Normal";

        lblSuggestions.Text = "Recommended Changes:\n" +
                             "• Add Fire coverage move\n" +
                             "• Consider replacing one Dragon\n" +
                             "• Add Stealth Rock setter\n" +
                             "• Consider Defog/Rapid Spin\n\n" +
                             "Overall Rating: B+\n" +
                             "Good balance of offense/defense";

        lblCoverage.Text = "Super Effective Coverage:\n" +
                          "Dragon: Garchomp, Dragapult | Ghost: Dragapult\n" +
                          "Ground: Garchomp | Steel: Corviknight, Ferrothorn\n" +
                          "Fairy: Clefable | Poison: Toxapex\n\n" +
                          "Types NOT covered super-effectively:\n" +
                          "Fire, Electric, Water, Normal";
    }
}
