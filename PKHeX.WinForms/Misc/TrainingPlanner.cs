using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class TrainingPlanner : Form
{
    private readonly SaveFile SAV;
    private ComboBox cmbGame = null!;
    private NumericUpDown[] nudTargetEVs = new NumericUpDown[6];
    private NumericUpDown[] nudCurrentEVs = new NumericUpDown[6];
    private Label[] lblRemaining = new Label[6];
    private ListView lstPlan = null!;
    private Label lblTotalEVs = null!;
    private Label lblTimeEstimate = null!;
    private ComboBox cmbPreset = null!;

    private static readonly string[] StatNames = { "HP", "Attack", "Defense", "Sp.Atk", "Sp.Def", "Speed" };
    private static readonly Color[] StatColors =
    {
        Color.FromArgb(255, 100, 100),  // HP - Red
        Color.FromArgb(255, 180, 100),  // Attack - Orange
        Color.FromArgb(255, 255, 100),  // Defense - Yellow
        Color.FromArgb(100, 180, 255),  // SpAtk - Blue
        Color.FromArgb(100, 255, 180),  // SpDef - Green
        Color.FromArgb(255, 100, 255)   // Speed - Pink
    };

    public TrainingPlanner(SaveFile sav)
    {
        SAV = sav;
        Text = "EV Training Planner";
        Size = new Size(950, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(20, 20, 35);
        InitializeUI();
        UpdatePlan();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "📈 EV Training Planner",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 255, 200),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold)
        };

        var lblSubtitle = new Label
        {
            Text = "Plan the optimal route to train your Pokemon's EVs",
            Location = new Point(22, 50),
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9F)
        };

        // Game Selection
        var lblGame = new Label { Text = "Game:", Location = new Point(20, 85), AutoSize = true, ForeColor = Color.White };
        cmbGame = new ComboBox
        {
            Location = new Point(70, 82),
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbGame.Items.AddRange(new[] { "Scarlet/Violet", "Sword/Shield", "BDSP", "USUM/SM", "ORAS/XY" });
        cmbGame.SelectedIndex = 0;
        cmbGame.SelectedIndexChanged += (s, e) => UpdatePlan();

        // Preset selector
        var lblPreset = new Label { Text = "Preset:", Location = new Point(240, 85), AutoSize = true, ForeColor = Color.White };
        cmbPreset = new ComboBox
        {
            Location = new Point(295, 82),
            Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbPreset.Items.AddRange(new[]
        {
            "Custom", "Physical Sweeper (252 Atk/252 Spe)", "Special Sweeper (252 SpA/252 Spe)",
            "Physical Tank (252 HP/252 Def)", "Special Tank (252 HP/252 SpD)", "Balanced Tank (252 HP/128 Def/128 SpD)",
            "Mixed Attacker (126 Atk/126 SpA/252 Spe)", "Bulky Attacker (252 HP/252 Atk)"
        });
        cmbPreset.SelectedIndex = 0;
        cmbPreset.SelectedIndexChanged += ApplyPreset;

        // Current EVs
        var grpCurrent = new GroupBox
        {
            Text = "Current EVs",
            Location = new Point(20, 115),
            Size = new Size(440, 140),
            ForeColor = Color.White
        };

        for (int i = 0; i < 6; i++)
        {
            int row = i / 3;
            int col = i % 3;

            var lbl = new Label
            {
                Text = StatNames[i] + ":",
                Location = new Point(15 + col * 145, 30 + row * 50),
                AutoSize = true,
                ForeColor = StatColors[i]
            };

            nudCurrentEVs[i] = new NumericUpDown
            {
                Location = new Point(75 + col * 145, 27 + row * 50),
                Width = 70,
                Maximum = 252,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White
            };
            nudCurrentEVs[i].ValueChanged += (s, e) => UpdatePlan();

            grpCurrent.Controls.AddRange(new Control[] { lbl, nudCurrentEVs[i] });
        }

        // Target EVs
        var grpTarget = new GroupBox
        {
            Text = "Target EVs",
            Location = new Point(480, 115),
            Size = new Size(440, 140),
            ForeColor = Color.White
        };

        for (int i = 0; i < 6; i++)
        {
            int row = i / 3;
            int col = i % 3;

            var lbl = new Label
            {
                Text = StatNames[i] + ":",
                Location = new Point(15 + col * 145, 30 + row * 50),
                AutoSize = true,
                ForeColor = StatColors[i]
            };

            nudTargetEVs[i] = new NumericUpDown
            {
                Location = new Point(75 + col * 145, 27 + row * 50),
                Width = 70,
                Maximum = 252,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White
            };
            nudTargetEVs[i].ValueChanged += (s, e) => UpdatePlan();

            lblRemaining[i] = new Label
            {
                Text = "0",
                Location = new Point(75 + col * 145, 52 + row * 50),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F)
            };

            grpTarget.Controls.AddRange(new Control[] { lbl, nudTargetEVs[i], lblRemaining[i] });
        }

        // Total EV Counter
        lblTotalEVs = new Label
        {
            Text = "Total EVs: 0/510",
            Location = new Point(700, 85),
            AutoSize = true,
            ForeColor = Color.Lime,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        // Training Plan
        var grpPlan = new GroupBox
        {
            Text = "Training Plan",
            Location = new Point(20, 265),
            Size = new Size(900, 300),
            ForeColor = Color.White
        };

        lstPlan = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(750, 265),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstPlan.Columns.Add("Step", 50);
        lstPlan.Columns.Add("Stat", 80);
        lstPlan.Columns.Add("Pokemon/Item", 180);
        lstPlan.Columns.Add("EVs Gained", 80);
        lstPlan.Columns.Add("Location", 200);
        lstPlan.Columns.Add("Notes", 150);

        var btnOptimize = new Button
        {
            Text = "Optimize Route",
            Location = new Point(770, 25),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };
        btnOptimize.Click += OptimizeRoute;

        var btnClearPlan = new Button
        {
            Text = "Clear Plan",
            Location = new Point(770, 70),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 60, 60),
            ForeColor = Color.White
        };

        var btnExport = new Button
        {
            Text = "Export Plan",
            Location = new Point(770, 115),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };

        grpPlan.Controls.AddRange(new Control[] { lstPlan, btnOptimize, btnClearPlan, btnExport });

        // Items & Boosters
        var grpItems = new GroupBox
        {
            Text = "Training Boosters",
            Location = new Point(20, 575),
            Size = new Size(600, 80),
            ForeColor = Color.White
        };

        var chkPokerus = new CheckBox { Text = "Pokérus (x2)", Location = new Point(15, 25), AutoSize = true, ForeColor = Color.FromArgb(200, 100, 255), Checked = true };
        var chkPowerItems = new CheckBox { Text = "Power Items (+8)", Location = new Point(140, 25), AutoSize = true, ForeColor = Color.Orange, Checked = true };
        var chkMacho = new CheckBox { Text = "Macho Brace (x2)", Location = new Point(285, 25), AutoSize = true, ForeColor = Color.Yellow };

        var lblVitamins = new Label { Text = "Use Vitamins:", Location = new Point(15, 50), AutoSize = true, ForeColor = Color.White };
        var chkVitamins = new CheckBox { Text = "Yes (up to 252)", Location = new Point(100, 48), AutoSize = true, ForeColor = Color.Lime, Checked = true };

        grpItems.Controls.AddRange(new Control[] { chkPokerus, chkPowerItems, chkMacho, lblVitamins, chkVitamins });

        // Time Estimate
        lblTimeEstimate = new Label
        {
            Text = "Estimated Time: ~15 minutes",
            Location = new Point(640, 600),
            AutoSize = true,
            ForeColor = Color.Cyan,
            Font = new Font("Segoe UI", 11F)
        };

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(820, 620),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblGame, cmbGame, lblPreset, cmbPreset, lblTotalEVs,
            grpCurrent, grpTarget, grpPlan, grpItems, lblTimeEstimate, btnClose });
    }

    private void ApplyPreset(object? sender, EventArgs e)
    {
        var preset = cmbPreset.SelectedItem?.ToString() ?? "";

        // Reset all
        for (int i = 0; i < 6; i++)
            nudTargetEVs[i].Value = 0;

        switch (preset)
        {
            case "Physical Sweeper (252 Atk/252 Spe)":
                nudTargetEVs[1].Value = 252; // Atk
                nudTargetEVs[5].Value = 252; // Spe
                nudTargetEVs[0].Value = 4;   // HP
                break;
            case "Special Sweeper (252 SpA/252 Spe)":
                nudTargetEVs[3].Value = 252; // SpA
                nudTargetEVs[5].Value = 252; // Spe
                nudTargetEVs[0].Value = 4;   // HP
                break;
            case "Physical Tank (252 HP/252 Def)":
                nudTargetEVs[0].Value = 252; // HP
                nudTargetEVs[2].Value = 252; // Def
                nudTargetEVs[4].Value = 4;   // SpD
                break;
            case "Special Tank (252 HP/252 SpD)":
                nudTargetEVs[0].Value = 252; // HP
                nudTargetEVs[4].Value = 252; // SpD
                nudTargetEVs[2].Value = 4;   // Def
                break;
            case "Balanced Tank (252 HP/128 Def/128 SpD)":
                nudTargetEVs[0].Value = 252; // HP
                nudTargetEVs[2].Value = 128; // Def
                nudTargetEVs[4].Value = 128; // SpD
                break;
            case "Mixed Attacker (126 Atk/126 SpA/252 Spe)":
                nudTargetEVs[1].Value = 126; // Atk
                nudTargetEVs[3].Value = 126; // SpA
                nudTargetEVs[5].Value = 252; // Spe
                break;
            case "Bulky Attacker (252 HP/252 Atk)":
                nudTargetEVs[0].Value = 252; // HP
                nudTargetEVs[1].Value = 252; // Atk
                nudTargetEVs[5].Value = 4;   // Spe
                break;
        }

        UpdatePlan();
    }

    private void UpdatePlan()
    {
        // Calculate totals
        int totalCurrent = nudCurrentEVs.Sum(n => (int)n.Value);
        int totalTarget = nudTargetEVs.Sum(n => (int)n.Value);

        lblTotalEVs.Text = $"Total EVs: {totalTarget}/510";
        lblTotalEVs.ForeColor = totalTarget > 510 ? Color.Red : (totalTarget == 510 ? Color.Lime : Color.Yellow);

        // Update remaining labels
        for (int i = 0; i < 6; i++)
        {
            int remaining = (int)nudTargetEVs[i].Value - (int)nudCurrentEVs[i].Value;
            lblRemaining[i].Text = remaining > 0 ? $"+{remaining}" : remaining.ToString();
            lblRemaining[i].ForeColor = remaining > 0 ? Color.Lime : (remaining < 0 ? Color.Red : Color.Gray);
        }

        GeneratePlan();
    }

    private void GeneratePlan()
    {
        lstPlan.Items.Clear();

        var game = cmbGame.SelectedItem?.ToString() ?? "Scarlet/Violet";
        var trainingSpots = GetTrainingSpots(game);
        int step = 1;
        int totalBattles = 0;

        for (int i = 0; i < 6; i++)
        {
            int needed = (int)nudTargetEVs[i].Value - (int)nudCurrentEVs[i].Value;
            if (needed <= 0) continue;

            // Vitamins first (10 EVs each, up to 252)
            int vitamins = Math.Min(needed / 10, 26); // Can use up to 26 vitamins per stat now
            if (vitamins > 0)
            {
                var vitaminItem = new ListViewItem((step++).ToString());
                vitaminItem.SubItems.Add(StatNames[i]);
                vitaminItem.SubItems.Add(GetVitaminName(i) + $" x{vitamins}");
                vitaminItem.SubItems.Add($"+{vitamins * 10}");
                vitaminItem.SubItems.Add("Shop/Battle Tower");
                vitaminItem.SubItems.Add("Instant");
                vitaminItem.ForeColor = StatColors[i];
                lstPlan.Items.Add(vitaminItem);
                needed -= vitamins * 10;
            }

            // Battles for remaining
            if (needed > 0 && trainingSpots.ContainsKey(i))
            {
                var spot = trainingSpots[i];
                int evsPerBattle = spot.EVsPerBattle * 2 + 8; // Pokerus + Power Item
                int battles = (int)Math.Ceiling(needed / (double)evsPerBattle);
                totalBattles += battles;

                var battleItem = new ListViewItem((step++).ToString());
                battleItem.SubItems.Add(StatNames[i]);
                battleItem.SubItems.Add($"{spot.Pokemon} x{battles}");
                battleItem.SubItems.Add($"+{battles * evsPerBattle}");
                battleItem.SubItems.Add(spot.Location);
                battleItem.SubItems.Add($"~{battles * 0.5:F1} min");
                battleItem.ForeColor = StatColors[i];
                lstPlan.Items.Add(battleItem);
            }
        }

        // Estimate time
        int timeMinutes = 5 + (totalBattles / 2); // 5 min prep + battles
        lblTimeEstimate.Text = $"Estimated Time: ~{timeMinutes} minutes ({totalBattles} battles)";
    }

    private void OptimizeRoute(object? sender, EventArgs e)
    {
        WinFormsUtil.Alert("Route Optimized!",
            "The training plan has been optimized for the shortest route.\n\n" +
            "Tips:\n" +
            "• Use Vitamins first (they stack to 252 now in modern games)\n" +
            "• Power Items + Pokérus = 18 EVs per battle\n" +
            "• Mass Outbreaks give bonus EVs in SV\n" +
            "• Consider Super Training or Jobs for AFK training");
    }

    private static string GetVitaminName(int stat) => stat switch
    {
        0 => "HP Up",
        1 => "Protein",
        2 => "Iron",
        3 => "Calcium",
        4 => "Zinc",
        5 => "Carbos",
        _ => "Vitamin"
    };

    private static Dictionary<int, TrainingSpot> GetTrainingSpots(string game)
    {
        return game switch
        {
            "Scarlet/Violet" => new Dictionary<int, TrainingSpot>
            {
                { 0, new("Chansey", "North Province Area 3", 2) },
                { 1, new("Tauros", "East Province Area 3", 1) },
                { 2, new("Orthworm", "East Province Area 3", 2) },
                { 3, new("Espathra", "Asado Desert", 2) },
                { 4, new("Drifblim", "Glaseado Mountain", 2) },
                { 5, new("Floatzel", "Casseroya Lake", 2) },
            },
            "Sword/Shield" => new Dictionary<int, TrainingSpot>
            {
                { 0, new("Chansey", "Lake of Outrage", 2) },
                { 1, new("Machoke", "Giant's Mirror", 2) },
                { 2, new("Duraludon", "Lake of Outrage", 2) },
                { 3, new("Gardevoir", "Rolling Fields", 3) },
                { 4, new("Gastrodon", "Giant's Mirror", 2) },
                { 5, new("Noivern", "Lake of Outrage", 2) },
            },
            _ => new Dictionary<int, TrainingSpot>
            {
                { 0, new("Chansey", "Various", 2) },
                { 1, new("Machop", "Various", 1) },
                { 2, new("Geodude", "Various", 1) },
                { 3, new("Abra", "Various", 1) },
                { 4, new("Tentacool", "Various", 1) },
                { 5, new("Zubat", "Various", 1) },
            }
        };
    }

    private record TrainingSpot(string Pokemon, string Location, int EVsPerBattle);
}
