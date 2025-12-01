using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;

namespace PKHeX.WinForms;

public class MissingEventsFinder : Form
{
    private readonly SaveFile SAV;
    private readonly HashSet<int> _ownedSpecies = new();
    private readonly HashSet<string> _ownedEventSignatures = new();

    private readonly ListView LV_Missing;
    private readonly ListView LV_Owned;
    private readonly ComboBox CB_Filter;
    private readonly Label L_MissingCount;
    private readonly Label L_OwnedCount;
    private readonly ProgressBar PB_Collection;
    private readonly CheckBox CHK_MythicalOnly;
    private readonly CheckBox CHK_ShinyOnly;

    // Major event Pokemon database
    private static readonly List<EventData> AllEvents = new()
    {
        // Gen 9 Mythicals
        new(1001, "Wo-Chien", "Scarlet/Violet", 2022, false, true, "Ruinous"),
        new(1002, "Chien-Pao", "Scarlet/Violet", 2022, false, true, "Ruinous"),
        new(1003, "Ting-Lu", "Scarlet/Violet", 2022, false, true, "Ruinous"),
        new(1004, "Chi-Yu", "Scarlet/Violet", 2022, false, true, "Ruinous"),
        new(1017, "Ogerpon", "Scarlet/Violet", 2023, false, true, "DLC"),
        new(1024, "Terapagos", "Scarlet/Violet", 2023, false, true, "DLC"),

        // Gen 8 Events
        new(893, "Zarude", "Sword/Shield", 2020, false, true, "Movie"),
        new(893, "Dada Zarude", "Sword/Shield", 2021, false, true, "Movie"),
        new(807, "Zeraora", "Sword/Shield", 2020, true, true, "HOME Event"),
        new(890, "Shiny Eternatus", "Sword/Shield", 2022, true, true, "GameStop"),
        new(892, "Gigantamax Urshifu", "Sword/Shield", 2020, false, true, "DLC"),
        new(898, "Calyrex", "Sword/Shield", 2020, false, true, "DLC"),
        new(894, "Regieleki", "Sword/Shield", 2020, false, true, "DLC"),
        new(895, "Regidrago", "Sword/Shield", 2020, false, true, "DLC"),
        new(896, "Glastrier", "Sword/Shield", 2020, false, true, "DLC"),
        new(897, "Spectrier", "Sword/Shield", 2020, false, true, "DLC"),

        // Gen 7 Events
        new(802, "Marshadow", "Sun/Moon", 2017, false, true, "Movie"),
        new(801, "Magearna", "Sun/Moon", 2016, false, true, "QR Code"),
        new(800, "Necrozma", "Ultra Sun/Moon", 2017, false, true, "Story"),
        new(800, "Shiny Necrozma", "Ultra Sun/Moon", 2019, true, true, "Japan Event"),
        new(803, "Poipole", "Ultra Sun/Moon", 2018, false, false, "Story"),
        new(803, "Shiny Poipole", "Ultra Sun/Moon", 2018, true, false, "Event"),
        new(804, "Naganadel", "Ultra Sun/Moon", 2018, false, false, "Evolution"),
        new(805, "Stakataka", "Ultra Sun/Moon", 2018, false, false, "Ultra Moon"),
        new(806, "Blacephalon", "Ultra Sun/Moon", 2018, false, false, "Ultra Sun"),
        new(807, "Zeraora", "Ultra Sun/Moon", 2018, false, true, "Movie"),
        new(718, "Shiny Zygarde", "Ultra Sun/Moon", 2018, true, true, "2018 Legends"),

        // Gen 6 Events
        new(719, "Diancie", "X/Y", 2014, false, true, "Movie"),
        new(719, "Shiny Diancie", "ORAS", 2015, true, true, "Japan Event"),
        new(720, "Hoopa", "ORAS", 2015, false, true, "Movie"),
        new(720, "Hoopa Unbound", "ORAS", 2015, false, true, "Movie"),
        new(721, "Volcanion", "ORAS", 2016, false, true, "Movie"),
        new(384, "Shiny Rayquaza", "ORAS", 2015, true, true, "Galileo"),

        // Gen 5 Events
        new(494, "Victini", "Black/White", 2011, false, true, "Movie"),
        new(494, "V-Create Victini", "Black/White", 2011, false, true, "Movie"),
        new(647, "Keldeo", "Black/White", 2012, false, true, "Event"),
        new(648, "Meloetta", "Black/White", 2012, false, true, "Event"),
        new(649, "Genesect", "Black 2/White 2", 2013, false, true, "Event"),
        new(649, "Shiny Genesect", "Black 2/White 2", 2013, true, true, "Japan Event"),
        new(646, "Kyurem", "Black/White", 2012, false, true, "Story"),

        // Gen 4 Events
        new(489, "Phione", "Diamond/Pearl", 2007, false, true, "Breeding"),
        new(490, "Manaphy", "Diamond/Pearl", 2007, false, true, "Pokemon Ranger"),
        new(491, "Darkrai", "Diamond/Pearl", 2008, false, true, "Event"),
        new(492, "Shaymin", "Diamond/Pearl", 2008, false, true, "Event"),
        new(492, "Shaymin Sky", "Platinum", 2009, false, true, "Event"),
        new(493, "Arceus", "Diamond/Pearl", 2009, false, true, "Event"),

        // Gen 3 Events
        new(385, "Jirachi", "Ruby/Sapphire", 2003, false, true, "Colosseum Bonus"),
        new(385, "WISHMKR Jirachi", "Ruby/Sapphire", 2003, false, true, "Colosseum"),
        new(386, "Deoxys", "FireRed/LeafGreen", 2004, false, true, "Event"),
        new(386, "Deoxys Attack", "FireRed", 2004, false, true, "Event"),
        new(386, "Deoxys Defense", "LeafGreen", 2004, false, true, "Event"),
        new(386, "Deoxys Speed", "Emerald", 2005, false, true, "Event"),

        // Gen 2 Events
        new(251, "Celebi", "Crystal", 2001, false, true, "Event"),
        new(251, "Shiny Celebi", "Crystal VC", 2018, true, true, "Virtual Console"),

        // Gen 1 Events
        new(151, "Mew", "Red/Blue", 1996, false, true, "Event"),
        new(151, "Shiny Mew", "Emerald", 2005, true, true, "Japan Event"),
        new(151, "Get Mew", "Scarlet/Violet", 2023, false, true, "Pokeball Plus"),

        // Cap Pikachus
        new(25, "Original Cap Pikachu", "Sun/Moon", 2017, false, false, "Event"),
        new(25, "Hoenn Cap Pikachu", "Sun/Moon", 2017, false, false, "Event"),
        new(25, "Sinnoh Cap Pikachu", "Sun/Moon", 2017, false, false, "Event"),
        new(25, "Unova Cap Pikachu", "Sun/Moon", 2017, false, false, "Event"),
        new(25, "Kalos Cap Pikachu", "Sun/Moon", 2017, false, false, "Event"),
        new(25, "Alola Cap Pikachu", "Ultra Sun/Moon", 2018, false, false, "Event"),
        new(25, "Partner Cap Pikachu", "Ultra Sun/Moon", 2018, false, false, "Event"),
        new(25, "World Cap Pikachu", "Sword/Shield", 2020, false, false, "Event"),

        // Other Notable Events
        new(658, "Ash-Greninja", "Sun/Moon", 2016, false, false, "Demo"),
        new(25, "Singing Pikachu", "Sword/Shield", 2021, false, false, "Event"),
        new(133, "Birthday Eevee", "Sword/Shield", 2021, false, false, "Pokemon Center"),
        new(25, "Flying Pikachu", "Scarlet/Violet", 2023, true, false, "Event"),
    };

    public MissingEventsFinder(SaveFile sav)
    {
        SAV = sav;
        Text = "Missing Events Finder";
        Size = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblTitle = new Label
        {
            Text = "Missing Event Pokemon Finder",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Filters
        var lblFilter = new Label { Text = "Game:", Location = new Point(350, 20), AutoSize = true, ForeColor = Color.White };
        CB_Filter = new ComboBox
        {
            Location = new Point(400, 17),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 60),
            ForeColor = Color.White
        };
        CB_Filter.Items.AddRange(new object[] { "All Games", "Gen 9", "Gen 8", "Gen 7", "Gen 6", "Gen 5", "Gen 4", "Gen 3" });
        CB_Filter.SelectedIndex = 0;
        CB_Filter.SelectedIndexChanged += (s, e) => RefreshDisplay();

        CHK_MythicalOnly = new CheckBox
        {
            Text = "Mythical/Legendary Only",
            Location = new Point(570, 18),
            AutoSize = true,
            ForeColor = Color.Cyan
        };
        CHK_MythicalOnly.CheckedChanged += (s, e) => RefreshDisplay();

        CHK_ShinyOnly = new CheckBox
        {
            Text = "Shiny Only",
            Location = new Point(750, 18),
            AutoSize = true,
            ForeColor = Color.Gold
        };
        CHK_ShinyOnly.CheckedChanged += (s, e) => RefreshDisplay();

        // Stats
        L_OwnedCount = new Label
        {
            Location = new Point(20, 55),
            AutoSize = true,
            ForeColor = Color.LightGreen,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        L_MissingCount = new Label
        {
            Location = new Point(200, 55),
            AutoSize = true,
            ForeColor = Color.Salmon,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        PB_Collection = new ProgressBar
        {
            Location = new Point(400, 55),
            Size = new Size(300, 20),
            Style = ProgressBarStyle.Continuous
        };

        var lblCollectionPct = new Label
        {
            Location = new Point(710, 55),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };

        // Missing events list
        var lblMissing = new Label
        {
            Text = "Missing Events:",
            Location = new Point(20, 90),
            AutoSize = true,
            ForeColor = Color.Salmon,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        LV_Missing = new ListView
        {
            Location = new Point(20, 115),
            Size = new Size(520, 520),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            GridLines = true
        };
        LV_Missing.Columns.Add("Pokemon", 150);
        LV_Missing.Columns.Add("Game", 120);
        LV_Missing.Columns.Add("Year", 60);
        LV_Missing.Columns.Add("Type", 100);
        LV_Missing.Columns.Add("Shiny", 50);

        // Owned events list
        var lblOwned = new Label
        {
            Text = "Owned Events:",
            Location = new Point(560, 90),
            AutoSize = true,
            ForeColor = Color.LightGreen,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        LV_Owned = new ListView
        {
            Location = new Point(560, 115),
            Size = new Size(520, 520),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            GridLines = true
        };
        LV_Owned.Columns.Add("Pokemon", 150);
        LV_Owned.Columns.Add("Game", 120);
        LV_Owned.Columns.Add("Year", 60);
        LV_Owned.Columns.Add("Type", 100);
        LV_Owned.Columns.Add("Shiny", 50);

        // Export button
        var btnExport = new Button
        {
            Text = "Export Missing List",
            Location = new Point(20, 645),
            Size = new Size(150, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 60, 100),
            ForeColor = Color.White
        };
        btnExport.Click += (s, e) => ExportMissingList();

        Controls.AddRange(new Control[] { lblTitle, lblFilter, CB_Filter, CHK_MythicalOnly, CHK_ShinyOnly,
            L_OwnedCount, L_MissingCount, PB_Collection, lblCollectionPct,
            lblMissing, LV_Missing, lblOwned, LV_Owned, btnExport });

        ScanSaveFile();
        RefreshDisplay();
    }

    private void ScanSaveFile()
    {
        _ownedSpecies.Clear();
        _ownedEventSignatures.Clear();

        // Scan all boxes
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                var pk = SAV.GetBoxSlotAtIndex(box, slot);
                if (pk.Species == 0) continue;

                _ownedSpecies.Add(pk.Species);

                // Create signature for event matching
                string signature = $"{pk.Species}_{pk.IsShiny}_{pk.Form}";
                _ownedEventSignatures.Add(signature);

                // Also add non-shiny version if we have shiny
                if (pk.IsShiny)
                    _ownedEventSignatures.Add($"{pk.Species}_False_{pk.Form}");
            }
        }

        // Scan party
        if (SAV.HasParty)
        {
            for (int i = 0; i < 6; i++)
            {
                var pk = SAV.GetPartySlotAtIndex(i);
                if (pk.Species == 0) continue;

                _ownedSpecies.Add(pk.Species);
                string signature = $"{pk.Species}_{pk.IsShiny}_{pk.Form}";
                _ownedEventSignatures.Add(signature);

                if (pk.IsShiny)
                    _ownedEventSignatures.Add($"{pk.Species}_False_{pk.Form}");
            }
        }
    }

    private void RefreshDisplay()
    {
        LV_Missing.Items.Clear();
        LV_Owned.Items.Clear();

        var filtered = AllEvents.AsEnumerable();

        // Game filter
        var gameFilter = CB_Filter.SelectedItem?.ToString() ?? "All Games";
        if (gameFilter != "All Games")
        {
            filtered = gameFilter switch
            {
                "Gen 9" => filtered.Where(e => e.Game.Contains("Scarlet") || e.Game.Contains("Violet")),
                "Gen 8" => filtered.Where(e => e.Game.Contains("Sword") || e.Game.Contains("Shield")),
                "Gen 7" => filtered.Where(e => e.Game.Contains("Sun") || e.Game.Contains("Moon")),
                "Gen 6" => filtered.Where(e => e.Game.Contains("X") || e.Game.Contains("Y") || e.Game.Contains("ORAS")),
                "Gen 5" => filtered.Where(e => e.Game.Contains("Black") || e.Game.Contains("White")),
                "Gen 4" => filtered.Where(e => e.Game.Contains("Diamond") || e.Game.Contains("Pearl") || e.Game.Contains("Platinum")),
                "Gen 3" => filtered.Where(e => e.Game.Contains("Ruby") || e.Game.Contains("Sapphire") || e.Game.Contains("Emerald") || e.Game.Contains("FireRed") || e.Game.Contains("LeafGreen")),
                _ => filtered
            };
        }

        if (CHK_MythicalOnly.Checked)
            filtered = filtered.Where(e => e.IsMythical);

        if (CHK_ShinyOnly.Checked)
            filtered = filtered.Where(e => e.IsShiny);

        int ownedCount = 0;
        int missingCount = 0;

        foreach (var evt in filtered.OrderBy(e => e.Species))
        {
            bool owned = _ownedSpecies.Contains(evt.Species);
            if (evt.IsShiny)
                owned = _ownedEventSignatures.Contains($"{evt.Species}_True_0");

            var item = new ListViewItem(evt.Name);
            item.SubItems.Add(evt.Game);
            item.SubItems.Add(evt.Year.ToString());
            item.SubItems.Add(evt.EventType);
            item.SubItems.Add(evt.IsShiny ? "Yes" : "");

            if (evt.IsShiny)
                item.ForeColor = Color.Gold;
            else if (evt.IsMythical)
                item.ForeColor = Color.Cyan;

            if (owned)
            {
                LV_Owned.Items.Add(item);
                ownedCount++;
            }
            else
            {
                LV_Missing.Items.Add(item);
                missingCount++;
            }
        }

        L_OwnedCount.Text = $"Owned: {ownedCount}";
        L_MissingCount.Text = $"Missing: {missingCount}";

        int total = ownedCount + missingCount;
        PB_Collection.Maximum = Math.Max(1, total);
        PB_Collection.Value = ownedCount;
    }

    private void ExportMissingList()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "Text File|*.txt",
            FileName = "MissingEventPokemon.txt"
        };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        var lines = new List<string> { "Missing Event Pokemon List", new string('=', 50), "" };

        foreach (ListViewItem item in LV_Missing.Items)
        {
            lines.Add($"{item.Text} - {item.SubItems[1].Text} ({item.SubItems[2].Text}) [{item.SubItems[3].Text}]");
        }

        System.IO.File.WriteAllLines(sfd.FileName, lines);
        WinFormsUtil.Alert($"Exported {LV_Missing.Items.Count} missing events!");
    }

    private class EventData
    {
        public int Species { get; }
        public string Name { get; }
        public string Game { get; }
        public int Year { get; }
        public bool IsShiny { get; }
        public bool IsMythical { get; }
        public string EventType { get; }

        public EventData(int species, string name, string game, int year, bool isShiny, bool isMythical, string eventType)
        {
            Species = species;
            Name = name;
            Game = game;
            Year = year;
            IsShiny = isShiny;
            IsMythical = isMythical;
            EventType = eventType;
        }
    }
}
