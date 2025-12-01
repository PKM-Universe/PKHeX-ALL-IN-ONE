using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms;

public partial class About : Form
{
    public About(AboutPage index = AboutPage.Changelog)
    {
        InitializeComponent();
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

        WinFormsUtil.TranslateInterface(this, Main.CurrentLanguage);
        RTB_Changelog.Text = Properties.Resources.changelog;
        RTB_Shortcuts.Text = Properties.Resources.shortcuts;
        RTB_PKMUpdates.Text = GetPKMUniverseChangelog();
        TC_About.SelectedIndex = (int)index;

        // Apply theme
        ApplyTheme();

        // Set up link click handlers
        LL_Discord.LinkClicked += (_, _) => OpenUrl("https://discord.gg/pkm-universe");
        LL_Kofi.LinkClicked += (_, _) => OpenUrl("https://ko-fi.com/pokemonlover8888");
        LL_GitHub.LinkClicked += (_, _) => OpenUrl("https://github.com/PKM-Universe/PKHeX-ALL-IN-ONE");
        LL_Website.LinkClicked += (_, _) => OpenUrl("https://pkmuniverse.com");
    }

    private void ApplyTheme()
    {
        var colors = ThemeManager.Colors;

        BackColor = colors.Background;
        ForeColor = colors.Text;

        // Header panel
        PNL_Header.BackColor = Color.Transparent;

        // Tabs
        TC_About.BackColor = colors.BackgroundSecondary;

        foreach (TabPage tab in TC_About.TabPages)
        {
            tab.BackColor = colors.BackgroundSecondary;
            tab.ForeColor = colors.Text;
        }

        // Rich text boxes
        RTB_Changelog.BackColor = colors.Surface;
        RTB_Changelog.ForeColor = colors.Text;
        RTB_Shortcuts.BackColor = colors.Surface;
        RTB_Shortcuts.ForeColor = colors.Text;
        RTB_PKMUpdates.BackColor = colors.Surface;
        RTB_PKMUpdates.ForeColor = colors.Text;
        RTB_Credits.BackColor = colors.Surface;
        RTB_Credits.ForeColor = colors.Text;

        // Labels
        L_Title.ForeColor = colors.Text;
        L_Tagline.ForeColor = colors.TextSecondary;
        L_Version.ForeColor = colors.TextMuted;
        L_Thanks.ForeColor = colors.TextSecondary;
        L_Copyright.ForeColor = colors.TextMuted;

        // Links
        LL_Discord.LinkColor = colors.AccentSecondary;
        LL_Discord.ActiveLinkColor = colors.Accent;
        LL_Kofi.LinkColor = Color.FromArgb(255, 92, 92); // Ko-fi red
        LL_Kofi.ActiveLinkColor = Color.FromArgb(255, 120, 120);
        LL_GitHub.LinkColor = colors.TextSecondary;
        LL_GitHub.ActiveLinkColor = colors.Text;
        LL_Website.LinkColor = colors.Accent;
        LL_Website.ActiveLinkColor = colors.AccentHover;

        // Footer
        PNL_Footer.BackColor = colors.BackgroundTertiary;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var colors = ThemeManager.Colors;

        // Draw header gradient
        var headerRect = new Rectangle(0, 0, Width, 120);
        using (var brush = new LinearGradientBrush(
            headerRect,
            colors.GradientStart,
            colors.GradientEnd,
            LinearGradientMode.Horizontal))
        {
            g.FillRectangle(brush, headerRect);
        }

        // Draw subtle pattern overlay
        using (var pen = new Pen(Color.FromArgb(20, 255, 255, 255)))
        {
            for (int i = 0; i < Width + 120; i += 20)
            {
                g.DrawLine(pen, i, 0, i - 120, 120);
            }
        }

        // Draw logo glow
        var iconCenter = new Point(PB_Icon.Left + PB_Icon.Width / 2, PB_Icon.Top + PB_Icon.Height / 2);
        using var path = new GraphicsPath();
        path.AddEllipse(iconCenter.X - 45, iconCenter.Y - 45, 90, 90);
        using var glowBrush = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(80, 255, 255, 255),
            SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) }
        };
        g.FillPath(glowBrush, path);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch { }
    }

    private static string GetPKMUniverseChangelog()
    {
        return @"PKM-Universe Updates
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Version 25.12 (December 2025)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━

NEW FEATURES:
• Animated Shiny Sparkles - Shiny Pokemon now display beautiful animated sparkle effects
• New Theme Presets - Added Neon, Ocean, Forest, and Sunset color themes
• PKM-Universe Updates Tab - This new changelog to track our exclusive features

ENHANCEMENTS:
• Integrated santacrab2's Auto-Legality Mod (ALM) for automatic legality fixing
• Enhanced Pokemon Preview panel with type badges and legality indicators
• Improved theme system with smooth gradients and modern styling
• Updated to .NET 10 for better performance

━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Version 25.01 (January 2025)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━

INITIAL PKM-UNIVERSE RELEASE:
• Custom branded interface with PKM-Universe theming
• Built-in Shiny Living Dex Generator
• Trade-Ready Indicator system
• Enhanced Dashboard screen
• Quick Templates for competitive builds
• Batch Operations Panel
• Pokemon Search & Filter system
• QR Code Generator
• Bulk Importer functionality
• Custom About page with social links

━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Coming Soon:
• More visual effects and animations
• Additional theme customization options
• Enhanced competitive analysis tools
• Community-requested features

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Join our Discord for the latest updates!
discord.gg/pkm-universe

Support development on Ko-fi:
ko-fi.com/pokemonlover8888
";
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate(); // Redraw gradient on resize
    }
}

public enum AboutPage
{
    Shortcuts,
    Changelog,
}
