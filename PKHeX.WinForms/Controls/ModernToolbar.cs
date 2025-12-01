using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms.Controls;

/// <summary>
/// Modern Toolbar with custom-rendered buttons and icons
/// </summary>
public class ModernToolbar : Panel
{
    private readonly List<ToolbarButton> _buttons = [];

    public event EventHandler<string>? ButtonClicked;

    public ModernToolbar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = 48;
        Dock = DockStyle.Top;
        Padding = new Padding(8, 4, 8, 4);
    }

    public void AddButton(string id, string text, string icon, string? tooltip = null)
    {
        var button = new ToolbarButton
        {
            Id = id,
            Text = text,
            Icon = icon,
            Tooltip = tooltip ?? text
        };
        _buttons.Add(button);
        RecalculateLayout();
        Invalidate();
    }

    public void AddSeparator()
    {
        _buttons.Add(new ToolbarButton { IsSeparator = true });
        RecalculateLayout();
        Invalidate();
    }

    private void RecalculateLayout()
    {
        int x = Padding.Left;
        int y = Padding.Top;
        int buttonHeight = Height - Padding.Top - Padding.Bottom;

        foreach (var btn in _buttons)
        {
            if (btn.IsSeparator)
            {
                btn.Bounds = new Rectangle(x, y + 8, 1, buttonHeight - 16);
                x += 12;
            }
            else
            {
                int width = Math.Max(80, TextRenderer.MeasureText(btn.Text, Font).Width + 40);
                btn.Bounds = new Rectangle(x, y, width, buttonHeight);
                x += width + 4;
            }
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        RecalculateLayout();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var colors = ThemeManager.Colors;

        // Draw toolbar background
        using (var bgBrush = new LinearGradientBrush(
            ClientRectangle,
            colors.Surface,
            colors.BackgroundSecondary,
            LinearGradientMode.Vertical))
        {
            g.FillRectangle(bgBrush, ClientRectangle);
        }

        // Draw bottom border
        using var borderPen = new Pen(colors.Border);
        g.DrawLine(borderPen, 0, Height - 1, Width, Height - 1);

        // Draw buttons
        foreach (var btn in _buttons)
        {
            if (btn.IsSeparator)
            {
                DrawSeparator(g, btn.Bounds, colors);
            }
            else
            {
                DrawButton(g, btn, colors);
            }
        }
    }

    private static void DrawSeparator(Graphics g, Rectangle bounds, ThemeColors colors)
    {
        using var pen = new Pen(colors.Border);
        g.DrawLine(pen, bounds.X, bounds.Y, bounds.X, bounds.Bottom);
    }

    private void DrawButton(Graphics g, ToolbarButton btn, ThemeColors colors)
    {
        var rect = btn.Bounds;
        var isHovered = btn.IsHovered;
        var isPressed = btn.IsPressed;

        // Button background
        if (isPressed)
        {
            using var brush = new SolidBrush(Color.FromArgb(80, colors.Accent));
            using var path = ThemeManager.CreateRoundedRectangle(rect, 6);
            g.FillPath(brush, path);
        }
        else if (isHovered)
        {
            using var brush = new SolidBrush(colors.ButtonHover);
            using var path = ThemeManager.CreateRoundedRectangle(rect, 6);
            g.FillPath(brush, path);
        }

        // Icon
        var iconSize = 20;
        var iconX = rect.X + 10;
        var iconY = rect.Y + (rect.Height - iconSize) / 2;
        DrawIcon(g, btn.Icon, iconX, iconY, iconSize, isHovered ? colors.Accent : colors.TextSecondary);

        // Text
        using var font = new Font("Segoe UI", 9f, FontStyle.Regular);
        var textX = iconX + iconSize + 6;
        var textColor = isHovered ? colors.Text : colors.TextSecondary;
        using var textBrush = new SolidBrush(textColor);
        var textRect = new Rectangle(textX, rect.Y, rect.Width - (textX - rect.X) - 8, rect.Height);
        var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
        g.DrawString(btn.Text, font, textBrush, textRect, sf);
    }

    private static void DrawIcon(Graphics g, string icon, int x, int y, int size, Color color)
    {
        using var font = new Font("Segoe UI Symbol", size * 0.7f);
        using var brush = new SolidBrush(color);

        var iconChar = icon switch
        {
            "save" => "\uE74E",
            "open" => "\uE838",
            "export" => "\uE8B5",
            "import" => "\uE8B6",
            "settings" => "\uE713",
            "search" => "\uE721",
            "legality" => "\uE73E",
            "box" => "\uE8F1",
            "pokemon" => "\uE8D6",
            "batch" => "\uE762",
            "theme" => "\uE771",
            "help" => "\uE897",
            "about" => "\uE946",
            _ => "\uE8A5"
        };

        g.DrawString(iconChar, font, brush, x, y);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        bool needsRepaint = false;

        foreach (var btn in _buttons)
        {
            if (btn.IsSeparator) continue;

            var wasHovered = btn.IsHovered;
            btn.IsHovered = btn.Bounds.Contains(e.Location);

            if (wasHovered != btn.IsHovered)
                needsRepaint = true;
        }

        if (needsRepaint)
            Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        foreach (var btn in _buttons)
        {
            btn.IsHovered = false;
            btn.IsPressed = false;
        }
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;

        foreach (var btn in _buttons)
        {
            if (!btn.IsSeparator && btn.Bounds.Contains(e.Location))
            {
                btn.IsPressed = true;
                Invalidate();
                break;
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left) return;

        foreach (var btn in _buttons)
        {
            if (!btn.IsSeparator && btn.IsPressed && btn.Bounds.Contains(e.Location))
            {
                ButtonClicked?.Invoke(this, btn.Id);
            }
            btn.IsPressed = false;
        }
        Invalidate();
    }

    private class ToolbarButton
    {
        public string Id { get; set; } = "";
        public string Text { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Tooltip { get; set; } = "";
        public Rectangle Bounds { get; set; }
        public bool IsHovered { get; set; }
        public bool IsPressed { get; set; }
        public bool IsSeparator { get; set; }
    }
}

/// <summary>
/// Quick action buttons panel for common operations
/// </summary>
public class QuickActionsBar : Panel
{
    public event EventHandler? LegalityCheckClicked;
    public event EventHandler? BatchEditClicked;
    public event EventHandler? SearchClicked;
    public event EventHandler? ExportClicked;

    private readonly Button _btnLegality;
    private readonly Button _btnBatch;
    private readonly Button _btnSearch;
    private readonly Button _btnExport;

    public QuickActionsBar()
    {
        Height = 36;
        Dock = DockStyle.Top;
        Padding = new Padding(8, 4, 8, 4);

        var colors = ThemeManager.Colors;
        BackColor = colors.BackgroundTertiary;

        _btnLegality = CreateQuickButton("Check Legality", colors.LegalGreen);
        _btnBatch = CreateQuickButton("Batch Edit", colors.Accent);
        _btnSearch = CreateQuickButton("Search", colors.Info);
        _btnExport = CreateQuickButton("Export", colors.Warning);

        _btnLegality.Click += (s, e) => LegalityCheckClicked?.Invoke(this, EventArgs.Empty);
        _btnBatch.Click += (s, e) => BatchEditClicked?.Invoke(this, EventArgs.Empty);
        _btnSearch.Click += (s, e) => SearchClicked?.Invoke(this, EventArgs.Empty);
        _btnExport.Click += (s, e) => ExportClicked?.Invoke(this, EventArgs.Empty);

        Controls.AddRange(new Control[] { _btnExport, _btnSearch, _btnBatch, _btnLegality });
    }

    private static Button CreateQuickButton(string text, Color accentColor)
    {
        var btn = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Height = 28,
            Width = 100,
            Dock = DockStyle.Left,
            Margin = new Padding(0, 0, 8, 0),
            Cursor = Cursors.Hand
        };

        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = Color.FromArgb(30, accentColor);
        btn.ForeColor = accentColor;
        btn.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);

        btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(50, accentColor);
        btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(30, accentColor);

        return btn;
    }
}
