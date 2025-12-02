using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace PKHeX.WinForms;

public class ThemeDesigner : Form
{
    private Panel pnlPreview = null!;
    private Panel pnlColorPicker = null!;
    private ListBox lstColors = null!;
    private TextBox txtThemeName = null!;
    private TrackBar trkHue = null!;
    private TrackBar trkSat = null!;
    private TrackBar trkLight = null!;
    private Label lblCurrentColor = null!;
    private Panel pnlCurrentColor = null!;
    private TextBox txtHexColor = null!;
    private ComboBox cmbPresets = null!;

    private readonly Dictionary<string, Color> themeColors = new();
    private string selectedColorKey = "";

    private static readonly string ThemesFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PKHeX", "Themes");

    private static readonly (string Key, string Name, Color Default)[] ColorDefinitions = new[]
    {
        // Backgrounds
        ("Background", "Main Background", Color.FromArgb(25, 25, 40)),
        ("BackgroundSecondary", "Secondary Background", Color.FromArgb(35, 35, 55)),
        ("BackgroundTertiary", "Tertiary Background", Color.FromArgb(45, 45, 65)),
        ("Surface", "Surface Color", Color.FromArgb(55, 55, 75)),
        ("SurfaceHover", "Surface Hover", Color.FromArgb(65, 65, 85)),

        // Primary Colors
        ("Primary", "Primary Accent", Color.FromArgb(100, 100, 255)),
        ("PrimaryDark", "Primary Dark", Color.FromArgb(70, 70, 200)),
        ("PrimaryLight", "Primary Light", Color.FromArgb(140, 140, 255)),
        ("Secondary", "Secondary Accent", Color.FromArgb(255, 100, 200)),
        ("Accent", "Accent Color", Color.FromArgb(100, 255, 200)),

        // Text
        ("Text", "Text Primary", Color.White),
        ("TextSecondary", "Text Secondary", Color.FromArgb(180, 180, 180)),
        ("TextMuted", "Text Muted", Color.FromArgb(120, 120, 120)),
        ("TextDisabled", "Text Disabled", Color.FromArgb(80, 80, 80)),

        // Borders
        ("Border", "Border Color", Color.FromArgb(70, 70, 90)),
        ("BorderLight", "Border Light", Color.FromArgb(90, 90, 110)),
        ("BorderFocus", "Border Focus", Color.FromArgb(100, 150, 255)),

        // Status
        ("Success", "Success/Legal", Color.FromArgb(100, 200, 100)),
        ("Warning", "Warning", Color.FromArgb(255, 180, 80)),
        ("Error", "Error/Illegal", Color.FromArgb(255, 100, 100)),
        ("Info", "Info", Color.FromArgb(100, 180, 255)),

        // Pokemon Specific
        ("Shiny", "Shiny Gold", Color.Gold),
        ("Legal", "Legal Green", Color.FromArgb(144, 238, 144)),
        ("Illegal", "Illegal Red", Color.FromArgb(255, 99, 71)),
        ("Legendary", "Legendary Purple", Color.FromArgb(180, 100, 255)),

        // UI Elements
        ("MenuBackground", "Menu Background", Color.FromArgb(40, 40, 60)),
        ("MenuHover", "Menu Hover", Color.FromArgb(60, 60, 80)),
        ("ButtonPrimary", "Button Primary", Color.FromArgb(60, 100, 160)),
        ("ButtonSecondary", "Button Secondary", Color.FromArgb(80, 80, 100)),
        ("ButtonSuccess", "Button Success", Color.FromArgb(60, 140, 60)),
        ("ButtonDanger", "Button Danger", Color.FromArgb(140, 60, 60)),
        ("InputBackground", "Input Background", Color.FromArgb(45, 45, 65)),
        ("InputBorder", "Input Border", Color.FromArgb(70, 70, 90)),

        // Box/Slots
        ("BoxBackground", "Box Background", Color.FromArgb(30, 30, 50)),
        ("SlotEmpty", "Empty Slot", Color.FromArgb(40, 40, 60)),
        ("SlotHover", "Slot Hover", Color.FromArgb(60, 60, 80)),
        ("SlotSelected", "Slot Selected", Color.FromArgb(80, 100, 140)),
    };

    public ThemeDesigner()
    {
        Text = "Theme Designer";
        Size = new Size(1100, 750);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeDefaults();
        InitializeUI();
        UpdatePreview();
    }

    private void InitializeDefaults()
    {
        foreach (var def in ColorDefinitions)
            themeColors[def.Key] = def.Default;
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "🎨 Theme Designer",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.FromArgb(255, 100, 200),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold)
        };

        var lblSubtitle = new Label
        {
            Text = "Create your own custom theme with live preview",
            Location = new Point(22, 50),
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9F)
        };

        // Theme Name
        var lblName = new Label { Text = "Theme Name:", Location = new Point(20, 85), AutoSize = true, ForeColor = Color.White };
        txtThemeName = new TextBox
        {
            Location = new Point(110, 82),
            Width = 200,
            Text = "My Custom Theme",
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        // Preset Selector
        var lblPreset = new Label { Text = "Base Preset:", Location = new Point(330, 85), AutoSize = true, ForeColor = Color.White };
        cmbPresets = new ComboBox
        {
            Location = new Point(420, 82),
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbPresets.Items.AddRange(new[] { "Dark (Default)", "Light", "Midnight", "Pokemon Red", "Pokemon Blue", "Neon Cyber", "Ocean", "Forest", "Sunset" });
        cmbPresets.SelectedIndex = 0;
        cmbPresets.SelectedIndexChanged += LoadPreset;

        // Color List
        var grpColors = new GroupBox
        {
            Text = "Theme Colors",
            Location = new Point(20, 115),
            Size = new Size(300, 480),
            ForeColor = Color.White
        };

        lstColors = new ListBox
        {
            Location = new Point(10, 25),
            Size = new Size(280, 445),
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            DrawMode = DrawMode.OwnerDrawFixed,
            ItemHeight = 28
        };
        lstColors.DrawItem += DrawColorItem;
        lstColors.SelectedIndexChanged += ColorSelected;

        foreach (var def in ColorDefinitions)
            lstColors.Items.Add(def.Key);

        grpColors.Controls.Add(lstColors);

        // Color Picker
        var grpPicker = new GroupBox
        {
            Text = "Color Editor",
            Location = new Point(340, 115),
            Size = new Size(320, 250),
            ForeColor = Color.White
        };

        lblCurrentColor = new Label
        {
            Text = "Selected: Background",
            Location = new Point(15, 28),
            Size = new Size(200, 20),
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        pnlCurrentColor = new Panel
        {
            Location = new Point(220, 25),
            Size = new Size(80, 25),
            BackColor = Color.FromArgb(25, 25, 40),
            BorderStyle = BorderStyle.FixedSingle
        };

        // HSL Sliders
        var lblHue = new Label { Text = "Hue:", Location = new Point(15, 65), AutoSize = true, ForeColor = Color.White };
        trkHue = new TrackBar
        {
            Location = new Point(80, 60),
            Size = new Size(200, 30),
            Minimum = 0,
            Maximum = 360,
            TickFrequency = 30,
            BackColor = Color.FromArgb(35, 35, 55)
        };
        trkHue.ValueChanged += SliderChanged;

        var lblSat = new Label { Text = "Saturation:", Location = new Point(15, 105), AutoSize = true, ForeColor = Color.White };
        trkSat = new TrackBar
        {
            Location = new Point(80, 100),
            Size = new Size(200, 30),
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            TickFrequency = 10,
            BackColor = Color.FromArgb(35, 35, 55)
        };
        trkSat.ValueChanged += SliderChanged;

        var lblLight = new Label { Text = "Lightness:", Location = new Point(15, 145), AutoSize = true, ForeColor = Color.White };
        trkLight = new TrackBar
        {
            Location = new Point(80, 140),
            Size = new Size(200, 30),
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            TickFrequency = 10,
            BackColor = Color.FromArgb(35, 35, 55)
        };
        trkLight.ValueChanged += SliderChanged;

        // Hex Input
        var lblHex = new Label { Text = "Hex:", Location = new Point(15, 185), AutoSize = true, ForeColor = Color.White };
        txtHexColor = new TextBox
        {
            Location = new Point(80, 182),
            Width = 100,
            Text = "#191928",
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        txtHexColor.TextChanged += HexChanged;

        var btnApply = new Button
        {
            Text = "Apply",
            Location = new Point(190, 180),
            Size = new Size(60, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 60),
            ForeColor = Color.White
        };
        btnApply.Click += ApplyColor;

        var btnColorDialog = new Button
        {
            Text = "Pick Color...",
            Location = new Point(15, 215),
            Size = new Size(100, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };
        btnColorDialog.Click += OpenColorDialog;

        grpPicker.Controls.AddRange(new Control[] { lblCurrentColor, pnlCurrentColor, lblHue, trkHue, lblSat, trkSat, lblLight, trkLight, lblHex, txtHexColor, btnApply, btnColorDialog });

        // Quick Colors
        var grpQuick = new GroupBox
        {
            Text = "Quick Colors",
            Location = new Point(340, 375),
            Size = new Size(320, 120),
            ForeColor = Color.White
        };

        var quickColors = new[]
        {
            (Color.FromArgb(25, 25, 40), "Dark BG"),
            (Color.FromArgb(240, 240, 245), "Light BG"),
            (Color.FromArgb(100, 100, 255), "Blue"),
            (Color.FromArgb(255, 100, 100), "Red"),
            (Color.FromArgb(100, 255, 100), "Green"),
            (Color.FromArgb(255, 200, 100), "Gold"),
            (Color.FromArgb(255, 100, 200), "Pink"),
            (Color.FromArgb(100, 255, 255), "Cyan"),
            (Color.FromArgb(200, 100, 255), "Purple"),
            (Color.FromArgb(255, 150, 50), "Orange"),
        };

        for (int i = 0; i < quickColors.Length; i++)
        {
            var (color, name) = quickColors[i];
            var btn = new Button
            {
                Location = new Point(15 + (i % 5) * 60, 25 + (i / 5) * 45),
                Size = new Size(55, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                Text = "",
                Tag = color
            };
            btn.FlatAppearance.BorderColor = Color.White;
            btn.Click += (s, e) => SetQuickColor((Color)((Button)s!).Tag!);

            var lbl = new ToolTip();
            lbl.SetToolTip(btn, name);

            grpQuick.Controls.Add(btn);
        }

        // Preview
        var grpPreview = new GroupBox
        {
            Text = "Live Preview",
            Location = new Point(680, 115),
            Size = new Size(390, 480),
            ForeColor = Color.White
        };

        pnlPreview = new Panel
        {
            Location = new Point(10, 25),
            Size = new Size(370, 445),
            BorderStyle = BorderStyle.FixedSingle
        };

        grpPreview.Controls.Add(pnlPreview);

        // Auto-generate
        var grpAuto = new GroupBox
        {
            Text = "Auto-Generate",
            Location = new Point(340, 505),
            Size = new Size(320, 90),
            ForeColor = Color.White
        };

        var btnAutoFromPrimary = new Button
        {
            Text = "Generate from Primary",
            Location = new Point(15, 30),
            Size = new Size(140, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };
        btnAutoFromPrimary.Click += GenerateFromPrimary;

        var btnAutoComplement = new Button
        {
            Text = "Complementary",
            Location = new Point(165, 30),
            Size = new Size(120, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 140),
            ForeColor = Color.White
        };

        var btnRandomize = new Button
        {
            Text = "Randomize All",
            Location = new Point(15, 65),
            Size = new Size(100, 20),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 100, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8F)
        };
        btnRandomize.Click += RandomizeColors;

        grpAuto.Controls.AddRange(new Control[] { btnAutoFromPrimary, btnAutoComplement, btnRandomize });

        // Bottom Buttons
        var btnSave = new Button
        {
            Text = "Save Theme",
            Location = new Point(680, 610),
            Size = new Size(120, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        btnSave.Click += SaveTheme;

        var btnLoad = new Button
        {
            Text = "Load Theme",
            Location = new Point(810, 610),
            Size = new Size(100, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };
        btnLoad.Click += LoadTheme;

        var btnApplyToApp = new Button
        {
            Text = "Apply to App",
            Location = new Point(920, 610),
            Size = new Size(100, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 100, 60),
            ForeColor = Color.White
        };

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(970, 670),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblName, txtThemeName, lblPreset, cmbPresets,
            grpColors, grpPicker, grpQuick, grpPreview, grpAuto, btnSave, btnLoad, btnApplyToApp, btnClose });

        lstColors.SelectedIndex = 0;
    }

    private void DrawColorItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        e.DrawBackground();

        var key = lstColors.Items[e.Index].ToString()!;
        var color = themeColors.ContainsKey(key) ? themeColors[key] : Color.White;
        var def = ColorDefinitions.FirstOrDefault(d => d.Key == key);

        // Color swatch
        using var brush = new SolidBrush(color);
        e.Graphics.FillRectangle(brush, e.Bounds.X + 5, e.Bounds.Y + 4, 20, 20);
        e.Graphics.DrawRectangle(Pens.White, e.Bounds.X + 5, e.Bounds.Y + 4, 20, 20);

        // Name
        using var textBrush = new SolidBrush(e.ForeColor);
        e.Graphics.DrawString(def.Name, e.Font!, textBrush, e.Bounds.X + 32, e.Bounds.Y + 5);

        e.DrawFocusRectangle();
    }

    private void ColorSelected(object? sender, EventArgs e)
    {
        if (lstColors.SelectedIndex < 0) return;

        selectedColorKey = lstColors.SelectedItem?.ToString() ?? "";
        if (!themeColors.ContainsKey(selectedColorKey)) return;

        var color = themeColors[selectedColorKey];
        var def = ColorDefinitions.FirstOrDefault(d => d.Key == selectedColorKey);

        lblCurrentColor.Text = $"Selected: {def.Name}";
        pnlCurrentColor.BackColor = color;
        txtHexColor.Text = ColorToHex(color);

        // Update sliders (approximate HSL)
        ColorToHSL(color, out int h, out int s, out int l);
        trkHue.Value = h;
        trkSat.Value = s;
        trkLight.Value = l;
    }

    private void SliderChanged(object? sender, EventArgs e)
    {
        var color = HSLToColor(trkHue.Value, trkSat.Value, trkLight.Value);
        pnlCurrentColor.BackColor = color;
        txtHexColor.Text = ColorToHex(color);
    }

    private void HexChanged(object? sender, EventArgs e)
    {
        try
        {
            var hex = txtHexColor.Text.TrimStart('#');
            if (hex.Length == 6)
            {
                var color = ColorTranslator.FromHtml("#" + hex);
                pnlCurrentColor.BackColor = color;
            }
        }
        catch { }
    }

    private void ApplyColor(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(selectedColorKey)) return;

        themeColors[selectedColorKey] = pnlCurrentColor.BackColor;
        lstColors.Invalidate();
        UpdatePreview();
    }

    private void SetQuickColor(Color color)
    {
        pnlCurrentColor.BackColor = color;
        txtHexColor.Text = ColorToHex(color);
        ColorToHSL(color, out int h, out int s, out int l);
        trkHue.Value = Math.Clamp(h, 0, 360);
        trkSat.Value = Math.Clamp(s, 0, 100);
        trkLight.Value = Math.Clamp(l, 0, 100);
    }

    private void OpenColorDialog(object? sender, EventArgs e)
    {
        using var cd = new ColorDialog { Color = pnlCurrentColor.BackColor, FullOpen = true };
        if (cd.ShowDialog() == DialogResult.OK)
        {
            SetQuickColor(cd.Color);
        }
    }

    private void LoadPreset(object? sender, EventArgs e)
    {
        var preset = cmbPresets.SelectedItem?.ToString() ?? "";

        switch (preset)
        {
            case "Light":
                ApplyLightPreset();
                break;
            case "Midnight":
                ApplyMidnightPreset();
                break;
            case "Pokemon Red":
                ApplyRedPreset();
                break;
            case "Pokemon Blue":
                ApplyBluePreset();
                break;
            case "Neon Cyber":
                ApplyNeonPreset();
                break;
            default:
                InitializeDefaults();
                break;
        }

        lstColors.Invalidate();
        UpdatePreview();
    }

    private void ApplyLightPreset()
    {
        themeColors["Background"] = Color.FromArgb(245, 245, 250);
        themeColors["BackgroundSecondary"] = Color.FromArgb(235, 235, 240);
        themeColors["BackgroundTertiary"] = Color.FromArgb(225, 225, 230);
        themeColors["Surface"] = Color.White;
        themeColors["Text"] = Color.FromArgb(30, 30, 40);
        themeColors["TextSecondary"] = Color.FromArgb(80, 80, 90);
        themeColors["Primary"] = Color.FromArgb(100, 100, 220);
    }

    private void ApplyMidnightPreset()
    {
        themeColors["Background"] = Color.FromArgb(10, 10, 20);
        themeColors["BackgroundSecondary"] = Color.FromArgb(20, 20, 35);
        themeColors["Primary"] = Color.FromArgb(80, 120, 200);
        themeColors["Accent"] = Color.FromArgb(100, 200, 255);
    }

    private void ApplyRedPreset()
    {
        themeColors["Primary"] = Color.FromArgb(220, 60, 60);
        themeColors["PrimaryDark"] = Color.FromArgb(180, 40, 40);
        themeColors["Accent"] = Color.FromArgb(255, 200, 100);
    }

    private void ApplyBluePreset()
    {
        themeColors["Primary"] = Color.FromArgb(60, 100, 200);
        themeColors["PrimaryDark"] = Color.FromArgb(40, 80, 160);
        themeColors["Accent"] = Color.FromArgb(100, 200, 255);
    }

    private void ApplyNeonPreset()
    {
        themeColors["Background"] = Color.FromArgb(15, 15, 25);
        themeColors["Primary"] = Color.FromArgb(0, 255, 255);
        themeColors["Secondary"] = Color.FromArgb(255, 0, 255);
        themeColors["Accent"] = Color.FromArgb(0, 255, 150);
        themeColors["Text"] = Color.FromArgb(220, 255, 255);
    }

    private void GenerateFromPrimary(object? sender, EventArgs e)
    {
        var primary = themeColors["Primary"];
        ColorToHSL(primary, out int h, out int s, out int l);

        // Generate harmonious colors
        themeColors["PrimaryDark"] = HSLToColor(h, s, Math.Max(10, l - 20));
        themeColors["PrimaryLight"] = HSLToColor(h, s, Math.Min(90, l + 20));
        themeColors["Secondary"] = HSLToColor((h + 180) % 360, s, l);
        themeColors["Accent"] = HSLToColor((h + 120) % 360, s, l);

        lstColors.Invalidate();
        UpdatePreview();
    }

    private void RandomizeColors(object? sender, EventArgs e)
    {
        var rng = new Random();
        foreach (var key in themeColors.Keys.ToList())
        {
            if (key.Contains("Text") || key.Contains("Legal") || key.Contains("Illegal"))
                continue;

            themeColors[key] = Color.FromArgb(rng.Next(256), rng.Next(256), rng.Next(256));
        }

        lstColors.Invalidate();
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        pnlPreview.BackColor = themeColors["Background"];
        pnlPreview.Controls.Clear();

        // Mini window preview
        var header = new Panel
        {
            Location = new Point(10, 10),
            Size = new Size(350, 30),
            BackColor = themeColors["BackgroundSecondary"]
        };

        var lblHeader = new Label
        {
            Text = "PKM-Universe",
            Location = new Point(10, 5),
            AutoSize = true,
            ForeColor = themeColors["Text"],
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        header.Controls.Add(lblHeader);

        // Menu bar
        var menuBar = new Panel
        {
            Location = new Point(10, 45),
            Size = new Size(350, 25),
            BackColor = themeColors["MenuBackground"]
        };

        var menuItems = new[] { "File", "Edit", "Tools", "Options" };
        for (int i = 0; i < menuItems.Length; i++)
        {
            var lbl = new Label
            {
                Text = menuItems[i],
                Location = new Point(10 + i * 60, 4),
                AutoSize = true,
                ForeColor = themeColors["Text"]
            };
            menuBar.Controls.Add(lbl);
        }

        // Content area
        var content = new Panel
        {
            Location = new Point(10, 75),
            Size = new Size(350, 200),
            BackColor = themeColors["BackgroundTertiary"]
        };

        // Sample Pokemon box slots
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                var slot = new Panel
                {
                    Location = new Point(10 + col * 55, 10 + row * 55),
                    Size = new Size(50, 50),
                    BackColor = (row == 1 && col == 2) ? themeColors["SlotSelected"] : themeColors["SlotEmpty"],
                    BorderStyle = BorderStyle.FixedSingle
                };
                content.Controls.Add(slot);
            }
        }

        // Buttons
        var btnPrimary = new Button
        {
            Text = "Primary",
            Location = new Point(10, 175),
            Size = new Size(80, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = themeColors["ButtonPrimary"],
            ForeColor = themeColors["Text"]
        };

        var btnSuccess = new Button
        {
            Text = "Success",
            Location = new Point(100, 175),
            Size = new Size(80, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = themeColors["ButtonSuccess"],
            ForeColor = Color.White
        };

        var btnDanger = new Button
        {
            Text = "Danger",
            Location = new Point(190, 175),
            Size = new Size(80, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = themeColors["ButtonDanger"],
            ForeColor = Color.White
        };

        content.Controls.AddRange(new Control[] { btnPrimary, btnSuccess, btnDanger });

        // Status indicators
        var status = new Panel
        {
            Location = new Point(10, 280),
            Size = new Size(350, 60),
            BackColor = themeColors["Surface"]
        };

        var lblLegal = new Label { Text = "✓ Legal", Location = new Point(10, 10), AutoSize = true, ForeColor = themeColors["Legal"] };
        var lblIllegal = new Label { Text = "✗ Illegal", Location = new Point(80, 10), AutoSize = true, ForeColor = themeColors["Illegal"] };
        var lblShiny = new Label { Text = "★ Shiny", Location = new Point(150, 10), AutoSize = true, ForeColor = themeColors["Shiny"] };
        var lblInfo = new Label { Text = "ℹ Info", Location = new Point(220, 10), AutoSize = true, ForeColor = themeColors["Info"] };
        var lblWarn = new Label { Text = "⚠ Warning", Location = new Point(280, 10), AutoSize = true, ForeColor = themeColors["Warning"] };

        status.Controls.AddRange(new Control[] { lblLegal, lblIllegal, lblShiny, lblInfo, lblWarn });

        // Input preview
        var input = new TextBox
        {
            Location = new Point(10, 40),
            Size = new Size(200, 20),
            BackColor = themeColors["InputBackground"],
            ForeColor = themeColors["Text"],
            Text = "Sample Input"
        };
        status.Controls.Add(input);

        pnlPreview.Controls.AddRange(new Control[] { header, menuBar, content, status });
    }

    private void SaveTheme(object? sender, EventArgs e)
    {
        try
        {
            if (!Directory.Exists(ThemesFolder))
                Directory.CreateDirectory(ThemesFolder);

            var themeName = txtThemeName.Text.Trim();
            if (string.IsNullOrEmpty(themeName))
                themeName = "CustomTheme";

            var safeName = string.Join("_", themeName.Split(Path.GetInvalidFileNameChars()));
            var path = Path.Combine(ThemesFolder, $"{safeName}.json");

            var themeData = new Dictionary<string, string>();
            foreach (var kvp in themeColors)
                themeData[kvp.Key] = ColorToHex(kvp.Value);

            var json = JsonSerializer.Serialize(new { Name = themeName, Colors = themeData }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);

            WinFormsUtil.Alert("Theme saved!", $"Saved to: {path}");
        }
        catch (Exception ex)
        {
            WinFormsUtil.Error("Failed to save theme", ex.Message);
        }
    }

    private void LoadTheme(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Load Theme",
            Filter = "Theme Files|*.json",
            InitialDirectory = ThemesFolder
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var json = File.ReadAllText(ofd.FileName);
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("Name", out var nameEl))
                    txtThemeName.Text = nameEl.GetString() ?? "Loaded Theme";

                if (doc.RootElement.TryGetProperty("Colors", out var colorsEl))
                {
                    foreach (var prop in colorsEl.EnumerateObject())
                    {
                        if (themeColors.ContainsKey(prop.Name))
                        {
                            try
                            {
                                themeColors[prop.Name] = ColorTranslator.FromHtml(prop.Value.GetString()!);
                            }
                            catch { }
                        }
                    }
                }

                lstColors.Invalidate();
                UpdatePreview();
                WinFormsUtil.Alert("Theme loaded!");
            }
            catch (Exception ex)
            {
                WinFormsUtil.Error("Failed to load theme", ex.Message);
            }
        }
    }

    // Helper methods
    private static string ColorToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    private static void ColorToHSL(Color c, out int h, out int s, out int l)
    {
        float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float lum = (max + min) / 2;

        if (max == min)
        {
            h = s = 0;
        }
        else
        {
            float d = max - min;
            float sat = lum > 0.5f ? d / (2 - max - min) : d / (max + min);

            float hue;
            if (max == r)
                hue = ((g - b) / d + (g < b ? 6 : 0)) / 6;
            else if (max == g)
                hue = ((b - r) / d + 2) / 6;
            else
                hue = ((r - g) / d + 4) / 6;

            h = (int)(hue * 360);
            s = (int)(sat * 100);
        }
        l = (int)(lum * 100);
    }

    private static Color HSLToColor(int h, int s, int l)
    {
        float hue = h / 360f;
        float sat = s / 100f;
        float lum = l / 100f;

        float r, g, b;
        if (sat == 0)
        {
            r = g = b = lum;
        }
        else
        {
            float q = lum < 0.5f ? lum * (1 + sat) : lum + sat - lum * sat;
            float p = 2 * lum - q;
            r = HueToRGB(p, q, hue + 1f / 3);
            g = HueToRGB(p, q, hue);
            b = HueToRGB(p, q, hue - 1f / 3);
        }

        return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
    }

    private static float HueToRGB(float p, float q, float t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1f / 6) return p + (q - p) * 6 * t;
        if (t < 1f / 2) return q;
        if (t < 2f / 3) return p + (q - p) * (2f / 3 - t) * 6;
        return p;
    }
}
