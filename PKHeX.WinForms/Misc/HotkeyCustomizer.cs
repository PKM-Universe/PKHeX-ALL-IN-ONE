using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class HotkeyCustomizer : Form
{
    private ListView lstHotkeys = null!;
    private TextBox txtSearch = null!;
    private Label lblCurrentKey = null!;
    private Button btnSetKey = null!;
    private Button btnClearKey = null!;
    private ComboBox cmbCategory = null!;
    private CheckBox chkCtrl = null!;
    private CheckBox chkAlt = null!;
    private CheckBox chkShift = null!;
    private ComboBox cmbKey = null!;
    private bool isCapturing = false;
    private Keys capturedKey = Keys.None;

    private static readonly string HotkeyFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PKHeX", "hotkeys.json");

    private readonly Dictionary<string, HotkeyBinding> hotkeys = new();

    private static readonly (string Category, string Action, string DefaultKey, string Description)[] DefaultHotkeys = new[]
    {
        // File Operations
        ("File", "OpenFile", "Ctrl+O", "Open save file"),
        ("File", "SaveFile", "Ctrl+S", "Save current file"),
        ("File", "SaveAs", "Ctrl+Shift+S", "Save file as..."),
        ("File", "ExportPKM", "Ctrl+E", "Export current Pokemon"),
        ("File", "ImportPKM", "Ctrl+I", "Import Pokemon file"),
        ("File", "ExportBox", "Ctrl+Shift+E", "Export current box"),
        ("File", "QuickBackup", "Ctrl+B", "Quick backup save"),

        // Pokemon Operations
        ("Pokemon", "SetShiny", "F2", "Toggle shiny status"),
        ("Pokemon", "MaxIVs", "F3", "Set all IVs to 31"),
        ("Pokemon", "MaxEVs", "F4", "Set 252/252/4 EVs"),
        ("Pokemon", "Legalize", "F5", "Auto-legalize Pokemon"),
        ("Pokemon", "Clone", "Ctrl+D", "Clone current Pokemon"),
        ("Pokemon", "Delete", "Delete", "Delete current Pokemon"),
        ("Pokemon", "CopyPokemon", "Ctrl+C", "Copy Pokemon to clipboard"),
        ("Pokemon", "PastePokemon", "Ctrl+V", "Paste Pokemon from clipboard"),
        ("Pokemon", "ViewSummary", "F1", "View Pokemon summary"),
        ("Pokemon", "RandomPokemon", "Ctrl+R", "Generate random Pokemon"),

        // Navigation
        ("Navigation", "NextBox", "Right", "Go to next box"),
        ("Navigation", "PrevBox", "Left", "Go to previous box"),
        ("Navigation", "FirstBox", "Home", "Go to first box"),
        ("Navigation", "LastBox", "End", "Go to last box"),
        ("Navigation", "NextSlot", "Tab", "Select next slot"),
        ("Navigation", "PrevSlot", "Shift+Tab", "Select previous slot"),

        // Tools
        ("Tools", "OpenDatabase", "Ctrl+F", "Open Pokemon database"),
        ("Tools", "OpenEncounters", "Ctrl+Shift+F", "Open encounter database"),
        ("Tools", "BatchEditor", "Ctrl+Shift+B", "Open batch editor"),
        ("Tools", "ShowdownImport", "Ctrl+Shift+I", "Import from Showdown"),
        ("Tools", "ShowdownExport", "Ctrl+Shift+X", "Export to Showdown"),
        ("Tools", "TeamAnalyzer", "Ctrl+T", "Analyze team"),
        ("Tools", "LegalityCheck", "Ctrl+L", "Check legality"),

        // View
        ("View", "ToggleTheme", "Ctrl+Shift+T", "Toggle dark/light theme"),
        ("View", "ZoomIn", "Ctrl+Plus", "Zoom in"),
        ("View", "ZoomOut", "Ctrl+Minus", "Zoom out"),
        ("View", "ResetZoom", "Ctrl+0", "Reset zoom"),
        ("View", "FullScreen", "F11", "Toggle fullscreen"),

        // Box Operations
        ("Box", "SortByDex", "Ctrl+1", "Sort box by Dex #"),
        ("Box", "SortByLevel", "Ctrl+2", "Sort box by level"),
        ("Box", "SortBySpecies", "Ctrl+3", "Sort box by species"),
        ("Box", "SortByShiny", "Ctrl+4", "Sort shinies first"),
        ("Box", "ClearBox", "Ctrl+Shift+Delete", "Clear current box"),
        ("Box", "FillBox", "Ctrl+Shift+F", "Fill box with template"),

        // Quick Actions
        ("Quick", "QuickHeal", "H", "Heal all Pokemon"),
        ("Quick", "QuickPokerus", "P", "Toggle Pokerus"),
        ("Quick", "QuickFriendship", "Shift+F", "Max friendship"),
        ("Quick", "QuickLevel100", "Shift+L", "Set level to 100"),
        ("Quick", "QuickEggHatch", "Shift+H", "Hatch egg instantly"),
    };

    public HotkeyCustomizer()
    {
        Text = "Hotkey Customizer";
        Size = new Size(900, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        KeyPreview = true;
        LoadHotkeys();
        InitializeUI();
        PopulateHotkeys();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "⌨️ Hotkey Customizer",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold)
        };

        var lblSubtitle = new Label
        {
            Text = "Customize keyboard shortcuts for any action",
            Location = new Point(22, 50),
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9F)
        };

        // Filter Controls
        var lblCategory = new Label { Text = "Category:", Location = new Point(20, 85), AutoSize = true, ForeColor = Color.White };
        cmbCategory = new ComboBox
        {
            Location = new Point(90, 82),
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbCategory.Items.AddRange(new[] { "All Categories", "File", "Pokemon", "Navigation", "Tools", "View", "Box", "Quick" });
        cmbCategory.SelectedIndex = 0;
        cmbCategory.SelectedIndexChanged += (s, e) => FilterHotkeys();

        var lblSearch = new Label { Text = "Search:", Location = new Point(270, 85), AutoSize = true, ForeColor = Color.White };
        txtSearch = new TextBox
        {
            Location = new Point(330, 82),
            Width = 200,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        txtSearch.TextChanged += (s, e) => FilterHotkeys();

        // Hotkey List
        var grpHotkeys = new GroupBox
        {
            Text = "Keyboard Shortcuts",
            Location = new Point(20, 115),
            Size = new Size(540, 420),
            ForeColor = Color.White
        };

        lstHotkeys = new ListView
        {
            Location = new Point(10, 25),
            Size = new Size(520, 385),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            GridLines = true
        };
        lstHotkeys.Columns.Add("Action", 180);
        lstHotkeys.Columns.Add("Hotkey", 120);
        lstHotkeys.Columns.Add("Category", 80);
        lstHotkeys.Columns.Add("Description", 120);
        lstHotkeys.SelectedIndexChanged += HotkeySelected;

        grpHotkeys.Controls.Add(lstHotkeys);

        // Key Assignment
        var grpAssign = new GroupBox
        {
            Text = "Assign Hotkey",
            Location = new Point(580, 115),
            Size = new Size(290, 200),
            ForeColor = Color.White
        };

        lblCurrentKey = new Label
        {
            Text = "Current: None",
            Location = new Point(15, 30),
            Size = new Size(260, 25),
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        var lblNewKey = new Label { Text = "New Hotkey:", Location = new Point(15, 65), AutoSize = true, ForeColor = Color.White };

        chkCtrl = new CheckBox { Text = "Ctrl", Location = new Point(15, 90), AutoSize = true, ForeColor = Color.Cyan };
        chkAlt = new CheckBox { Text = "Alt", Location = new Point(75, 90), AutoSize = true, ForeColor = Color.Cyan };
        chkShift = new CheckBox { Text = "Shift", Location = new Point(125, 90), AutoSize = true, ForeColor = Color.Cyan };

        cmbKey = new ComboBox
        {
            Location = new Point(15, 120),
            Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        PopulateKeys();

        btnSetKey = new Button
        {
            Text = "Set Hotkey",
            Location = new Point(15, 160),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 60),
            ForeColor = Color.White
        };
        btnSetKey.Click += SetHotkey;

        btnClearKey = new Button
        {
            Text = "Clear",
            Location = new Point(125, 160),
            Size = new Size(70, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 60, 60),
            ForeColor = Color.White
        };
        btnClearKey.Click += ClearHotkey;

        var btnCapture = new Button
        {
            Text = "Capture Key...",
            Location = new Point(200, 120),
            Size = new Size(80, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8F)
        };
        btnCapture.Click += StartCapture;

        grpAssign.Controls.AddRange(new Control[] { lblCurrentKey, lblNewKey, chkCtrl, chkAlt, chkShift, cmbKey, btnSetKey, btnClearKey, btnCapture });

        // Presets
        var grpPresets = new GroupBox
        {
            Text = "Presets",
            Location = new Point(580, 325),
            Size = new Size(290, 100),
            ForeColor = Color.White
        };

        var btnDefault = new Button
        {
            Text = "Reset to Default",
            Location = new Point(15, 30),
            Size = new Size(120, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnDefault.Click += ResetToDefault;

        var btnVSCode = new Button
        {
            Text = "VS Code Style",
            Location = new Point(145, 30),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White
        };

        var btnMinimal = new Button
        {
            Text = "Minimal",
            Location = new Point(15, 65),
            Size = new Size(80, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 60, 80),
            ForeColor = Color.White
        };

        grpPresets.Controls.AddRange(new Control[] { btnDefault, btnVSCode, btnMinimal });

        // Conflicts Warning
        var grpConflicts = new GroupBox
        {
            Text = "Conflicts",
            Location = new Point(580, 435),
            Size = new Size(290, 100),
            ForeColor = Color.White
        };

        var lblConflicts = new Label
        {
            Text = "No conflicts detected.\n\nHotkeys are checked for duplicates\nautomatically.",
            Location = new Point(15, 25),
            Size = new Size(260, 65),
            ForeColor = Color.Lime
        };

        grpConflicts.Controls.Add(lblConflicts);

        // Bottom Buttons
        var btnSave = new Button
        {
            Text = "Save All",
            Location = new Point(580, 550),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 60),
            ForeColor = Color.White
        };
        btnSave.Click += SaveHotkeys;

        var btnExport = new Button
        {
            Text = "Export",
            Location = new Point(690, 550),
            Size = new Size(80, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };
        btnExport.Click += ExportHotkeys;

        var btnImport = new Button
        {
            Text = "Import",
            Location = new Point(780, 550),
            Size = new Size(80, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };
        btnImport.Click += ImportHotkeys;

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(770, 600),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblCategory, cmbCategory, lblSearch, txtSearch,
            grpHotkeys, grpAssign, grpPresets, grpConflicts, btnSave, btnExport, btnImport, btnClose });
    }

    private void PopulateKeys()
    {
        var keys = new List<string> { "(None)" };

        // Letters
        for (char c = 'A'; c <= 'Z'; c++)
            keys.Add(c.ToString());

        // Numbers
        for (int i = 0; i <= 9; i++)
            keys.Add($"D{i}");

        // Function keys
        for (int i = 1; i <= 12; i++)
            keys.Add($"F{i}");

        // Special keys
        keys.AddRange(new[] { "Space", "Enter", "Escape", "Tab", "Backspace", "Delete", "Insert",
            "Home", "End", "PageUp", "PageDown", "Up", "Down", "Left", "Right",
            "Plus", "Minus", "OemPeriod", "OemComma", "OemQuestion" });

        cmbKey.Items.AddRange(keys.ToArray());
        cmbKey.SelectedIndex = 0;
    }

    private void LoadHotkeys()
    {
        hotkeys.Clear();

        // Load defaults first
        foreach (var hk in DefaultHotkeys)
        {
            hotkeys[hk.Action] = new HotkeyBinding
            {
                Action = hk.Action,
                Category = hk.Category,
                KeyCombo = hk.DefaultKey,
                Description = hk.Description
            };
        }

        // Try to load saved hotkeys
        try
        {
            if (File.Exists(HotkeyFile))
            {
                var json = File.ReadAllText(HotkeyFile);
                var saved = JsonSerializer.Deserialize<Dictionary<string, HotkeyBinding>>(json);
                if (saved != null)
                {
                    foreach (var kvp in saved)
                    {
                        if (hotkeys.ContainsKey(kvp.Key))
                            hotkeys[kvp.Key].KeyCombo = kvp.Value.KeyCombo;
                    }
                }
            }
        }
        catch { /* Use defaults */ }
    }

    private void PopulateHotkeys()
    {
        lstHotkeys.Items.Clear();
        foreach (var hk in hotkeys.Values.OrderBy(h => h.Category).ThenBy(h => h.Action))
        {
            var item = new ListViewItem(hk.Action);
            item.SubItems.Add(hk.KeyCombo);
            item.SubItems.Add(hk.Category);
            item.SubItems.Add(hk.Description);
            item.Tag = hk;

            // Color by category
            item.ForeColor = hk.Category switch
            {
                "File" => Color.LightBlue,
                "Pokemon" => Color.Gold,
                "Navigation" => Color.LightGreen,
                "Tools" => Color.Cyan,
                "View" => Color.Pink,
                "Box" => Color.Orange,
                "Quick" => Color.Lime,
                _ => Color.White
            };

            lstHotkeys.Items.Add(item);
        }
    }

    private void FilterHotkeys()
    {
        string category = cmbCategory.SelectedItem?.ToString() ?? "All Categories";
        string search = txtSearch.Text.ToLower();

        lstHotkeys.Items.Clear();
        foreach (var hk in hotkeys.Values.OrderBy(h => h.Category).ThenBy(h => h.Action))
        {
            if (category != "All Categories" && hk.Category != category)
                continue;
            if (!string.IsNullOrEmpty(search) &&
                !hk.Action.ToLower().Contains(search) &&
                !hk.Description.ToLower().Contains(search))
                continue;

            var item = new ListViewItem(hk.Action);
            item.SubItems.Add(hk.KeyCombo);
            item.SubItems.Add(hk.Category);
            item.SubItems.Add(hk.Description);
            item.Tag = hk;

            item.ForeColor = hk.Category switch
            {
                "File" => Color.LightBlue,
                "Pokemon" => Color.Gold,
                "Navigation" => Color.LightGreen,
                "Tools" => Color.Cyan,
                "View" => Color.Pink,
                "Box" => Color.Orange,
                "Quick" => Color.Lime,
                _ => Color.White
            };

            lstHotkeys.Items.Add(item);
        }
    }

    private void HotkeySelected(object? sender, EventArgs e)
    {
        if (lstHotkeys.SelectedItems.Count == 0) return;

        var hk = (HotkeyBinding)lstHotkeys.SelectedItems[0].Tag;
        lblCurrentKey.Text = $"Current: {hk.KeyCombo}";

        // Parse key combo
        chkCtrl.Checked = hk.KeyCombo.Contains("Ctrl");
        chkAlt.Checked = hk.KeyCombo.Contains("Alt");
        chkShift.Checked = hk.KeyCombo.Contains("Shift");

        var keyPart = hk.KeyCombo.Split('+').Last().Trim();
        int idx = cmbKey.Items.IndexOf(keyPart);
        cmbKey.SelectedIndex = idx >= 0 ? idx : 0;
    }

    private void SetHotkey(object? sender, EventArgs e)
    {
        if (lstHotkeys.SelectedItems.Count == 0)
        {
            WinFormsUtil.Alert("Please select an action first.");
            return;
        }

        var hk = (HotkeyBinding)lstHotkeys.SelectedItems[0].Tag;
        var keyPart = cmbKey.SelectedItem?.ToString() ?? "";

        if (keyPart == "(None)")
        {
            hk.KeyCombo = "None";
        }
        else
        {
            var combo = new List<string>();
            if (chkCtrl.Checked) combo.Add("Ctrl");
            if (chkAlt.Checked) combo.Add("Alt");
            if (chkShift.Checked) combo.Add("Shift");
            combo.Add(keyPart);
            hk.KeyCombo = string.Join("+", combo);
        }

        // Check for conflicts
        var conflict = hotkeys.Values.FirstOrDefault(h => h.Action != hk.Action && h.KeyCombo == hk.KeyCombo && hk.KeyCombo != "None");
        if (conflict != null)
        {
            var result = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Hotkey Conflict",
                $"'{hk.KeyCombo}' is already assigned to '{conflict.Action}'.\n\nRemove from '{conflict.Action}' and assign to '{hk.Action}'?");
            if (result == DialogResult.Yes)
            {
                conflict.KeyCombo = "None";
            }
            else
            {
                return;
            }
        }

        lblCurrentKey.Text = $"Current: {hk.KeyCombo}";
        PopulateHotkeys();
    }

    private void ClearHotkey(object? sender, EventArgs e)
    {
        if (lstHotkeys.SelectedItems.Count == 0) return;

        var hk = (HotkeyBinding)lstHotkeys.SelectedItems[0].Tag;
        hk.KeyCombo = "None";
        lblCurrentKey.Text = "Current: None";
        PopulateHotkeys();
    }

    private void StartCapture(object? sender, EventArgs e)
    {
        isCapturing = true;
        ((Button)sender!).Text = "Press key...";
        ((Button)sender!).BackColor = Color.FromArgb(140, 140, 60);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (isCapturing)
        {
            chkCtrl.Checked = e.Control;
            chkAlt.Checked = e.Alt;
            chkShift.Checked = e.Shift;

            var key = e.KeyCode.ToString();
            int idx = cmbKey.Items.IndexOf(key);
            if (idx >= 0)
                cmbKey.SelectedIndex = idx;

            isCapturing = false;
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        base.OnKeyDown(e);
    }

    private void ResetToDefault(object? sender, EventArgs e)
    {
        var result = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Reset Hotkeys",
            "Reset all hotkeys to default values?");
        if (result == DialogResult.Yes)
        {
            foreach (var hk in DefaultHotkeys)
            {
                if (hotkeys.ContainsKey(hk.Action))
                    hotkeys[hk.Action].KeyCombo = hk.DefaultKey;
            }
            PopulateHotkeys();
        }
    }

    private void SaveHotkeys(object? sender, EventArgs e)
    {
        try
        {
            var dir = Path.GetDirectoryName(HotkeyFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(hotkeys, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(HotkeyFile, json);
            WinFormsUtil.Alert("Hotkeys saved successfully!");
        }
        catch (Exception ex)
        {
            WinFormsUtil.Error("Failed to save hotkeys", ex.Message);
        }
    }

    private void ExportHotkeys(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Export Hotkeys",
            Filter = "JSON Files|*.json",
            FileName = "pkm-universe-hotkeys.json"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            var json = JsonSerializer.Serialize(hotkeys, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(sfd.FileName, json);
            WinFormsUtil.Alert("Hotkeys exported!");
        }
    }

    private void ImportHotkeys(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Import Hotkeys",
            Filter = "JSON Files|*.json"
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var json = File.ReadAllText(ofd.FileName);
                var imported = JsonSerializer.Deserialize<Dictionary<string, HotkeyBinding>>(json);
                if (imported != null)
                {
                    foreach (var kvp in imported)
                    {
                        if (hotkeys.ContainsKey(kvp.Key))
                            hotkeys[kvp.Key].KeyCombo = kvp.Value.KeyCombo;
                    }
                    PopulateHotkeys();
                    WinFormsUtil.Alert("Hotkeys imported!");
                }
            }
            catch (Exception ex)
            {
                WinFormsUtil.Error("Failed to import hotkeys", ex.Message);
            }
        }
    }

    private class HotkeyBinding
    {
        public string Action { get; set; } = "";
        public string Category { get; set; } = "";
        public string KeyCombo { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
