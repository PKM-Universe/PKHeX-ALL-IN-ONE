using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class ShinyHuntTracker : Form
{
    private readonly SaveFile SAV;
    private ListView lstHunts = null!;
    private Label lblCurrentCount = null!;
    private Label lblCurrentOdds = null!;
    private Label lblEstimated = null!;
    private ComboBox cmbMethod = null!;
    private ComboBox cmbGame = null!;
    private NumericUpDown nudChain = null!;
    private CheckBox chkCharm = null!;
    private TextBox txtTargetSpecies = null!;
    private ProgressBar prgOdds = null!;
    private Timer tmrAutoSave = null!;

    private ShinyHunt? currentHunt = null;
    private readonly List<ShinyHunt> allHunts = new();

    private static readonly string HuntsFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PKHeX", "shiny_hunts.json");

    public ShinyHuntTracker(SaveFile sav)
    {
        SAV = sav;
        Text = "Shiny Hunt Tracker";
        Size = new Size(950, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(20, 20, 35);
        KeyPreview = true;
        LoadHunts();
        InitializeUI();
        RefreshHunts();

        tmrAutoSave = new Timer { Interval = 30000 };
        tmrAutoSave.Tick += (s, e) => SaveHunts();
        tmrAutoSave.Start();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "✨ Shiny Hunt Tracker",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold)
        };

        var lblSubtitle = new Label
        {
            Text = "Track your shiny hunting progress - Press SPACE or click to add encounters",
            Location = new Point(22, 55),
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9F)
        };

        // Current Hunt Panel
        var grpCurrent = new GroupBox
        {
            Text = "Current Hunt",
            Location = new Point(20, 85),
            Size = new Size(580, 200),
            ForeColor = Color.White
        };

        var lblTarget = new Label { Text = "Target:", Location = new Point(15, 30), AutoSize = true, ForeColor = Color.White };
        txtTargetSpecies = new TextBox
        {
            Location = new Point(80, 27),
            Width = 150,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White,
            Text = "Pikachu"
        };

        var lblGame = new Label { Text = "Game:", Location = new Point(250, 30), AutoSize = true, ForeColor = Color.White };
        cmbGame = new ComboBox
        {
            Location = new Point(300, 27),
            Width = 130,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbGame.Items.AddRange(new[] { "Scarlet/Violet", "Legends Arceus", "BDSP", "Sword/Shield", "Let's Go", "USUM", "Sun/Moon", "ORAS", "XY", "Gen 5", "Gen 4", "Gen 3", "Gen 2" });
        cmbGame.SelectedIndex = 0;
        cmbGame.SelectedIndexChanged += (s, e) => UpdateOdds();

        var lblMethod = new Label { Text = "Method:", Location = new Point(15, 65), AutoSize = true, ForeColor = Color.White };
        cmbMethod = new ComboBox
        {
            Location = new Point(80, 62),
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbMethod.Items.AddRange(new[] { "Random Encounter", "Masuda Method", "Chain Fishing", "Poké Radar", "DexNav", "SOS Chain", "Mass Outbreak", "Sandwich Boost", "Full Odds" });
        cmbMethod.SelectedIndex = 0;
        cmbMethod.SelectedIndexChanged += (s, e) => UpdateOdds();

        var lblChain = new Label { Text = "Chain:", Location = new Point(250, 65), AutoSize = true, ForeColor = Color.White };
        nudChain = new NumericUpDown
        {
            Location = new Point(300, 62),
            Width = 80,
            Maximum = 9999,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        nudChain.ValueChanged += (s, e) => UpdateOdds();

        chkCharm = new CheckBox
        {
            Text = "Shiny Charm",
            Location = new Point(400, 62),
            AutoSize = true,
            ForeColor = Color.Gold
        };
        chkCharm.CheckedChanged += (s, e) => UpdateOdds();

        // Big Counter Display
        lblCurrentCount = new Label
        {
            Text = "0",
            Location = new Point(450, 95),
            Size = new Size(120, 70),
            ForeColor = Color.FromArgb(100, 255, 200),
            Font = new Font("Segoe UI", 48F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblEncounters = new Label
        {
            Text = "Encounters",
            Location = new Point(450, 165),
            Size = new Size(120, 20),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Encounter buttons
        var btnAdd1 = new Button
        {
            Text = "+1",
            Location = new Point(15, 110),
            Size = new Size(80, 50),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };
        btnAdd1.Click += (s, e) => AddEncounters(1);

        var btnAdd5 = new Button
        {
            Text = "+5",
            Location = new Point(105, 110),
            Size = new Size(60, 50),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F)
        };
        btnAdd5.Click += (s, e) => AddEncounters(5);

        var btnAdd10 = new Button
        {
            Text = "+10",
            Location = new Point(175, 110),
            Size = new Size(60, 50),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F)
        };
        btnAdd10.Click += (s, e) => AddEncounters(10);

        var btnSubtract = new Button
        {
            Text = "-1",
            Location = new Point(245, 110),
            Size = new Size(50, 50),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 100, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12F)
        };
        btnSubtract.Click += (s, e) => AddEncounters(-1);

        var btnReset = new Button
        {
            Text = "Reset",
            Location = new Point(305, 110),
            Size = new Size(60, 50),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 60, 60),
            ForeColor = Color.White
        };
        btnReset.Click += ResetCount;

        var btnFound = new Button
        {
            Text = "✨ FOUND!",
            Location = new Point(375, 110),
            Size = new Size(70, 50),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Gold,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        btnFound.Click += ShinyFound;

        grpCurrent.Controls.AddRange(new Control[] { lblTarget, txtTargetSpecies, lblGame, cmbGame, lblMethod, cmbMethod,
            lblChain, nudChain, chkCharm, lblCurrentCount, lblEncounters, btnAdd1, btnAdd5, btnAdd10, btnSubtract, btnReset, btnFound });

        // Odds Display
        var grpOdds = new GroupBox
        {
            Text = "Shiny Odds",
            Location = new Point(620, 85),
            Size = new Size(300, 200),
            ForeColor = Color.White
        };

        lblCurrentOdds = new Label
        {
            Text = "1/4096",
            Location = new Point(15, 35),
            Size = new Size(270, 40),
            ForeColor = Color.Cyan,
            Font = new Font("Segoe UI", 24F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblOddsLabel = new Label
        {
            Text = "Current Odds per Encounter",
            Location = new Point(15, 80),
            Size = new Size(270, 20),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleCenter
        };

        prgOdds = new ProgressBar
        {
            Location = new Point(15, 110),
            Size = new Size(270, 20),
            Maximum = 100,
            Style = ProgressBarStyle.Continuous
        };

        lblEstimated = new Label
        {
            Text = "~0% chance by now\nExpected: ~4096 encounters",
            Location = new Point(15, 140),
            Size = new Size(270, 50),
            ForeColor = Color.LightGray,
            TextAlign = ContentAlignment.MiddleCenter
        };

        grpOdds.Controls.AddRange(new Control[] { lblCurrentOdds, lblOddsLabel, prgOdds, lblEstimated });

        // Hunt History
        var grpHistory = new GroupBox
        {
            Text = "Hunt History",
            Location = new Point(20, 295),
            Size = new Size(900, 300),
            ForeColor = Color.White
        };

        lstHunts = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(750, 265),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstHunts.Columns.Add("Target", 120);
        lstHunts.Columns.Add("Game", 100);
        lstHunts.Columns.Add("Method", 100);
        lstHunts.Columns.Add("Encounters", 80);
        lstHunts.Columns.Add("Status", 80);
        lstHunts.Columns.Add("Started", 100);
        lstHunts.Columns.Add("Completed", 100);
        lstHunts.DoubleClick += LoadHunt;

        var btnNewHunt = new Button
        {
            Text = "New Hunt",
            Location = new Point(770, 25),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 60),
            ForeColor = Color.White
        };
        btnNewHunt.Click += StartNewHunt;

        var btnDeleteHunt = new Button
        {
            Text = "Delete Hunt",
            Location = new Point(770, 70),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 60, 60),
            ForeColor = Color.White
        };
        btnDeleteHunt.Click += DeleteHunt;

        var btnExportStats = new Button
        {
            Text = "Export Stats",
            Location = new Point(770, 115),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };

        grpHistory.Controls.AddRange(new Control[] { lstHunts, btnNewHunt, btnDeleteHunt, btnExportStats });

        // Stats Summary
        var grpStats = new GroupBox
        {
            Text = "Lifetime Stats",
            Location = new Point(20, 605),
            Size = new Size(600, 50),
            ForeColor = Color.White
        };

        var lblStats = new Label
        {
            Text = $"Total Hunts: {allHunts.Count} | Shinies Found: {allHunts.Count(h => h.Found)} | " +
                   $"Total Encounters: {allHunts.Sum(h => h.Encounters):N0} | Avg per Shiny: {(allHunts.Any(h => h.Found) ? allHunts.Where(h => h.Found).Average(h => h.Encounters):0):N0}",
            Location = new Point(15, 18),
            AutoSize = true,
            ForeColor = Color.LightGray
        };
        grpStats.Controls.Add(lblStats);

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(820, 620),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => { SaveHunts(); Close(); };

        Controls.AddRange(new Control[] { lblTitle, lblSubtitle, grpCurrent, grpOdds, grpHistory, grpStats, btnClose });

        UpdateOdds();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space)
        {
            AddEncounters(1);
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
        {
            AddEncounters(1);
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    private void AddEncounters(int count)
    {
        if (currentHunt == null)
            StartNewHunt(this, EventArgs.Empty);

        currentHunt!.Encounters = Math.Max(0, currentHunt.Encounters + count);
        lblCurrentCount.Text = currentHunt.Encounters.ToString("N0");

        // Update chain if applicable
        if (count > 0 && cmbMethod.SelectedItem?.ToString()?.Contains("Chain") == true)
            nudChain.Value = Math.Min(nudChain.Maximum, nudChain.Value + count);

        UpdateOdds();
    }

    private void UpdateOdds()
    {
        int baseOdds = GetBaseOdds();
        int effectiveOdds = CalculateEffectiveOdds(baseOdds);

        lblCurrentOdds.Text = $"1/{effectiveOdds:N0}";

        // Calculate cumulative probability
        int encounters = currentHunt?.Encounters ?? 0;
        double prob = 1.0 - Math.Pow(1.0 - (1.0 / effectiveOdds), encounters);
        int percent = (int)(prob * 100);

        prgOdds.Value = Math.Min(100, percent);
        lblEstimated.Text = $"~{percent}% cumulative chance\nExpected: ~{effectiveOdds:N0} encounters";

        // Color based on progress
        if (percent < 25)
            prgOdds.ForeColor = Color.LightBlue;
        else if (percent < 50)
            prgOdds.ForeColor = Color.LightGreen;
        else if (percent < 75)
            prgOdds.ForeColor = Color.Yellow;
        else
            prgOdds.ForeColor = Color.Orange;
    }

    private int GetBaseOdds()
    {
        var game = cmbGame.SelectedItem?.ToString() ?? "";

        if (game.Contains("Gen 2"))
            return 8192;
        if (game.Contains("Gen 3") || game.Contains("Gen 4") || game.Contains("Gen 5"))
            return 8192;

        return 4096; // Gen 6+
    }

    private int CalculateEffectiveOdds(int baseOdds)
    {
        var method = cmbMethod.SelectedItem?.ToString() ?? "";
        int chain = (int)nudChain.Value;
        bool hasCharm = chkCharm.Checked;

        double odds = baseOdds;

        // Shiny Charm
        if (hasCharm && baseOdds == 4096)
            odds = 4096.0 / 3; // ~1365

        // Method bonuses
        if (method == "Masuda Method")
            odds = hasCharm ? 512 : 683;
        else if (method == "Chain Fishing" && chain >= 20)
            odds = Math.Max(100, odds / (1 + chain * 0.1));
        else if (method == "Poké Radar" && chain >= 40)
            odds = 99;
        else if (method == "SOS Chain" && chain >= 31)
            odds = hasCharm ? 273 : 315;
        else if (method == "Mass Outbreak")
            odds = hasCharm ? 819 : 1024;
        else if (method == "Sandwich Boost")
            odds = hasCharm ? 512 : 683;

        return (int)Math.Max(1, odds);
    }

    private void StartNewHunt(object? sender, EventArgs e)
    {
        if (currentHunt != null && currentHunt.Encounters > 0 && !currentHunt.Found)
        {
            var result = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Save Current Hunt?",
                $"Save the current hunt for {currentHunt.Target} ({currentHunt.Encounters} encounters)?");
            if (result == DialogResult.Yes)
            {
                allHunts.Add(currentHunt);
                SaveHunts();
            }
        }

        currentHunt = new ShinyHunt
        {
            Target = txtTargetSpecies.Text,
            Game = cmbGame.SelectedItem?.ToString() ?? "Unknown",
            Method = cmbMethod.SelectedItem?.ToString() ?? "Random",
            Encounters = 0,
            Found = false,
            StartedAt = DateTime.Now
        };

        lblCurrentCount.Text = "0";
        nudChain.Value = 0;
        RefreshHunts();
    }

    private void ResetCount(object? sender, EventArgs e)
    {
        var result = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Reset Counter?",
            "Reset the encounter counter to 0?");
        if (result == DialogResult.Yes)
        {
            if (currentHunt != null)
                currentHunt.Encounters = 0;
            lblCurrentCount.Text = "0";
            nudChain.Value = 0;
            UpdateOdds();
        }
    }

    private void ShinyFound(object? sender, EventArgs e)
    {
        if (currentHunt == null) return;

        currentHunt.Found = true;
        currentHunt.CompletedAt = DateTime.Now;
        allHunts.Add(currentHunt);
        SaveHunts();

        WinFormsUtil.Alert("🎉 Congratulations!",
            $"You found a shiny {currentHunt.Target}!\n\n" +
            $"Encounters: {currentHunt.Encounters:N0}\n" +
            $"Time: {(currentHunt.CompletedAt - currentHunt.StartedAt):hh\\:mm\\:ss}");

        RefreshHunts();
        currentHunt = null;
        lblCurrentCount.Text = "0";
    }

    private void LoadHunt(object? sender, EventArgs e)
    {
        if (lstHunts.SelectedItems.Count == 0) return;

        var hunt = (ShinyHunt)lstHunts.SelectedItems[0].Tag;
        if (hunt.Found)
        {
            WinFormsUtil.Alert("Hunt Complete", $"This hunt was completed on {hunt.CompletedAt:g}");
            return;
        }

        currentHunt = hunt;
        txtTargetSpecies.Text = hunt.Target;
        cmbGame.SelectedItem = hunt.Game;
        cmbMethod.SelectedItem = hunt.Method;
        lblCurrentCount.Text = hunt.Encounters.ToString("N0");
        UpdateOdds();

        allHunts.Remove(hunt); // Remove from history while active
    }

    private void DeleteHunt(object? sender, EventArgs e)
    {
        if (lstHunts.SelectedItems.Count == 0) return;

        var hunt = (ShinyHunt)lstHunts.SelectedItems[0].Tag;
        var result = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Delete Hunt?",
            $"Delete the hunt for {hunt.Target}?");

        if (result == DialogResult.Yes)
        {
            allHunts.Remove(hunt);
            SaveHunts();
            RefreshHunts();
        }
    }

    private void RefreshHunts()
    {
        lstHunts.Items.Clear();
        foreach (var hunt in allHunts.OrderByDescending(h => h.StartedAt))
        {
            var item = new ListViewItem(hunt.Target);
            item.SubItems.Add(hunt.Game);
            item.SubItems.Add(hunt.Method);
            item.SubItems.Add(hunt.Encounters.ToString("N0"));
            item.SubItems.Add(hunt.Found ? "✨ Found!" : "In Progress");
            item.SubItems.Add(hunt.StartedAt.ToString("MM/dd/yy"));
            item.SubItems.Add(hunt.Found ? hunt.CompletedAt?.ToString("MM/dd/yy") ?? "" : "");
            item.Tag = hunt;

            if (hunt.Found)
                item.ForeColor = Color.Gold;
            else
                item.ForeColor = Color.LightGray;

            lstHunts.Items.Add(item);
        }
    }

    private void LoadHunts()
    {
        try
        {
            if (File.Exists(HuntsFile))
            {
                var json = File.ReadAllText(HuntsFile);
                var hunts = JsonSerializer.Deserialize<List<ShinyHunt>>(json);
                if (hunts != null)
                    allHunts.AddRange(hunts);
            }
        }
        catch { }
    }

    private void SaveHunts()
    {
        try
        {
            var dir = Path.GetDirectoryName(HuntsFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(allHunts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(HuntsFile, json);
        }
        catch { }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        tmrAutoSave?.Stop();
        tmrAutoSave?.Dispose();
        SaveHunts();
        base.OnFormClosing(e);
    }

    private class ShinyHunt
    {
        public string Target { get; set; } = "";
        public string Game { get; set; } = "";
        public string Method { get; set; } = "";
        public int Encounters { get; set; }
        public bool Found { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
