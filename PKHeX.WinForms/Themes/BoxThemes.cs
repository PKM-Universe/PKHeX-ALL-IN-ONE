using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PKHeX.WinForms.Themes;

/// <summary>
/// Box Theme System - Custom visual themes for box backgrounds
/// </summary>
public static class BoxThemeManager
{
    private static BoxTheme _currentTheme = BoxThemes.Default;
    private static readonly Dictionary<string, BoxTheme> _themes = new();

    public static BoxTheme CurrentTheme => _currentTheme;

    static BoxThemeManager()
    {
        RegisterThemes();
    }

    private static void RegisterThemes()
    {
        _themes["Default"] = BoxThemes.Default;
        _themes["Gradient Blue"] = BoxThemes.GradientBlue;
        _themes["Gradient Purple"] = BoxThemes.GradientPurple;
        _themes["Pokemon Red"] = BoxThemes.PokemonRed;
        _themes["Pokemon Blue"] = BoxThemes.PokemonBlue;
        _themes["Shiny Gold"] = BoxThemes.ShinyGold;
        _themes["Dark Carbon"] = BoxThemes.DarkCarbon;
        _themes["Forest"] = BoxThemes.Forest;
        _themes["Ocean"] = BoxThemes.Ocean;
        _themes["Sunset"] = BoxThemes.Sunset;
        _themes["Midnight"] = BoxThemes.Midnight;
        _themes["PKM Universe"] = BoxThemes.PKMUniverse;
    }

    public static IEnumerable<string> GetThemeNames() => _themes.Keys;

    public static void SetTheme(string themeName)
    {
        if (_themes.TryGetValue(themeName, out var theme))
        {
            _currentTheme = theme;
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static event EventHandler? ThemeChanged;

    public static void PaintBoxBackground(Graphics g, Rectangle rect)
    {
        _currentTheme.PaintBackground(g, rect);
    }

    public static void PaintSlotBackground(Graphics g, Rectangle rect, bool isSelected, bool isHovered)
    {
        _currentTheme.PaintSlot(g, rect, isSelected, isHovered);
    }
}

/// <summary>
/// Box theme definition
/// </summary>
public class BoxTheme
{
    public string Name { get; set; } = "Default";
    public Color BackgroundColor1 { get; set; } = Color.FromArgb(30, 30, 35);
    public Color BackgroundColor2 { get; set; } = Color.FromArgb(40, 40, 50);
    public Color SlotColor { get; set; } = Color.FromArgb(50, 50, 60);
    public Color SlotBorder { get; set; } = Color.FromArgb(70, 70, 80);
    public Color SlotHover { get; set; } = Color.FromArgb(60, 60, 75);
    public Color SlotSelected { get; set; } = Color.FromArgb(80, 130, 200);
    public Color AccentColor { get; set; } = Color.FromArgb(100, 149, 237);
    public bool UseGradient { get; set; } = true;
    public LinearGradientMode GradientMode { get; set; } = LinearGradientMode.Vertical;
    public bool HasPattern { get; set; } = false;
    public PatternStyle Pattern { get; set; } = PatternStyle.None;

    public virtual void PaintBackground(Graphics g, Rectangle rect)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        if (UseGradient)
        {
            using var brush = new LinearGradientBrush(rect, BackgroundColor1, BackgroundColor2, GradientMode);
            g.FillRectangle(brush, rect);
        }
        else
        {
            using var brush = new SolidBrush(BackgroundColor1);
            g.FillRectangle(brush, rect);
        }

        if (HasPattern)
        {
            PaintPattern(g, rect);
        }

        // Accent border
        using var pen = new Pen(Color.FromArgb(60, AccentColor), 2);
        g.DrawRectangle(pen, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
    }

    protected virtual void PaintPattern(Graphics g, Rectangle rect)
    {
        switch (Pattern)
        {
            case PatternStyle.Dots:
                PaintDotPattern(g, rect);
                break;
            case PatternStyle.Lines:
                PaintLinePattern(g, rect);
                break;
            case PatternStyle.Grid:
                PaintGridPattern(g, rect);
                break;
            case PatternStyle.Pokeball:
                PaintPokeballPattern(g, rect);
                break;
            case PatternStyle.Hexagon:
                PaintHexagonPattern(g, rect);
                break;
        }
    }

    private void PaintDotPattern(Graphics g, Rectangle rect)
    {
        using var brush = new SolidBrush(Color.FromArgb(15, AccentColor));
        for (int x = rect.X; x < rect.Right; x += 20)
        {
            for (int y = rect.Y; y < rect.Bottom; y += 20)
            {
                g.FillEllipse(brush, x, y, 4, 4);
            }
        }
    }

    private void PaintLinePattern(Graphics g, Rectangle rect)
    {
        using var pen = new Pen(Color.FromArgb(12, AccentColor));
        for (int i = 0; i < rect.Width + rect.Height; i += 15)
        {
            g.DrawLine(pen, rect.X + i, rect.Y, rect.X, rect.Y + i);
        }
    }

    private void PaintGridPattern(Graphics g, Rectangle rect)
    {
        using var pen = new Pen(Color.FromArgb(10, AccentColor));
        for (int x = rect.X; x < rect.Right; x += 40)
        {
            g.DrawLine(pen, x, rect.Y, x, rect.Bottom);
        }
        for (int y = rect.Y; y < rect.Bottom; y += 40)
        {
            g.DrawLine(pen, rect.X, y, rect.Right, y);
        }
    }

    private void PaintPokeballPattern(Graphics g, Rectangle rect)
    {
        using var pen = new Pen(Color.FromArgb(8, AccentColor), 2);
        var size = 60;

        for (int x = rect.X - size; x < rect.Right + size; x += size * 2)
        {
            for (int y = rect.Y - size; y < rect.Bottom + size; y += size * 2)
            {
                // Circle
                g.DrawEllipse(pen, x, y, size, size);
                // Line
                g.DrawLine(pen, x, y + size / 2, x + size, y + size / 2);
                // Center
                g.DrawEllipse(pen, x + size / 2 - 8, y + size / 2 - 8, 16, 16);
            }
        }
    }

    private void PaintHexagonPattern(Graphics g, Rectangle rect)
    {
        using var pen = new Pen(Color.FromArgb(10, AccentColor));
        var size = 30;
        var rowOffset = false;

        for (int y = rect.Y - size; y < rect.Bottom + size; y += (int)(size * 1.5))
        {
            var offset = rowOffset ? size : 0;
            for (int x = rect.X - size + offset; x < rect.Right + size; x += size * 2)
            {
                DrawHexagon(g, pen, x, y, size);
            }
            rowOffset = !rowOffset;
        }
    }

    private static void DrawHexagon(Graphics g, Pen pen, int cx, int cy, int size)
    {
        var points = new PointF[6];
        for (int i = 0; i < 6; i++)
        {
            var angle = (60 * i - 30) * Math.PI / 180;
            points[i] = new PointF(
                cx + size * (float)Math.Cos(angle),
                cy + size * (float)Math.Sin(angle)
            );
        }
        g.DrawPolygon(pen, points);
    }

    public virtual void PaintSlot(Graphics g, Rectangle rect, bool isSelected, bool isHovered)
    {
        var bgColor = isSelected ? SlotSelected : (isHovered ? SlotHover : SlotColor);
        var borderColor = isSelected ? AccentColor : SlotBorder;

        using (var path = ThemeManager.CreateRoundedRectangle(rect, 6))
        {
            using var brush = new SolidBrush(bgColor);
            g.FillPath(brush, path);

            using var pen = new Pen(borderColor);
            g.DrawPath(pen, path);
        }
    }
}

/// <summary>
/// Predefined box themes
/// </summary>
public static class BoxThemes
{
    public static BoxTheme Default => new()
    {
        Name = "Default",
        BackgroundColor1 = Color.FromArgb(30, 30, 35),
        BackgroundColor2 = Color.FromArgb(40, 40, 50),
        SlotColor = Color.FromArgb(50, 50, 60),
        SlotBorder = Color.FromArgb(70, 70, 80),
        AccentColor = Color.FromArgb(100, 149, 237),
        HasPattern = true,
        Pattern = PatternStyle.Grid
    };

    public static BoxTheme GradientBlue => new()
    {
        Name = "Gradient Blue",
        BackgroundColor1 = Color.FromArgb(25, 55, 95),
        BackgroundColor2 = Color.FromArgb(15, 35, 65),
        SlotColor = Color.FromArgb(35, 65, 105),
        SlotBorder = Color.FromArgb(55, 95, 145),
        SlotSelected = Color.FromArgb(65, 135, 205),
        AccentColor = Color.FromArgb(100, 180, 255),
        GradientMode = LinearGradientMode.ForwardDiagonal,
        HasPattern = true,
        Pattern = PatternStyle.Dots
    };

    public static BoxTheme GradientPurple => new()
    {
        Name = "Gradient Purple",
        BackgroundColor1 = Color.FromArgb(55, 25, 85),
        BackgroundColor2 = Color.FromArgb(35, 15, 55),
        SlotColor = Color.FromArgb(65, 35, 95),
        SlotBorder = Color.FromArgb(95, 55, 135),
        SlotSelected = Color.FromArgb(125, 85, 175),
        AccentColor = Color.FromArgb(180, 120, 255),
        GradientMode = LinearGradientMode.BackwardDiagonal,
        HasPattern = true,
        Pattern = PatternStyle.Hexagon
    };

    public static BoxTheme PokemonRed => new()
    {
        Name = "Pokemon Red",
        BackgroundColor1 = Color.FromArgb(180, 50, 50),
        BackgroundColor2 = Color.FromArgb(120, 30, 30),
        SlotColor = Color.FromArgb(90, 40, 40),
        SlotBorder = Color.FromArgb(200, 80, 80),
        SlotSelected = Color.FromArgb(220, 100, 100),
        AccentColor = Color.FromArgb(255, 120, 120),
        HasPattern = true,
        Pattern = PatternStyle.Pokeball
    };

    public static BoxTheme PokemonBlue => new()
    {
        Name = "Pokemon Blue",
        BackgroundColor1 = Color.FromArgb(40, 80, 150),
        BackgroundColor2 = Color.FromArgb(25, 50, 100),
        SlotColor = Color.FromArgb(35, 70, 120),
        SlotBorder = Color.FromArgb(60, 110, 180),
        SlotSelected = Color.FromArgb(80, 140, 220),
        AccentColor = Color.FromArgb(100, 160, 255),
        HasPattern = true,
        Pattern = PatternStyle.Pokeball
    };

    public static BoxTheme ShinyGold => new()
    {
        Name = "Shiny Gold",
        BackgroundColor1 = Color.FromArgb(80, 60, 20),
        BackgroundColor2 = Color.FromArgb(50, 40, 15),
        SlotColor = Color.FromArgb(70, 55, 25),
        SlotBorder = Color.FromArgb(180, 140, 60),
        SlotSelected = Color.FromArgb(220, 180, 80),
        SlotHover = Color.FromArgb(90, 70, 30),
        AccentColor = Color.FromArgb(255, 215, 0),
        HasPattern = true,
        Pattern = PatternStyle.Lines
    };

    public static BoxTheme DarkCarbon => new()
    {
        Name = "Dark Carbon",
        BackgroundColor1 = Color.FromArgb(25, 25, 25),
        BackgroundColor2 = Color.FromArgb(15, 15, 15),
        SlotColor = Color.FromArgb(35, 35, 35),
        SlotBorder = Color.FromArgb(55, 55, 55),
        SlotSelected = Color.FromArgb(60, 60, 60),
        SlotHover = Color.FromArgb(45, 45, 45),
        AccentColor = Color.FromArgb(80, 80, 80),
        HasPattern = true,
        Pattern = PatternStyle.Hexagon
    };

    public static BoxTheme Forest => new()
    {
        Name = "Forest",
        BackgroundColor1 = Color.FromArgb(30, 70, 40),
        BackgroundColor2 = Color.FromArgb(20, 50, 30),
        SlotColor = Color.FromArgb(35, 65, 45),
        SlotBorder = Color.FromArgb(60, 120, 70),
        SlotSelected = Color.FromArgb(80, 160, 90),
        AccentColor = Color.FromArgb(120, 200, 100),
        HasPattern = true,
        Pattern = PatternStyle.Dots
    };

    public static BoxTheme Ocean => new()
    {
        Name = "Ocean",
        BackgroundColor1 = Color.FromArgb(20, 60, 80),
        BackgroundColor2 = Color.FromArgb(15, 40, 60),
        SlotColor = Color.FromArgb(25, 55, 75),
        SlotBorder = Color.FromArgb(50, 100, 130),
        SlotSelected = Color.FromArgb(70, 140, 180),
        AccentColor = Color.FromArgb(100, 200, 220),
        GradientMode = LinearGradientMode.Vertical,
        HasPattern = true,
        Pattern = PatternStyle.Lines
    };

    public static BoxTheme Sunset => new()
    {
        Name = "Sunset",
        BackgroundColor1 = Color.FromArgb(140, 60, 40),
        BackgroundColor2 = Color.FromArgb(80, 30, 50),
        SlotColor = Color.FromArgb(100, 50, 45),
        SlotBorder = Color.FromArgb(180, 100, 80),
        SlotSelected = Color.FromArgb(220, 140, 100),
        AccentColor = Color.FromArgb(255, 180, 100),
        GradientMode = LinearGradientMode.Vertical,
        HasPattern = true,
        Pattern = PatternStyle.Grid
    };

    public static BoxTheme Midnight => new()
    {
        Name = "Midnight",
        BackgroundColor1 = Color.FromArgb(20, 20, 40),
        BackgroundColor2 = Color.FromArgb(10, 10, 25),
        SlotColor = Color.FromArgb(30, 30, 50),
        SlotBorder = Color.FromArgb(50, 50, 80),
        SlotSelected = Color.FromArgb(70, 70, 120),
        AccentColor = Color.FromArgb(100, 100, 180),
        HasPattern = true,
        Pattern = PatternStyle.Dots
    };

    public static BoxTheme PKMUniverse => new()
    {
        Name = "PKM Universe",
        BackgroundColor1 = Color.FromArgb(45, 25, 70),
        BackgroundColor2 = Color.FromArgb(25, 15, 45),
        SlotColor = Color.FromArgb(55, 35, 80),
        SlotBorder = Color.FromArgb(100, 60, 140),
        SlotSelected = Color.FromArgb(140, 80, 200),
        SlotHover = Color.FromArgb(75, 45, 100),
        AccentColor = Color.FromArgb(180, 100, 255),
        GradientMode = LinearGradientMode.ForwardDiagonal,
        HasPattern = true,
        Pattern = PatternStyle.Hexagon
    };
}

/// <summary>
/// Pattern styles for box backgrounds
/// </summary>
public enum PatternStyle
{
    None,
    Dots,
    Lines,
    Grid,
    Pokeball,
    Hexagon
}

/// <summary>
/// Box theme selector control
/// </summary>
public class BoxThemeSelector : ComboBox
{
    public BoxThemeSelector()
    {
        DropDownStyle = ComboBoxStyle.DropDownList;
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = 30;

        foreach (var themeName in BoxThemeManager.GetThemeNames())
        {
            Items.Add(themeName);
        }

        if (Items.Count > 0)
            SelectedIndex = 0;

        SelectedIndexChanged += (s, e) =>
        {
            if (SelectedItem is string themeName)
            {
                BoxThemeManager.SetTheme(themeName);
            }
        };
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        e.DrawBackground();

        var colors = ThemeManager.Colors;
        var themeName = Items[e.Index].ToString();

        using var font = new Font("Segoe UI", 10);
        using var brush = new SolidBrush(colors.Text);

        // Draw preview swatch
        var swatchRect = new Rectangle(e.Bounds.X + 5, e.Bounds.Y + 5, 20, e.Bounds.Height - 10);
        var previewColor = themeName switch
        {
            "Default" => Color.FromArgb(100, 149, 237),
            "Gradient Blue" => Color.FromArgb(100, 180, 255),
            "Gradient Purple" => Color.FromArgb(180, 120, 255),
            "Pokemon Red" => Color.FromArgb(220, 60, 60),
            "Pokemon Blue" => Color.FromArgb(60, 120, 200),
            "Shiny Gold" => Color.FromArgb(255, 215, 0),
            "Dark Carbon" => Color.FromArgb(60, 60, 60),
            "Forest" => Color.FromArgb(80, 160, 90),
            "Ocean" => Color.FromArgb(100, 200, 220),
            "Sunset" => Color.FromArgb(255, 180, 100),
            "Midnight" => Color.FromArgb(100, 100, 180),
            "PKM Universe" => Color.FromArgb(180, 100, 255),
            _ => colors.Accent
        };

        using var swatchBrush = new SolidBrush(previewColor);
        e.Graphics.FillRectangle(swatchBrush, swatchRect);

        // Draw text
        e.Graphics.DrawString(themeName, font, brush, e.Bounds.X + 30, e.Bounds.Y + 5);

        e.DrawFocusRectangle();
    }
}
