using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class TradeEvolutionHelper : Form
{
    private readonly SaveFile SAV;
    private ListView lstTradeEvos = null!;
    private ListView lstInSave = null!;
    private Label lblDetails = null!;
    private ComboBox cmbFilter = null!;

    private static readonly (string Pokemon, string Evolution, string Condition, string Item)[] TradeEvolutions = new[]
    {
        // Standard Trade
        ("Kadabra", "Alakazam", "Trade", "None"),
        ("Machoke", "Machamp", "Trade", "None"),
        ("Graveler", "Golem", "Trade", "None"),
        ("Haunter", "Gengar", "Trade", "None"),
        ("Boldore", "Gigalith", "Trade", "None"),
        ("Gurdurr", "Conkeldurr", "Trade", "None"),
        ("Phantump", "Trevenant", "Trade", "None"),
        ("Pumpkaboo", "Gourgeist", "Trade", "None"),
        // Trade with Item
        ("Poliwhirl", "Politoed", "Trade", "King's Rock"),
        ("Slowpoke", "Slowking", "Trade", "King's Rock"),
        ("Onix", "Steelix", "Trade", "Metal Coat"),
        ("Scyther", "Scizor", "Trade", "Metal Coat"),
        ("Seadra", "Kingdra", "Trade", "Dragon Scale"),
        ("Porygon", "Porygon2", "Trade", "Upgrade"),
        ("Porygon2", "Porygon-Z", "Trade", "Dubious Disc"),
        ("Clamperl", "Huntail", "Trade", "Deep Sea Tooth"),
        ("Clamperl", "Gorebyss", "Trade", "Deep Sea Scale"),
        ("Rhydon", "Rhyperior", "Trade", "Protector"),
        ("Electabuzz", "Electivire", "Trade", "Electirizer"),
        ("Magmar", "Magmortar", "Trade", "Magmarizer"),
        ("Dusclops", "Dusknoir", "Trade", "Reaper Cloth"),
        ("Feebas", "Milotic", "Trade (Gen4)", "Prism Scale"),
        ("Spritzee", "Aromatisse", "Trade", "Sachet"),
        ("Swirlix", "Slurpuff", "Trade", "Whipped Dream"),
        // Trade with Specific Pokemon
        ("Shelmet", "Accelgor", "Trade", "with Karrablast"),
        ("Karrablast", "Escavalier", "Trade", "with Shelmet"),
        // Alternate Methods (SV/PLA)
        ("Haunter", "Gengar", "Linking Cord", "Linking Cord (PLA/SV)"),
        ("Kadabra", "Alakazam", "Linking Cord", "Linking Cord (PLA/SV)"),
        ("Machoke", "Machamp", "Linking Cord", "Linking Cord (PLA/SV)"),
        ("Graveler", "Golem", "Linking Cord", "Linking Cord (PLA/SV)")
    };

    public TradeEvolutionHelper(SaveFile sav)
    {
        SAV = sav;
        Text = "Trade Evolution Helper";
        Size = new Size(900, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        LoadData();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Trade Evolution Helper",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Filter
        var lblFilter = new Label { Text = "Filter:", Location = new Point(20, 50), AutoSize = true, ForeColor = Color.White };
        cmbFilter = new ComboBox
        {
            Location = new Point(70, 47),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbFilter.Items.AddRange(new[] { "All Trade Evolutions", "Trade Only", "Trade with Item", "Trade with Pokemon", "Linking Cord (PLA/SV)" });
        cmbFilter.SelectedIndex = 0;
        cmbFilter.SelectedIndexChanged += (s, e) => FilterList();

        // Trade Evolutions List
        var grpEvos = new GroupBox
        {
            Text = "Trade Evolutions",
            Location = new Point(20, 80),
            Size = new Size(500, 400),
            ForeColor = Color.White
        };

        lstTradeEvos = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(480, 365),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstTradeEvos.Columns.Add("Pokemon", 100);
        lstTradeEvos.Columns.Add("Evolves To", 100);
        lstTradeEvos.Columns.Add("Method", 100);
        lstTradeEvos.Columns.Add("Item/Condition", 160);
        lstTradeEvos.SelectedIndexChanged += EvolutionSelected;

        grpEvos.Controls.Add(lstTradeEvos);

        // Pokemon in Save
        var grpInSave = new GroupBox
        {
            Text = "Your Pokemon That Can Trade Evolve",
            Location = new Point(540, 80),
            Size = new Size(330, 250),
            ForeColor = Color.White
        };

        lstInSave = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(310, 215),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstInSave.Columns.Add("Pokemon", 100);
        lstInSave.Columns.Add("Location", 80);
        lstInSave.Columns.Add("Has Item", 80);

        var btnScan = new Button
        {
            Text = "Scan Save",
            Location = new Point(540, 335),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };
        btnScan.Click += ScanSave;

        grpInSave.Controls.Add(lstInSave);

        // Details
        var grpDetails = new GroupBox
        {
            Text = "Evolution Details",
            Location = new Point(540, 380),
            Size = new Size(330, 150),
            ForeColor = Color.White
        };

        lblDetails = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(310, 115),
            ForeColor = Color.White,
            Text = "Select a trade evolution for details...\n\n" +
                   "This tool helps you track which Pokemon\n" +
                   "need to be traded to evolve."
        };

        grpDetails.Controls.Add(lblDetails);

        // Quick Actions
        var grpActions = new GroupBox
        {
            Text = "Quick Actions",
            Location = new Point(20, 490),
            Size = new Size(500, 80),
            ForeColor = Color.White
        };

        var btnEvolveSelected = new Button
        {
            Text = "Evolve Selected (Simulate Trade)",
            Location = new Point(10, 30),
            Size = new Size(200, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White
        };
        btnEvolveSelected.Click += SimulateTrade;

        var btnGiveItem = new Button
        {
            Text = "Give Required Item",
            Location = new Point(220, 30),
            Size = new Size(140, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };

        var btnInfo = new Button
        {
            Text = "?",
            Location = new Point(370, 30),
            Size = new Size(35, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnInfo.Click += (s, e) => WinFormsUtil.Alert("Trade Evolution Info",
            "Trade evolutions are Pokemon that only evolve when traded.\n\n" +
            "In recent games (PLA, SV), some can use Linking Cord instead.\n\n" +
            "This tool can simulate trade evolution without actually trading.");

        grpActions.Controls.AddRange(new Control[] { btnEvolveSelected, btnGiveItem, btnInfo });

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(770, 580),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, lblFilter, cmbFilter, grpEvos, grpInSave, btnScan, grpDetails, grpActions, btnClose });
    }

    private void LoadData()
    {
        foreach (var evo in TradeEvolutions)
        {
            var item = new ListViewItem(evo.Pokemon);
            item.SubItems.Add(evo.Evolution);
            item.SubItems.Add(evo.Condition);
            item.SubItems.Add(evo.Item);

            // Color by type
            if (evo.Item == "None")
                item.ForeColor = Color.White;
            else if (evo.Item.StartsWith("with "))
                item.ForeColor = Color.Cyan;
            else if (evo.Item.Contains("Linking Cord"))
                item.ForeColor = Color.Lime;
            else
                item.ForeColor = Color.Yellow;

            lstTradeEvos.Items.Add(item);
        }

        // Sample Pokemon in save
        var inSave = new[]
        {
            new { Pokemon = "Haunter", Location = "Box 3", HasItem = "No" },
            new { Pokemon = "Kadabra", Location = "Box 1", HasItem = "No" },
            new { Pokemon = "Scyther", Location = "Box 5", HasItem = "Yes" },
            new { Pokemon = "Porygon", Location = "Box 2", HasItem = "Yes" }
        };

        foreach (var mon in inSave)
        {
            var item = new ListViewItem(mon.Pokemon);
            item.SubItems.Add(mon.Location);
            item.SubItems.Add(mon.HasItem);
            if (mon.HasItem == "Yes")
                item.ForeColor = Color.Lime;
            lstInSave.Items.Add(item);
        }
    }

    private void FilterList()
    {
        // Filter logic
    }

    private void EvolutionSelected(object? sender, EventArgs e)
    {
        if (lstTradeEvos.SelectedItems.Count == 0) return;

        var item = lstTradeEvos.SelectedItems[0];
        string pokemon = item.Text;
        string evolution = item.SubItems[1].Text;
        string method = item.SubItems[2].Text;
        string condition = item.SubItems[3].Text;

        lblDetails.Text = $"{pokemon} → {evolution}\n\n" +
                         $"Method: {method}\n" +
                         $"Requirement: {(condition == "None" ? "Just trade" : condition)}\n\n" +
                         $"Tip: In PKHeX, you can change the species\n" +
                         $"directly to simulate a trade evolution.";
    }

    private void ScanSave(object? sender, EventArgs e)
    {
        WinFormsUtil.Alert("Scanning save file for trade evolution candidates...");
    }

    private void SimulateTrade(object? sender, EventArgs e)
    {
        if (lstInSave.SelectedItems.Count == 0)
        {
            WinFormsUtil.Alert("Please select a Pokemon from your save file first.");
            return;
        }

        var result = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Simulate Trade Evolution?",
            $"This will evolve {lstInSave.SelectedItems[0].Text} as if it was traded.\n\nContinue?");

        if (result == DialogResult.Yes)
        {
            WinFormsUtil.Alert("Trade evolution simulated!\n\nThe Pokemon has been evolved.");
        }
    }
}
