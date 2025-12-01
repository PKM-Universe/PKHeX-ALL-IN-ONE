using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class ShowdownImportExport : Form
{
    private readonly SaveFile SAV;
    private readonly IPKMView Editor;
    private readonly TextBox TB_Showdown;
    private readonly Label L_Status;

    public ShowdownImportExport(SaveFile sav, IPKMView editor)
    {
        SAV = sav;
        Editor = editor;
        Text = "Pokemon Showdown Import/Export";
        Size = new Size(600, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblTitle = new Label { Text = "Paste Showdown Format or Export Current Pokemon", Location = new Point(20, 15), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };

        TB_Showdown = new TextBox
        {
            Location = new Point(20, 45),
            Size = new Size(540, 300),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(40, 40, 60),
            ForeColor = Color.White,
            Font = new Font("Consolas", 10F)
        };

        var btnImport = new Button { Text = "Import to Editor", Location = new Point(20, 360), Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 100, 60), ForeColor = Color.White };
        btnImport.Click += (s, e) => ImportShowdown();

        var btnExport = new Button { Text = "Export Current", Location = new Point(160, 360), Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 100), ForeColor = Color.White };
        btnExport.Click += (s, e) => ExportShowdown();

        var btnExportParty = new Button { Text = "Export Party", Location = new Point(300, 360), Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 100), ForeColor = Color.White };
        btnExportParty.Click += (s, e) => ExportParty();

        var btnClear = new Button { Text = "Clear", Location = new Point(440, 360), Size = new Size(80, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(80, 50, 50), ForeColor = Color.White };
        btnClear.Click += (s, e) => TB_Showdown.Clear();

        L_Status = new Label { Location = new Point(20, 410), AutoSize = true, ForeColor = Color.LightGray };

        var lblHelp = new Label { Text = "Format: Species @ Item\nAbility: Name\nEVs: 252 Atk / 252 Spe / 4 HP\nNature Nature\n- Move 1\n- Move 2...", Location = new Point(20, 430), Size = new Size(540, 50), ForeColor = Color.Gray, Font = new Font("Segoe UI", 8F) };

        Controls.AddRange(new Control[] { lblTitle, TB_Showdown, btnImport, btnExport, btnExportParty, btnClear, L_Status, lblHelp });
    }

    private void ImportShowdown()
    {
        if (string.IsNullOrWhiteSpace(TB_Showdown.Text)) { L_Status.Text = "No text to import!"; return; }
        try
        {
            var set = new ShowdownSet(TB_Showdown.Text);
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
            if (set.Shiny) pk.SetShiny();

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

    private void ExportShowdown()
    {
        var pk = Editor.PreparePKM();
        if (pk.Species == 0) { L_Status.Text = "No Pokemon loaded!"; return; }
        TB_Showdown.Text = ConvertToShowdown(pk);
        L_Status.Text = "Exported to Showdown format!";
        L_Status.ForeColor = Color.LightGreen;
    }

    private void ExportParty()
    {
        if (!SAV.HasParty) { L_Status.Text = "No party available!"; return; }
        var sb = new StringBuilder();
        for (int i = 0; i < 6; i++)
        {
            var pk = SAV.GetPartySlotAtIndex(i);
            if (pk.Species != 0)
            {
                sb.AppendLine(ConvertToShowdown(pk));
                sb.AppendLine();
            }
        }
        TB_Showdown.Text = sb.ToString();
        L_Status.Text = "Exported party to Showdown format!";
        L_Status.ForeColor = Color.LightGreen;
    }

    private string ConvertToShowdown(PKM pk)
    {
        var sb = new StringBuilder();
        var name = GameInfo.Strings.specieslist[pk.Species];
        var item = pk.HeldItem > 0 ? GameInfo.Strings.itemlist[pk.HeldItem] : "";
        sb.Append(name);
        if (!string.IsNullOrEmpty(item)) sb.Append($" @ {item}");
        sb.AppendLine();

        var ability = GameInfo.Strings.abilitylist[pk.Ability];
        sb.AppendLine($"Ability: {ability}");
        sb.AppendLine($"Level: {pk.CurrentLevel}");
        if (pk.IsShiny) sb.AppendLine("Shiny: Yes");

        var evs = new List<string>();
        if (pk.EV_HP > 0) evs.Add($"{pk.EV_HP} HP");
        if (pk.EV_ATK > 0) evs.Add($"{pk.EV_ATK} Atk");
        if (pk.EV_DEF > 0) evs.Add($"{pk.EV_DEF} Def");
        if (pk.EV_SPA > 0) evs.Add($"{pk.EV_SPA} SpA");
        if (pk.EV_SPD > 0) evs.Add($"{pk.EV_SPD} SpD");
        if (pk.EV_SPE > 0) evs.Add($"{pk.EV_SPE} Spe");
        if (evs.Count > 0) sb.AppendLine($"EVs: {string.Join(" / ", evs)}");

        sb.AppendLine($"{GameInfo.Strings.natures[(int)pk.Nature]} Nature");

        var ivs = new List<string>();
        if (pk.IV_HP < 31) ivs.Add($"{pk.IV_HP} HP");
        if (pk.IV_ATK < 31) ivs.Add($"{pk.IV_ATK} Atk");
        if (pk.IV_DEF < 31) ivs.Add($"{pk.IV_DEF} Def");
        if (pk.IV_SPA < 31) ivs.Add($"{pk.IV_SPA} SpA");
        if (pk.IV_SPD < 31) ivs.Add($"{pk.IV_SPD} SpD");
        if (pk.IV_SPE < 31) ivs.Add($"{pk.IV_SPE} Spe");
        if (ivs.Count > 0) sb.AppendLine($"IVs: {string.Join(" / ", ivs)}");

        if (pk.Move1 != 0) sb.AppendLine($"- {GameInfo.Strings.movelist[pk.Move1]}");
        if (pk.Move2 != 0) sb.AppendLine($"- {GameInfo.Strings.movelist[pk.Move2]}");
        if (pk.Move3 != 0) sb.AppendLine($"- {GameInfo.Strings.movelist[pk.Move3]}");
        if (pk.Move4 != 0) sb.AppendLine($"- {GameInfo.Strings.movelist[pk.Move4]}");

        return sb.ToString();
    }
}
