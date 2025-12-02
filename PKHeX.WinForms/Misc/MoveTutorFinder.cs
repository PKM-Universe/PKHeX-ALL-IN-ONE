using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class MoveTutorFinder : Form
{
    private readonly SaveFile SAV;
    private ComboBox cmbSpecies = null!;
    private ComboBox cmbGame = null!;
    private ComboBox cmbMoveType = null!;
    private TextBox txtSearchMove = null!;
    private ListView lstMoves = null!;
    private ListView lstLocations = null!;
    private Label lblMoveDetails = null!;

    private static readonly string[] Games = { "Scarlet/Violet", "BDSP", "Sword/Shield", "USUM", "ORAS", "B2W2", "Platinum", "Emerald", "FRLG" };
    private static readonly string[] MoveTypes = { "All Types", "Normal", "Fire", "Water", "Electric", "Grass", "Ice", "Fighting", "Poison", "Ground", "Flying", "Psychic", "Bug", "Rock", "Ghost", "Dragon", "Dark", "Steel", "Fairy" };

    public MoveTutorFinder(SaveFile sav)
    {
        SAV = sav;
        Text = "Move Tutor Finder";
        Size = new Size(900, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        LoadSampleData();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Move Tutor Finder",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Filters
        var grpFilters = new GroupBox
        {
            Text = "Search Filters",
            Location = new Point(20, 50),
            Size = new Size(840, 80),
            ForeColor = Color.White
        };

        var lblSpecies = new Label { Text = "Pokemon:", Location = new Point(10, 30), AutoSize = true, ForeColor = Color.White };
        cmbSpecies = new ComboBox
        {
            Location = new Point(80, 27),
            Width = 150,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbSpecies.Items.AddRange(new[] { "Pikachu", "Charizard", "Garchomp", "Tyranitar", "Dragonite", "Mewtwo", "Greninja", "Lucario" });
        cmbSpecies.SelectedIndex = 0;
        cmbSpecies.SelectedIndexChanged += (s, e) => FilterMoves();

        var lblGame = new Label { Text = "Game:", Location = new Point(250, 30), AutoSize = true, ForeColor = Color.White };
        cmbGame = new ComboBox
        {
            Location = new Point(300, 27),
            Width = 130,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbGame.Items.AddRange(Games);
        cmbGame.SelectedIndex = 0;
        cmbGame.SelectedIndexChanged += (s, e) => FilterMoves();

        var lblType = new Label { Text = "Type:", Location = new Point(450, 30), AutoSize = true, ForeColor = Color.White };
        cmbMoveType = new ComboBox
        {
            Location = new Point(500, 27),
            Width = 100,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbMoveType.Items.AddRange(MoveTypes);
        cmbMoveType.SelectedIndex = 0;
        cmbMoveType.SelectedIndexChanged += (s, e) => FilterMoves();

        var lblSearch = new Label { Text = "Search:", Location = new Point(620, 30), AutoSize = true, ForeColor = Color.White };
        txtSearchMove = new TextBox
        {
            Location = new Point(680, 27),
            Width = 140,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        txtSearchMove.TextChanged += (s, e) => FilterMoves();

        grpFilters.Controls.AddRange(new Control[] { lblSpecies, cmbSpecies, lblGame, cmbGame, lblType, cmbMoveType, lblSearch, txtSearchMove });

        // Available Tutor Moves
        var grpMoves = new GroupBox
        {
            Text = "Available Tutor Moves",
            Location = new Point(20, 140),
            Size = new Size(400, 350),
            ForeColor = Color.White
        };

        lstMoves = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(380, 315),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstMoves.Columns.Add("Move", 120);
        lstMoves.Columns.Add("Type", 70);
        lstMoves.Columns.Add("Power", 55);
        lstMoves.Columns.Add("Acc", 45);
        lstMoves.Columns.Add("Category", 80);
        lstMoves.SelectedIndexChanged += MoveSelected;

        grpMoves.Controls.Add(lstMoves);

        // Tutor Locations
        var grpLocations = new GroupBox
        {
            Text = "Tutor Locations",
            Location = new Point(440, 140),
            Size = new Size(420, 180),
            ForeColor = Color.White
        };

        lstLocations = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(400, 145),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstLocations.Columns.Add("Game", 100);
        lstLocations.Columns.Add("Location", 150);
        lstLocations.Columns.Add("Cost", 140);

        grpLocations.Controls.Add(lstLocations);

        // Move Details
        var grpDetails = new GroupBox
        {
            Text = "Move Details",
            Location = new Point(440, 330),
            Size = new Size(420, 160),
            ForeColor = Color.White
        };

        lblMoveDetails = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(400, 125),
            ForeColor = Color.White,
            Text = "Select a move to view details..."
        };

        grpDetails.Controls.Add(lblMoveDetails);

        // Quick Info
        var grpInfo = new GroupBox
        {
            Text = "Move Tutor Info",
            Location = new Point(20, 500),
            Size = new Size(840, 80),
            ForeColor = Color.White
        };

        var lblInfo = new Label
        {
            Text = "Move Tutors teach special moves that can't be learned through level up or TMs.\n" +
                   "Some moves are exclusive to certain games. Check compatibility before transferring Pokemon.\n" +
                   "In SV, all Move Tutors are located at the Pokemon Centers.",
            Location = new Point(10, 20),
            Size = new Size(820, 50),
            ForeColor = Color.Gray
        };

        grpInfo.Controls.Add(lblInfo);

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(760, 590),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, grpFilters, grpMoves, grpLocations, grpDetails, grpInfo, btnClose });
    }

    private void LoadSampleData()
    {
        var moves = new[]
        {
            new { Move = "Draco Meteor", Type = "Dragon", Power = "130", Acc = "90", Category = "Special" },
            new { Move = "Iron Head", Type = "Steel", Power = "80", Acc = "100", Category = "Physical" },
            new { Move = "Fire Punch", Type = "Fire", Power = "75", Acc = "100", Category = "Physical" },
            new { Move = "Ice Punch", Type = "Ice", Power = "75", Acc = "100", Category = "Physical" },
            new { Move = "Thunder Punch", Type = "Fire", Power = "75", Acc = "100", Category = "Physical" },
            new { Move = "Outrage", Type = "Dragon", Power = "120", Acc = "100", Category = "Physical" },
            new { Move = "Superpower", Type = "Fighting", Power = "120", Acc = "100", Category = "Physical" },
            new { Move = "Stealth Rock", Type = "Rock", Power = "-", Acc = "-", Category = "Status" },
            new { Move = "Knock Off", Type = "Dark", Power = "65", Acc = "100", Category = "Physical" },
            new { Move = "Earth Power", Type = "Ground", Power = "90", Acc = "100", Category = "Special" },
            new { Move = "Heat Wave", Type = "Fire", Power = "95", Acc = "90", Category = "Special" },
            new { Move = "Giga Drain", Type = "Grass", Power = "75", Acc = "100", Category = "Special" }
        };

        foreach (var move in moves)
        {
            var item = new ListViewItem(move.Move);
            item.SubItems.Add(move.Type);
            item.SubItems.Add(move.Power);
            item.SubItems.Add(move.Acc);
            item.SubItems.Add(move.Category);
            lstMoves.Items.Add(item);
        }
    }

    private void FilterMoves()
    {
        // Filtering logic would go here
    }

    private void MoveSelected(object? sender, EventArgs e)
    {
        if (lstMoves.SelectedItems.Count == 0) return;

        var item = lstMoves.SelectedItems[0];
        string moveName = item.Text;

        lblMoveDetails.Text = $"Move: {moveName}\n" +
                             $"Type: {item.SubItems[1].Text}\n" +
                             $"Power: {item.SubItems[2].Text}\n" +
                             $"Accuracy: {item.SubItems[3].Text}\n" +
                             $"Category: {item.SubItems[4].Text}\n\n" +
                             $"Priority: 0\n" +
                             $"PP: 5-10\n" +
                             $"Contact: {(item.SubItems[4].Text == "Physical" ? "Yes" : "No")}";

        // Update locations
        lstLocations.Items.Clear();
        var locations = new[]
        {
            new { Game = "Scarlet/Violet", Location = "Any Pokemon Center", Cost = "LP/TM Materials" },
            new { Game = "Sword/Shield", Location = "Wyndon Stadium", Cost = "Watts" },
            new { Game = "USUM", Location = "Battle Tree", Cost = "BP" },
            new { Game = "ORAS", Location = "Battle Resort", Cost = "8 BP" }
        };

        foreach (var loc in locations)
        {
            var locItem = new ListViewItem(loc.Game);
            locItem.SubItems.Add(loc.Location);
            locItem.SubItems.Add(loc.Cost);
            lstLocations.Items.Add(locItem);
        }
    }
}
