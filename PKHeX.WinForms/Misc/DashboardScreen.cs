using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms;

/// <summary>
/// Dashboard Home Screen - Beautiful welcome screen with save file statistics
/// </summary>
public class DashboardScreen : Panel
{
    private SaveFile? _sav;
    private DashboardStats _stats = new();
    private readonly Timer _animationTimer;
    private float _animationProgress;

    public event EventHandler? OpenFileClicked;
    public event EventHandler? ViewBoxesClicked;
    public event EventHandler? LegalityCheckClicked;
    public event EventHandler? BatchEditorClicked;

    public DashboardScreen()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
        Dock = DockStyle.Fill;

        _animationTimer = new Timer { Interval = 16 };
        _animationTimer.Tick += (s, e) =>
        {
            _animationProgress += 0.02f;
            if (_animationProgress >= 1)
            {
                _animationProgress = 1;
                _animationTimer.Stop();
            }
            Invalidate();
        };
    }

    public void SetSaveFile(SaveFile? sav)
    {
        _sav = sav;
        _stats = DashboardStats.Calculate(sav);
        _animationProgress = 0;
        _animationTimer.Start();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var colors = ThemeManager.Colors;

        // Background gradient
        using (var bgBrush = new LinearGradientBrush(
            ClientRectangle,
            colors.Background,
            colors.BackgroundSecondary,
            LinearGradientMode.ForwardDiagonal))
        {
            g.FillRectangle(bgBrush, ClientRectangle);
        }

        // Decorative circles in background
        DrawDecorativeElements(g, colors);

        if (_sav == null)
        {
            DrawWelcomeScreen(g, colors);
        }
        else
        {
            DrawDashboard(g, colors);
        }
    }

    private void DrawDecorativeElements(Graphics g, ThemeColors colors)
    {
        using var brush = new SolidBrush(Color.FromArgb(10, colors.Accent));

        // Large circles
        g.FillEllipse(brush, Width - 300, -100, 400, 400);
        g.FillEllipse(brush, -150, Height - 200, 350, 350);

        // Gradient accent line
        var lineRect = new Rectangle(0, 0, Width, 4);
        using var lineBrush = new LinearGradientBrush(lineRect,
            colors.GradientStart, colors.GradientEnd, LinearGradientMode.Horizontal);
        g.FillRectangle(lineBrush, lineRect);
    }

    private void DrawWelcomeScreen(Graphics g, ThemeColors colors)
    {
        var centerY = Height / 2 - 80;

        // Logo/Title
        using var titleFont = new Font("Segoe UI", 36, FontStyle.Bold);
        using var titleBrush = new LinearGradientBrush(
            new Rectangle(0, centerY - 50, Width, 60),
            colors.GradientStart, colors.GradientEnd, LinearGradientMode.Horizontal);
        var title = "PKM Universe";
        var titleSize = g.MeasureString(title, titleFont);
        g.DrawString(title, titleFont, titleBrush, (Width - titleSize.Width) / 2, centerY);

        // Subtitle
        using var subFont = new Font("Segoe UI", 14);
        using var subBrush = new SolidBrush(colors.TextSecondary);
        var subtitle = "The Ultimate Pokemon Save Editor";
        var subSize = g.MeasureString(subtitle, subFont);
        g.DrawString(subtitle, subFont, subBrush, (Width - subSize.Width) / 2, centerY + 55);

        // Action buttons
        var buttonY = centerY + 120;
        var buttonWidth = 200;
        var buttonHeight = 50;
        var buttonSpacing = 20;
        var startX = (Width - (buttonWidth * 2 + buttonSpacing)) / 2;

        // Open File button
        DrawActionButton(g, new Rectangle(startX, buttonY, buttonWidth, buttonHeight),
            "Open Save File", colors.Primary, colors);

        // Recent Files placeholder
        DrawActionButton(g, new Rectangle(startX + buttonWidth + buttonSpacing, buttonY, buttonWidth, buttonHeight),
            "Recent Files", colors.BackgroundTertiary, colors, true);

        // Features list
        var featuresY = buttonY + 100;
        using var featureFont = new Font("Segoe UI", 11);
        var features = new[] { "Auto Legality", "Batch Editing", "Living Dex Generator", "Theme Support" };
        var featureX = (Width - 400) / 2;

        foreach (var (feature, i) in features.Select((f, i) => (f, i)))
        {
            var x = featureX + (i % 2) * 200;
            var y = featuresY + (i / 2) * 30;
            DrawFeatureItem(g, x, y, feature, colors);
        }
    }

    private void DrawDashboard(Graphics g, ThemeColors colors)
    {
        var padding = 30;
        var y = padding + 10;

        // Header with game info
        DrawGameHeader(g, padding, y, colors);
        y += 100;

        // Stats cards row
        var cardWidth = (Width - padding * 2 - 40) / 4;
        var cardHeight = 100;
        var cardY = y;

        DrawStatCard(g, new Rectangle(padding, cardY, cardWidth, cardHeight),
            "Total Pokemon", _stats.TotalPokemon.ToString(), colors.Primary, colors);
        DrawStatCard(g, new Rectangle(padding + cardWidth + 10, cardY, cardWidth, cardHeight),
            "Shiny Pokemon", _stats.ShinyCount.ToString(), colors.ShinyGold, colors);
        DrawStatCard(g, new Rectangle(padding + (cardWidth + 10) * 2, cardY, cardWidth, cardHeight),
            "Legal", $"{_stats.LegalCount}", colors.LegalGreen, colors);
        DrawStatCard(g, new Rectangle(padding + (cardWidth + 10) * 3, cardY, cardWidth, cardHeight),
            "Illegal", $"{_stats.IllegalCount}", colors.IllegalRed, colors);

        y = cardY + cardHeight + 30;

        // Legality progress bar
        DrawLegalityProgress(g, new Rectangle(padding, y, Width - padding * 2, 60), colors);
        y += 80;

        // Type distribution
        DrawTypeDistribution(g, new Rectangle(padding, y, (Width - padding * 2 - 20) / 2, 200), colors);

        // Quick actions
        DrawQuickActions(g, new Rectangle(padding + (Width - padding * 2) / 2 + 10, y, (Width - padding * 2 - 20) / 2, 200), colors);
    }

    private void DrawGameHeader(Graphics g, int x, int y, ThemeColors colors)
    {
        // Background card
        var rect = new Rectangle(x, y, Width - x * 2, 90);
        using (var path = ThemeManager.CreateRoundedRectangle(rect, 12))
        {
            using var brush = new LinearGradientBrush(rect, colors.GradientStart, colors.GradientEnd, LinearGradientMode.Horizontal);
            g.FillPath(brush, path);
        }

        // Game name
        using var gameFont = new Font("Segoe UI", 22, FontStyle.Bold);
        using var whiteBrush = new SolidBrush(Color.White);
        var gameName = _sav?.Version.ToString() ?? "Unknown Game";
        g.DrawString(gameName, gameFont, whiteBrush, x + 20, y + 15);

        // Trainer info
        using var trainerFont = new Font("Segoe UI", 12);
        var trainerText = $"Trainer: {_sav?.OT}  |  TID: {_sav?.TID16}  |  Playtime: {_sav?.PlayTimeString ?? "N/A"}";
        g.DrawString(trainerText, trainerFont, whiteBrush, x + 20, y + 55);
    }

    private void DrawStatCard(Graphics g, Rectangle rect, string label, string value, Color accentColor, ThemeColors colors)
    {
        // Animate entry
        var animatedRect = new Rectangle(rect.X, rect.Y + (int)((1 - _animationProgress) * 20), rect.Width, rect.Height);
        var alpha = (int)(255 * _animationProgress);

        using (var path = ThemeManager.CreateRoundedRectangle(animatedRect, 10))
        {
            using var bgBrush = new SolidBrush(Color.FromArgb(alpha, colors.Surface));
            g.FillPath(bgBrush, path);

            using var borderPen = new Pen(Color.FromArgb(alpha, colors.Border));
            g.DrawPath(borderPen, path);
        }

        // Accent line
        var accentRect = new Rectangle(animatedRect.X, animatedRect.Y, 4, animatedRect.Height);
        using (var path = ThemeManager.CreateRoundedRectangle(accentRect, 2))
        {
            using var accentBrush = new SolidBrush(Color.FromArgb(alpha, accentColor));
            g.FillPath(accentBrush, path);
        }

        // Value
        using var valueFont = new Font("Segoe UI", 28, FontStyle.Bold);
        using var valueBrush = new SolidBrush(Color.FromArgb(alpha, colors.Text));
        g.DrawString(value, valueFont, valueBrush, animatedRect.X + 20, animatedRect.Y + 15);

        // Label
        using var labelFont = new Font("Segoe UI", 10);
        using var labelBrush = new SolidBrush(Color.FromArgb(alpha, colors.TextSecondary));
        g.DrawString(label, labelFont, labelBrush, animatedRect.X + 20, animatedRect.Y + 60);
    }

    private void DrawLegalityProgress(Graphics g, Rectangle rect, ThemeColors colors)
    {
        using (var path = ThemeManager.CreateRoundedRectangle(rect, 8))
        {
            using var bgBrush = new SolidBrush(colors.Surface);
            g.FillPath(bgBrush, path);
        }

        // Title
        using var titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
        using var titleBrush = new SolidBrush(colors.Text);
        g.DrawString("Legality Overview", titleFont, titleBrush, rect.X + 15, rect.Y + 8);

        // Progress bar background
        var barRect = new Rectangle(rect.X + 15, rect.Y + 35, rect.Width - 30, 16);
        using (var barPath = ThemeManager.CreateRoundedRectangle(barRect, 8))
        {
            using var barBg = new SolidBrush(colors.BackgroundTertiary);
            g.FillPath(barBg, barPath);
        }

        // Progress fill
        if (_stats.TotalPokemon > 0)
        {
            var legalPercent = (float)_stats.LegalCount / _stats.TotalPokemon;
            var fillWidth = (int)(barRect.Width * legalPercent * _animationProgress);
            if (fillWidth > 0)
            {
                var fillRect = new Rectangle(barRect.X, barRect.Y, fillWidth, barRect.Height);
                using var fillPath = ThemeManager.CreateRoundedRectangle(fillRect, 8);
                using var fillBrush = new SolidBrush(colors.LegalGreen);
                g.FillPath(fillBrush, fillPath);
            }

            // Percentage text
            using var percentFont = new Font("Segoe UI", 9, FontStyle.Bold);
            var percentText = $"{(int)(legalPercent * 100)}% Legal";
            g.DrawString(percentText, percentFont, titleBrush, rect.Right - 80, rect.Y + 8);
        }
    }

    private void DrawTypeDistribution(Graphics g, Rectangle rect, ThemeColors colors)
    {
        using (var path = ThemeManager.CreateRoundedRectangle(rect, 10))
        {
            using var bgBrush = new SolidBrush(colors.Surface);
            g.FillPath(bgBrush, path);
            using var borderPen = new Pen(colors.Border);
            g.DrawPath(borderPen, path);
        }

        using var titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
        using var titleBrush = new SolidBrush(colors.Text);
        g.DrawString("Type Distribution", titleFont, titleBrush, rect.X + 15, rect.Y + 12);

        // Draw type bars
        var barY = rect.Y + 40;
        var barHeight = 18;
        var maxWidth = rect.Width - 100;

        var topTypes = _stats.TypeCounts.OrderByDescending(t => t.Value).Take(5).ToList();
        var maxCount = topTypes.FirstOrDefault().Value;
        if (maxCount == 0) maxCount = 1;

        foreach (var (type, count) in topTypes)
        {
            var barWidth = (int)((float)count / maxCount * maxWidth * _animationProgress);
            var typeColor = GetTypeColor(type);

            // Type name
            using var typeFont = new Font("Segoe UI", 9);
            g.DrawString(type, typeFont, titleBrush, rect.X + 15, barY + 2);

            // Bar
            var barRect = new Rectangle(rect.X + 70, barY, barWidth, barHeight - 4);
            using (var barPath = ThemeManager.CreateRoundedRectangle(barRect, 4))
            {
                using var barBrush = new SolidBrush(typeColor);
                g.FillPath(barBrush, barPath);
            }

            // Count
            using var countBrush = new SolidBrush(colors.TextSecondary);
            g.DrawString(count.ToString(), typeFont, countBrush, rect.X + 75 + barWidth, barY + 2);

            barY += barHeight + 6;
        }
    }

    private void DrawQuickActions(Graphics g, Rectangle rect, ThemeColors colors)
    {
        using (var path = ThemeManager.CreateRoundedRectangle(rect, 10))
        {
            using var bgBrush = new SolidBrush(colors.Surface);
            g.FillPath(bgBrush, path);
            using var borderPen = new Pen(colors.Border);
            g.DrawPath(borderPen, path);
        }

        using var titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
        using var titleBrush = new SolidBrush(colors.Text);
        g.DrawString("Quick Actions", titleFont, titleBrush, rect.X + 15, rect.Y + 12);

        // Action buttons
        var buttonY = rect.Y + 45;
        var buttonHeight = 35;
        var buttonSpacing = 8;

        var actions = new[] {
            ("View Boxes", colors.Primary),
            ("Check Legality", colors.LegalGreen),
            ("Batch Editor", colors.Accent),
            ("Generate Living Dex", colors.ShinyGold)
        };

        foreach (var (action, color) in actions)
        {
            var btnRect = new Rectangle(rect.X + 15, buttonY, rect.Width - 30, buttonHeight);
            DrawQuickActionButton(g, btnRect, action, color, colors);
            buttonY += buttonHeight + buttonSpacing;
        }
    }

    private void DrawQuickActionButton(Graphics g, Rectangle rect, string text, Color accentColor, ThemeColors colors)
    {
        using (var path = ThemeManager.CreateRoundedRectangle(rect, 6))
        {
            using var bgBrush = new SolidBrush(Color.FromArgb(30, accentColor));
            g.FillPath(bgBrush, path);
        }

        using var font = new Font("Segoe UI", 10, FontStyle.Bold);
        using var brush = new SolidBrush(accentColor);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(text, font, brush, rect, sf);
    }

    private void DrawActionButton(Graphics g, Rectangle rect, string text, Color bgColor, ThemeColors colors, bool outline = false)
    {
        using (var path = ThemeManager.CreateRoundedRectangle(rect, 8))
        {
            if (outline)
            {
                using var borderPen = new Pen(colors.Border, 2);
                g.DrawPath(borderPen, path);
            }
            else
            {
                using var bgBrush = new SolidBrush(bgColor);
                g.FillPath(bgBrush, path);
            }
        }

        using var font = new Font("Segoe UI", 12, FontStyle.Bold);
        using var brush = new SolidBrush(outline ? colors.Text : Color.White);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(text, font, brush, rect, sf);
    }

    private static void DrawFeatureItem(Graphics g, int x, int y, string text, ThemeColors colors)
    {
        using var checkBrush = new SolidBrush(colors.LegalGreen);
        using var font = new Font("Segoe UI", 10);
        g.DrawString("✓", font, checkBrush, x, y);
        using var textBrush = new SolidBrush(colors.Text);
        g.DrawString(text, font, textBrush, x + 20, y);
    }

    private static Color GetTypeColor(string typeName)
    {
        return typeName switch
        {
            "Normal" => Color.FromArgb(168, 167, 122),
            "Fire" => Color.FromArgb(238, 129, 48),
            "Water" => Color.FromArgb(99, 144, 240),
            "Electric" => Color.FromArgb(247, 208, 44),
            "Grass" => Color.FromArgb(122, 199, 76),
            "Ice" => Color.FromArgb(150, 217, 214),
            "Fighting" => Color.FromArgb(194, 46, 40),
            "Poison" => Color.FromArgb(163, 62, 161),
            "Ground" => Color.FromArgb(226, 191, 101),
            "Flying" => Color.FromArgb(169, 143, 243),
            "Psychic" => Color.FromArgb(249, 85, 135),
            "Bug" => Color.FromArgb(166, 185, 26),
            "Rock" => Color.FromArgb(182, 161, 54),
            "Ghost" => Color.FromArgb(115, 87, 151),
            "Dragon" => Color.FromArgb(111, 53, 252),
            "Dark" => Color.FromArgb(112, 87, 70),
            "Steel" => Color.FromArgb(183, 183, 206),
            "Fairy" => Color.FromArgb(214, 133, 173),
            _ => Color.Gray
        };
    }
}

/// <summary>
/// Dashboard statistics calculator
/// </summary>
public class DashboardStats
{
    public int TotalPokemon { get; set; }
    public int ShinyCount { get; set; }
    public int LegalCount { get; set; }
    public int IllegalCount { get; set; }
    public int EventCount { get; set; }
    public Dictionary<string, int> TypeCounts { get; set; } = new();

    public static DashboardStats Calculate(SaveFile? sav)
    {
        var stats = new DashboardStats();
        if (sav == null) return stats;

        var typeNames = new[] { "Normal", "Fighting", "Flying", "Poison", "Ground", "Rock", "Bug", "Ghost",
            "Steel", "Fire", "Water", "Grass", "Electric", "Psychic", "Ice", "Dragon", "Dark", "Fairy" };

        foreach (var name in typeNames)
            stats.TypeCounts[name] = 0;

        // Count Pokemon in boxes
        for (int box = 0; box < sav.BoxCount; box++)
        {
            for (int slot = 0; slot < sav.BoxSlotCount; slot++)
            {
                var pk = sav.GetBoxSlotAtIndex(box, slot);
                if (pk.Species == 0) continue;

                stats.TotalPokemon++;

                if (pk.IsShiny) stats.ShinyCount++;
                if (pk.FatefulEncounter) stats.EventCount++;

                var la = new LegalityAnalysis(pk);
                if (la.Valid)
                    stats.LegalCount++;
                else
                    stats.IllegalCount++;

                // Count types
                var pi = pk.PersonalInfo;
                var type1 = GetTypeName(pi.Type1);
                var type2 = GetTypeName(pi.Type2);

                if (stats.TypeCounts.ContainsKey(type1))
                    stats.TypeCounts[type1]++;
                if (type1 != type2 && stats.TypeCounts.ContainsKey(type2))
                    stats.TypeCounts[type2]++;
            }
        }

        return stats;
    }

    private static string GetTypeName(int typeId)
    {
        return typeId switch
        {
            0 => "Normal", 1 => "Fighting", 2 => "Flying", 3 => "Poison",
            4 => "Ground", 5 => "Rock", 6 => "Bug", 7 => "Ghost",
            8 => "Steel", 9 => "Fire", 10 => "Water", 11 => "Grass",
            12 => "Electric", 13 => "Psychic", 14 => "Ice", 15 => "Dragon",
            16 => "Dark", 17 => "Fairy", _ => "Normal"
        };
    }
}
