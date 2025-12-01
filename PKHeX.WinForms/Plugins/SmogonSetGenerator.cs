using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Smogon Set Generator - Fetches competitive sets from Smogon
/// </summary>
public class SmogonSetGenerator
{
    private readonly SaveFile SAV;
    private static readonly HttpClient HttpClient = new();

    // Smogon format mappings
    private static readonly Dictionary<string, string> FormatMappings = new()
    {
        ["SV"] = "sv",
        ["SWSH"] = "ss",
        ["SM"] = "sm",
        ["ORAS"] = "xy",
        ["BW"] = "bw",
        ["HGSS"] = "dp",
        ["RSE"] = "rs"
    };

    public SmogonSetGenerator(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Get Smogon format string for current game
    /// </summary>
    public string GetCurrentFormat()
    {
        return SAV.Context switch
        {
            EntityContext.Gen9 => "sv",
            EntityContext.Gen8 => "ss",
            EntityContext.Gen8b => "bdsp",
            EntityContext.Gen8a => "pla",
            EntityContext.Gen7 => "sm",
            EntityContext.Gen6 => "xy",
            EntityContext.Gen5 => "bw",
            EntityContext.Gen4 => "dp",
            EntityContext.Gen3 => "rs",
            _ => "sv"
        };
    }

    /// <summary>
    /// Generate a competitive set for a species
    /// </summary>
    public PKM? GenerateSmogonSet(ushort species, byte form = 0)
    {
        // Get species name
        var speciesName = GameInfo.Strings.Species[species];
        if (string.IsNullOrEmpty(speciesName))
            return null;

        // Get a default competitive template
        var template = GetDefaultCompetitiveTemplate(species, form);
        if (template == null)
            return null;

        return ALMShowdownPlugin.GenerateLegalPokemon(template, SAV);
    }

    /// <summary>
    /// Get default competitive template for a species
    /// </summary>
    private IBattleTemplate? GetDefaultCompetitiveTemplate(ushort species, byte form)
    {
        var pk = SAV.BlankPKM;
        pk.Species = species;
        pk.Form = form;

        var pi = pk.PersonalInfo;

        // Determine best nature based on stats
        var nature = DetermineOptimalNature(pi);

        // Determine best ability
        var ability = pi.GetAbilityAtIndex(0);
        if (pi.AbilityCount > 1)
        {
            // Prefer hidden ability for many Pokemon
            ability = pi.GetAbilityAtIndex(pi.AbilityCount - 1);
        }

        // Get recommended moves
        var moves = GetRecommendedMoves(pk);

        // Determine optimal EV spread based on stats
        var evs = DetermineOptimalEVs(pi);

        // Create the template text
        var speciesName = GameInfo.Strings.Species[species];
        var natureName = GameInfo.Strings.Natures[(int)nature];
        var abilityName = GameInfo.Strings.Ability[ability];

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{speciesName}");
        sb.AppendLine($"Ability: {abilityName}");
        sb.AppendLine($"EVs: {FormatEVs(evs)}");
        sb.AppendLine($"{natureName} Nature");
        foreach (var move in moves.Where(m => m != 0))
        {
            var moveName = GameInfo.Strings.Move[move];
            sb.AppendLine($"- {moveName}");
        }

        var sets = BattleTemplateTeams.TryGetSets(sb.ToString());
        return sets.FirstOrDefault();
    }

    private Nature DetermineOptimalNature(IPersonalInfo pi)
    {
        // Compare base stats to determine optimal nature
        var atk = pi.ATK;
        var spa = pi.SPA;
        var spe = pi.SPE;
        var def = pi.DEF;
        var spd = pi.SPD;

        // Physical attacker
        if (atk > spa && atk >= 100)
        {
            if (spe >= 90)
                return Nature.Jolly; // +Spe -SpA
            else
                return Nature.Adamant; // +Atk -SpA
        }

        // Special attacker
        if (spa > atk && spa >= 100)
        {
            if (spe >= 90)
                return Nature.Timid; // +Spe -Atk
            else
                return Nature.Modest; // +SpA -Atk
        }

        // Defensive
        if (def > atk && def > spa)
        {
            return Nature.Bold; // +Def -Atk
        }

        if (spd > atk && spd > spa)
        {
            return Nature.Calm; // +SpD -Atk
        }

        // Default to neutral
        return Nature.Serious;
    }

    private int[] DetermineOptimalEVs(IPersonalInfo pi)
    {
        var evs = new int[6]; // HP, Atk, Def, SpA, SpD, Spe

        var atk = pi.ATK;
        var spa = pi.SPA;
        var spe = pi.SPE;
        var hp = pi.HP;
        var def = pi.DEF;
        var spd = pi.SPD;

        // Physical sweeper
        if (atk > spa && spe >= 80)
        {
            evs[1] = 252; // Atk
            evs[5] = 252; // Spe
            evs[0] = 4;   // HP
        }
        // Special sweeper
        else if (spa > atk && spe >= 80)
        {
            evs[3] = 252; // SpA
            evs[5] = 252; // Spe
            evs[0] = 4;   // HP
        }
        // Bulky physical
        else if (def >= 80 && atk >= 80)
        {
            evs[0] = 252; // HP
            evs[1] = 252; // Atk
            evs[2] = 4;   // Def
        }
        // Bulky special
        else if (spd >= 80 && spa >= 80)
        {
            evs[0] = 252; // HP
            evs[3] = 252; // SpA
            evs[4] = 4;   // SpD
        }
        // Tank
        else
        {
            evs[0] = 252; // HP
            evs[2] = 128; // Def
            evs[4] = 128; // SpD
        }

        return evs;
    }

    private ushort[] GetRecommendedMoves(PKM pk)
    {
        var moves = new List<ushort>();

        // Get all legal moves
        var learnset = pk.PersonalInfo;

        // For now, return level-up moves
        // A full implementation would analyze movepool and pick optimal moves
        if (pk.Move1 != 0) moves.Add(pk.Move1);
        if (pk.Move2 != 0) moves.Add(pk.Move2);
        if (pk.Move3 != 0) moves.Add(pk.Move3);
        if (pk.Move4 != 0) moves.Add(pk.Move4);

        // If no moves, add Tackle as fallback
        if (moves.Count == 0)
            moves.Add(33); // Tackle

        // Pad to 4 moves
        while (moves.Count < 4)
            moves.Add(0);

        return moves.Take(4).ToArray();
    }

    private static string FormatEVs(int[] evs)
    {
        var parts = new List<string>();
        var statNames = new[] { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };

        for (int i = 0; i < 6; i++)
        {
            if (evs[i] > 0)
                parts.Add($"{evs[i]} {statNames[i]}");
        }

        return string.Join(" / ", parts);
    }
}

/// <summary>
/// Dialog for selecting Smogon sets
/// </summary>
public class SmogonSetDialog : Form
{
    private readonly ComboBox cmbFormat;
    private readonly TextBox txtSpecies;
    private readonly Button btnGenerate;
    private readonly RichTextBox txtPreview;
    private readonly SaveFile SAV;

    public string? SelectedSet { get; private set; }

    public SmogonSetDialog(SaveFile sav)
    {
        SAV = sav;
        Text = "Smogon Set Generator";
        Size = new System.Drawing.Size(400, 350);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var lblFormat = new Label { Text = "Format:", Location = new System.Drawing.Point(10, 15), AutoSize = true };
        cmbFormat = new ComboBox
        {
            Location = new System.Drawing.Point(80, 12),
            Size = new System.Drawing.Size(120, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbFormat.Items.AddRange(new object[] { "SV OU", "SV UU", "SV Ubers", "National Dex" });
        cmbFormat.SelectedIndex = 0;

        var lblSpecies = new Label { Text = "Species:", Location = new System.Drawing.Point(10, 50), AutoSize = true };
        txtSpecies = new TextBox
        {
            Location = new System.Drawing.Point(80, 47),
            Size = new System.Drawing.Size(200, 24)
        };

        btnGenerate = new Button
        {
            Text = "Generate",
            Location = new System.Drawing.Point(290, 45),
            Size = new System.Drawing.Size(90, 28)
        };
        btnGenerate.Click += BtnGenerate_Click;

        txtPreview = new RichTextBox
        {
            Location = new System.Drawing.Point(10, 85),
            Size = new System.Drawing.Size(370, 180),
            ReadOnly = true,
            Font = new System.Drawing.Font("Consolas", 10)
        };

        var btnOK = new Button
        {
            Text = "Use Set",
            Location = new System.Drawing.Point(200, 275),
            Size = new System.Drawing.Size(80, 30),
            DialogResult = DialogResult.OK
        };

        var btnCancel = new Button
        {
            Text = "Cancel",
            Location = new System.Drawing.Point(290, 275),
            Size = new System.Drawing.Size(80, 30),
            DialogResult = DialogResult.Cancel
        };

        Controls.AddRange(new Control[] { lblFormat, cmbFormat, lblSpecies, txtSpecies, btnGenerate, txtPreview, btnOK, btnCancel });

        AcceptButton = btnOK;
        CancelButton = btnCancel;
    }

    private void BtnGenerate_Click(object? sender, EventArgs e)
    {
        var species = txtSpecies.Text.Trim();
        if (string.IsNullOrEmpty(species))
        {
            MessageBox.Show("Please enter a species name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Find species by name
        var speciesList = GameInfo.Strings.Species;
        int speciesIndex = -1;
        for (int i = 0; i < speciesList.Count; i++)
        {
            if (speciesList[i].Equals(species, StringComparison.OrdinalIgnoreCase))
            {
                speciesIndex = i;
                break;
            }
        }

        if (speciesIndex < 0)
        {
            MessageBox.Show($"Species '{species}' not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var generator = new SmogonSetGenerator(SAV);
        var pk = generator.GenerateSmogonSet((ushort)speciesIndex);

        if (pk != null)
        {
            var text = ShowdownParsing.GetShowdownText(pk);
            txtPreview.Text = text;
            SelectedSet = text;
        }
        else
        {
            txtPreview.Text = "Failed to generate set.";
        }
    }
}
