using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class PokemonSearchDialog : Form
{
    private readonly SaveFile SAV;
    private readonly TextBox TB_Search;
    private readonly ListView LV_Results;
    private readonly ComboBox CB_Filter;
    private readonly CheckBox CHK_Shiny;
    private readonly CheckBox CHK_Legendary;
    public (int Box, int Slot)? SelectedLocation { get; private set; }

    public PokemonSearchDialog(SaveFile sav)
    {
        SAV = sav;
        Text = "Search Pokemon";
        Size = new Size(500, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblSearch = new Label { Text = "Search:", Location = new Point(15, 18), AutoSize = true, ForeColor = Color.White };
        TB_Search = new TextBox { Location = new Point(70, 15), Width = 200, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        TB_Search.TextChanged += (s, e) => PerformSearch();

        CB_Filter = new ComboBox { Location = new Point(280, 15), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        CB_Filter.Items.AddRange(new object[] { "All", "Box Only", "Party Only" });
        CB_Filter.SelectedIndex = 0;
        CB_Filter.SelectedIndexChanged += (s, e) => PerformSearch();

        CHK_Shiny = new CheckBox { Text = "Shiny Only", Location = new Point(15, 50), AutoSize = true, ForeColor = Color.Gold };
        CHK_Shiny.CheckedChanged += (s, e) => PerformSearch();

        CHK_Legendary = new CheckBox { Text = "Legendary Only", Location = new Point(120, 50), AutoSize = true, ForeColor = Color.FromArgb(180, 100, 255) };
        CHK_Legendary.CheckedChanged += (s, e) => PerformSearch();

        LV_Results = new ListView
        {
            Location = new Point(15, 85),
            Size = new Size(450, 320),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            GridLines = true
        };
        LV_Results.Columns.Add("Pokemon", 150);
        LV_Results.Columns.Add("Level", 50);
        LV_Results.Columns.Add("Location", 100);
        LV_Results.Columns.Add("Shiny", 50);
        LV_Results.Columns.Add("IVs", 80);
        LV_Results.DoubleClick += (s, e) => SelectPokemon();

        var btnGoTo = new Button { Text = "Go To", Location = new Point(280, 420), Size = new Size(80, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(80, 50, 120), ForeColor = Color.White };
        btnGoTo.Click += (s, e) => SelectPokemon();

        var btnClose = new Button { Text = "Close", Location = new Point(370, 420), Size = new Size(80, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 50, 70), ForeColor = Color.White };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblSearch, TB_Search, CB_Filter, CHK_Shiny, CHK_Legendary, LV_Results, btnGoTo, btnClose });
        PerformSearch();
    }

    private void PerformSearch()
    {
        LV_Results.Items.Clear();
        string search = TB_Search.Text.ToLower();
        var results = new List<(PKM pk, int box, int slot)>();

        if (CB_Filter.SelectedIndex != 2)
        {
            for (int box = 0; box < SAV.BoxCount; box++)
            {
                for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
                {
                    var pk = SAV.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species != 0 && MatchesFilter(pk, search))
                        results.Add((pk, box, slot));
                }
            }
        }

        if (CB_Filter.SelectedIndex != 1 && SAV.HasParty)
        {
            for (int i = 0; i < 6; i++)
            {
                var pk = SAV.GetPartySlotAtIndex(i);
                if (pk.Species != 0 && MatchesFilter(pk, search))
                    results.Add((pk, -1, i));
            }
        }

        foreach (var (pk, box, slot) in results.Take(100))
        {
            var name = GameInfo.Strings.specieslist[pk.Species];
            var location = box >= 0 ? $"Box {box + 1}, Slot {slot + 1}" : $"Party Slot {slot + 1}";
            var ivTotal = pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPA + pk.IV_SPD + pk.IV_SPE;
            var item = new ListViewItem(new[] { name, pk.CurrentLevel.ToString(), location, pk.IsShiny ? "Yes" : "No", $"{ivTotal}/186" });
            item.Tag = (box, slot);
            if (pk.IsShiny) item.ForeColor = Color.Gold;
            LV_Results.Items.Add(item);
        }
    }

    private bool MatchesFilter(PKM pk, string search)
    {
        if (CHK_Shiny.Checked && !pk.IsShiny) return false;
        if (CHK_Legendary.Checked && !IsLegendary(pk.Species)) return false;
        if (string.IsNullOrEmpty(search)) return true;
        var name = GameInfo.Strings.specieslist[pk.Species].ToLower();
        return name.Contains(search);
    }

    private static bool IsLegendary(ushort species)
    {
        var legends = new HashSet<int> { 144,145,146,150,151,243,244,245,249,250,251,377,378,379,380,381,382,383,384,385,386,480,481,482,483,484,485,486,487,488,489,490,491,492,493,494,638,639,640,641,642,643,644,645,646,647,648,649,716,717,718,719,720,721,785,786,787,788,789,790,791,792,800,801,802,807,808,809,888,889,890,891,892,893,894,895,896,897,898,905,1001,1002,1003,1004,1007,1008,1014,1015,1016,1017,1024,1025 };
        return legends.Contains(species);
    }

    private void SelectPokemon()
    {
        if (LV_Results.SelectedItems.Count == 0) return;
        SelectedLocation = ((int, int))LV_Results.SelectedItems[0].Tag;
        DialogResult = DialogResult.OK;
        Close();
    }
}
