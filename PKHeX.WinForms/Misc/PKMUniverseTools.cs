using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using PKHeX.WinForms.Controls;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms;

/// <summary>
/// PKM-Universe Tools - Collection of productivity and quality of life features
/// </summary>
public partial class PKMUniverseTools : Form
{
    private readonly SaveFile SAV;
    private readonly IPKMView Editor;
    private TabControl TC_Tools = null!;

    // Tool panels
    private readonly BoxOrganizerPanel _boxOrganizer;
    private readonly BulkShinyPanel _bulkShiny;
    private readonly LivingDexTracker _livingDex;
    private readonly RecentHistoryPanel _recentHistory;
    private readonly FavoritesPanel _favorites;
    private readonly WonderTradeSimulator _wonderTrade;
    private readonly TeamExporter _teamExporter;
    private readonly StatsRadarChart _radarChart;

    public PKMUniverseTools(SaveFile sav, IPKMView editor)
    {
        SAV = sav;
        Editor = editor;

        InitializeComponent();

        // Initialize tool panels
        _boxOrganizer = new BoxOrganizerPanel(SAV);
        _bulkShiny = new BulkShinyPanel(SAV);
        _livingDex = new LivingDexTracker(SAV);
        _recentHistory = new RecentHistoryPanel(SAV, editor);
        _favorites = new FavoritesPanel(SAV, editor);
        _wonderTrade = new WonderTradeSimulator(SAV, editor);
        _teamExporter = new TeamExporter(SAV);
        _radarChart = new StatsRadarChart();

        SetupTabs();
        ApplyTheme();
    }

    private void InitializeComponent()
    {
        Text = "PKM-Universe Tools";
        Size = new Size(900, 700);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(800, 600);

        TC_Tools = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F)
        };

        Controls.Add(TC_Tools);
    }

    private void SetupTabs()
    {
        // Box Organizer Tab
        var tabOrganizer = new TabPage("Box Organizer") { Padding = new Padding(10) };
        tabOrganizer.Controls.Add(_boxOrganizer);
        _boxOrganizer.Dock = DockStyle.Fill;
        TC_Tools.TabPages.Add(tabOrganizer);

        // Bulk Shiny Tab
        var tabBulkShiny = new TabPage("Bulk Shiny") { Padding = new Padding(10) };
        tabBulkShiny.Controls.Add(_bulkShiny);
        _bulkShiny.Dock = DockStyle.Fill;
        TC_Tools.TabPages.Add(tabBulkShiny);

        // Living Dex Tab
        var tabLivingDex = new TabPage("Living Dex Tracker") { Padding = new Padding(10) };
        tabLivingDex.Controls.Add(_livingDex);
        _livingDex.Dock = DockStyle.Fill;
        TC_Tools.TabPages.Add(tabLivingDex);

        // Recent History Tab
        var tabHistory = new TabPage("Recent History") { Padding = new Padding(10) };
        tabHistory.Controls.Add(_recentHistory);
        _recentHistory.Dock = DockStyle.Fill;
        TC_Tools.TabPages.Add(tabHistory);

        // Favorites Tab
        var tabFavorites = new TabPage("Favorites") { Padding = new Padding(10) };
        tabFavorites.Controls.Add(_favorites);
        _favorites.Dock = DockStyle.Fill;
        TC_Tools.TabPages.Add(tabFavorites);

        // Wonder Trade Tab
        var tabWonderTrade = new TabPage("Wonder Trade") { Padding = new Padding(10) };
        tabWonderTrade.Controls.Add(_wonderTrade);
        _wonderTrade.Dock = DockStyle.Fill;
        TC_Tools.TabPages.Add(tabWonderTrade);

        // Team Exporter Tab
        var tabTeamExport = new TabPage("Team Export") { Padding = new Padding(10) };
        tabTeamExport.Controls.Add(_teamExporter);
        _teamExporter.Dock = DockStyle.Fill;
        TC_Tools.TabPages.Add(tabTeamExport);

        // Stats Radar Tab
        var tabRadar = new TabPage("Stats Radar") { Padding = new Padding(10) };
        tabRadar.Controls.Add(_radarChart);
        _radarChart.Dock = DockStyle.Fill;
        TC_Tools.TabPages.Add(tabRadar);
    }

    private void ApplyTheme()
    {
        var colors = ThemeManager.Colors;
        BackColor = colors.Background;
        ForeColor = colors.Text;
        TC_Tools.BackColor = colors.BackgroundSecondary;
    }

    public void SetPokemon(PKM pk)
    {
        _radarChart.SetPokemon(pk);
    }
}

#region Box Organizer Panel
public class BoxOrganizerPanel : UserControl
{
    private readonly SaveFile SAV;
    private readonly ComboBox CB_SortType;
    private readonly Button BTN_Sort;
    private readonly Button BTN_SortAll;
    private readonly CheckBox CHK_Reverse;
    private readonly Label L_Status;

    public BoxOrganizerPanel(SaveFile sav)
    {
        SAV = sav;

        var lbl = new Label { Text = "Sort By:", Location = new Point(10, 15), AutoSize = true };

        CB_SortType = new ComboBox
        {
            Location = new Point(80, 12),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        CB_SortType.Items.AddRange(new object[]
        {
            "National Dex Number",
            "Species Name (A-Z)",
            "Level (High to Low)",
            "Shiny First",
            "Type",
            "Generation",
            "Legendary/Mythical First",
            "IV Total",
            "Friendship"
        });
        CB_SortType.SelectedIndex = 0;

        CHK_Reverse = new CheckBox
        {
            Text = "Reverse Order",
            Location = new Point(300, 14),
            AutoSize = true
        };

        BTN_Sort = new Button
        {
            Text = "Sort Current Box",
            Location = new Point(10, 50),
            Size = new Size(150, 35),
            FlatStyle = FlatStyle.Flat
        };
        BTN_Sort.Click += (s, e) => SortBox(SAV.CurrentBox);

        BTN_SortAll = new Button
        {
            Text = "Sort All Boxes",
            Location = new Point(170, 50),
            Size = new Size(150, 35),
            FlatStyle = FlatStyle.Flat
        };
        BTN_SortAll.Click += (s, e) => SortAllBoxes();

        L_Status = new Label
        {
            Text = "Ready to organize!",
            Location = new Point(10, 100),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Italic)
        };

        Controls.AddRange(new Control[] { lbl, CB_SortType, CHK_Reverse, BTN_Sort, BTN_SortAll, L_Status });

        // Add info panel
        var infoPanel = new Panel
        {
            Location = new Point(10, 140),
            Size = new Size(450, 200),
            BorderStyle = BorderStyle.FixedSingle
        };

        var infoLabel = new Label
        {
            Text = @"Box Organizer Tips:
• National Dex: Sorts by Pokedex number
• Species Name: Alphabetical order
• Level: Highest level Pokemon first
• Shiny First: All shinies at the front
• Type: Groups by primary type
• Generation: Groups by origin generation
• Legendary First: Legends and Mythicals first
• IV Total: Best IVs first (competitive sorting)
• Friendship: Most friendly Pokemon first",
            Location = new Point(10, 10),
            Size = new Size(430, 180),
            Font = new Font("Segoe UI", 9F)
        };
        infoPanel.Controls.Add(infoLabel);
        Controls.Add(infoPanel);
    }

    private void SortBox(int box)
    {
        var pokemon = new List<PKM>();
        for (int i = 0; i < SAV.BoxSlotCount; i++)
        {
            var pk = SAV.GetBoxSlotAtIndex(box, i);
            if (pk.Species != 0)
                pokemon.Add(pk);
        }

        var sorted = SortPokemon(pokemon);
        if (CHK_Reverse.Checked)
            sorted.Reverse();

        // Clear box
        for (int i = 0; i < SAV.BoxSlotCount; i++)
        {
            var blank = SAV.BlankPKM;
            SAV.SetBoxSlotAtIndex(blank, box, i);
        }

        // Place sorted Pokemon
        for (int i = 0; i < sorted.Count && i < SAV.BoxSlotCount; i++)
        {
            SAV.SetBoxSlotAtIndex(sorted[i], box, i);
        }

        L_Status.Text = $"Sorted {sorted.Count} Pokemon in Box {box + 1}!";
    }

    private void SortAllBoxes()
    {
        var allPokemon = new List<PKM>();

        for (int box = 0; box < SAV.BoxCount; box++)
        {
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                var pk = SAV.GetBoxSlotAtIndex(box, slot);
                if (pk.Species != 0)
                    allPokemon.Add(pk);
            }
        }

        var sorted = SortPokemon(allPokemon);
        if (CHK_Reverse.Checked)
            sorted.Reverse();

        // Clear all boxes
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                SAV.SetBoxSlotAtIndex(SAV.BlankPKM, box, slot);
            }
        }

        // Place all sorted Pokemon
        int index = 0;
        for (int box = 0; box < SAV.BoxCount && index < sorted.Count; box++)
        {
            for (int slot = 0; slot < SAV.BoxSlotCount && index < sorted.Count; slot++)
            {
                SAV.SetBoxSlotAtIndex(sorted[index], box, slot);
                index++;
            }
        }

        L_Status.Text = $"Sorted {sorted.Count} Pokemon across all boxes!";
    }

    private List<PKM> SortPokemon(List<PKM> pokemon)
    {
        return CB_SortType.SelectedIndex switch
        {
            0 => pokemon.OrderBy(p => p.Species).ThenBy(p => p.Form).ToList(),
            1 => pokemon.OrderBy(p => GameInfo.Strings.specieslist[p.Species]).ToList(),
            2 => pokemon.OrderByDescending(p => p.CurrentLevel).ToList(),
            3 => pokemon.OrderByDescending(p => p.IsShiny).ThenBy(p => p.Species).ToList(),
            4 => pokemon.OrderBy(p => p.PersonalInfo.Type1).ThenBy(p => p.Species).ToList(),
            5 => pokemon.OrderBy(p => p.Generation).ThenBy(p => p.Species).ToList(),
            6 => pokemon.OrderByDescending(p => IsLegendaryOrMythical(p.Species)).ThenBy(p => p.Species).ToList(),
            7 => pokemon.OrderByDescending(p => p.IV_HP + p.IV_ATK + p.IV_DEF + p.IV_SPA + p.IV_SPD + p.IV_SPE).ToList(),
            8 => pokemon.OrderByDescending(p => p.CurrentFriendship).ToList(),
            _ => pokemon
        };
    }

    private static bool IsLegendaryOrMythical(ushort species)
    {
        // List of legendary and mythical Pokemon species IDs
        var legendaries = new HashSet<int>
        {
            144, 145, 146, 150, 151, // Gen 1
            243, 244, 245, 249, 250, 251, // Gen 2
            377, 378, 379, 380, 381, 382, 383, 384, 385, 386, // Gen 3
            480, 481, 482, 483, 484, 485, 486, 487, 488, 489, 490, 491, 492, 493, // Gen 4
            494, 638, 639, 640, 641, 642, 643, 644, 645, 646, 647, 648, 649, // Gen 5
            716, 717, 718, 719, 720, 721, // Gen 6
            785, 786, 787, 788, 789, 790, 791, 792, 793, 794, 795, 796, 797, 798, 799, 800, 801, 802, 807, 808, 809, // Gen 7
            888, 889, 890, 891, 892, 893, 894, 895, 896, 897, 898, // Gen 8
            905, 1001, 1002, 1003, 1004, 1007, 1008, 1014, 1015, 1016, 1017, 1024, 1025 // Gen 9
        };
        return legendaries.Contains(species);
    }
}
#endregion

#region Bulk Shiny Panel
public class BulkShinyPanel : UserControl
{
    private readonly SaveFile SAV;
    private readonly RadioButton RB_CurrentBox;
    private readonly RadioButton RB_AllBoxes;
    private readonly CheckBox CHK_Legal;
    private readonly Button BTN_MakeShiny;
    private readonly Button BTN_RemoveShiny;
    private readonly Label L_Status;
    private readonly ProgressBar PB_Progress;

    public BulkShinyPanel(SaveFile sav)
    {
        SAV = sav;

        var grpTarget = new GroupBox
        {
            Text = "Target",
            Location = new Point(10, 10),
            Size = new Size(200, 80)
        };

        RB_CurrentBox = new RadioButton
        {
            Text = "Current Box Only",
            Location = new Point(15, 25),
            Checked = true,
            AutoSize = true
        };

        RB_AllBoxes = new RadioButton
        {
            Text = "All Boxes",
            Location = new Point(15, 50),
            AutoSize = true
        };

        grpTarget.Controls.AddRange(new Control[] { RB_CurrentBox, RB_AllBoxes });

        CHK_Legal = new CheckBox
        {
            Text = "Try to maintain legality (adjust TID/SID)",
            Location = new Point(220, 30),
            AutoSize = true,
            Checked = true
        };

        BTN_MakeShiny = new Button
        {
            Text = "✨ Make All Shiny",
            Location = new Point(10, 100),
            Size = new Size(180, 45),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        BTN_MakeShiny.Click += (s, e) => ConvertToShiny(true);

        BTN_RemoveShiny = new Button
        {
            Text = "Remove Shiny Status",
            Location = new Point(200, 100),
            Size = new Size(180, 45),
            FlatStyle = FlatStyle.Flat
        };
        BTN_RemoveShiny.Click += (s, e) => ConvertToShiny(false);

        PB_Progress = new ProgressBar
        {
            Location = new Point(10, 160),
            Size = new Size(370, 25),
            Style = ProgressBarStyle.Continuous
        };

        L_Status = new Label
        {
            Text = "Ready to convert!",
            Location = new Point(10, 195),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F)
        };

        Controls.AddRange(new Control[] { grpTarget, CHK_Legal, BTN_MakeShiny, BTN_RemoveShiny, PB_Progress, L_Status });
    }

    private void ConvertToShiny(bool makeShiny)
    {
        int count = 0;
        int startBox = RB_CurrentBox.Checked ? SAV.CurrentBox : 0;
        int endBox = RB_CurrentBox.Checked ? SAV.CurrentBox + 1 : SAV.BoxCount;

        int totalSlots = (endBox - startBox) * SAV.BoxSlotCount;
        PB_Progress.Maximum = totalSlots;
        PB_Progress.Value = 0;

        for (int box = startBox; box < endBox; box++)
        {
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                var pk = SAV.GetBoxSlotAtIndex(box, slot);
                if (pk.Species != 0)
                {
                    if (makeShiny && !pk.IsShiny)
                    {
                        if (CHK_Legal.Checked)
                            pk.SetShiny();
                        else
                            pk.SetIsShiny(true);
                        SAV.SetBoxSlotAtIndex(pk, box, slot);
                        count++;
                    }
                    else if (!makeShiny && pk.IsShiny)
                    {
                        pk.SetIsShiny(false);
                        SAV.SetBoxSlotAtIndex(pk, box, slot);
                        count++;
                    }
                }
                PB_Progress.Value++;
            }
        }

        L_Status.Text = makeShiny
            ? $"✨ Made {count} Pokemon shiny!"
            : $"Removed shiny from {count} Pokemon";
    }
}
#endregion

// LivingDexTracker moved to separate file: LivingDexTracker.cs

#region Recent History Panel
public class RecentHistoryPanel : UserControl
{
    private readonly SaveFile SAV;
    private readonly IPKMView Editor;
    private readonly ListBox LB_History;
    private readonly Button BTN_Load;
    private readonly Button BTN_Clear;
    private static readonly List<PKM> _history = new();

    public RecentHistoryPanel(SaveFile sav, IPKMView editor)
    {
        SAV = sav;
        Editor = editor;

        var lbl = new Label
        {
            Text = "Recently Edited Pokemon:",
            Location = new Point(10, 10),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        LB_History = new ListBox
        {
            Location = new Point(10, 35),
            Size = new Size(400, 300),
            Font = new Font("Segoe UI", 10F)
        };
        LB_History.DoubleClick += (s, e) => LoadSelected();

        BTN_Load = new Button
        {
            Text = "Load Selected",
            Location = new Point(10, 345),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat
        };
        BTN_Load.Click += (s, e) => LoadSelected();

        BTN_Clear = new Button
        {
            Text = "Clear History",
            Location = new Point(140, 345),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat
        };
        BTN_Clear.Click += (s, e) => ClearHistory();

        Controls.AddRange(new Control[] { lbl, LB_History, BTN_Load, BTN_Clear });

        RefreshList();
    }

    public static void AddToHistory(PKM pk)
    {
        if (pk.Species == 0) return;

        // Remove if already exists
        _history.RemoveAll(p => p.Species == pk.Species && p.IsShiny == pk.IsShiny);

        // Add to front
        _history.Insert(0, pk.Clone());

        // Keep only last 50
        while (_history.Count > 50)
            _history.RemoveAt(_history.Count - 1);
    }

    private void RefreshList()
    {
        LB_History.Items.Clear();
        foreach (var pk in _history)
        {
            var shiny = pk.IsShiny ? "✨ " : "";
            var name = GameInfo.Strings.specieslist[pk.Species];
            LB_History.Items.Add($"{shiny}{name} (Lv.{pk.CurrentLevel})");
        }
    }

    private void LoadSelected()
    {
        if (LB_History.SelectedIndex < 0 || LB_History.SelectedIndex >= _history.Count)
            return;

        var pk = _history[LB_History.SelectedIndex];
        Editor.PopulateFields(pk.Clone(), false);
    }

    private void ClearHistory()
    {
        _history.Clear();
        RefreshList();
    }
}
#endregion

#region Favorites Panel
public class FavoritesPanel : UserControl
{
    private readonly SaveFile SAV;
    private readonly IPKMView Editor;
    private readonly ListBox LB_Favorites;
    private readonly Button BTN_Add;
    private readonly Button BTN_Load;
    private readonly Button BTN_Remove;
    private readonly TextBox TB_Name;
    private static readonly Dictionary<string, PKM> _favorites = new();

    public FavoritesPanel(SaveFile sav, IPKMView editor)
    {
        SAV = sav;
        Editor = editor;

        var lbl = new Label
        {
            Text = "Favorite Pokemon Builds:",
            Location = new Point(10, 10),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        LB_Favorites = new ListBox
        {
            Location = new Point(10, 35),
            Size = new Size(400, 250),
            Font = new Font("Segoe UI", 10F)
        };
        LB_Favorites.DoubleClick += (s, e) => LoadSelected();

        var lblName = new Label { Text = "Name:", Location = new Point(10, 295), AutoSize = true };

        TB_Name = new TextBox
        {
            Location = new Point(60, 292),
            Width = 200
        };

        BTN_Add = new Button
        {
            Text = "Add Current",
            Location = new Point(270, 290),
            Size = new Size(100, 28),
            FlatStyle = FlatStyle.Flat
        };
        BTN_Add.Click += (s, e) => AddCurrent();

        BTN_Load = new Button
        {
            Text = "Load",
            Location = new Point(10, 330),
            Size = new Size(80, 35),
            FlatStyle = FlatStyle.Flat
        };
        BTN_Load.Click += (s, e) => LoadSelected();

        BTN_Remove = new Button
        {
            Text = "Remove",
            Location = new Point(100, 330),
            Size = new Size(80, 35),
            FlatStyle = FlatStyle.Flat
        };
        BTN_Remove.Click += (s, e) => RemoveSelected();

        Controls.AddRange(new Control[] { lbl, LB_Favorites, lblName, TB_Name, BTN_Add, BTN_Load, BTN_Remove });

        RefreshList();
    }

    private void AddCurrent()
    {
        var pk = Editor.PreparePKM();
        if (pk.Species == 0)
        {
            MessageBox.Show("No Pokemon to save!");
            return;
        }

        var name = string.IsNullOrEmpty(TB_Name.Text)
            ? $"{GameInfo.Strings.specieslist[pk.Species]} Build"
            : TB_Name.Text;

        if (_favorites.ContainsKey(name))
        {
            if (MessageBox.Show($"Overwrite '{name}'?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
        }

        _favorites[name] = pk.Clone();
        TB_Name.Clear();
        RefreshList();
    }

    private void LoadSelected()
    {
        if (LB_Favorites.SelectedItem == null) return;
        var name = LB_Favorites.SelectedItem.ToString()!;
        if (_favorites.TryGetValue(name, out var pk))
            Editor.PopulateFields(pk.Clone(), false);
    }

    private void RemoveSelected()
    {
        if (LB_Favorites.SelectedItem == null) return;
        var name = LB_Favorites.SelectedItem.ToString()!;
        _favorites.Remove(name);
        RefreshList();
    }

    private void RefreshList()
    {
        LB_Favorites.Items.Clear();
        foreach (var name in _favorites.Keys)
            LB_Favorites.Items.Add(name);
    }
}
#endregion

#region Wonder Trade Simulator
public class WonderTradeSimulator : UserControl
{
    private readonly SaveFile SAV;
    private readonly IPKMView Editor;
    private readonly PictureBox PB_Offer;
    private readonly PictureBox PB_Receive;
    private readonly Label L_Offer;
    private readonly Label L_Receive;
    private readonly Button BTN_Trade;
    private PKM? _received;
    private readonly Random _random = new();

    public WonderTradeSimulator(SaveFile sav, IPKMView editor)
    {
        SAV = sav;
        Editor = editor;

        var lblTitle = new Label
        {
            Text = "Wonder Trade Simulator",
            Location = new Point(10, 10),
            AutoSize = true,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };

        var lblOffer = new Label { Text = "You Offer:", Location = new Point(50, 50), AutoSize = true };
        PB_Offer = new PictureBox
        {
            Location = new Point(50, 75),
            Size = new Size(120, 120),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle
        };
        L_Offer = new Label { Text = "Load a Pokemon", Location = new Point(50, 200), AutoSize = true };

        var lblArrow = new Label
        {
            Text = "⟷",
            Location = new Point(200, 120),
            Font = new Font("Segoe UI", 24F),
            AutoSize = true
        };

        var lblReceive = new Label { Text = "You Receive:", Location = new Point(280, 50), AutoSize = true };
        PB_Receive = new PictureBox
        {
            Location = new Point(280, 75),
            Size = new Size(120, 120),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(30, 30, 40)
        };
        L_Receive = new Label { Text = "???", Location = new Point(280, 200), AutoSize = true };

        BTN_Trade = new Button
        {
            Text = "✨ Wonder Trade!",
            Location = new Point(120, 250),
            Size = new Size(200, 50),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };
        BTN_Trade.Click += (s, e) => DoWonderTrade();

        var btnAccept = new Button
        {
            Text = "Accept Trade",
            Location = new Point(120, 310),
            Size = new Size(95, 35),
            FlatStyle = FlatStyle.Flat
        };
        btnAccept.Click += (s, e) => AcceptTrade();

        var btnReject = new Button
        {
            Text = "Reject",
            Location = new Point(225, 310),
            Size = new Size(95, 35),
            FlatStyle = FlatStyle.Flat
        };
        btnReject.Click += (s, e) => RejectTrade();

        Controls.AddRange(new Control[] { lblTitle, lblOffer, PB_Offer, L_Offer, lblArrow, lblReceive, PB_Receive, L_Receive, BTN_Trade, btnAccept, btnReject });
    }

    public void SetOffer(PKM pk)
    {
        if (pk.Species == 0) return;
        PB_Offer.Image = pk.Sprite(SAV);
        var shiny = pk.IsShiny ? "✨ " : "";
        L_Offer.Text = $"{shiny}{GameInfo.Strings.specieslist[pk.Species]}";
    }

    private void DoWonderTrade()
    {
        var offer = Editor.PreparePKM();
        if (offer.Species == 0)
        {
            MessageBox.Show("Load a Pokemon to trade first!");
            return;
        }

        SetOffer(offer);

        // Generate random Pokemon
        int species = _random.Next(1, Math.Min((int)SAV.MaxSpeciesID, 1000));
        var pk = SAV.BlankPKM;
        pk.Species = (ushort)species;
        pk.CurrentLevel = (byte)_random.Next(1, 100);

        // 5% chance shiny
        if (_random.Next(100) < 5)
            pk.SetShiny();

        // Random IVs
        pk.IV_HP = (byte)_random.Next(32);
        pk.IV_ATK = (byte)_random.Next(32);
        pk.IV_DEF = (byte)_random.Next(32);
        pk.IV_SPA = (byte)_random.Next(32);
        pk.IV_SPD = (byte)_random.Next(32);
        pk.IV_SPE = (byte)_random.Next(32);

        pk.RefreshAbility(_random.Next(3));
        pk.Nature = (Nature)_random.Next(25);
        pk.OriginalTrainerName = GetRandomTrainerName();
        pk.Language = _random.Next(1, 10);

        _received = pk;
        PB_Receive.Image = pk.Sprite(SAV);
        var shiny = pk.IsShiny ? "✨ " : "";
        L_Receive.Text = $"{shiny}{GameInfo.Strings.specieslist[pk.Species]} (Lv.{pk.CurrentLevel})";
    }

    private void AcceptTrade()
    {
        if (_received == null)
        {
            MessageBox.Show("Do a Wonder Trade first!");
            return;
        }
        Editor.PopulateFields(_received, false);
        MessageBox.Show("Trade accepted! Pokemon loaded into editor.");
    }

    private void RejectTrade()
    {
        _received = null;
        PB_Receive.Image = null;
        L_Receive.Text = "???";
    }

    private string GetRandomTrainerName()
    {
        var names = new[] { "Ash", "Misty", "Brock", "Gary", "Red", "Blue", "Green", "Gold", "Silver", "May", "Brendan", "Dawn", "Lucas", "Cynthia", "Steven", "Lance", "Leon", "Hop", "Nemona" };
        return names[_random.Next(names.Length)];
    }
}
#endregion

#region Team Exporter
public class TeamExporter : UserControl
{
    private readonly SaveFile SAV;
    private readonly Button BTN_ExportParty;
    private readonly Button BTN_ExportBox;
    private readonly PictureBox PB_Preview;
    private readonly CheckBox CHK_IncludeStats;
    private readonly CheckBox CHK_IncludeMoves;

    public TeamExporter(SaveFile sav)
    {
        SAV = sav;

        var lbl = new Label
        {
            Text = "Export Team to Image",
            Location = new Point(10, 10),
            AutoSize = true,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };

        CHK_IncludeStats = new CheckBox
        {
            Text = "Include Stats",
            Location = new Point(10, 40),
            Checked = true,
            AutoSize = true
        };

        CHK_IncludeMoves = new CheckBox
        {
            Text = "Include Moves",
            Location = new Point(130, 40),
            Checked = true,
            AutoSize = true
        };

        BTN_ExportParty = new Button
        {
            Text = "Export Party",
            Location = new Point(10, 70),
            Size = new Size(120, 40),
            FlatStyle = FlatStyle.Flat
        };
        BTN_ExportParty.Click += (s, e) => ExportParty();

        BTN_ExportBox = new Button
        {
            Text = "Export Current Box",
            Location = new Point(140, 70),
            Size = new Size(140, 40),
            FlatStyle = FlatStyle.Flat
        };
        BTN_ExportBox.Click += (s, e) => ExportBox();

        PB_Preview = new PictureBox
        {
            Location = new Point(10, 120),
            Size = new Size(500, 300),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(30, 30, 40)
        };

        Controls.AddRange(new Control[] { lbl, CHK_IncludeStats, CHK_IncludeMoves, BTN_ExportParty, BTN_ExportBox, PB_Preview });
    }

    private void ExportParty()
    {
        if (!SAV.HasParty)
        {
            MessageBox.Show("This save doesn't have a party!");
            return;
        }

        var party = new List<PKM>();
        for (int i = 0; i < 6; i++)
        {
            var pk = SAV.GetPartySlotAtIndex(i);
            if (pk.Species != 0)
                party.Add(pk);
        }

        if (party.Count == 0)
        {
            MessageBox.Show("Party is empty!");
            return;
        }

        var img = GenerateTeamImage(party, "Party");
        PB_Preview.Image = img;
        SaveImage(img, "Party");
    }

    private void ExportBox()
    {
        var pokemon = new List<PKM>();
        for (int i = 0; i < SAV.BoxSlotCount; i++)
        {
            var pk = SAV.GetBoxSlotAtIndex(SAV.CurrentBox, i);
            if (pk.Species != 0)
                pokemon.Add(pk);
        }

        if (pokemon.Count == 0)
        {
            MessageBox.Show("Box is empty!");
            return;
        }

        var img = GenerateTeamImage(pokemon.Take(6).ToList(), $"Box {SAV.CurrentBox + 1}");
        PB_Preview.Image = img;
        SaveImage(img, $"Box{SAV.CurrentBox + 1}");
    }

    private Bitmap GenerateTeamImage(List<PKM> team, string title)
    {
        int width = 600;
        int height = 150 + (team.Count * 100);
        var bmp = new Bitmap(width, height);

        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Background gradient
        using (var brush = new LinearGradientBrush(new Rectangle(0, 0, width, height),
            Color.FromArgb(30, 30, 50), Color.FromArgb(50, 30, 60), LinearGradientMode.Vertical))
        {
            g.FillRectangle(brush, 0, 0, width, height);
        }

        // Title
        using (var font = new Font("Segoe UI", 20F, FontStyle.Bold))
        {
            g.DrawString($"PKM-Universe - {title}", font, Brushes.White, 20, 20);
        }

        // Pokemon
        int y = 80;
        foreach (var pk in team)
        {
            // Sprite
            var sprite = pk.Sprite(SAV);
            if (sprite != null)
                g.DrawImage(sprite, 20, y, 68, 56);

            // Name
            var shiny = pk.IsShiny ? "✨ " : "";
            var name = $"{shiny}{GameInfo.Strings.specieslist[pk.Species]}";
            using (var font = new Font("Segoe UI", 14F, FontStyle.Bold))
            {
                g.DrawString(name, font, Brushes.White, 100, y);
            }

            // Level
            using (var font = new Font("Segoe UI", 10F))
            {
                g.DrawString($"Lv. {pk.CurrentLevel}", font, Brushes.LightGray, 100, y + 25);
            }

            if (CHK_IncludeStats.Checked)
            {
                using var font = new Font("Segoe UI", 8F);
                var ivs = $"IVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}";
                g.DrawString(ivs, font, Brushes.LightGray, 200, y + 5);
            }

            if (CHK_IncludeMoves.Checked)
            {
                using var font = new Font("Segoe UI", 8F);
                var moves = new List<string>();
                if (pk.Move1 != 0) moves.Add(GameInfo.Strings.movelist[pk.Move1]);
                if (pk.Move2 != 0) moves.Add(GameInfo.Strings.movelist[pk.Move2]);
                if (pk.Move3 != 0) moves.Add(GameInfo.Strings.movelist[pk.Move3]);
                if (pk.Move4 != 0) moves.Add(GameInfo.Strings.movelist[pk.Move4]);
                g.DrawString(string.Join(", ", moves), font, Brushes.LightGray, 200, y + 25);
            }

            y += 80;
        }

        return bmp;
    }

    private void SaveImage(Bitmap img, string name)
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "PNG Image|*.png",
            FileName = $"PKM_Universe_Team_{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            img.Save(sfd.FileName, ImageFormat.Png);
            MessageBox.Show($"Saved to: {sfd.FileName}");
        }
    }
}
#endregion

#region Stats Radar Chart
public class StatsRadarChart : UserControl
{
    private PKM? _pokemon;
    private readonly Label L_Pokemon;
    private readonly RadioButton RB_IVs;
    private readonly RadioButton RB_EVs;
    private readonly RadioButton RB_Base;

    public StatsRadarChart()
    {
        DoubleBuffered = true;

        L_Pokemon = new Label
        {
            Text = "Load a Pokemon to view stats",
            Location = new Point(10, 10),
            AutoSize = true,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };

        RB_IVs = new RadioButton
        {
            Text = "IVs",
            Location = new Point(10, 40),
            Checked = true,
            AutoSize = true
        };
        RB_IVs.CheckedChanged += (s, e) => Invalidate();

        RB_EVs = new RadioButton
        {
            Text = "EVs",
            Location = new Point(70, 40),
            AutoSize = true
        };
        RB_EVs.CheckedChanged += (s, e) => Invalidate();

        RB_Base = new RadioButton
        {
            Text = "Base Stats",
            Location = new Point(130, 40),
            AutoSize = true
        };
        RB_Base.CheckedChanged += (s, e) => Invalidate();

        Controls.AddRange(new Control[] { L_Pokemon, RB_IVs, RB_EVs, RB_Base });

        Paint += OnPaint;
    }

    public void SetPokemon(PKM pk)
    {
        _pokemon = pk;
        if (pk.Species > 0)
        {
            var shiny = pk.IsShiny ? "✨ " : "";
            L_Pokemon.Text = $"{shiny}{GameInfo.Strings.specieslist[pk.Species]} - Stats Radar";
        }
        Invalidate();
    }

    private void OnPaint(object? sender, PaintEventArgs e)
    {
        if (_pokemon == null || _pokemon.Species == 0)
            return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Get stats
        int[] stats;
        int maxStat;
        string[] labels = { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };

        if (RB_IVs.Checked)
        {
            stats = new[] { _pokemon.IV_HP, _pokemon.IV_ATK, _pokemon.IV_DEF, _pokemon.IV_SPA, _pokemon.IV_SPD, _pokemon.IV_SPE };
            maxStat = 31;
        }
        else if (RB_EVs.Checked)
        {
            stats = new[] { _pokemon.EV_HP, _pokemon.EV_ATK, _pokemon.EV_DEF, _pokemon.EV_SPA, _pokemon.EV_SPD, _pokemon.EV_SPE };
            maxStat = 252;
        }
        else
        {
            var pi = _pokemon.PersonalInfo;
            stats = new[] { pi.HP, pi.ATK, pi.DEF, pi.SPA, pi.SPD, pi.SPE };
            maxStat = 255;
        }

        // Draw radar chart
        int centerX = Width / 2;
        int centerY = Height / 2 + 30;
        int radius = Math.Min(Width, Height) / 3;

        // Draw background hexagon
        var bgPoints = new PointF[6];
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 2 + i * Math.PI / 3;
            bgPoints[i] = new PointF(
                centerX + (float)(radius * Math.Cos(angle)),
                centerY - (float)(radius * Math.Sin(angle))
            );
        }

        using (var bgBrush = new SolidBrush(Color.FromArgb(40, 100, 100, 150)))
        {
            g.FillPolygon(bgBrush, bgPoints);
        }

        // Draw grid lines
        using var gridPen = new Pen(Color.FromArgb(60, 150, 150, 200), 1);
        for (int level = 1; level <= 4; level++)
        {
            var gridPoints = new PointF[6];
            float r = radius * level / 4f;
            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 2 + i * Math.PI / 3;
                gridPoints[i] = new PointF(
                    centerX + (float)(r * Math.Cos(angle)),
                    centerY - (float)(r * Math.Sin(angle))
                );
            }
            g.DrawPolygon(gridPen, gridPoints);
        }

        // Draw stat polygon
        var statPoints = new PointF[6];
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 2 + i * Math.PI / 3;
            float r = radius * stats[i] / (float)maxStat;
            statPoints[i] = new PointF(
                centerX + (float)(r * Math.Cos(angle)),
                centerY - (float)(r * Math.Sin(angle))
            );
        }

        using (var fillBrush = new SolidBrush(Color.FromArgb(100, 138, 43, 226)))
        {
            g.FillPolygon(fillBrush, statPoints);
        }

        using (var outlinePen = new Pen(Color.FromArgb(200, 138, 43, 226), 2))
        {
            g.DrawPolygon(outlinePen, statPoints);
        }

        // Draw labels
        using var font = new Font("Segoe UI", 9F, FontStyle.Bold);
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 2 + i * Math.PI / 3;
            float x = centerX + (float)((radius + 25) * Math.Cos(angle));
            float y = centerY - (float)((radius + 25) * Math.Sin(angle));

            var text = $"{labels[i]}: {stats[i]}";
            var size = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.White, x - size.Width / 2, y - size.Height / 2);
        }
    }
}
#endregion
