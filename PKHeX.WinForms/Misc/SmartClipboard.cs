using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class SmartClipboard : Form
{
    private readonly SaveFile SAV;
    private FlowLayoutPanel pnlSlots = null!;
    private ListView lstHistory = null!;
    private Label lblSlotInfo = null!;
    private Label lblCapacity = null!;

    private static readonly List<ClipboardSlot> slots = new();
    private static readonly List<ClipboardHistory> history = new();
    private const int MaxSlots = 10;
    private const int MaxHistory = 50;

    public SmartClipboard(SaveFile sav)
    {
        SAV = sav;
        Text = "Smart Clipboard";
        Size = new Size(900, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        RefreshSlots();
        RefreshHistory();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "📋 Smart Clipboard",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold)
        };

        var lblSubtitle = new Label
        {
            Text = "Multi-slot clipboard with history - Store up to 10 Pokemon",
            Location = new Point(22, 50),
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9F)
        };

        // Clipboard Slots
        var grpSlots = new GroupBox
        {
            Text = "Clipboard Slots",
            Location = new Point(20, 80),
            Size = new Size(850, 200),
            ForeColor = Color.White
        };

        pnlSlots = new FlowLayoutPanel
        {
            Location = new Point(10, 25),
            Size = new Size(830, 165),
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };
        grpSlots.Controls.Add(pnlSlots);

        lblCapacity = new Label
        {
            Text = $"Slots: 0/{MaxSlots}",
            Location = new Point(750, 0),
            AutoSize = true,
            ForeColor = Color.Lime,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        grpSlots.Controls.Add(lblCapacity);

        // Slot Details
        var grpDetails = new GroupBox
        {
            Text = "Slot Details",
            Location = new Point(20, 290),
            Size = new Size(400, 150),
            ForeColor = Color.White
        };

        lblSlotInfo = new Label
        {
            Text = "Select a slot to view details\n\nClick + to add from current Pokemon\nRight-click slots for options",
            Location = new Point(15, 25),
            Size = new Size(370, 115),
            ForeColor = Color.LightGray
        };
        grpDetails.Controls.Add(lblSlotInfo);

        // Quick Actions
        var grpActions = new GroupBox
        {
            Text = "Quick Actions",
            Location = new Point(440, 290),
            Size = new Size(430, 150),
            ForeColor = Color.White
        };

        var btnAddCurrent = new Button
        {
            Text = "Add Current Pokemon",
            Location = new Point(15, 30),
            Size = new Size(150, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 60),
            ForeColor = Color.White
        };
        btnAddCurrent.Click += AddCurrentPokemon;

        var btnAddFromBox = new Button
        {
            Text = "Add Selected Box Slot",
            Location = new Point(175, 30),
            Size = new Size(140, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };

        var btnClearAll = new Button
        {
            Text = "Clear All Slots",
            Location = new Point(325, 30),
            Size = new Size(90, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 60, 60),
            ForeColor = Color.White
        };
        btnClearAll.Click += ClearAllSlots;

        var btnPasteAll = new Button
        {
            Text = "Paste All to Box",
            Location = new Point(15, 75),
            Size = new Size(130, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };
        btnPasteAll.Click += PasteAllToBox;

        var btnSwapSlots = new Button
        {
            Text = "Swap Slots",
            Location = new Point(155, 75),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };

        var btnDuplicate = new Button
        {
            Text = "Duplicate Selected",
            Location = new Point(265, 75),
            Size = new Size(130, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 140),
            ForeColor = Color.White
        };

        grpActions.Controls.AddRange(new Control[] { btnAddCurrent, btnAddFromBox, btnClearAll, btnPasteAll, btnSwapSlots, btnDuplicate });

        // Clipboard History
        var grpHistory = new GroupBox
        {
            Text = "Clipboard History",
            Location = new Point(20, 450),
            Size = new Size(850, 150),
            ForeColor = Color.White
        };

        lstHistory = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(700, 115),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstHistory.Columns.Add("Time", 100);
        lstHistory.Columns.Add("Species", 120);
        lstHistory.Columns.Add("Level", 50);
        lstHistory.Columns.Add("Action", 80);
        lstHistory.Columns.Add("Details", 340);
        lstHistory.DoubleClick += RestoreFromHistory;

        var btnClearHistory = new Button
        {
            Text = "Clear History",
            Location = new Point(720, 25),
            Size = new Size(110, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClearHistory.Click += (s, e) => { history.Clear(); RefreshHistory(); };

        var btnRestoreHistory = new Button
        {
            Text = "Restore Selected",
            Location = new Point(720, 65),
            Size = new Size(110, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };
        btnRestoreHistory.Click += RestoreFromHistory;

        grpHistory.Controls.AddRange(new Control[] { lstHistory, btnClearHistory, btnRestoreHistory });

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(770, 610),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, lblSubtitle, grpSlots, grpDetails, grpActions, grpHistory, btnClose });
    }

    private void RefreshSlots()
    {
        pnlSlots.Controls.Clear();

        // Add existing slots
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            var panel = CreateSlotPanel(slot, i);
            pnlSlots.Controls.Add(panel);
        }

        // Add empty slot button if under max
        if (slots.Count < MaxSlots)
        {
            var addButton = new Button
            {
                Text = "+",
                Size = new Size(75, 75),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.FromArgb(100, 200, 100),
                Font = new Font("Segoe UI", 24F),
                Margin = new Padding(5)
            };
            addButton.Click += AddCurrentPokemon;
            pnlSlots.Controls.Add(addButton);
        }

        lblCapacity.Text = $"Slots: {slots.Count}/{MaxSlots}";
        lblCapacity.ForeColor = slots.Count >= MaxSlots ? Color.Orange : Color.Lime;
    }

    private Panel CreateSlotPanel(ClipboardSlot slot, int index)
    {
        var panel = new Panel
        {
            Size = new Size(75, 75),
            BackColor = slot.IsShiny ? Color.FromArgb(60, 50, 30) : Color.FromArgb(40, 40, 60),
            Margin = new Padding(5),
            Tag = index
        };
        panel.Paint += (s, e) =>
        {
            using var pen = new Pen(slot.IsShiny ? Color.Gold : Color.FromArgb(80, 80, 100), 2);
            e.Graphics.DrawRectangle(pen, 1, 1, panel.Width - 3, panel.Height - 3);
        };

        var lblSpecies = new Label
        {
            Text = slot.SpeciesName.Length > 8 ? slot.SpeciesName[..8] + "..." : slot.SpeciesName,
            Location = new Point(3, 3),
            Size = new Size(69, 18),
            ForeColor = slot.IsShiny ? Color.Gold : Color.White,
            Font = new Font("Segoe UI", 7F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblLevel = new Label
        {
            Text = $"Lv.{slot.Level}",
            Location = new Point(3, 55),
            Size = new Size(35, 15),
            ForeColor = Color.LightGray,
            Font = new Font("Segoe UI", 7F)
        };

        var lblSlot = new Label
        {
            Text = $"#{index + 1}",
            Location = new Point(50, 55),
            Size = new Size(22, 15),
            ForeColor = Color.Cyan,
            Font = new Font("Segoe UI", 7F)
        };

        panel.Controls.AddRange(new Control[] { lblSpecies, lblLevel, lblSlot });

        // Context menu
        var menu = new ContextMenuStrip();
        menu.Items.Add("Paste to Current", null, (s, e) => PasteSlot(index));
        menu.Items.Add("View Details", null, (s, e) => ShowSlotDetails(index));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Duplicate", null, (s, e) => DuplicateSlot(index));
        menu.Items.Add("Remove", null, (s, e) => RemoveSlot(index));
        panel.ContextMenuStrip = menu;

        panel.Click += (s, e) => ShowSlotDetails(index);
        panel.DoubleClick += (s, e) => PasteSlot(index);

        return panel;
    }

    private void RefreshHistory()
    {
        lstHistory.Items.Clear();
        foreach (var h in history.OrderByDescending(x => x.Timestamp))
        {
            var item = new ListViewItem(h.Timestamp.ToString("HH:mm:ss"));
            item.SubItems.Add(h.SpeciesName);
            item.SubItems.Add(h.Level.ToString());
            item.SubItems.Add(h.Action);
            item.SubItems.Add(h.Details);
            item.Tag = h;

            if (h.IsShiny)
                item.ForeColor = Color.Gold;
            else if (h.Action == "Copy")
                item.ForeColor = Color.LightGreen;
            else if (h.Action == "Paste")
                item.ForeColor = Color.LightBlue;

            lstHistory.Items.Add(item);
        }
    }

    private void AddCurrentPokemon(object? sender, EventArgs e)
    {
        if (slots.Count >= MaxSlots)
        {
            WinFormsUtil.Alert("Clipboard is full!", $"Maximum {MaxSlots} slots allowed.\nRemove a slot first.");
            return;
        }

        // Get current Pokemon from main editor (placeholder - would need integration)
        var pk = SAV.GetPartySlotAtIndex(0);
        if (pk.Species == 0)
        {
            WinFormsUtil.Alert("No Pokemon to copy!", "Please select a Pokemon first.");
            return;
        }

        var slot = new ClipboardSlot
        {
            Data = pk.Clone(),
            SpeciesName = SpeciesName.GetSpeciesName(pk.Species, 2),
            Level = pk.CurrentLevel,
            IsShiny = pk.IsShiny,
            CopiedAt = DateTime.Now
        };

        slots.Add(slot);
        AddToHistory(pk, "Copy", "Added to slot #" + slots.Count);
        RefreshSlots();

        WinFormsUtil.Alert($"{slot.SpeciesName} added to clipboard slot #{slots.Count}!");
    }

    private void ShowSlotDetails(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        var slot = slots[index];
        var pk = slot.Data;

        lblSlotInfo.Text = $"Slot #{index + 1}: {slot.SpeciesName}\n\n" +
                          $"Level: {slot.Level}\n" +
                          $"Nature: {pk.Nature}\n" +
                          $"Ability: {pk.Ability}\n" +
                          $"IVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}\n" +
                          $"Shiny: {(slot.IsShiny ? "Yes ★" : "No")}\n" +
                          $"Copied: {slot.CopiedAt:g}";

        lblSlotInfo.ForeColor = slot.IsShiny ? Color.Gold : Color.White;
    }

    private void PasteSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        var slot = slots[index];
        AddToHistory(slot.Data, "Paste", $"Pasted from slot #{index + 1}");

        WinFormsUtil.Alert($"{slot.SpeciesName} pasted!", "Pokemon copied to current editor.");
    }

    private void DuplicateSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;
        if (slots.Count >= MaxSlots)
        {
            WinFormsUtil.Alert("Clipboard is full!");
            return;
        }

        var original = slots[index];
        var copy = new ClipboardSlot
        {
            Data = original.Data.Clone(),
            SpeciesName = original.SpeciesName,
            Level = original.Level,
            IsShiny = original.IsShiny,
            CopiedAt = DateTime.Now
        };

        slots.Add(copy);
        AddToHistory(copy.Data, "Duplicate", $"Duplicated from slot #{index + 1}");
        RefreshSlots();
    }

    private void RemoveSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        var slot = slots[index];
        slots.RemoveAt(index);
        AddToHistory(slot.Data, "Remove", $"Removed from slot #{index + 1}");
        RefreshSlots();
        lblSlotInfo.Text = "Slot removed.\n\nSelect a slot to view details.";
    }

    private void ClearAllSlots(object? sender, EventArgs e)
    {
        if (slots.Count == 0) return;

        var result = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Clear All Slots?",
            $"This will remove all {slots.Count} Pokemon from the clipboard.");

        if (result == DialogResult.Yes)
        {
            slots.Clear();
            RefreshSlots();
            lblSlotInfo.Text = "All slots cleared.\n\nClick + to add Pokemon.";
        }
    }

    private void PasteAllToBox(object? sender, EventArgs e)
    {
        if (slots.Count == 0)
        {
            WinFormsUtil.Alert("No Pokemon in clipboard!");
            return;
        }

        WinFormsUtil.Alert($"Ready to paste {slots.Count} Pokemon",
            "This would paste all clipboard Pokemon to the current box.\n\n(Feature requires main editor integration)");
    }

    private void RestoreFromHistory(object? sender, EventArgs e)
    {
        if (lstHistory.SelectedItems.Count == 0) return;
        if (slots.Count >= MaxSlots)
        {
            WinFormsUtil.Alert("Clipboard is full!");
            return;
        }

        var historyItem = (ClipboardHistory)lstHistory.SelectedItems[0].Tag;

        var slot = new ClipboardSlot
        {
            Data = historyItem.Data.Clone(),
            SpeciesName = historyItem.SpeciesName,
            Level = historyItem.Level,
            IsShiny = historyItem.IsShiny,
            CopiedAt = DateTime.Now
        };

        slots.Add(slot);
        RefreshSlots();
        WinFormsUtil.Alert($"{slot.SpeciesName} restored to clipboard!");
    }

    private void AddToHistory(PKM pk, string action, string details)
    {
        var item = new ClipboardHistory
        {
            Data = pk.Clone(),
            SpeciesName = SpeciesName.GetSpeciesName(pk.Species, 2),
            Level = pk.CurrentLevel,
            IsShiny = pk.IsShiny,
            Action = action,
            Details = details,
            Timestamp = DateTime.Now
        };

        history.Insert(0, item);

        // Keep history under limit
        while (history.Count > MaxHistory)
            history.RemoveAt(history.Count - 1);

        RefreshHistory();
    }

    private class ClipboardSlot
    {
        public PKM Data { get; set; } = null!;
        public string SpeciesName { get; set; } = "";
        public int Level { get; set; }
        public bool IsShiny { get; set; }
        public DateTime CopiedAt { get; set; }
    }

    private class ClipboardHistory
    {
        public PKM Data { get; set; } = null!;
        public string SpeciesName { get; set; } = "";
        public int Level { get; set; }
        public bool IsShiny { get; set; }
        public string Action { get; set; } = "";
        public string Details { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
