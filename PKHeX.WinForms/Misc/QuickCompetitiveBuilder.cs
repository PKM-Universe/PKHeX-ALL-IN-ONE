using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms;

/// <summary>
/// Quick Competitive Builder - One-click competitive Pokemon generation
/// </summary>
public partial class QuickCompetitiveBuilder : Form
{
    private readonly SaveFile SAV;
    private readonly IPKMView Editor;
    private readonly ComboBox CB_Species;
    private readonly ListBox LB_Presets;
    private readonly TextBox TB_Preview;
    private readonly Button BTN_Apply;

    private static readonly Dictionary<int, List<CompetitivePreset>> Presets = InitializePresets();

    public QuickCompetitiveBuilder(SaveFile sav, IPKMView editor)
    {
        SAV = sav;
        Editor = editor;

        Text = "Quick Competitive Builder";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(700, 500);

        var lblSpecies = new Label { Text = "Pokemon:", Location = new Point(10, 15), AutoSize = true };

        CB_Species = new ComboBox
        {
            Location = new Point(80, 12),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // Populate species with preset Pokemon
        var speciesWithPresets = Presets.Keys
            .Where(s => s <= GameInfo.Strings.specieslist.Length)
            .OrderBy(s => GameInfo.Strings.specieslist[s])
            .ToList();

        foreach (var species in speciesWithPresets)
            CB_Species.Items.Add(GameInfo.Strings.specieslist[species]);

        CB_Species.SelectedIndexChanged += (s, e) => LoadPresetsForSpecies();

        var lblPresets = new Label { Text = "Competitive Sets:", Location = new Point(10, 50), AutoSize = true };

        LB_Presets = new ListBox
        {
            Location = new Point(10, 75),
            Size = new Size(300, 350),
            Font = new Font("Segoe UI", 10F)
        };
        LB_Presets.SelectedIndexChanged += (s, e) => ShowPreview();

        var lblPreview = new Label { Text = "Set Preview:", Location = new Point(330, 50), AutoSize = true };

        TB_Preview = new TextBox
        {
            Location = new Point(330, 75),
            Size = new Size(430, 350),
            Multiline = true,
            ReadOnly = true,
            Font = new Font("Consolas", 10F),
            ScrollBars = ScrollBars.Vertical
        };

        BTN_Apply = new Button
        {
            Text = "Apply This Set",
            Location = new Point(10, 440),
            Size = new Size(150, 45),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        BTN_Apply.Click += (s, e) => ApplyPreset();

        var btnQuick6IV = new Button
        {
            Text = "Quick: 6IV + Shiny",
            Location = new Point(170, 440),
            Size = new Size(130, 45),
            FlatStyle = FlatStyle.Flat
        };
        btnQuick6IV.Click += (s, e) => QuickApply6IVShiny();

        var btnQuickLegal = new Button
        {
            Text = "Quick: Legal Comp",
            Location = new Point(310, 440),
            Size = new Size(130, 45),
            FlatStyle = FlatStyle.Flat
        };
        btnQuickLegal.Click += (s, e) => QuickApplyLegalComp();

        Controls.AddRange(new Control[] { lblSpecies, CB_Species, lblPresets, LB_Presets, lblPreview, TB_Preview, BTN_Apply, btnQuick6IV, btnQuickLegal });

        // Add category buttons
        AddCategoryButtons();
        ApplyTheme();

        if (CB_Species.Items.Count > 0)
            CB_Species.SelectedIndex = 0;
    }

    private void AddCategoryButtons()
    {
        var categories = new[] { "OU", "UU", "Ubers", "VGC", "Random" };
        int x = 450;

        foreach (var cat in categories)
        {
            var btn = new Button
            {
                Text = cat,
                Location = new Point(x, 440),
                Size = new Size(70, 45),
                FlatStyle = FlatStyle.Flat,
                Tag = cat
            };
            btn.Click += (s, e) => FilterByCategory(cat);
            Controls.Add(btn);
            x += 75;
        }
    }

    private void ApplyTheme()
    {
        var colors = ThemeManager.Colors;
        BackColor = colors.Background;
        ForeColor = colors.Text;
        CB_Species.BackColor = colors.InputBackground;
        CB_Species.ForeColor = colors.Text;
        LB_Presets.BackColor = colors.InputBackground;
        LB_Presets.ForeColor = colors.Text;
        TB_Preview.BackColor = colors.Surface;
        TB_Preview.ForeColor = colors.Text;
    }

    private void LoadPresetsForSpecies()
    {
        LB_Presets.Items.Clear();
        TB_Preview.Clear();

        if (CB_Species.SelectedIndex < 0) return;

        var speciesName = CB_Species.SelectedItem?.ToString();
        var species = Array.FindIndex(GameInfo.Strings.specieslist, s => s == speciesName);

        if (species > 0 && Presets.TryGetValue(species, out var presets))
        {
            foreach (var preset in presets)
                LB_Presets.Items.Add($"{preset.Name} ({preset.Tier})");
        }
        else
        {
            LB_Presets.Items.Add("No presets available - Try Quick options!");
        }

        if (LB_Presets.Items.Count > 0)
            LB_Presets.SelectedIndex = 0;
    }

    private void ShowPreview()
    {
        if (CB_Species.SelectedIndex < 0 || LB_Presets.SelectedIndex < 0)
            return;

        var speciesName = CB_Species.SelectedItem?.ToString();
        var species = Array.FindIndex(GameInfo.Strings.specieslist, s => s == speciesName);

        if (Presets.TryGetValue(species, out var presets) && LB_Presets.SelectedIndex < presets.Count)
        {
            var preset = presets[LB_Presets.SelectedIndex];
            TB_Preview.Text = preset.GetShowdownFormat(speciesName ?? "Pokemon");
        }
    }

    private void ApplyPreset()
    {
        if (CB_Species.SelectedIndex < 0 || LB_Presets.SelectedIndex < 0)
            return;

        var speciesName = CB_Species.SelectedItem?.ToString();
        var species = Array.FindIndex(GameInfo.Strings.specieslist, s => s == speciesName);

        if (!Presets.TryGetValue(species, out var presets) || LB_Presets.SelectedIndex >= presets.Count)
        {
            MessageBox.Show("No preset selected!");
            return;
        }

        var preset = presets[LB_Presets.SelectedIndex];
        var pk = CreatePokemonFromPreset(species, preset);

        if (pk != null)
        {
            Editor.PopulateFields(pk, false);
            MessageBox.Show($"Applied: {preset.Name}!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private PKM? CreatePokemonFromPreset(int species, CompetitivePreset preset)
    {
        var pk = SAV.BlankPKM;
        pk.Species = (ushort)species;
        pk.CurrentLevel = 100;

        // Set nature
        if (Enum.TryParse<Nature>(preset.Nature, out var nature))
            pk.Nature = nature;

        // Set ability
        pk.RefreshAbility(preset.AbilitySlot);

        // Set IVs
        pk.IV_HP = (byte)preset.IVs[0];
        pk.IV_ATK = (byte)preset.IVs[1];
        pk.IV_DEF = (byte)preset.IVs[2];
        pk.IV_SPA = (byte)preset.IVs[3];
        pk.IV_SPD = (byte)preset.IVs[4];
        pk.IV_SPE = (byte)preset.IVs[5];

        // Set EVs
        pk.EV_HP = (byte)preset.EVs[0];
        pk.EV_ATK = (byte)preset.EVs[1];
        pk.EV_DEF = (byte)preset.EVs[2];
        pk.EV_SPA = (byte)preset.EVs[3];
        pk.EV_SPD = (byte)preset.EVs[4];
        pk.EV_SPE = (byte)preset.EVs[5];

        // Set moves
        for (int i = 0; i < preset.Moves.Length && i < 4; i++)
        {
            var moveIndex = Array.FindIndex(GameInfo.Strings.movelist, m =>
                m.Equals(preset.Moves[i], StringComparison.OrdinalIgnoreCase));
            if (moveIndex > 0)
            {
                switch (i)
                {
                    case 0: pk.Move1 = (ushort)moveIndex; break;
                    case 1: pk.Move2 = (ushort)moveIndex; break;
                    case 2: pk.Move3 = (ushort)moveIndex; break;
                    case 3: pk.Move4 = (ushort)moveIndex; break;
                }
            }
        }

        pk.HealPP();
        return pk;
    }

    private void QuickApply6IVShiny()
    {
        var pk = Editor.PreparePKM();
        if (pk.Species == 0)
        {
            MessageBox.Show("Load a Pokemon first!");
            return;
        }

        pk.IV_HP = pk.IV_ATK = pk.IV_DEF = pk.IV_SPA = pk.IV_SPD = pk.IV_SPE = 31;
        pk.SetShiny();
        pk.CurrentLevel = 100;

        Editor.PopulateFields(pk, false);
        MessageBox.Show("Applied 6IV + Shiny + Lv100!", "Quick Apply", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void QuickApplyLegalComp()
    {
        var pk = Editor.PreparePKM();
        if (pk.Species == 0)
        {
            MessageBox.Show("Load a Pokemon first!");
            return;
        }

        pk.IV_HP = pk.IV_ATK = pk.IV_DEF = pk.IV_SPA = pk.IV_SPD = pk.IV_SPE = 31;
        pk.CurrentLevel = 100;

        // Apply common competitive EV spread (252/252/4)
        pk.EV_HP = 0;
        pk.EV_ATK = 0;
        pk.EV_DEF = 0;
        pk.EV_SPA = 0;
        pk.EV_SPD = 4;
        pk.EV_SPE = 0;

        // Determine if physical or special attacker based on base stats
        var pi = pk.PersonalInfo;
        if (pi.ATK >= pi.SPA)
        {
            pk.EV_ATK = 252;
            pk.EV_SPE = 252;
            pk.Nature = Nature.Jolly; // +Spe -SpA
        }
        else
        {
            pk.EV_SPA = 252;
            pk.EV_SPE = 252;
            pk.Nature = Nature.Timid; // +Spe -Atk
        }

        pk.HealPP();
        Editor.PopulateFields(pk, false);
        MessageBox.Show("Applied competitive build based on stats!", "Quick Apply", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void FilterByCategory(string category)
    {
        // Filter presets list by tier
        if (CB_Species.SelectedIndex < 0) return;

        var speciesName = CB_Species.SelectedItem?.ToString();
        var species = Array.FindIndex(GameInfo.Strings.specieslist, s => s == speciesName);

        if (species > 0 && Presets.TryGetValue(species, out var presets))
        {
            LB_Presets.Items.Clear();
            var filtered = category == "Random"
                ? presets
                : presets.Where(p => p.Tier.Contains(category, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var preset in filtered)
                LB_Presets.Items.Add($"{preset.Name} ({preset.Tier})");

            if (LB_Presets.Items.Count > 0)
                LB_Presets.SelectedIndex = 0;
        }
    }

    private static Dictionary<int, List<CompetitivePreset>> InitializePresets()
    {
        var presets = new Dictionary<int, List<CompetitivePreset>>();

        // Charizard
        presets[6] = new List<CompetitivePreset>
        {
            new("Solar Power Sweeper", "OU", "Timid", 1, new[] { 0, 0, 0, 252, 4, 252 }, new[] { 31, 0, 31, 31, 31, 31 }, new[] { "Fire Blast", "Solar Beam", "Focus Blast", "Roost" }),
            new("Dragon Dance", "UU", "Jolly", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Dragon Dance", "Flare Blitz", "Earthquake", "Dragon Claw" })
        };

        // Pikachu
        presets[25] = new List<CompetitivePreset>
        {
            new("Light Ball Attacker", "NU", "Timid", 0, new[] { 0, 0, 0, 252, 4, 252 }, new[] { 31, 0, 31, 31, 31, 31 }, new[] { "Thunderbolt", "Grass Knot", "Volt Switch", "Hidden Power Ice" }),
            new("Physical Pikachu", "NU", "Jolly", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Volt Tackle", "Iron Tail", "Knock Off", "Fake Out" })
        };

        // Mewtwo
        presets[150] = new List<CompetitivePreset>
        {
            new("Special Sweeper", "Ubers", "Timid", 0, new[] { 0, 0, 0, 252, 4, 252 }, new[] { 31, 0, 31, 31, 31, 31 }, new[] { "Psystrike", "Ice Beam", "Fire Blast", "Calm Mind" }),
            new("Physical Attacker", "Ubers", "Jolly", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Psycho Cut", "Earthquake", "Stone Edge", "Bulk Up" })
        };

        // Dragonite
        presets[149] = new List<CompetitivePreset>
        {
            new("Dragon Dance", "OU", "Adamant", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Dragon Dance", "Outrage", "Earthquake", "Extreme Speed" }),
            new("Bulky Dragon Dance", "OU", "Jolly", 0, new[] { 252, 0, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Dragon Dance", "Dragon Claw", "Fire Punch", "Roost" })
        };

        // Garchomp
        presets[445] = new List<CompetitivePreset>
        {
            new("Swords Dance", "OU", "Jolly", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Swords Dance", "Earthquake", "Outrage", "Stone Edge" }),
            new("Scarf Revenge Killer", "OU", "Jolly", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Earthquake", "Outrage", "Stone Edge", "Fire Fang" })
        };

        // Lucario
        presets[448] = new List<CompetitivePreset>
        {
            new("Nasty Plot", "UU", "Timid", 0, new[] { 0, 0, 0, 252, 4, 252 }, new[] { 31, 0, 31, 31, 31, 31 }, new[] { "Nasty Plot", "Aura Sphere", "Flash Cannon", "Vacuum Wave" }),
            new("Swords Dance", "UU", "Jolly", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Swords Dance", "Close Combat", "Bullet Punch", "Extreme Speed" })
        };

        // Greninja
        presets[658] = new List<CompetitivePreset>
        {
            new("Protean Attacker", "OU", "Timid", 1, new[] { 0, 0, 0, 252, 4, 252 }, new[] { 31, 0, 31, 31, 31, 31 }, new[] { "Hydro Pump", "Dark Pulse", "Ice Beam", "Grass Knot" }),
            new("Physical Protean", "OU", "Jolly", 1, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Gunk Shot", "Low Kick", "Rock Slide", "U-turn" })
        };

        // Mimikyu
        presets[778] = new List<CompetitivePreset>
        {
            new("Swords Dance", "OU", "Jolly", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Swords Dance", "Play Rough", "Shadow Claw", "Shadow Sneak" }),
            new("VGC Support", "VGC", "Adamant", 0, new[] { 4, 252, 0, 0, 0, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Play Rough", "Shadow Sneak", "Trick Room", "Will-O-Wisp" })
        };

        // Gengar
        presets[94] = new List<CompetitivePreset>
        {
            new("Nasty Plot Sweeper", "OU", "Timid", 0, new[] { 0, 0, 0, 252, 4, 252 }, new[] { 31, 0, 31, 31, 31, 31 }, new[] { "Nasty Plot", "Shadow Ball", "Sludge Wave", "Focus Blast" }),
            new("Choice Specs", "OU", "Timid", 0, new[] { 0, 0, 0, 252, 4, 252 }, new[] { 31, 0, 31, 31, 31, 31 }, new[] { "Shadow Ball", "Sludge Wave", "Focus Blast", "Trick" })
        };

        // Tyranitar
        presets[248] = new List<CompetitivePreset>
        {
            new("Dragon Dance", "OU", "Jolly", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Dragon Dance", "Stone Edge", "Crunch", "Earthquake" }),
            new("Band Attacker", "OU", "Adamant", 0, new[] { 252, 252, 0, 0, 4, 0 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Stone Edge", "Crunch", "Earthquake", "Pursuit" })
        };

        // Salamence
        presets[373] = new List<CompetitivePreset>
        {
            new("Dragon Dance", "OU", "Jolly", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Dragon Dance", "Outrage", "Earthquake", "Fire Fang" }),
            new("Mixed Attacker", "OU", "Naive", 0, new[] { 0, 252, 0, 4, 0, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Draco Meteor", "Earthquake", "Fire Blast", "Outrage" })
        };

        // Metagross
        presets[376] = new List<CompetitivePreset>
        {
            new("Agility Sweeper", "OU", "Adamant", 0, new[] { 0, 252, 0, 0, 4, 252 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Agility", "Meteor Mash", "Earthquake", "Zen Headbutt" }),
            new("Band Attacker", "OU", "Adamant", 0, new[] { 252, 252, 0, 0, 4, 0 }, new[] { 31, 31, 31, 31, 31, 31 }, new[] { "Meteor Mash", "Earthquake", "Hammer Arm", "Bullet Punch" })
        };

        return presets;
    }
}

public class CompetitivePreset
{
    public string Name { get; }
    public string Tier { get; }
    public string Nature { get; }
    public int AbilitySlot { get; }
    public int[] EVs { get; }
    public int[] IVs { get; }
    public string[] Moves { get; }

    public CompetitivePreset(string name, string tier, string nature, int abilitySlot, int[] evs, int[] ivs, string[] moves)
    {
        Name = name;
        Tier = tier;
        Nature = nature;
        AbilitySlot = abilitySlot;
        EVs = evs;
        IVs = ivs;
        Moves = moves;
    }

    public string GetShowdownFormat(string species)
    {
        var evStr = $"{EVs[0]} HP / {EVs[1]} Atk / {EVs[2]} Def / {EVs[3]} SpA / {EVs[4]} SpD / {EVs[5]} Spe";
        var ivStr = IVs.All(iv => iv == 31) ? "" : $"IVs: {IVs[0]} HP / {IVs[1]} Atk / {IVs[2]} Def / {IVs[3]} SpA / {IVs[4]} SpD / {IVs[5]} Spe\n";

        return $@"{species}
Ability: Slot {AbilitySlot}
{ivStr}EVs: {evStr}
{Nature} Nature
- {Moves[0]}
- {Moves[1]}
- {Moves[2]}
- {Moves[3]}

=== {Name} ({Tier}) ===
This is a {Tier} competitive set.";
    }
}
