using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class BallCollectionTracker : Form
{
    private readonly SaveFile SAV;
    private ListView lstBalls = null!;
    private ListView lstPokemon = null!;
    private Label lblStats = null!;
    private ProgressBar prgCompletion = null!;
    private ComboBox cmbFilter = null!;

    private static readonly (string Name, string Description, string Game)[] Balls = new[]
    {
        ("Poke Ball", "Standard ball", "All"),
        ("Great Ball", "Better catch rate", "All"),
        ("Ultra Ball", "High catch rate", "All"),
        ("Master Ball", "100% catch rate", "All"),
        ("Safari Ball", "Safari Zone only", "HGSS/FRLG"),
        ("Sport Ball", "Bug Catching Contest", "HGSS"),
        ("Level Ball", "Based on level diff", "HGSS/SM+"),
        ("Lure Ball", "Fishing bonus", "HGSS/SM+"),
        ("Moon Ball", "Moon Stone evos", "HGSS/SM+"),
        ("Friend Ball", "High friendship", "HGSS/SM+"),
        ("Love Ball", "Opposite gender", "HGSS/SM+"),
        ("Heavy Ball", "Heavy Pokemon", "HGSS/SM+"),
        ("Fast Ball", "High speed Pokemon", "HGSS/SM+"),
        ("Premier Ball", "Fancy Poke Ball", "All"),
        ("Repeat Ball", "Previously caught", "Gen3+"),
        ("Timer Ball", "More turns = better", "Gen3+"),
        ("Nest Ball", "Low level bonus", "Gen3+"),
        ("Net Ball", "Water/Bug bonus", "Gen3+"),
        ("Dive Ball", "Underwater bonus", "Gen3+"),
        ("Luxury Ball", "Friendship bonus", "Gen3+"),
        ("Heal Ball", "Heals on catch", "Gen4+"),
        ("Quick Ball", "First turn bonus", "Gen4+"),
        ("Dusk Ball", "Night/cave bonus", "Gen4+"),
        ("Cherish Ball", "Event Pokemon", "Gen4+"),
        ("Dream Ball", "Dream World/Radar", "Gen5+/BDSP"),
        ("Beast Ball", "Ultra Beasts", "SM/USUM/SwSh"),
        ("Strange Ball", "Legends Arceus", "PLA"),
        ("Hisuian Balls", "PLA exclusive", "PLA")
    };

    public BallCollectionTracker(SaveFile sav)
    {
        SAV = sav;
        Text = "Ball Collection Tracker";
        Size = new Size(900, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        LoadCollection();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Ball Collection Tracker",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Filter
        var lblFilter = new Label { Text = "Filter:", Location = new Point(20, 50), AutoSize = true, ForeColor = Color.White };
        cmbFilter = new ComboBox
        {
            Location = new Point(70, 47),
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbFilter.Items.AddRange(new[] { "All Balls", "Collected", "Missing", "Apricorn Balls", "Special Balls" });
        cmbFilter.SelectedIndex = 0;
        cmbFilter.SelectedIndexChanged += (s, e) => FilterBalls();

        // Progress
        var grpProgress = new GroupBox
        {
            Text = "Collection Progress",
            Location = new Point(250, 40),
            Size = new Size(620, 70),
            ForeColor = Color.White
        };

        prgCompletion = new ProgressBar
        {
            Location = new Point(10, 25),
            Size = new Size(450, 25),
            Style = ProgressBarStyle.Continuous
        };

        lblStats = new Label
        {
            Text = "0/28 Balls Collected (0%)",
            Location = new Point(470, 28),
            AutoSize = true,
            ForeColor = Color.Lime,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        grpProgress.Controls.AddRange(new Control[] { prgCompletion, lblStats });

        // Ball List
        var grpBalls = new GroupBox
        {
            Text = "Ball Types",
            Location = new Point(20, 120),
            Size = new Size(400, 450),
            ForeColor = Color.White
        };

        lstBalls = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(380, 380),
            View = View.Details,
            FullRowSelect = true,
            CheckBoxes = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstBalls.Columns.Add("Ball", 120);
        lstBalls.Columns.Add("Description", 150);
        lstBalls.Columns.Add("Origin", 100);
        lstBalls.ItemChecked += BallChecked;
        lstBalls.SelectedIndexChanged += BallSelected;

        var btnMarkAll = new Button
        {
            Text = "Mark All Collected",
            Location = new Point(10, 410),
            Size = new Size(130, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White
        };
        btnMarkAll.Click += (s, e) =>
        {
            foreach (ListViewItem item in lstBalls.Items)
                item.Checked = true;
        };

        var btnClearAll = new Button
        {
            Text = "Clear All",
            Location = new Point(150, 410),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 60, 60),
            ForeColor = Color.White
        };
        btnClearAll.Click += (s, e) =>
        {
            foreach (ListViewItem item in lstBalls.Items)
                item.Checked = false;
        };

        grpBalls.Controls.AddRange(new Control[] { lstBalls, btnMarkAll, btnClearAll });

        // Pokemon in Ball
        var grpPokemon = new GroupBox
        {
            Text = "Pokemon in Selected Ball",
            Location = new Point(440, 120),
            Size = new Size(430, 450),
            ForeColor = Color.White
        };

        lstPokemon = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(410, 380),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstPokemon.Columns.Add("Pokemon", 150);
        lstPokemon.Columns.Add("Location", 140);
        lstPokemon.Columns.Add("Shiny", 60);
        lstPokemon.Columns.Add("OT", 100);

        var btnScanSave = new Button
        {
            Text = "Scan Save File",
            Location = new Point(10, 410),
            Size = new Size(120, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };
        btnScanSave.Click += ScanSaveFile;

        grpPokemon.Controls.AddRange(new Control[] { lstPokemon, btnScanSave });

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(770, 580),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, lblFilter, cmbFilter, grpProgress, grpBalls, grpPokemon, btnClose });
    }

    private void LoadCollection()
    {
        foreach (var ball in Balls)
        {
            var item = new ListViewItem(ball.Name);
            item.SubItems.Add(ball.Description);
            item.SubItems.Add(ball.Game);
            lstBalls.Items.Add(item);
        }

        // Simulate some collected balls
        lstBalls.Items[0].Checked = true; // Poke Ball
        lstBalls.Items[1].Checked = true; // Great Ball
        lstBalls.Items[2].Checked = true; // Ultra Ball
        lstBalls.Items[13].Checked = true; // Premier Ball
        lstBalls.Items[21].Checked = true; // Quick Ball

        UpdateStats();
    }

    private void FilterBalls()
    {
        // Filter logic
    }

    private void BallChecked(object? sender, ItemCheckedEventArgs e)
    {
        UpdateStats();
    }

    private void BallSelected(object? sender, EventArgs e)
    {
        if (lstBalls.SelectedItems.Count == 0) return;

        lstPokemon.Items.Clear();
        string ballName = lstBalls.SelectedItems[0].Text;

        // Sample Pokemon in this ball
        var pokemon = new[]
        {
            new { Name = "Pikachu", Location = "Box 1", Shiny = "No", OT = "Eric" },
            new { Name = "Eevee", Location = "Box 2", Shiny = "Yes", OT = "Eric" },
            new { Name = "Charizard", Location = "Party", Shiny = "No", OT = "Eric" }
        };

        foreach (var mon in pokemon)
        {
            var item = new ListViewItem(mon.Name);
            item.SubItems.Add(mon.Location);
            item.SubItems.Add(mon.Shiny);
            item.SubItems.Add(mon.OT);
            if (mon.Shiny == "Yes")
                item.ForeColor = Color.Gold;
            lstPokemon.Items.Add(item);
        }
    }

    private void UpdateStats()
    {
        int collected = lstBalls.Items.Cast<ListViewItem>().Count(i => i.Checked);
        int total = lstBalls.Items.Count;
        int percent = (int)((double)collected / total * 100);

        prgCompletion.Value = percent;
        lblStats.Text = $"{collected}/{total} Balls Collected ({percent}%)";
        lblStats.ForeColor = percent == 100 ? Color.Gold : Color.Lime;
    }

    private void ScanSaveFile(object? sender, EventArgs e)
    {
        WinFormsUtil.Alert("Scanning save file for Pokemon in different ball types...\n\nThis will catalog all your Pokemon by their Poke Ball.");
    }
}
