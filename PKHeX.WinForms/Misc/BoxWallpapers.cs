using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms;

/// <summary>
/// Custom Box Wallpapers System - Themed box backgrounds
/// </summary>
public partial class BoxWallpaperManager : Form
{
    private readonly SaveFile SAV;
    private readonly Action OnWallpaperChanged;
    private readonly Panel PNL_Preview;
    private readonly ListBox LB_Wallpapers;
    private readonly ComboBox CB_Box;

    public static Dictionary<int, BoxWallpaperTheme> CustomWallpapers { get; } = new();

    public BoxWallpaperManager(SaveFile sav, Action onChanged)
    {
        SAV = sav;
        OnWallpaperChanged = onChanged;

        Text = "Box Wallpapers";
        Size = new Size(700, 500);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var lblBox = new Label { Text = "Apply to Box:", Location = new Point(10, 15), AutoSize = true };

        CB_Box = new ComboBox
        {
            Location = new Point(100, 12),
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        CB_Box.Items.Add("All Boxes");
        for (int i = 0; i < SAV.BoxCount; i++)
            CB_Box.Items.Add($"Box {i + 1}");
        CB_Box.SelectedIndex = 0;

        var lblWallpaper = new Label { Text = "Select Theme:", Location = new Point(10, 50), AutoSize = true };

        LB_Wallpapers = new ListBox
        {
            Location = new Point(10, 75),
            Size = new Size(200, 350),
            Font = new Font("Segoe UI", 10F)
        };
        LB_Wallpapers.SelectedIndexChanged += (s, e) => UpdatePreview();

        // Add wallpaper themes
        foreach (var theme in GetAvailableThemes())
            LB_Wallpapers.Items.Add(theme.Name);

        if (LB_Wallpapers.Items.Count > 0)
            LB_Wallpapers.SelectedIndex = 0;

        PNL_Preview = new Panel
        {
            Location = new Point(230, 50),
            Size = new Size(440, 280),
            BorderStyle = BorderStyle.FixedSingle
        };
        PNL_Preview.Paint += PNL_Preview_Paint;

        var btnApply = new Button
        {
            Text = "Apply Wallpaper",
            Location = new Point(230, 350),
            Size = new Size(150, 40),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        btnApply.Click += (s, e) => ApplyWallpaper();

        var btnReset = new Button
        {
            Text = "Reset to Default",
            Location = new Point(400, 350),
            Size = new Size(130, 40),
            FlatStyle = FlatStyle.Flat
        };
        btnReset.Click += (s, e) => ResetWallpaper();

        Controls.AddRange(new Control[] { lblBox, CB_Box, lblWallpaper, LB_Wallpapers, PNL_Preview, btnApply, btnReset });

        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var colors = ThemeManager.Colors;
        BackColor = colors.Background;
        ForeColor = colors.Text;
        LB_Wallpapers.BackColor = colors.InputBackground;
        LB_Wallpapers.ForeColor = colors.Text;
        CB_Box.BackColor = colors.InputBackground;
        CB_Box.ForeColor = colors.Text;
    }

    private void UpdatePreview()
    {
        PNL_Preview.Invalidate();
    }

    private void PNL_Preview_Paint(object? sender, PaintEventArgs e)
    {
        if (LB_Wallpapers.SelectedIndex < 0) return;

        var themes = GetAvailableThemes();
        if (LB_Wallpapers.SelectedIndex >= themes.Length) return;

        var theme = themes[LB_Wallpapers.SelectedIndex];
        DrawWallpaper(e.Graphics, PNL_Preview.ClientRectangle, theme);

        // Draw sample box grid
        DrawSampleBoxGrid(e.Graphics, PNL_Preview.ClientRectangle);
    }

    private static void DrawWallpaper(Graphics g, Rectangle rect, BoxWallpaperTheme theme)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Background gradient
        using (var brush = new LinearGradientBrush(rect, theme.Color1, theme.Color2, theme.GradientAngle))
        {
            g.FillRectangle(brush, rect);
        }

        // Pattern overlay
        switch (theme.Pattern)
        {
            case WallpaperPattern.Pokeballs:
                DrawPokeballPattern(g, rect, theme.PatternColor);
                break;
            case WallpaperPattern.Stars:
                DrawStarPattern(g, rect, theme.PatternColor);
                break;
            case WallpaperPattern.Dots:
                DrawDotPattern(g, rect, theme.PatternColor);
                break;
            case WallpaperPattern.Waves:
                DrawWavePattern(g, rect, theme.PatternColor);
                break;
            case WallpaperPattern.Hexagons:
                DrawHexagonPattern(g, rect, theme.PatternColor);
                break;
            case WallpaperPattern.Diamonds:
                DrawDiamondPattern(g, rect, theme.PatternColor);
                break;
        }
    }

    private static void DrawSampleBoxGrid(Graphics g, Rectangle rect)
    {
        // Draw sample slot grid (6x5)
        int cols = 6, rows = 5;
        int slotW = (rect.Width - 40) / cols;
        int slotH = (rect.Height - 40) / rows;
        int startX = 20, startY = 20;

        using var pen = new Pen(Color.FromArgb(100, 255, 255, 255), 1);
        using var brush = new SolidBrush(Color.FromArgb(30, 255, 255, 255));

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var slotRect = new Rectangle(startX + col * slotW, startY + row * slotH, slotW - 4, slotH - 4);
                g.FillRectangle(brush, slotRect);
                g.DrawRectangle(pen, slotRect);
            }
        }
    }

    private static void DrawPokeballPattern(Graphics g, Rectangle rect, Color color)
    {
        using var pen = new Pen(Color.FromArgb(40, color), 2);
        for (int x = -20; x < rect.Width + 40; x += 60)
        {
            for (int y = -20; y < rect.Height + 40; y += 60)
            {
                g.DrawEllipse(pen, x, y, 40, 40);
                g.DrawLine(pen, x, y + 20, x + 40, y + 20);
            }
        }
    }

    private static void DrawStarPattern(Graphics g, Rectangle rect, Color color)
    {
        using var brush = new SolidBrush(Color.FromArgb(30, color));
        var random = new Random(42);
        for (int i = 0; i < 50; i++)
        {
            int x = random.Next(rect.Width);
            int y = random.Next(rect.Height);
            int size = random.Next(3, 8);
            DrawStar(g, brush, x, y, size);
        }
    }

    private static void DrawStar(Graphics g, Brush brush, int x, int y, int size)
    {
        var points = new PointF[10];
        for (int i = 0; i < 10; i++)
        {
            double angle = Math.PI / 2 + i * Math.PI / 5;
            float r = i % 2 == 0 ? size : size / 2f;
            points[i] = new PointF(x + (float)(r * Math.Cos(angle)), y - (float)(r * Math.Sin(angle)));
        }
        g.FillPolygon(brush, points);
    }

    private static void DrawDotPattern(Graphics g, Rectangle rect, Color color)
    {
        using var brush = new SolidBrush(Color.FromArgb(25, color));
        for (int x = 0; x < rect.Width; x += 20)
        {
            for (int y = 0; y < rect.Height; y += 20)
            {
                g.FillEllipse(brush, x, y, 6, 6);
            }
        }
    }

    private static void DrawWavePattern(Graphics g, Rectangle rect, Color color)
    {
        using var pen = new Pen(Color.FromArgb(30, color), 2);
        for (int y = 0; y < rect.Height + 50; y += 30)
        {
            var points = new List<PointF>();
            for (int x = 0; x <= rect.Width; x += 5)
            {
                float wave = (float)(10 * Math.Sin(x * 0.05));
                points.Add(new PointF(x, y + wave));
            }
            if (points.Count > 1)
                g.DrawCurve(pen, points.ToArray());
        }
    }

    private static void DrawHexagonPattern(Graphics g, Rectangle rect, Color color)
    {
        using var pen = new Pen(Color.FromArgb(25, color), 1);
        int size = 25;
        float h = size * 2;
        float w = (float)(Math.Sqrt(3) * size);

        for (int row = -1; row < rect.Height / h + 2; row++)
        {
            for (int col = -1; col < rect.Width / w + 2; col++)
            {
                float x = col * w + (row % 2 == 0 ? 0 : w / 2);
                float y = row * h * 0.75f;
                DrawHexagon(g, pen, x, y, size);
            }
        }
    }

    private static void DrawHexagon(Graphics g, Pen pen, float cx, float cy, int size)
    {
        var points = new PointF[6];
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 6 + i * Math.PI / 3;
            points[i] = new PointF(cx + size * (float)Math.Cos(angle), cy + size * (float)Math.Sin(angle));
        }
        g.DrawPolygon(pen, points);
    }

    private static void DrawDiamondPattern(Graphics g, Rectangle rect, Color color)
    {
        using var pen = new Pen(Color.FromArgb(30, color), 1);
        int size = 20;
        for (int x = -size; x < rect.Width + size; x += size * 2)
        {
            for (int y = -size; y < rect.Height + size; y += size * 2)
            {
                var points = new PointF[]
                {
                    new(x, y - size),
                    new(x + size, y),
                    new(x, y + size),
                    new(x - size, y)
                };
                g.DrawPolygon(pen, points);
            }
        }
    }

    private void ApplyWallpaper()
    {
        if (LB_Wallpapers.SelectedIndex < 0) return;

        var themes = GetAvailableThemes();
        var theme = themes[LB_Wallpapers.SelectedIndex];

        if (CB_Box.SelectedIndex == 0)
        {
            // Apply to all boxes
            for (int i = 0; i < SAV.BoxCount; i++)
                CustomWallpapers[i] = theme;
        }
        else
        {
            // Apply to selected box
            CustomWallpapers[CB_Box.SelectedIndex - 1] = theme;
        }

        OnWallpaperChanged?.Invoke();
        MessageBox.Show("Wallpaper applied!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ResetWallpaper()
    {
        if (CB_Box.SelectedIndex == 0)
            CustomWallpapers.Clear();
        else
            CustomWallpapers.Remove(CB_Box.SelectedIndex - 1);

        OnWallpaperChanged?.Invoke();
        MessageBox.Show("Wallpaper reset to default!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void DrawBoxBackground(Graphics g, Rectangle rect, int boxIndex)
    {
        if (CustomWallpapers.TryGetValue(boxIndex, out var theme))
        {
            DrawWallpaper(g, rect, theme);
        }
    }

    private static BoxWallpaperTheme[] GetAvailableThemes()
    {
        return new[]
        {
            new BoxWallpaperTheme("Classic Purple", Color.FromArgb(75, 0, 130), Color.FromArgb(138, 43, 226), 45, WallpaperPattern.None, Color.White),
            new BoxWallpaperTheme("Pokeball Red", Color.FromArgb(180, 40, 40), Color.FromArgb(220, 80, 80), 135, WallpaperPattern.Pokeballs, Color.White),
            new BoxWallpaperTheme("Ocean Blue", Color.FromArgb(0, 50, 100), Color.FromArgb(0, 150, 200), 90, WallpaperPattern.Waves, Color.LightCyan),
            new BoxWallpaperTheme("Forest Green", Color.FromArgb(20, 60, 20), Color.FromArgb(50, 150, 50), 135, WallpaperPattern.None, Color.LightGreen),
            new BoxWallpaperTheme("Starry Night", Color.FromArgb(10, 10, 30), Color.FromArgb(30, 30, 80), 90, WallpaperPattern.Stars, Color.Yellow),
            new BoxWallpaperTheme("Sunset Orange", Color.FromArgb(200, 80, 20), Color.FromArgb(255, 150, 50), 45, WallpaperPattern.None, Color.White),
            new BoxWallpaperTheme("Neon Cyber", Color.FromArgb(10, 10, 20), Color.FromArgb(30, 20, 50), 135, WallpaperPattern.Hexagons, Color.Cyan),
            new BoxWallpaperTheme("Pink Dream", Color.FromArgb(200, 100, 150), Color.FromArgb(255, 180, 200), 45, WallpaperPattern.Dots, Color.White),
            new BoxWallpaperTheme("Golden Luxury", Color.FromArgb(100, 70, 20), Color.FromArgb(200, 150, 50), 90, WallpaperPattern.Diamonds, Color.Gold),
            new BoxWallpaperTheme("Ice Crystal", Color.FromArgb(150, 200, 255), Color.FromArgb(200, 230, 255), 135, WallpaperPattern.Hexagons, Color.White),
            new BoxWallpaperTheme("Shiny Hunter", Color.FromArgb(50, 50, 50), Color.FromArgb(80, 80, 80), 90, WallpaperPattern.Stars, Color.Gold),
            new BoxWallpaperTheme("Electric Yellow", Color.FromArgb(200, 180, 0), Color.FromArgb(255, 220, 50), 45, WallpaperPattern.None, Color.White),
            new BoxWallpaperTheme("Ghost Purple", Color.FromArgb(60, 30, 80), Color.FromArgb(120, 60, 150), 135, WallpaperPattern.Dots, Color.Purple),
            new BoxWallpaperTheme("Dragon Fire", Color.FromArgb(150, 30, 0), Color.FromArgb(255, 100, 0), 45, WallpaperPattern.Waves, Color.Orange),
            new BoxWallpaperTheme("Fairy Pink", Color.FromArgb(255, 150, 200), Color.FromArgb(255, 200, 230), 90, WallpaperPattern.Stars, Color.White)
        };
    }
}

public class BoxWallpaperTheme
{
    public string Name { get; }
    public Color Color1 { get; }
    public Color Color2 { get; }
    public float GradientAngle { get; }
    public WallpaperPattern Pattern { get; }
    public Color PatternColor { get; }

    public BoxWallpaperTheme(string name, Color c1, Color c2, float angle, WallpaperPattern pattern, Color patternColor)
    {
        Name = name;
        Color1 = c1;
        Color2 = c2;
        GradientAngle = angle;
        Pattern = pattern;
        PatternColor = patternColor;
    }
}

public enum WallpaperPattern
{
    None,
    Pokeballs,
    Stars,
    Dots,
    Waves,
    Hexagons,
    Diamonds
}
