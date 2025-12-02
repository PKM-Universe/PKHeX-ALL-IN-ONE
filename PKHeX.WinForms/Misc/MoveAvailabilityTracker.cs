using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public partial class MoveAvailabilityTracker : Form
{
    private readonly SaveFile SAV;
    private ComboBox cmbPokemon = null!;
    private ComboBox cmbGeneration = null!;
    private ListView lstAvailableMoves = null!;
    private ListView lstLearnMethods = null!;
    private TextBox txtSearchMove = null!;
    private RichTextBox rtbMoveDetails = null!;
    private CheckBox chkLevelUp = null!;
    private CheckBox chkTM = null!;
    private CheckBox chkEgg = null!;
    private CheckBox chkTutor = null!;
    private Label lblMoveCount = null!;
    private Panel pnlMovePreview = null!;

    // Simplified move database (in real implementation, would load from game data)
    private static readonly Dictionary<string, MoveData> MoveDatabase = new()
    {
        ["Earthquake"] = new() { Name = "Earthquake", Type = "Ground", Category = "Physical", Power = 100, Accuracy = 100, PP = 10, Priority = 0, Description = "Hits all adjacent Pokemon." },
        ["Thunderbolt"] = new() { Name = "Thunderbolt", Type = "Electric", Category = "Special", Power = 90, Accuracy = 100, PP = 15, Priority = 0, Description = "10% chance to paralyze." },
        ["Ice Beam"] = new() { Name = "Ice Beam", Type = "Ice", Category = "Special", Power = 90, Accuracy = 100, PP = 10, Priority = 0, Description = "10% chance to freeze." },
        ["Flamethrower"] = new() { Name = "Flamethrower", Type = "Fire", Category = "Special", Power = 90, Accuracy = 100, PP = 15, Priority = 0, Description = "10% chance to burn." },
        ["Surf"] = new() { Name = "Surf", Type = "Water", Category = "Special", Power = 90, Accuracy = 100, PP = 15, Priority = 0, Description = "Hits all adjacent Pokemon." },
        ["Protect"] = new() { Name = "Protect", Type = "Normal", Category = "Status", Power = 0, Accuracy = 0, PP = 10, Priority = 4, Description = "Protects from moves." },
        ["Swords Dance"] = new() { Name = "Swords Dance", Type = "Normal", Category = "Status", Power = 0, Accuracy = 0, PP = 20, Priority = 0, Description = "+2 Attack." },
        ["Dragon Dance"] = new() { Name = "Dragon Dance", Type = "Dragon", Category = "Status", Power = 0, Accuracy = 0, PP = 20, Priority = 0, Description = "+1 Attack, +1 Speed." },
        ["Stealth Rock"] = new() { Name = "Stealth Rock", Type = "Rock", Category = "Status", Power = 0, Accuracy = 0, PP = 20, Priority = 0, Description = "Sets entry hazard." },
        ["U-turn"] = new() { Name = "U-turn", Type = "Bug", Category = "Physical", Power = 70, Accuracy = 100, PP = 20, Priority = 0, Description = "Switch out after attacking." },
        ["Volt Switch"] = new() { Name = "Volt Switch", Type = "Electric", Category = "Special", Power = 70, Accuracy = 100, PP = 20, Priority = 0, Description = "Switch out after attacking." },
        ["Close Combat"] = new() { Name = "Close Combat", Type = "Fighting", Category = "Physical", Power = 120, Accuracy = 100, PP = 5, Priority = 0, Description = "Lowers user's Defense/Sp.Def." },
        ["Draco Meteor"] = new() { Name = "Draco Meteor", Type = "Dragon", Category = "Special", Power = 130, Accuracy = 90, PP = 5, Priority = 0, Description = "Lowers user's Sp.Atk by 2." },
        ["Knock Off"] = new() { Name = "Knock Off", Type = "Dark", Category = "Physical", Power = 65, Accuracy = 100, PP = 20, Priority = 0, Description = "Removes held item. +50% power if item removed." },
        ["Rapid Spin"] = new() { Name = "Rapid Spin", Type = "Normal", Category = "Physical", Power = 50, Accuracy = 100, PP = 40, Priority = 0, Description = "Removes hazards. +1 Speed." },
        ["Roost"] = new() { Name = "Roost", Type = "Flying", Category = "Status", Power = 0, Accuracy = 0, PP = 10, Priority = 0, Description = "Heals 50% HP." },
        ["Recover"] = new() { Name = "Recover", Type = "Normal", Category = "Status", Power = 0, Accuracy = 0, PP = 10, Priority = 0, Description = "Heals 50% HP." },
        ["Toxic"] = new() { Name = "Toxic", Type = "Poison", Category = "Status", Power = 0, Accuracy = 90, PP = 10, Priority = 0, Description = "Badly poisons target." },
        ["Will-O-Wisp"] = new() { Name = "Will-O-Wisp", Type = "Fire", Category = "Status", Power = 0, Accuracy = 85, PP = 15, Priority = 0, Description = "Burns target." },
        ["Thunder Wave"] = new() { Name = "Thunder Wave", Type = "Electric", Category = "Status", Power = 0, Accuracy = 90, PP = 20, Priority = 0, Description = "Paralyzes target." }
    };

    // Pokemon move availability (simplified - real implementation would use game data)
    private static readonly Dictionary<string, Dictionary<string, List<string>>> PokemonMoves = new()
    {
        ["Garchomp"] = new()
        {
            ["Level Up"] = new() { "Dragon Claw", "Earthquake", "Crunch", "Dragon Rush", "Outrage" },
            ["TM"] = new() { "Earthquake", "Swords Dance", "Protect", "Stealth Rock", "Fire Fang", "Stone Edge" },
            ["Egg"] = new() { "Outrage", "Iron Head", "Double Edge" },
            ["Tutor"] = new() { "Draco Meteor", "Stealth Rock", "Iron Head" }
        },
        ["Dragonite"] = new()
        {
            ["Level Up"] = new() { "Dragon Dance", "Outrage", "Hurricane", "Dragon Rush" },
            ["TM"] = new() { "Earthquake", "Thunderbolt", "Ice Beam", "Roost", "Protect" },
            ["Egg"] = new() { "Extreme Speed", "Dragon Dance", "Aqua Jet" },
            ["Tutor"] = new() { "Draco Meteor", "Iron Head", "Superpower" }
        },
        ["Tyranitar"] = new()
        {
            ["Level Up"] = new() { "Crunch", "Stone Edge", "Earthquake", "Thrash" },
            ["TM"] = new() { "Earthquake", "Stealth Rock", "Protect", "Fire Punch", "Ice Punch" },
            ["Egg"] = new() { "Dragon Dance", "Pursuit", "Iron Head" },
            ["Tutor"] = new() { "Stealth Rock", "Iron Head", "Superpower" }
        }
    };

    public MoveAvailabilityTracker(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
        LoadPokemonList();
    }

    private void InitializeComponent()
    {
        Text = "Move Pool & Availability Analyzer";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        // Pokemon Selection
        var grpSelection = new GroupBox
        {
            Text = "Pokemon Selection",
            Location = new Point(20, 20),
            Size = new Size(350, 120),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        var lblPokemon = new Label
        {
            Text = "Pokemon:",
            Location = new Point(15, 30),
            Size = new Size(70, 25),
            ForeColor = Color.White
        };

        cmbPokemon = new ComboBox
        {
            Location = new Point(90, 27),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbPokemon.SelectedIndexChanged += CmbPokemon_SelectedIndexChanged;

        var lblGen = new Label
        {
            Text = "Generation:",
            Location = new Point(15, 65),
            Size = new Size(70, 25),
            ForeColor = Color.White
        };

        cmbGeneration = new ComboBox
        {
            Location = new Point(90, 62),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbGeneration.Items.AddRange(new[] { "Gen 9 (SV)", "Gen 8 (SwSh)", "Gen 7 (SM/USUM)", "Gen 6 (XY/ORAS)", "All Generations" });
        cmbGeneration.SelectedIndex = 0;
        cmbGeneration.SelectedIndexChanged += (s, e) => RefreshMoveList();

        grpSelection.Controls.AddRange(new Control[] { lblPokemon, cmbPokemon, lblGen, cmbGeneration });

        // Learn Method Filters
        var grpFilters = new GroupBox
        {
            Text = "Learn Method Filters",
            Location = new Point(390, 20),
            Size = new Size(400, 120),
            ForeColor = Color.FromArgb(255, 200, 100)
        };

        chkLevelUp = new CheckBox { Text = "Level Up", Location = new Point(20, 30), Size = new Size(90, 25), ForeColor = Color.White, Checked = true };
        chkTM = new CheckBox { Text = "TM/TR", Location = new Point(120, 30), Size = new Size(80, 25), ForeColor = Color.White, Checked = true };
        chkEgg = new CheckBox { Text = "Egg Moves", Location = new Point(210, 30), Size = new Size(90, 25), ForeColor = Color.White, Checked = true };
        chkTutor = new CheckBox { Text = "Move Tutor", Location = new Point(310, 30), Size = new Size(100, 25), ForeColor = Color.White, Checked = true };

        chkLevelUp.CheckedChanged += (s, e) => RefreshMoveList();
        chkTM.CheckedChanged += (s, e) => RefreshMoveList();
        chkEgg.CheckedChanged += (s, e) => RefreshMoveList();
        chkTutor.CheckedChanged += (s, e) => RefreshMoveList();

        var lblSearch = new Label { Text = "Search:", Location = new Point(20, 70), Size = new Size(60, 25), ForeColor = Color.White };
        txtSearchMove = new TextBox
        {
            Location = new Point(85, 67),
            Size = new Size(200, 25),
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        txtSearchMove.TextChanged += (s, e) => RefreshMoveList();

        lblMoveCount = new Label
        {
            Text = "Moves: 0",
            Location = new Point(300, 70),
            Size = new Size(90, 25),
            ForeColor = Color.Cyan
        };

        grpFilters.Controls.AddRange(new Control[] { chkLevelUp, chkTM, chkEgg, chkTutor, lblSearch, txtSearchMove, lblMoveCount });

        // Available Moves List
        var grpMoves = new GroupBox
        {
            Text = "Available Moves",
            Location = new Point(20, 150),
            Size = new Size(550, 400),
            ForeColor = Color.FromArgb(100, 255, 150)
        };

        lstAvailableMoves = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(520, 360),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstAvailableMoves.Columns.Add("Move", 120);
        lstAvailableMoves.Columns.Add("Type", 70);
        lstAvailableMoves.Columns.Add("Category", 70);
        lstAvailableMoves.Columns.Add("Power", 55);
        lstAvailableMoves.Columns.Add("Acc", 45);
        lstAvailableMoves.Columns.Add("Method", 80);
        lstAvailableMoves.SelectedIndexChanged += LstAvailableMoves_SelectedIndexChanged;

        grpMoves.Controls.Add(lstAvailableMoves);

        // Learn Methods for Selected Move
        var grpMethods = new GroupBox
        {
            Text = "How to Obtain Selected Move",
            Location = new Point(590, 150),
            Size = new Size(580, 200),
            ForeColor = Color.FromArgb(255, 200, 100)
        };

        lstLearnMethods = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(550, 160),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstLearnMethods.Columns.Add("Method", 100);
        lstLearnMethods.Columns.Add("Generation", 80);
        lstLearnMethods.Columns.Add("Game", 100);
        lstLearnMethods.Columns.Add("Details", 250);

        grpMethods.Controls.Add(lstLearnMethods);

        // Move Details Panel
        var grpDetails = new GroupBox
        {
            Text = "Move Details",
            Location = new Point(590, 360),
            Size = new Size(580, 190),
            ForeColor = Color.FromArgb(200, 200, 200)
        };

        pnlMovePreview = new Panel
        {
            Location = new Point(15, 25),
            Size = new Size(550, 50),
            BackColor = Color.FromArgb(35, 35, 55)
        };

        rtbMoveDetails = new RichTextBox
        {
            Location = new Point(15, 85),
            Size = new Size(550, 90),
            BackColor = Color.FromArgb(30, 30, 45),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };

        grpDetails.Controls.AddRange(new Control[] { pnlMovePreview, rtbMoveDetails });

        // Legality Note
        var grpLegality = new GroupBox
        {
            Text = "Legality Notes",
            Location = new Point(20, 560),
            Size = new Size(1150, 170),
            ForeColor = Color.FromArgb(255, 100, 100)
        };

        var rtbLegality = new RichTextBox
        {
            Location = new Point(15, 25),
            Size = new Size(1120, 130),
            BackColor = Color.FromArgb(30, 30, 45),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };
        rtbLegality.Text = @"MOVE LEGALITY INFORMATION:

• Level Up: Moves learned naturally by leveling up. Always legal.
• TM/TR: Moves taught via Technical Machines. Varies by game.
• Egg Moves: Inherited from parents. Requires specific breeding chains.
• Move Tutor: Special tutors in certain games. May require items/BP.

⚠ Cross-Generation Notes:
- Some moves were removed in Gen 8/9 (e.g., Hidden Power, Return, Pursuit)
- Egg moves may have different availability across games
- Move Tutors are game-specific (e.g., Draco Meteor requires Move Tutor in older games)
- TM/TR lists change between games - always verify for target game";

        grpLegality.Controls.Add(rtbLegality);

        Controls.AddRange(new Control[] { grpSelection, grpFilters, grpMoves, grpMethods, grpDetails, grpLegality });
    }

    private void LoadPokemonList()
    {
        cmbPokemon.Items.Clear();

        // Add from save
        var partyPokemon = SAV.PartyData.Where(p => p.Species != 0)
            .Select(p => SpeciesName.GetSpeciesName(p.Species, 2))
            .Distinct();

        foreach (var name in partyPokemon)
            cmbPokemon.Items.Add(name);

        // Add some common Pokemon
        var commonPokemon = new[] { "Garchomp", "Dragonite", "Tyranitar", "Salamence", "Metagross",
            "Gengar", "Alakazam", "Machamp", "Arcanine", "Gyarados" };

        foreach (var name in commonPokemon)
        {
            if (!cmbPokemon.Items.Contains(name))
                cmbPokemon.Items.Add(name);
        }

        if (cmbPokemon.Items.Count > 0)
            cmbPokemon.SelectedIndex = 0;
    }

    private void CmbPokemon_SelectedIndexChanged(object? sender, EventArgs e)
    {
        RefreshMoveList();
    }

    private void RefreshMoveList()
    {
        lstAvailableMoves.Items.Clear();
        var pokemonName = cmbPokemon.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(pokemonName)) return;

        var searchFilter = txtSearchMove.Text.ToLower();
        var moves = new List<(string Move, string Method)>();

        if (PokemonMoves.TryGetValue(pokemonName, out var pokemonMoveData))
        {
            if (chkLevelUp.Checked && pokemonMoveData.ContainsKey("Level Up"))
                moves.AddRange(pokemonMoveData["Level Up"].Select(m => (m, "Level Up")));

            if (chkTM.Checked && pokemonMoveData.ContainsKey("TM"))
                moves.AddRange(pokemonMoveData["TM"].Select(m => (m, "TM")));

            if (chkEgg.Checked && pokemonMoveData.ContainsKey("Egg"))
                moves.AddRange(pokemonMoveData["Egg"].Select(m => (m, "Egg")));

            if (chkTutor.Checked && pokemonMoveData.ContainsKey("Tutor"))
                moves.AddRange(pokemonMoveData["Tutor"].Select(m => (m, "Tutor")));
        }
        else
        {
            // Default moves for unknown Pokemon
            moves.AddRange(MoveDatabase.Keys.Take(10).Select(m => (m, "TM")));
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(searchFilter))
            moves = moves.Where(m => m.Move.ToLower().Contains(searchFilter)).ToList();

        // Remove duplicates, keeping first method found
        moves = moves.DistinctBy(m => m.Move).ToList();

        foreach (var (moveName, method) in moves)
        {
            if (MoveDatabase.TryGetValue(moveName, out var moveData))
            {
                var item = new ListViewItem(moveName);
                item.SubItems.Add(moveData.Type);
                item.SubItems.Add(moveData.Category);
                item.SubItems.Add(moveData.Power > 0 ? moveData.Power.ToString() : "-");
                item.SubItems.Add(moveData.Accuracy > 0 ? moveData.Accuracy.ToString() : "-");
                item.SubItems.Add(method);
                item.Tag = moveData;

                // Color by type
                item.ForeColor = GetTypeColor(moveData.Type);
                lstAvailableMoves.Items.Add(item);
            }
            else
            {
                var item = new ListViewItem(moveName);
                item.SubItems.Add("???");
                item.SubItems.Add("???");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                item.SubItems.Add(method);
                lstAvailableMoves.Items.Add(item);
            }
        }

        lblMoveCount.Text = $"Moves: {lstAvailableMoves.Items.Count}";
    }

    private void LstAvailableMoves_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstAvailableMoves.SelectedItems.Count == 0) return;

        var selectedItem = lstAvailableMoves.SelectedItems[0];
        var moveName = selectedItem.Text;

        // Update learn methods
        lstLearnMethods.Items.Clear();
        var pokemonName = cmbPokemon.SelectedItem?.ToString();

        if (!string.IsNullOrEmpty(pokemonName) && PokemonMoves.TryGetValue(pokemonName, out var pokemonMoveData))
        {
            foreach (var (method, moves) in pokemonMoveData)
            {
                if (moves.Contains(moveName))
                {
                    var item = new ListViewItem(method);
                    item.SubItems.Add("Gen 9");
                    item.SubItems.Add("Scarlet/Violet");
                    item.SubItems.Add(GetMethodDetails(method, moveName));
                    lstLearnMethods.Items.Add(item);
                }
            }
        }

        // Update move details
        if (selectedItem.Tag is MoveData moveData)
        {
            UpdateMovePreview(moveData);
            rtbMoveDetails.Clear();
            rtbMoveDetails.SelectionColor = Color.Cyan;
            rtbMoveDetails.AppendText($"{moveData.Name}\n");
            rtbMoveDetails.SelectionColor = Color.White;
            rtbMoveDetails.AppendText($"Type: {moveData.Type} | Category: {moveData.Category}\n");
            rtbMoveDetails.AppendText($"Power: {(moveData.Power > 0 ? moveData.Power.ToString() : "-")} | ");
            rtbMoveDetails.AppendText($"Accuracy: {(moveData.Accuracy > 0 ? moveData.Accuracy.ToString() + "%" : "-")} | ");
            rtbMoveDetails.AppendText($"PP: {moveData.PP}\n\n");
            rtbMoveDetails.SelectionColor = Color.Gray;
            rtbMoveDetails.AppendText(moveData.Description);
        }
    }

    private void UpdateMovePreview(MoveData move)
    {
        pnlMovePreview.Controls.Clear();

        var typeColor = GetTypeColor(move.Type);
        pnlMovePreview.BackColor = Color.FromArgb(40, typeColor.R / 3, typeColor.G / 3, typeColor.B / 3);

        var lblName = new Label
        {
            Text = move.Name,
            Location = new Point(10, 5),
            Size = new Size(200, 25),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 12F)
        };

        var lblType = new Label
        {
            Text = move.Type.ToUpper(),
            Location = new Point(10, 28),
            Size = new Size(80, 18),
            BackColor = typeColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 8F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblCategory = new Label
        {
            Text = move.Category,
            Location = new Point(100, 28),
            Size = new Size(70, 18),
            BackColor = move.Category == "Physical" ? Color.FromArgb(200, 100, 50) :
                       move.Category == "Special" ? Color.FromArgb(100, 100, 200) :
                       Color.FromArgb(100, 100, 100),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblPower = new Label
        {
            Text = $"PWR: {(move.Power > 0 ? move.Power.ToString() : "-")}",
            Location = new Point(400, 10),
            Size = new Size(70, 20),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F)
        };

        var lblAcc = new Label
        {
            Text = $"ACC: {(move.Accuracy > 0 ? move.Accuracy + "%" : "-")}",
            Location = new Point(480, 10),
            Size = new Size(70, 20),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F)
        };

        pnlMovePreview.Controls.AddRange(new Control[] { lblName, lblType, lblCategory, lblPower, lblAcc });
    }

    private string GetMethodDetails(string method, string moveName) => method switch
    {
        "Level Up" => "Learned naturally at level 1-100",
        "TM" => "Teach using TM/TR item",
        "Egg" => "Breed with compatible parent",
        "Tutor" => "Special move tutor (may require BP)",
        _ => "Unknown method"
    };

    private Color GetTypeColor(string type) => type switch
    {
        "Normal" => Color.FromArgb(168, 168, 120),
        "Fire" => Color.FromArgb(240, 128, 48),
        "Water" => Color.FromArgb(104, 144, 240),
        "Electric" => Color.FromArgb(248, 208, 48),
        "Grass" => Color.FromArgb(120, 200, 80),
        "Ice" => Color.FromArgb(152, 216, 216),
        "Fighting" => Color.FromArgb(192, 48, 40),
        "Poison" => Color.FromArgb(160, 64, 160),
        "Ground" => Color.FromArgb(224, 192, 104),
        "Flying" => Color.FromArgb(168, 144, 240),
        "Psychic" => Color.FromArgb(248, 88, 136),
        "Bug" => Color.FromArgb(168, 184, 32),
        "Rock" => Color.FromArgb(184, 160, 56),
        "Ghost" => Color.FromArgb(112, 88, 152),
        "Dragon" => Color.FromArgb(112, 56, 248),
        "Dark" => Color.FromArgb(112, 88, 72),
        "Steel" => Color.FromArgb(184, 184, 208),
        "Fairy" => Color.FromArgb(238, 153, 172),
        _ => Color.White
    };

    private class MoveData
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Category { get; set; } = "";
        public int Power { get; set; }
        public int Accuracy { get; set; }
        public int PP { get; set; }
        public int Priority { get; set; }
        public string Description { get; set; } = "";
    }
}
