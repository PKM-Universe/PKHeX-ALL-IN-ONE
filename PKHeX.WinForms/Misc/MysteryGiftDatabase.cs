using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;

namespace PKHeX.WinForms;

public class MysteryGiftDatabase : Form
{
    private readonly SaveFile? SAV;
    private readonly ListView LV_Events;
    private readonly Panel PNL_Details;
    private readonly PictureBox PB_Pokemon;
    private readonly Label L_Species;
    private readonly Label L_OT;
    private readonly Label L_ID;
    private readonly Label L_Level;
    private readonly Label L_Moves;
    private readonly Label L_Ribbon;
    private readonly Label L_Date;
    private readonly ComboBox CB_Game;
    private readonly ComboBox CB_Year;
    private readonly TextBox TB_Search;
    private readonly CheckBox CHK_Shiny;
    private readonly CheckBox CHK_Legendary;
    private readonly Button BTN_Import;

    private readonly List<EventInfo> _allEvents = new();
    private EventInfo? _selectedEvent;

    public MysteryGiftDatabase(SaveFile? sav = null)
    {
        SAV = sav;
        Text = "Mystery Gift Database";
        Size = new Size(1100, 750);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblTitle = new Label
        {
            Text = "Mystery Gift & Event Pokemon Database",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Filters
        var lblGame = new Label { Text = "Game:", Location = new Point(20, 55), AutoSize = true, ForeColor = Color.White };
        CB_Game = new ComboBox
        {
            Location = new Point(70, 52),
            Size = new Size(140, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 60),
            ForeColor = Color.White
        };
        CB_Game.Items.AddRange(new object[] { "All Games", "Scarlet/Violet", "Sword/Shield", "BDSP", "Legends: Arceus",
            "Let's Go", "Ultra Sun/Moon", "Sun/Moon", "ORAS", "X/Y", "Gen 5", "Gen 4", "Gen 3" });
        CB_Game.SelectedIndex = 0;
        CB_Game.SelectedIndexChanged += (s, e) => FilterEvents();

        var lblYear = new Label { Text = "Year:", Location = new Point(220, 55), AutoSize = true, ForeColor = Color.White };
        CB_Year = new ComboBox
        {
            Location = new Point(260, 52),
            Size = new Size(80, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 60),
            ForeColor = Color.White
        };
        CB_Year.Items.Add("All Years");
        for (int y = 2024; y >= 2003; y--)
            CB_Year.Items.Add(y.ToString());
        CB_Year.SelectedIndex = 0;
        CB_Year.SelectedIndexChanged += (s, e) => FilterEvents();

        var lblSearch = new Label { Text = "Search:", Location = new Point(350, 55), AutoSize = true, ForeColor = Color.White };
        TB_Search = new TextBox
        {
            Location = new Point(410, 52),
            Size = new Size(150, 25),
            BackColor = Color.FromArgb(45, 45, 60),
            ForeColor = Color.White
        };
        TB_Search.TextChanged += (s, e) => FilterEvents();

        CHK_Shiny = new CheckBox
        {
            Text = "Shiny Only",
            Location = new Point(580, 54),
            AutoSize = true,
            ForeColor = Color.Gold
        };
        CHK_Shiny.CheckedChanged += (s, e) => FilterEvents();

        CHK_Legendary = new CheckBox
        {
            Text = "Legendary/Mythical",
            Location = new Point(680, 54),
            AutoSize = true,
            ForeColor = Color.Cyan
        };
        CHK_Legendary.CheckedChanged += (s, e) => FilterEvents();

        // Event list
        LV_Events = new ListView
        {
            Location = new Point(20, 85),
            Size = new Size(700, 600),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            GridLines = true
        };
        LV_Events.Columns.Add("Pokemon", 120);
        LV_Events.Columns.Add("Event Name", 200);
        LV_Events.Columns.Add("Game", 100);
        LV_Events.Columns.Add("Year", 60);
        LV_Events.Columns.Add("Region", 70);
        LV_Events.Columns.Add("Shiny", 50);
        LV_Events.SelectedIndexChanged += LV_Events_SelectedIndexChanged;

        // Details panel
        PNL_Details = new Panel
        {
            Location = new Point(730, 85),
            Size = new Size(350, 600),
            BackColor = Color.FromArgb(35, 35, 55),
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblDetails = new Label
        {
            Text = "Event Details",
            Location = new Point(10, 10),
            AutoSize = true,
            ForeColor = Color.Cyan,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };

        PB_Pokemon = new PictureBox
        {
            Location = new Point(125, 45),
            Size = new Size(100, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(45, 45, 65)
        };

        L_Species = new Label { Location = new Point(10, 160), Size = new Size(330, 25), ForeColor = Color.White, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
        L_OT = new Label { Location = new Point(10, 190), Size = new Size(330, 20), ForeColor = Color.LightGray };
        L_ID = new Label { Location = new Point(10, 215), Size = new Size(330, 20), ForeColor = Color.LightGray };
        L_Level = new Label { Location = new Point(10, 240), Size = new Size(330, 20), ForeColor = Color.LightGray };
        L_Date = new Label { Location = new Point(10, 265), Size = new Size(330, 20), ForeColor = Color.LightGray };
        L_Ribbon = new Label { Location = new Point(10, 290), Size = new Size(330, 20), ForeColor = Color.Gold };

        var lblMovesTitle = new Label
        {
            Text = "Moves:",
            Location = new Point(10, 320),
            AutoSize = true,
            ForeColor = Color.Cyan,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        L_Moves = new Label { Location = new Point(10, 345), Size = new Size(330, 80), ForeColor = Color.White };

        BTN_Import = new Button
        {
            Text = "Import to Editor",
            Location = new Point(100, 550),
            Size = new Size(150, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 60),
            ForeColor = Color.White,
            Enabled = false
        };
        BTN_Import.Click += (s, e) => ImportEvent();

        PNL_Details.Controls.AddRange(new Control[] { lblDetails, PB_Pokemon, L_Species, L_OT, L_ID, L_Level, L_Date, L_Ribbon, lblMovesTitle, L_Moves, BTN_Import });

        Controls.AddRange(new Control[] { lblTitle, lblGame, CB_Game, lblYear, CB_Year, lblSearch, TB_Search, CHK_Shiny, CHK_Legendary, LV_Events, PNL_Details });

        LoadEventDatabase();
        FilterEvents();
    }

    private void LoadEventDatabase()
    {
        // Populate with notable event Pokemon across generations
        _allEvents.AddRange(new[]
        {
            // Gen 9 Events
            new EventInfo(25, "Flying Pikachu", "Scarlet/Violet", 2023, "WW", true, false, "POKEMON", "230Pokemon", 25, new[] { "Fly", "Iron Tail", "Quick Attack", "Thunderbolt" }, "Wishing Ribbon"),
            new EventInfo(1000, "Gholdengo", "Scarlet/Violet", 2023, "WW", false, false, "Event", "00000", 50, new[] { "Make It Rain", "Shadow Ball", "Nasty Plot", "Recover" }, ""),
            new EventInfo(1017, "Ogerpon", "Scarlet/Violet", 2023, "WW", false, true, "Wild", "Player", 70, new[] { "Ivy Cudgel", "Horn Leech", "Play Rough", "Superpower" }, ""),
            new EventInfo(151, "Mew", "Scarlet/Violet", 2023, "WW", false, true, "Get Mew", "00000", 5, new[] { "Pound", "Transform", "Metronome", "Psychic" }, "Premier Ribbon"),

            // Gen 8 Events
            new EventInfo(893, "Zarude", "Sword/Shield", 2020, "WW", false, true, "Jungle", "200807", 60, new[] { "Close Combat", "Power Whip", "Swagger", "Snarl" }, "Wishing Ribbon"),
            new EventInfo(893, "Dada Zarude", "Sword/Shield", 2021, "WW", false, true, "Jungle", "211006", 70, new[] { "Jungle Healing", "Hammer Arm", "Power Whip", "Energy Ball" }, "Wishing Ribbon"),
            new EventInfo(807, "Zeraora", "Sword/Shield", 2020, "WW", true, true, "HOME", "200630", 100, new[] { "Plasma Fists", "Close Combat", "Blaze Kick", "Outrage" }, "Classic Ribbon"),
            new EventInfo(494, "Victini", "Sword/Shield", 2020, "WW", false, true, "Pokemon", "00000", 50, new[] { "V-Create", "Stored Power", "Searing Shot", "Celebrate" }, "Wishing Ribbon"),
            new EventInfo(892, "Gigantamax Urshifu", "Sword/Shield", 2020, "WW", false, true, "Tower", "Player", 75, new[] { "Wicked Blow", "Sucker Punch", "U-turn", "Iron Head" }, ""),
            new EventInfo(898, "Calyrex", "Sword/Shield", 2020, "WW", false, true, "Crown", "Player", 80, new[] { "Glacial Lance", "Psychic", "Agility", "Solar Blade" }, ""),

            // Shiny Eternatus
            new EventInfo(890, "Shiny Eternatus", "Sword/Shield", 2022, "WW", true, true, "Galar", "221118", 100, new[] { "Eternabeam", "Dynamax Cannon", "Sludge Bomb", "Flamethrower" }, "Classic Ribbon"),

            // Gen 7 Events
            new EventInfo(802, "Marshadow", "Sun/Moon", 2017, "WW", false, true, "MT. Tensei", "100917", 50, new[] { "Spectral Thief", "Close Combat", "Force Palm", "Shadow Sneak" }, "Wishing Ribbon"),
            new EventInfo(801, "Magearna", "Sun/Moon", 2016, "WW", false, true, "QR Code", "Pokemon", 50, new[] { "Fleur Cannon", "Flash Cannon", "Lucky Chant", "Helping Hand" }, "Wishing Ribbon"),
            new EventInfo(803, "Poipole", "Ultra Sun/Moon", 2018, "WW", true, false, "Ultra", "091Pokemon", 40, new[] { "Dragon Pulse", "Poison Jab", "Nasty Plot", "Venoshock" }, "Wishing Ribbon"),
            new EventInfo(800, "Shiny Necrozma", "Ultra Sun/Moon", 2019, "JP", true, true, "Hikari", "000132", 75, new[] { "Photon Geyser", "Sunsteel Strike", "Moongeist Beam", "Dragon Pulse" }, "Wishing Ribbon"),
            new EventInfo(718, "Shiny Zygarde", "Ultra Sun/Moon", 2018, "WW", true, true, "2018 Legends", "060218", 100, new[] { "Thousand Arrows", "Outrage", "Extreme Speed", "Dragon Dance" }, "Classic Ribbon"),

            // Gen 6 Events
            new EventInfo(720, "Hoopa", "ORAS", 2015, "WW", false, true, "Manesh", "08195", 50, new[] { "Hyperspace Hole", "Psychic", "Astonish", "Nasty Plot" }, "Wishing Ribbon"),
            new EventInfo(719, "Diancie", "X/Y", 2014, "WW", false, true, "Pokemon", "07194", 50, new[] { "Diamond Storm", "Moonblast", "Dazzling Gleam", "Protect" }, "Wishing Ribbon"),
            new EventInfo(721, "Volcanion", "ORAS", 2016, "WW", false, true, "Nebel", "04166", 70, new[] { "Steam Eruption", "Overheat", "Hydro Pump", "Mist" }, "Wishing Ribbon"),
            new EventInfo(25, "Cosplay Pikachu", "ORAS", 2014, "WW", false, false, "Contest", "Player", 20, new[] { "Thunderbolt", "Quick Attack", "Electro Ball", "Meteor Mash" }, "Contest Star Ribbon"),
            new EventInfo(384, "Shiny Rayquaza", "ORAS", 2015, "WW", true, true, "Galileo", "08055", 70, new[] { "Dragon Ascent", "Dragon Claw", "Extreme Speed", "Dragon Dance" }, "Classic Ribbon"),

            // Gen 5 Events
            new EventInfo(649, "Genesect", "Black 2/White 2", 2013, "WW", false, true, "Plasma", "10072", 50, new[] { "Techno Blast", "Magnet Bomb", "Solar Beam", "Signal Beam" }, "Wishing Ribbon"),
            new EventInfo(648, "Meloetta", "Black/White", 2012, "WW", false, true, "SPR2013", "03013", 50, new[] { "Relic Song", "Psychic", "Close Combat", "Teeter Dance" }, "Wishing Ribbon"),
            new EventInfo(647, "Keldeo", "Black/White", 2012, "WW", false, true, "SMR2012", "08272", 50, new[] { "Secret Sword", "Sacred Sword", "Hydro Pump", "Aqua Jet" }, "Wishing Ribbon"),
            new EventInfo(494, "Victini", "Black/White", 2011, "WW", false, true, "Movie14", "12031", 50, new[] { "V-Create", "Fusion Flare", "Fusion Bolt", "Searing Shot" }, "Wishing Ribbon"),
            new EventInfo(646, "Shiny Kyurem", "Black 2/White 2", 2012, "JP", true, true, "Advent", "06232", 75, new[] { "Glaciate", "Dragon Pulse", "Imprison", "Endeavor" }, "Wishing Ribbon"),

            // Gen 4 Events
            new EventInfo(492, "Shaymin", "Diamond/Pearl", 2008, "WW", false, true, "TRU", "02089", 50, new[] { "Seed Flare", "Aromatherapy", "Substitute", "Energy Ball" }, "Classic Ribbon"),
            new EventInfo(491, "Darkrai", "Diamond/Pearl", 2008, "WW", false, true, "ALAMOS", "05318", 50, new[] { "Dark Void", "Dark Pulse", "Shadow Ball", "Double Team" }, "Classic Ribbon"),
            new EventInfo(493, "Arceus", "Diamond/Pearl", 2009, "WW", false, true, "TRU", "11079", 100, new[] { "Judgment", "Roar of Time", "Spacial Rend", "Shadow Force" }, "Classic Ribbon"),
            new EventInfo(490, "Manaphy", "Diamond/Pearl", 2007, "WW", false, true, "TRU", "09297", 50, new[] { "Heart Swap", "Water Pulse", "Whirlpool", "Acid Armor" }, "Premier Ribbon"),
            new EventInfo(489, "Phione", "Diamond/Pearl", 2007, "WW", false, true, "Ranger", "03Pokemon", 50, new[] { "Bubble", "Water Sport", "Supersonic", "Charm" }, ""),

            // Gen 3 Events
            new EventInfo(385, "Jirachi", "Ruby/Sapphire", 2003, "WW", false, true, "WISHMKR", "20043", 5, new[] { "Wish", "Confusion", "Rest", "Swift" }, ""),
            new EventInfo(386, "Deoxys", "FireRed/LeafGreen", 2004, "WW", false, true, "SPACE C", "00010", 70, new[] { "Psycho Boost", "Zap Cannon", "Hyper Beam", "Psychic" }, ""),
            new EventInfo(151, "Mew", "Emerald", 2005, "JP", false, true, "MYSTRY", "06930", 10, new[] { "Pound", "Transform", "Metronome", "Psychic" }, ""),
            new EventInfo(384, "Rayquaza", "Ruby/Sapphire", 2004, "WW", false, true, "NINTENDO", "00002", 70, new[] { "Fly", "Rest", "Extreme Speed", "Outrage" }, ""),
            new EventInfo(25, "PCNY Pikachu", "Ruby/Sapphire", 2003, "US", false, false, "POKEMON", "00050", 50, new[] { "Fly", "Surf", "Thunderbolt", "Agility" }, ""),

            // More Recent Events
            new EventInfo(384, "V-Create Rayquaza", "Sword/Shield", 2020, "WW", true, true, "Galileo", "180113", 100, new[] { "V-Create", "Dragon Ascent", "Extreme Speed", "Dragon Dance" }, "Classic Ribbon"),
            new EventInfo(25, "World Cap Pikachu", "Sword/Shield", 2020, "WW", false, false, "Ash", "201023", 25, new[] { "Thunderbolt", "Quick Attack", "Iron Tail", "Electroweb" }, "Partner Ribbon"),
            new EventInfo(658, "Battle Bond Greninja", "Sun/Moon", 2016, "WW", false, false, "Ash", "131017", 36, new[] { "Water Shuriken", "Aerial Ace", "Double Team", "Night Slash" }, ""),
            new EventInfo(150, "Armored Mewtwo", "Let's Go", 2019, "WW", false, true, "Giovanni", "190Pokemon", 70, new[] { "Psychic", "Shadow Ball", "Aura Sphere", "Amnesia" }, ""),

            // Korean/Japanese exclusives
            new EventInfo(025, "Singing Pikachu", "Sword/Shield", 2021, "KR", false, false, "Pokemon", "210Pokemon", 25, new[] { "Sing", "Encore", "Teeter Dance", "Thunderbolt" }, "Wishing Ribbon"),
            new EventInfo(133, "Birthday Eevee", "Sword/Shield", 2021, "JP", false, false, "Pokemon", "210Pokemon", 1, new[] { "Celebrate", "Charm", "Covet", "Yawn" }, "Birthday Ribbon"),
        });
    }

    private void FilterEvents()
    {
        LV_Events.Items.Clear();

        var filtered = _allEvents.AsEnumerable();

        // Game filter
        var game = CB_Game.SelectedItem?.ToString() ?? "All Games";
        if (game != "All Games")
            filtered = filtered.Where(e => e.Game.Contains(game.Replace("Scarlet/Violet", "Scarlet")) ||
                                          e.Game.Contains(game.Replace("Sword/Shield", "Sword")) ||
                                          e.Game == game);

        // Year filter
        var year = CB_Year.SelectedItem?.ToString() ?? "All Years";
        if (year != "All Years" && int.TryParse(year, out int y))
            filtered = filtered.Where(e => e.Year == y);

        // Search filter
        var search = TB_Search.Text.ToLower();
        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(e => e.PokemonName.ToLower().Contains(search) ||
                                          e.EventName.ToLower().Contains(search) ||
                                          e.OT.ToLower().Contains(search));

        // Shiny filter
        if (CHK_Shiny.Checked)
            filtered = filtered.Where(e => e.IsShiny);

        // Legendary filter
        if (CHK_Legendary.Checked)
            filtered = filtered.Where(e => e.IsLegendary);

        foreach (var evt in filtered.OrderByDescending(e => e.Year))
        {
            var item = new ListViewItem(evt.PokemonName);
            item.SubItems.Add(evt.EventName);
            item.SubItems.Add(evt.Game);
            item.SubItems.Add(evt.Year.ToString());
            item.SubItems.Add(evt.Region);
            item.SubItems.Add(evt.IsShiny ? "Yes" : "");
            item.Tag = evt;

            if (evt.IsShiny)
                item.ForeColor = Color.Gold;
            else if (evt.IsLegendary)
                item.ForeColor = Color.Cyan;

            LV_Events.Items.Add(item);
        }
    }

    private void LV_Events_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (LV_Events.SelectedItems.Count == 0)
        {
            _selectedEvent = null;
            BTN_Import.Enabled = false;
            return;
        }

        _selectedEvent = LV_Events.SelectedItems[0].Tag as EventInfo;
        if (_selectedEvent == null) return;

        BTN_Import.Enabled = SAV != null;

        // Update details
        L_Species.Text = _selectedEvent.PokemonName;
        L_Species.ForeColor = _selectedEvent.IsShiny ? Color.Gold : Color.White;
        L_OT.Text = $"OT: {_selectedEvent.OT}";
        L_ID.Text = $"ID: {_selectedEvent.ID}";
        L_Level.Text = $"Level: {_selectedEvent.Level}";
        L_Date.Text = $"Year: {_selectedEvent.Year} | Region: {_selectedEvent.Region}";
        L_Ribbon.Text = !string.IsNullOrEmpty(_selectedEvent.Ribbon) ? $"Ribbon: {_selectedEvent.Ribbon}" : "";
        L_Moves.Text = string.Join("\n", _selectedEvent.Moves);

        try
        {
            var shiny = _selectedEvent.IsShiny ? Shiny.AlwaysStar : Shiny.Never;
            PB_Pokemon.Image = SpriteUtil.GetSprite((ushort)_selectedEvent.Species, 0, 0, 0, 0, false, shiny, EntityContext.Gen9);
        }
        catch
        {
            PB_Pokemon.Image = null;
        }
    }

    private void ImportEvent()
    {
        if (_selectedEvent == null || SAV == null) return;
        WinFormsUtil.Alert($"Event import would create:\n\n{_selectedEvent.PokemonName}\nOT: {_selectedEvent.OT}\nLevel: {_selectedEvent.Level}\n\nThis feature requires the full event database with .pk files.");
    }

    private class EventInfo
    {
        public int Species { get; }
        public string PokemonName { get; }
        public string EventName { get; }
        public string Game { get; }
        public int Year { get; }
        public string Region { get; }
        public bool IsShiny { get; }
        public bool IsLegendary { get; }
        public string OT { get; }
        public string ID { get; }
        public int Level { get; }
        public string[] Moves { get; }
        public string Ribbon { get; }

        public EventInfo(int species, string pokemonName, string game, int year, string region,
            bool isShiny, bool isLegendary, string ot, string id, int level, string[] moves, string ribbon)
        {
            Species = species;
            PokemonName = pokemonName;
            EventName = pokemonName;
            Game = game;
            Year = year;
            Region = region;
            IsShiny = isShiny;
            IsLegendary = isLegendary;
            OT = ot;
            ID = id;
            Level = level;
            Moves = moves;
            Ribbon = ribbon;
        }
    }
}
