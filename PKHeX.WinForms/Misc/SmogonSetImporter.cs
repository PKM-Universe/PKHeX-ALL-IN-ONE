using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class SmogonSetImporter : Form
{
    private readonly SaveFile SAV;
    private readonly IPKMView Editor;
    private readonly ComboBox CB_Pokemon;
    private readonly ComboBox CB_Format;
    private readonly ListBox LB_Sets;
    private readonly TextBox TB_Preview;
    private readonly Label L_Status;
    private readonly List<string> _sets = new();

    private static readonly Dictionary<string, string[]> PopularSets = new()
    {
        { "Garchomp", new[] {
            "Garchomp @ Choice Scarf\nAbility: Rough Skin\nEVs: 252 Atk / 4 SpD / 252 Spe\nJolly Nature\n- Earthquake\n- Outrage\n- Stone Edge\n- Fire Fang",
            "Garchomp @ Rocky Helmet\nAbility: Rough Skin\nEVs: 252 HP / 176 Def / 80 Spe\nJolly Nature\n- Stealth Rock\n- Earthquake\n- Dragon Tail\n- Toxic"
        }},
        { "Dragapult", new[] {
            "Dragapult @ Choice Specs\nAbility: Infiltrator\nEVs: 252 SpA / 4 SpD / 252 Spe\nTimid Nature\n- Shadow Ball\n- Draco Meteor\n- Fire Blast\n- U-turn",
            "Dragapult @ Heavy-Duty Boots\nAbility: Infiltrator\nEVs: 252 Atk / 4 SpD / 252 Spe\nJolly Nature\n- Dragon Darts\n- Phantom Force\n- Dragon Dance\n- Sucker Punch"
        }},
        { "Cinderace", new[] {
            "Cinderace @ Heavy-Duty Boots\nAbility: Libero\nEVs: 252 Atk / 4 SpD / 252 Spe\nJolly Nature\n- Pyro Ball\n- High Jump Kick\n- Sucker Punch\n- U-turn"
        }},
        { "Toxapex", new[] {
            "Toxapex @ Black Sludge\nAbility: Regenerator\nEVs: 252 HP / 252 Def / 4 SpD\nBold Nature\n- Scald\n- Recover\n- Haze\n- Toxic Spikes"
        }},
        { "Ferrothorn", new[] {
            "Ferrothorn @ Leftovers\nAbility: Iron Barbs\nEVs: 252 HP / 252 Def / 4 SpD\nRelaxed Nature\nIVs: 0 Spe\n- Stealth Rock\n- Leech Seed\n- Power Whip\n- Knock Off"
        }},
        { "Landorus-Therian", new[] {
            "Landorus-Therian @ Choice Scarf\nAbility: Intimidate\nEVs: 252 Atk / 4 SpD / 252 Spe\nJolly Nature\n- Earthquake\n- U-turn\n- Stone Edge\n- Superpower"
        }},
        { "Corviknight", new[] {
            "Corviknight @ Leftovers\nAbility: Pressure\nEVs: 252 HP / 168 Def / 88 SpD\nImpish Nature\n- Body Press\n- Roost\n- Defog\n- U-turn"
        }},
        { "Clefable", new[] {
            "Clefable @ Leftovers\nAbility: Magic Guard\nEVs: 252 HP / 252 Def / 4 SpD\nBold Nature\n- Moonblast\n- Soft-Boiled\n- Calm Mind\n- Thunder Wave"
        }}
    };

    public SmogonSetImporter(SaveFile sav, IPKMView editor)
    {
        SAV = sav;
        Editor = editor;
        Text = "Smogon Set Importer";
        Size = new Size(600, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblPokemon = new Label { Text = "Pokemon:", Location = new Point(20, 18), AutoSize = true, ForeColor = Color.White };
        CB_Pokemon = new ComboBox { Location = new Point(90, 15), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        foreach (var name in PopularSets.Keys) CB_Pokemon.Items.Add(name);
        CB_Pokemon.SelectedIndexChanged += (s, e) => LoadSets();

        var lblFormat = new Label { Text = "Format:", Location = new Point(260, 18), AutoSize = true, ForeColor = Color.White };
        CB_Format = new ComboBox { Location = new Point(320, 15), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        CB_Format.Items.AddRange(new object[] { "OU", "UU", "Ubers", "VGC" });
        CB_Format.SelectedIndex = 0;

        var lblSets = new Label { Text = "Available Sets:", Location = new Point(20, 55), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
        LB_Sets = new ListBox { Location = new Point(20, 80), Size = new Size(200, 200), BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        LB_Sets.SelectedIndexChanged += (s, e) => UpdatePreview();

        var lblPreview = new Label { Text = "Set Preview:", Location = new Point(240, 55), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
        TB_Preview = new TextBox { Location = new Point(240, 80), Size = new Size(320, 200), Multiline = true, ReadOnly = true, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White, Font = new Font("Consolas", 9F) };

        var btnImport = new Button { Text = "Import Set", Location = new Point(240, 300), Size = new Size(150, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 100, 60), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
        btnImport.Click += (s, e) => ImportSet();

        L_Status = new Label { Location = new Point(20, 360), Size = new Size(540, 25), ForeColor = Color.LightGray };

        var lblNote = new Label { Text = "Note: These are popular competitive sets from Smogon. Sets may need legality adjustments.", Location = new Point(20, 400), Size = new Size(540, 40), ForeColor = Color.Gray, Font = new Font("Segoe UI", 8F) };

        Controls.AddRange(new Control[] { lblPokemon, CB_Pokemon, lblFormat, CB_Format, lblSets, LB_Sets, lblPreview, TB_Preview, btnImport, L_Status, lblNote });

        if (CB_Pokemon.Items.Count > 0) CB_Pokemon.SelectedIndex = 0;
    }

    private void LoadSets()
    {
        LB_Sets.Items.Clear();
        _sets.Clear();
        if (CB_Pokemon.SelectedItem == null) return;

        var pokemon = CB_Pokemon.SelectedItem.ToString()!;
        if (PopularSets.TryGetValue(pokemon, out var sets))
        {
            for (int i = 0; i < sets.Length; i++)
            {
                _sets.Add(sets[i]);
                LB_Sets.Items.Add($"Set {i + 1}");
            }
        }
        if (LB_Sets.Items.Count > 0) LB_Sets.SelectedIndex = 0;
    }

    private void UpdatePreview()
    {
        if (LB_Sets.SelectedIndex >= 0 && LB_Sets.SelectedIndex < _sets.Count)
            TB_Preview.Text = _sets[LB_Sets.SelectedIndex];
        else
            TB_Preview.Text = "";
    }

    private void ImportSet()
    {
        if (string.IsNullOrWhiteSpace(TB_Preview.Text)) { L_Status.Text = "No set selected!"; return; }
        try
        {
            var set = new ShowdownSet(TB_Preview.Text);
            var pk = SAV.BlankPKM;
            pk.Species = (ushort)set.Species;
            pk.Form = (byte)set.Form;
            pk.CurrentLevel = (byte)set.Level;
            pk.StatNature = pk.Nature = set.Nature;

            pk.Move1 = (ushort)(set.Moves.Length > 0 ? set.Moves[0] : 0);
            pk.Move2 = (ushort)(set.Moves.Length > 1 ? set.Moves[1] : 0);
            pk.Move3 = (ushort)(set.Moves.Length > 2 ? set.Moves[2] : 0);
            pk.Move4 = (ushort)(set.Moves.Length > 3 ? set.Moves[3] : 0);

            pk.EV_HP = set.EVs[0]; pk.EV_ATK = set.EVs[1]; pk.EV_DEF = set.EVs[2];
            pk.EV_SPA = set.EVs[3]; pk.EV_SPD = set.EVs[4]; pk.EV_SPE = set.EVs[5];
            pk.IV_HP = set.IVs[0]; pk.IV_ATK = set.IVs[1]; pk.IV_DEF = set.IVs[2];
            pk.IV_SPA = set.IVs[3]; pk.IV_SPD = set.IVs[4]; pk.IV_SPE = set.IVs[5];

            if (set.Ability >= 0) pk.RefreshAbility(set.Ability);
            pk.HeldItem = set.HeldItem;

            Editor.PopulateFields(pk, false);
            L_Status.Text = $"Imported {GameInfo.Strings.specieslist[pk.Species]} successfully!";
            L_Status.ForeColor = Color.LightGreen;
        }
        catch (Exception ex)
        {
            L_Status.Text = $"Import failed: {ex.Message}";
            L_Status.ForeColor = Color.Salmon;
        }
    }
}
