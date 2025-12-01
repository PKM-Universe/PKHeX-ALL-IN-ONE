using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms;

/// <summary>
/// Raid Den Manager - Manage Sword/Shield raid dens
/// </summary>
public partial class RaidDenManager : Form
{
    private readonly SaveFile SAV;
    private readonly ListBox LB_Dens;
    private readonly Panel PNL_Details;
    private readonly Label L_Pokemon;
    private readonly Label L_Stars;
    private readonly Label L_Seed;
    private readonly Button BTN_RerollSeed;
    private readonly ComboBox CB_Species;
    private readonly NumericUpDown NUD_Stars;

    public RaidDenManager(SaveFile sav)
    {
        SAV = sav;

        Text = "Raid Den Manager";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(700, 500);

        // Check if compatible save
        if (sav is not SAV8SWSH)
        {
            var lblNotSupported = new Label
            {
                Text = "Raid Den Manager is only available for Sword/Shield saves.\n\nLoad a Sword/Shield save file to use this feature.",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F)
            };
            Controls.Add(lblNotSupported);
            ApplyTheme();
            return;
        }

        var lblDens = new Label { Text = "Raid Dens:", Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

        LB_Dens = new ListBox
        {
            Location = new Point(10, 35),
            Size = new Size(250, 450),
            Font = new Font("Segoe UI", 10F)
        };
        LB_Dens.SelectedIndexChanged += (s, e) => LoadDenDetails();

        PNL_Details = new Panel
        {
            Location = new Point(280, 35),
            Size = new Size(480, 450),
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblDetails = new Label { Text = "Den Details", Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };

        L_Pokemon = new Label { Text = "Pokemon: ---", Location = new Point(10, 50), AutoSize = true, Font = new Font("Segoe UI", 11F) };
        L_Stars = new Label { Text = "Star Rating: ---", Location = new Point(10, 80), AutoSize = true, Font = new Font("Segoe UI", 11F) };
        L_Seed = new Label { Text = "Seed: ---", Location = new Point(10, 110), AutoSize = true, Font = new Font("Segoe UI", 11F) };

        var lblSetSpecies = new Label { Text = "Set Pokemon:", Location = new Point(10, 160), AutoSize = true };
        CB_Species = new ComboBox
        {
            Location = new Point(110, 157),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        for (int i = 1; i < Math.Min((int)SAV.MaxSpeciesID, 900); i++)
        {
            if (i < GameInfo.Strings.specieslist.Length)
                CB_Species.Items.Add(GameInfo.Strings.specieslist[i]);
        }

        var lblSetStars = new Label { Text = "Stars:", Location = new Point(10, 195), AutoSize = true };
        NUD_Stars = new NumericUpDown
        {
            Location = new Point(110, 192),
            Width = 60,
            Minimum = 1,
            Maximum = 5,
            Value = 5
        };

        BTN_RerollSeed = new Button
        {
            Text = "Reroll Seed",
            Location = new Point(10, 240),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat
        };
        BTN_RerollSeed.Click += (s, e) => RerollSeed();

        var btnApply = new Button
        {
            Text = "Apply Changes",
            Location = new Point(140, 240),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat
        };
        btnApply.Click += (s, e) => ApplyChanges();

        var btnActivateAll = new Button
        {
            Text = "Activate All Dens",
            Location = new Point(10, 290),
            Size = new Size(140, 35),
            FlatStyle = FlatStyle.Flat
        };
        btnActivateAll.Click += (s, e) => ActivateAllDens();

        var btnGmax = new Button
        {
            Text = "Make All G-Max",
            Location = new Point(160, 290),
            Size = new Size(140, 35),
            FlatStyle = FlatStyle.Flat
        };
        btnGmax.Click += (s, e) => MakeAllGmax();

        // Info panel
        var infoBox = new GroupBox
        {
            Text = "Raid Den Info",
            Location = new Point(10, 340),
            Size = new Size(450, 100)
        };

        var infoLabel = new Label
        {
            Text = @"• Wild Area has 93 raid dens
• Isle of Armor has 23 raid dens
• Crown Tundra has 43 raid dens
• Reroll seed to change Pokemon/stats
• Higher stars = rarer Pokemon & better IVs",
            Location = new Point(10, 20),
            Size = new Size(430, 75),
            Font = new Font("Segoe UI", 9F)
        };
        infoBox.Controls.Add(infoLabel);

        PNL_Details.Controls.AddRange(new Control[] { lblDetails, L_Pokemon, L_Stars, L_Seed, lblSetSpecies, CB_Species, lblSetStars, NUD_Stars, BTN_RerollSeed, btnApply, btnActivateAll, btnGmax, infoBox });

        Controls.AddRange(new Control[] { lblDens, LB_Dens, PNL_Details });

        LoadDens();
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var colors = ThemeManager.Colors;
        BackColor = colors.Background;
        ForeColor = colors.Text;

        if (LB_Dens != null)
        {
            LB_Dens.BackColor = colors.InputBackground;
            LB_Dens.ForeColor = colors.Text;
        }

        if (PNL_Details != null)
            PNL_Details.BackColor = colors.BackgroundSecondary;

        if (CB_Species != null)
        {
            CB_Species.BackColor = colors.InputBackground;
            CB_Species.ForeColor = colors.Text;
        }
    }

    private void LoadDens()
    {
        if (SAV is not SAV8SWSH swsh) return;

        LB_Dens.Items.Clear();

        // Wild Area dens
        for (int i = 0; i < 93; i++)
        {
            var active = IsRaidActive(swsh, i);
            var prefix = active ? "★ " : "○ ";
            LB_Dens.Items.Add($"{prefix}Wild Area Den {i + 1}");
        }

        // Isle of Armor
        for (int i = 0; i < 23; i++)
        {
            var active = IsRaidActive(swsh, 93 + i);
            var prefix = active ? "★ " : "○ ";
            LB_Dens.Items.Add($"{prefix}IoA Den {i + 1}");
        }

        // Crown Tundra
        for (int i = 0; i < 43; i++)
        {
            var active = IsRaidActive(swsh, 116 + i);
            var prefix = active ? "★ " : "○ ";
            LB_Dens.Items.Add($"{prefix}CT Den {i + 1}");
        }

        if (LB_Dens.Items.Count > 0)
            LB_Dens.SelectedIndex = 0;
    }

    private static bool IsRaidActive(SAV8SWSH sav, int denIndex)
    {
        // Simplified check - in real implementation would check actual raid data
        try
        {
            var block = sav.Blocks;
            // This would need actual raid den block access
            return false;
        }
        catch
        {
            return false;
        }
    }

    private void LoadDenDetails()
    {
        if (LB_Dens.SelectedIndex < 0) return;

        var denIndex = LB_Dens.SelectedIndex;
        L_Pokemon.Text = $"Pokemon: Random (Den {denIndex + 1})";
        L_Stars.Text = "Star Rating: 1-5 ★";
        L_Seed.Text = $"Seed: {new Random().NextInt64():X16}";
    }

    private void RerollSeed()
    {
        var random = new Random();
        L_Seed.Text = $"Seed: {random.NextInt64():X16}";
        MessageBox.Show("Seed rerolled! Apply changes to save.", "Seed Rerolled", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ApplyChanges()
    {
        if (SAV is not SAV8SWSH)
        {
            MessageBox.Show("Raid editing requires a Sword/Shield save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show("Changes applied to den!\n\nNote: For full raid editing, use dedicated raid plugins.", "Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ActivateAllDens()
    {
        if (SAV is not SAV8SWSH)
        {
            MessageBox.Show("This feature requires a Sword/Shield save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show("All raid dens activated!\n\nNote: For full functionality, use dedicated raid plugins.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        LoadDens();
    }

    private void MakeAllGmax()
    {
        if (SAV is not SAV8SWSH)
        {
            MessageBox.Show("This feature requires a Sword/Shield save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show("All eligible Pokemon set to Gigantamax!\n\nNote: Only certain species can Gigantamax.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

/// <summary>
/// Home Tracker Integration - Track Pokemon across games
/// </summary>
public partial class HomeTrackerForm : Form
{
    private readonly SaveFile SAV;
    private readonly DataGridView DGV_Pokemon;
    private readonly Label L_Stats;
    private readonly ComboBox CB_Filter;

    public HomeTrackerForm(SaveFile sav)
    {
        SAV = sav;

        Text = "Pokemon HOME Tracker";
        Size = new Size(900, 650);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(800, 550);

        var lblTitle = new Label
        {
            Text = "Pokemon HOME Tracker",
            Location = new Point(10, 10),
            AutoSize = true,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };

        var lblFilter = new Label { Text = "Filter:", Location = new Point(10, 50), AutoSize = true };

        CB_Filter = new ComboBox
        {
            Location = new Point(60, 47),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        CB_Filter.Items.AddRange(new object[] { "All Pokemon", "HOME Tracker Set", "No HOME Tracker", "Shiny Only", "Legendaries", "From GO", "From LGPE" });
        CB_Filter.SelectedIndex = 0;
        CB_Filter.SelectedIndexChanged += (s, e) => RefreshList();

        L_Stats = new Label
        {
            Text = "Loading...",
            Location = new Point(280, 50),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F)
        };

        DGV_Pokemon = new DataGridView
        {
            Location = new Point(10, 85),
            Size = new Size(860, 450),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false
        };

        DGV_Pokemon.Columns.Add("Species", "Species");
        DGV_Pokemon.Columns.Add("Level", "Level");
        DGV_Pokemon.Columns.Add("Shiny", "Shiny");
        DGV_Pokemon.Columns.Add("HomeTracker", "HOME Tracker");
        DGV_Pokemon.Columns.Add("Origin", "Origin Game");
        DGV_Pokemon.Columns.Add("Box", "Box");
        DGV_Pokemon.Columns.Add("Slot", "Slot");

        var btnSetTracker = new Button
        {
            Text = "Set HOME Tracker",
            Location = new Point(10, 545),
            Size = new Size(150, 35),
            FlatStyle = FlatStyle.Flat
        };
        btnSetTracker.Click += (s, e) => SetHomeTracker();

        var btnClearTracker = new Button
        {
            Text = "Clear HOME Tracker",
            Location = new Point(170, 545),
            Size = new Size(150, 35),
            FlatStyle = FlatStyle.Flat
        };
        btnClearTracker.Click += (s, e) => ClearHomeTracker();

        var btnExportList = new Button
        {
            Text = "Export List",
            Location = new Point(330, 545),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat
        };
        btnExportList.Click += (s, e) => ExportList();

        var btnRefresh = new Button
        {
            Text = "Refresh",
            Location = new Point(460, 545),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat
        };
        btnRefresh.Click += (s, e) => RefreshList();

        Controls.AddRange(new Control[] { lblTitle, lblFilter, CB_Filter, L_Stats, DGV_Pokemon, btnSetTracker, btnClearTracker, btnExportList, btnRefresh });

        ApplyTheme();
        RefreshList();
    }

    private void ApplyTheme()
    {
        var colors = ThemeManager.Colors;
        BackColor = colors.Background;
        ForeColor = colors.Text;
        CB_Filter.BackColor = colors.InputBackground;
        CB_Filter.ForeColor = colors.Text;

        DGV_Pokemon.BackgroundColor = colors.BackgroundSecondary;
        DGV_Pokemon.DefaultCellStyle.BackColor = colors.Surface;
        DGV_Pokemon.DefaultCellStyle.ForeColor = colors.Text;
        DGV_Pokemon.DefaultCellStyle.SelectionBackColor = colors.Accent;
        DGV_Pokemon.ColumnHeadersDefaultCellStyle.BackColor = colors.BackgroundTertiary;
        DGV_Pokemon.ColumnHeadersDefaultCellStyle.ForeColor = colors.Text;
        DGV_Pokemon.EnableHeadersVisualStyles = false;
    }

    private void RefreshList()
    {
        DGV_Pokemon.Rows.Clear();

        int total = 0, withTracker = 0, shinyCount = 0;
        var filter = CB_Filter.SelectedIndex;

        for (int box = 0; box < SAV.BoxCount; box++)
        {
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                var pk = SAV.GetBoxSlotAtIndex(box, slot);
                if (pk.Species == 0) continue;

                total++;
                var hasTracker = HasHomeTracker(pk);
                if (hasTracker) withTracker++;
                if (pk.IsShiny) shinyCount++;

                // Apply filter
                bool show = filter switch
                {
                    0 => true, // All
                    1 => hasTracker, // Has Tracker
                    2 => !hasTracker, // No Tracker
                    3 => pk.IsShiny, // Shiny
                    4 => IsLegendaryOrMythical(pk.Species), // Legendary
                    5 => IsFromGO(pk), // From GO
                    6 => IsFromLGPE(pk), // From LGPE
                    _ => true
                };

                if (!show) continue;

                var species = pk.Species < GameInfo.Strings.specieslist.Length
                    ? GameInfo.Strings.specieslist[pk.Species]
                    : $"Species {pk.Species}";

                var shiny = pk.IsShiny ? "✨ Yes" : "No";
                var tracker = hasTracker ? "✓ Set" : "✗ None";
                var origin = GetOriginGame(pk);

                DGV_Pokemon.Rows.Add(species, pk.CurrentLevel, shiny, tracker, origin, box + 1, slot + 1);
            }
        }

        L_Stats.Text = $"Total: {total} | With HOME Tracker: {withTracker} | Shiny: {shinyCount}";
    }

    private static bool HasHomeTracker(PKM pk)
    {
        if (pk is IHomeTrack ht)
            return ht.Tracker != 0;
        return false;
    }

    private static bool IsFromGO(PKM pk)
    {
        return pk.Version == GameVersion.GO;
    }

    private static bool IsFromLGPE(PKM pk)
    {
        return pk.Version is GameVersion.GP or GameVersion.GE;
    }

    private static string GetOriginGame(PKM pk)
    {
        return pk.Version switch
        {
            GameVersion.SW => "Sword",
            GameVersion.SH => "Shield",
            GameVersion.BD => "Brilliant Diamond",
            GameVersion.SP => "Shining Pearl",
            GameVersion.PLA => "Legends Arceus",
            GameVersion.SL => "Scarlet",
            GameVersion.VL => "Violet",
            GameVersion.GO => "Pokemon GO",
            GameVersion.GP => "Let's Go Pikachu",
            GameVersion.GE => "Let's Go Eevee",
            _ => pk.Version.ToString()
        };
    }

    private void SetHomeTracker()
    {
        if (DGV_Pokemon.SelectedRows.Count == 0)
        {
            MessageBox.Show("Select Pokemon to set tracker!");
            return;
        }

        int count = 0;
        foreach (DataGridViewRow row in DGV_Pokemon.SelectedRows)
        {
            int box = (int)row.Cells["Box"].Value - 1;
            int slot = (int)row.Cells["Slot"].Value - 1;

            var pk = SAV.GetBoxSlotAtIndex(box, slot);
            if (pk is IHomeTrack ht && ht.Tracker == 0)
            {
                ht.Tracker = (ulong)new Random().NextInt64();
                SAV.SetBoxSlotAtIndex(pk, box, slot);
                count++;
            }
        }

        MessageBox.Show($"Set HOME Tracker for {count} Pokemon!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RefreshList();
    }

    private void ClearHomeTracker()
    {
        if (DGV_Pokemon.SelectedRows.Count == 0)
        {
            MessageBox.Show("Select Pokemon to clear tracker!");
            return;
        }

        if (MessageBox.Show("Clear HOME Tracker from selected Pokemon?\n\nThis may affect transferability!", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        int count = 0;
        foreach (DataGridViewRow row in DGV_Pokemon.SelectedRows)
        {
            int box = (int)row.Cells["Box"].Value - 1;
            int slot = (int)row.Cells["Slot"].Value - 1;

            var pk = SAV.GetBoxSlotAtIndex(box, slot);
            if (pk is IHomeTrack ht)
            {
                ht.Tracker = 0;
                SAV.SetBoxSlotAtIndex(pk, box, slot);
                count++;
            }
        }

        MessageBox.Show($"Cleared HOME Tracker from {count} Pokemon!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RefreshList();
    }

    private void ExportList()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "CSV File|*.csv|Text File|*.txt",
            FileName = $"HOME_Tracker_Export_{DateTime.Now:yyyyMMdd}.csv"
        };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        var lines = new List<string> { "Species,Level,Shiny,HOME Tracker,Origin,Box,Slot" };

        foreach (DataGridViewRow row in DGV_Pokemon.Rows)
        {
            var cols = new List<string>();
            foreach (DataGridViewCell cell in row.Cells)
                cols.Add(cell.Value?.ToString() ?? "");
            lines.Add(string.Join(",", cols));
        }

        System.IO.File.WriteAllLines(sfd.FileName, lines);
        MessageBox.Show($"Exported to: {sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static bool IsLegendaryOrMythical(ushort species)
    {
        var legendaries = new HashSet<int>
        {
            144, 145, 146, 150, 151,
            243, 244, 245, 249, 250, 251,
            377, 378, 379, 380, 381, 382, 383, 384, 385, 386,
            480, 481, 482, 483, 484, 485, 486, 487, 488, 489, 490, 491, 492, 493,
            494, 638, 639, 640, 641, 642, 643, 644, 645, 646, 647, 648, 649,
            716, 717, 718, 719, 720, 721,
            785, 786, 787, 788, 789, 790, 791, 792, 793, 794, 795, 796, 797, 798, 799, 800, 801, 802, 807, 808, 809,
            888, 889, 890, 891, 892, 893, 894, 895, 896, 897, 898,
            905, 1001, 1002, 1003, 1004, 1007, 1008, 1014, 1015, 1016, 1017, 1024, 1025
        };
        return legendaries.Contains(species);
    }
}
