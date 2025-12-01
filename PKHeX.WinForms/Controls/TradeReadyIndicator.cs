using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms.Controls;

/// <summary>
/// Trade Ready Indicator - Visual indicator showing if a Pokemon is ready for trading
/// </summary>
public class TradeReadyIndicator : Panel
{
    private PKM? _pokemon;
    private TradeReadiness _readiness;
    private readonly ToolTip _tooltip;

    public TradeReadyIndicator()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(180, 40);
        _tooltip = new ToolTip();
        _readiness = new TradeReadiness();
    }

    public void SetPokemon(PKM? pk)
    {
        _pokemon = pk;
        _readiness = TradeReadiness.Analyze(pk);
        Invalidate();

        // Update tooltip
        if (pk != null && pk.Species != 0)
        {
            _tooltip.SetToolTip(this, _readiness.GetDetails());
        }
        else
        {
            _tooltip.SetToolTip(this, "No Pokemon selected");
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var colors = ThemeManager.Colors;

        // Background
        using (var path = ThemeManager.CreateRoundedRectangle(ClientRectangle, 8))
        {
            using var bgBrush = new SolidBrush(colors.Surface);
            g.FillPath(bgBrush, path);

            using var borderPen = new Pen(colors.Border);
            g.DrawPath(borderPen, path);
        }

        if (_pokemon == null || _pokemon.Species == 0)
        {
            using var font = new Font("Segoe UI", 9, FontStyle.Italic);
            using var brush = new SolidBrush(colors.TextMuted);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("No Pokemon", font, brush, ClientRectangle, sf);
            return;
        }

        // Status icon
        var iconRect = new Rectangle(8, 8, 24, 24);
        DrawStatusIcon(g, iconRect, _readiness);

        // Status text
        var textX = iconRect.Right + 8;
        using var statusFont = new Font("Segoe UI", 10, FontStyle.Bold);
        using var statusBrush = new SolidBrush(_readiness.GetStatusColor());
        g.DrawString(_readiness.StatusText, statusFont, statusBrush, textX, 8);

        // Sub-status
        using var subFont = new Font("Segoe UI", 8);
        using var subBrush = new SolidBrush(colors.TextSecondary);
        g.DrawString(_readiness.SubStatusText, subFont, subBrush, textX, 24);
    }

    private static void DrawStatusIcon(Graphics g, Rectangle rect, TradeReadiness readiness)
    {
        var color = readiness.GetStatusColor();

        // Circle background
        using var bgBrush = new SolidBrush(Color.FromArgb(40, color));
        g.FillEllipse(bgBrush, rect);

        // Icon
        using var font = new Font("Segoe UI", 12, FontStyle.Bold);
        using var brush = new SolidBrush(color);
        var icon = readiness.Status switch
        {
            TradeStatus.Ready => "✓",
            TradeStatus.Warning => "!",
            TradeStatus.NotReady => "✗",
            _ => "?"
        };
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(icon, font, brush, rect, sf);
    }
}

/// <summary>
/// Trade readiness analysis result
/// </summary>
public class TradeReadiness
{
    public TradeStatus Status { get; set; } = TradeStatus.Unknown;
    public string StatusText { get; set; } = "Unknown";
    public string SubStatusText { get; set; } = "";
    public bool IsLegal { get; set; }
    public bool HasNickname { get; set; }
    public bool HasHeldItem { get; set; }
    public bool IsShiny { get; set; }
    public bool IsEvent { get; set; }
    public bool HasPokerus { get; set; }
    public string[] Issues { get; set; } = [];

    public static TradeReadiness Analyze(PKM? pk)
    {
        var result = new TradeReadiness();

        if (pk == null || pk.Species == 0)
        {
            result.Status = TradeStatus.Unknown;
            result.StatusText = "No Pokemon";
            return result;
        }

        // Check legality
        var la = new LegalityAnalysis(pk);
        result.IsLegal = la.Valid;

        // Check other properties
        result.HasNickname = pk.IsNicknamed;
        result.HasHeldItem = pk.HeldItem > 0;
        result.IsShiny = pk.IsShiny;
        result.HasPokerus = pk.IsPokerusInfected || pk.IsPokerusCured;

        // Check if event Pokemon
        result.IsEvent = pk.FatefulEncounter;

        // Collect issues
        var issues = new System.Collections.Generic.List<string>();

        if (!result.IsLegal)
        {
            issues.Add("Not legal - may be rejected online");

            // Get specific issues
            foreach (var check in la.Results)
            {
                if (!check.Valid)
                {
                    issues.Add($"- {check.Identifier}");
                    if (issues.Count >= 5) break; // Limit issues shown
                }
            }
        }

        if (result.IsEvent)
            issues.Add("Event Pokemon - may have trade restrictions");

        result.Issues = issues.ToArray();

        // Determine overall status
        if (!result.IsLegal)
        {
            result.Status = TradeStatus.NotReady;
            result.StatusText = "Not Trade Ready";
            result.SubStatusText = "Legality issues detected";
        }
        else if (result.IsEvent)
        {
            result.Status = TradeStatus.Warning;
            result.StatusText = "Trade Caution";
            result.SubStatusText = "Event - check restrictions";
        }
        else
        {
            result.Status = TradeStatus.Ready;
            result.StatusText = "Trade Ready";
            result.SubStatusText = result.IsShiny ? "Shiny & Legal" : "Legal for trading";
        }

        return result;
    }

    public Color GetStatusColor()
    {
        var colors = ThemeManager.Colors;
        return Status switch
        {
            TradeStatus.Ready => colors.LegalGreen,
            TradeStatus.Warning => colors.Warning,
            TradeStatus.NotReady => colors.IllegalRed,
            _ => colors.TextMuted
        };
    }

    public string GetDetails()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Status: {StatusText}");
        sb.AppendLine($"Legal: {(IsLegal ? "Yes" : "No")}");
        sb.AppendLine($"Shiny: {(IsShiny ? "Yes" : "No")}");
        sb.AppendLine($"Event: {(IsEvent ? "Yes" : "No")}");
        sb.AppendLine($"Nicknamed: {(HasNickname ? "Yes" : "No")}");
        sb.AppendLine($"Held Item: {(HasHeldItem ? "Yes" : "No")}");

        if (Issues.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Issues:");
            foreach (var issue in Issues)
            {
                sb.AppendLine(issue);
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// Trade status enumeration
/// </summary>
public enum TradeStatus
{
    Unknown,
    Ready,
    Warning,
    NotReady
}

/// <summary>
/// Compact trade status badge for box view
/// </summary>
public class TradeStatusBadge : Control
{
    private TradeStatus _status = TradeStatus.Unknown;

    public TradeStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            Invalidate();
        }
    }

    public TradeStatusBadge()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer, true);
        Size = new Size(16, 16);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var colors = ThemeManager.Colors;
        var color = _status switch
        {
            TradeStatus.Ready => colors.LegalGreen,
            TradeStatus.Warning => colors.Warning,
            TradeStatus.NotReady => colors.IllegalRed,
            _ => colors.TextMuted
        };

        // Draw circle
        var rect = new Rectangle(2, 2, Width - 4, Height - 4);
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, rect);

        // Draw icon
        var icon = _status switch
        {
            TradeStatus.Ready => "✓",
            TradeStatus.Warning => "!",
            TradeStatus.NotReady => "✗",
            _ => "?"
        };

        using var font = new Font("Segoe UI", 8, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(icon, font, textBrush, ClientRectangle, sf);
    }
}

/// <summary>
/// Trade compatibility checker between two Pokemon
/// </summary>
public static class TradeCompatibilityChecker
{
    public static TradeCompatibility Check(PKM? source, PKM? target)
    {
        var result = new TradeCompatibility();

        if (source == null || target == null)
        {
            result.IsCompatible = false;
            result.Reason = "One or both Pokemon are null";
            return result;
        }

        // Check if they're the same format
        if (source.Format != target.Format)
        {
            result.IsCompatible = false;
            result.Reason = $"Format mismatch: {source.Format} vs {target.Format}";
            return result;
        }

        // Check legality of both
        var sourceLA = new LegalityAnalysis(source);
        var targetLA = new LegalityAnalysis(target);

        if (!sourceLA.Valid || !targetLA.Valid)
        {
            result.IsCompatible = false;
            result.Reason = "One or both Pokemon have legality issues";
            return result;
        }

        result.IsCompatible = true;
        result.Reason = "Both Pokemon are trade-compatible";
        return result;
    }
}

public class TradeCompatibility
{
    public bool IsCompatible { get; set; }
    public string Reason { get; set; } = "";
}
