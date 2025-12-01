using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PKHeX.WinForms.Themes;

/// <summary>
/// PKM-Universe Theme Manager - Modern dark/light themes with gradients and effects
/// </summary>
public static class ThemeManager
{
    public static Theme CurrentTheme { get; private set; } = Theme.Dark;
    public static event EventHandler? ThemeChanged;

    private static readonly Dictionary<string, ThemeColors> Themes = new()
    {
        ["Light"] = new ThemeColors
        {
            Background = Color.FromArgb(248, 249, 252),
            BackgroundSecondary = Color.FromArgb(240, 242, 248),
            BackgroundTertiary = Color.FromArgb(230, 232, 240),
            Surface = Color.White,
            SurfaceHover = Color.FromArgb(245, 245, 250),
            Primary = Color.FromArgb(138, 43, 226),
            Secondary = Color.FromArgb(100, 100, 120),
            Text = Color.FromArgb(30, 30, 45),
            TextSecondary = Color.FromArgb(80, 80, 100),
            TextMuted = Color.FromArgb(140, 140, 160),
            Border = Color.FromArgb(200, 205, 220),
            BorderLight = Color.FromArgb(220, 225, 235),
            Success = Color.FromArgb(39, 174, 96),
            Warning = Color.FromArgb(211, 166, 5),
            Error = Color.FromArgb(192, 57, 43),
            Info = Color.FromArgb(41, 128, 185),
            Accent = Color.FromArgb(138, 43, 226),
            AccentHover = Color.FromArgb(118, 23, 206),
            AccentSecondary = Color.FromArgb(0, 150, 200),
            ButtonBackground = Color.FromArgb(245, 245, 250),
            ButtonHover = Color.FromArgb(138, 43, 226),
            InputBackground = Color.White,
            MenuBackground = Color.FromArgb(250, 250, 255),
            GradientStart = Color.FromArgb(138, 43, 226),
            GradientEnd = Color.FromArgb(100, 20, 180),
            ShinyGold = Color.FromArgb(218, 165, 32),
            LegalGreen = Color.FromArgb(39, 174, 96),
            IllegalRed = Color.FromArgb(192, 57, 43)
        },
        ["Dark"] = new ThemeColors
        {
            Background = Color.FromArgb(18, 18, 24),
            BackgroundSecondary = Color.FromArgb(28, 28, 36),
            BackgroundTertiary = Color.FromArgb(38, 38, 48),
            Surface = Color.FromArgb(45, 45, 58),
            SurfaceHover = Color.FromArgb(55, 55, 70),
            Primary = Color.FromArgb(138, 43, 226),
            Secondary = Color.FromArgb(150, 150, 170),
            Text = Color.FromArgb(240, 240, 245),
            TextSecondary = Color.FromArgb(180, 180, 190),
            TextMuted = Color.FromArgb(120, 120, 135),
            Border = Color.FromArgb(60, 60, 75),
            BorderLight = Color.FromArgb(80, 80, 100),
            Success = Color.FromArgb(46, 204, 113),
            Warning = Color.FromArgb(241, 196, 15),
            Error = Color.FromArgb(231, 76, 60),
            Info = Color.FromArgb(52, 152, 219),
            Accent = Color.FromArgb(138, 43, 226),
            AccentHover = Color.FromArgb(158, 63, 246),
            AccentSecondary = Color.FromArgb(0, 191, 255),
            ButtonBackground = Color.FromArgb(45, 45, 58),
            ButtonHover = Color.FromArgb(138, 43, 226),
            InputBackground = Color.FromArgb(35, 35, 45),
            MenuBackground = Color.FromArgb(25, 25, 32),
            GradientStart = Color.FromArgb(138, 43, 226),
            GradientEnd = Color.FromArgb(75, 0, 130),
            ShinyGold = Color.FromArgb(255, 215, 0),
            LegalGreen = Color.FromArgb(46, 204, 113),
            IllegalRed = Color.FromArgb(231, 76, 60)
        },
        ["Midnight"] = new ThemeColors
        {
            Background = Color.FromArgb(12, 12, 20),
            BackgroundSecondary = Color.FromArgb(20, 20, 32),
            BackgroundTertiary = Color.FromArgb(30, 30, 45),
            Surface = Color.FromArgb(25, 25, 40),
            SurfaceHover = Color.FromArgb(35, 35, 55),
            Primary = Color.FromArgb(130, 100, 200),
            Secondary = Color.FromArgb(120, 120, 150),
            Text = Color.FromArgb(230, 230, 245),
            TextSecondary = Color.FromArgb(170, 170, 200),
            TextMuted = Color.FromArgb(110, 110, 140),
            Border = Color.FromArgb(50, 50, 75),
            BorderLight = Color.FromArgb(70, 70, 100),
            Success = Color.FromArgb(100, 200, 150),
            Warning = Color.FromArgb(255, 200, 100),
            Error = Color.FromArgb(255, 100, 120),
            Info = Color.FromArgb(100, 150, 220),
            Accent = Color.FromArgb(180, 150, 255),
            AccentHover = Color.FromArgb(200, 170, 255),
            AccentSecondary = Color.FromArgb(100, 200, 255),
            ButtonBackground = Color.FromArgb(40, 40, 60),
            ButtonHover = Color.FromArgb(130, 100, 200),
            InputBackground = Color.FromArgb(20, 20, 35),
            MenuBackground = Color.FromArgb(18, 18, 28),
            GradientStart = Color.FromArgb(100, 50, 180),
            GradientEnd = Color.FromArgb(50, 20, 100),
            ShinyGold = Color.FromArgb(255, 215, 100),
            LegalGreen = Color.FromArgb(100, 200, 150),
            IllegalRed = Color.FromArgb(255, 100, 120)
        },
        ["Pokemon Red"] = new ThemeColors
        {
            Background = Color.FromArgb(35, 20, 20),
            BackgroundSecondary = Color.FromArgb(50, 30, 30),
            BackgroundTertiary = Color.FromArgb(65, 40, 40),
            Surface = Color.FromArgb(55, 35, 35),
            SurfaceHover = Color.FromArgb(75, 45, 45),
            Primary = Color.FromArgb(220, 60, 60),
            Secondary = Color.FromArgb(150, 100, 100),
            Text = Color.FromArgb(255, 240, 240),
            TextSecondary = Color.FromArgb(220, 180, 180),
            TextMuted = Color.FromArgb(160, 120, 120),
            Border = Color.FromArgb(100, 60, 60),
            BorderLight = Color.FromArgb(120, 80, 80),
            Success = Color.FromArgb(100, 200, 100),
            Warning = Color.FromArgb(255, 200, 100),
            Error = Color.FromArgb(255, 80, 80),
            Info = Color.FromArgb(100, 150, 200),
            Accent = Color.FromArgb(255, 80, 80),
            AccentHover = Color.FromArgb(255, 100, 100),
            AccentSecondary = Color.FromArgb(255, 150, 100),
            ButtonBackground = Color.FromArgb(80, 45, 45),
            ButtonHover = Color.FromArgb(200, 60, 60),
            InputBackground = Color.FromArgb(45, 28, 28),
            MenuBackground = Color.FromArgb(45, 25, 25),
            GradientStart = Color.FromArgb(200, 50, 50),
            GradientEnd = Color.FromArgb(120, 20, 20),
            ShinyGold = Color.FromArgb(255, 215, 0),
            LegalGreen = Color.FromArgb(100, 200, 100),
            IllegalRed = Color.FromArgb(255, 80, 80)
        },
        ["Pokemon Blue"] = new ThemeColors
        {
            Background = Color.FromArgb(20, 25, 45),
            BackgroundSecondary = Color.FromArgb(30, 40, 65),
            BackgroundTertiary = Color.FromArgb(40, 55, 85),
            Surface = Color.FromArgb(35, 45, 70),
            SurfaceHover = Color.FromArgb(45, 60, 95),
            Primary = Color.FromArgb(60, 120, 220),
            Secondary = Color.FromArgb(100, 130, 180),
            Text = Color.FromArgb(240, 245, 255),
            TextSecondary = Color.FromArgb(180, 200, 230),
            TextMuted = Color.FromArgb(120, 140, 170),
            Border = Color.FromArgb(60, 85, 130),
            BorderLight = Color.FromArgb(80, 110, 160),
            Success = Color.FromArgb(100, 200, 150),
            Warning = Color.FromArgb(255, 200, 100),
            Error = Color.FromArgb(255, 100, 120),
            Info = Color.FromArgb(100, 180, 255),
            Accent = Color.FromArgb(100, 150, 255),
            AccentHover = Color.FromArgb(130, 180, 255),
            AccentSecondary = Color.FromArgb(0, 200, 255),
            ButtonBackground = Color.FromArgb(50, 70, 110),
            ButtonHover = Color.FromArgb(60, 120, 220),
            InputBackground = Color.FromArgb(28, 38, 58),
            MenuBackground = Color.FromArgb(25, 35, 55),
            GradientStart = Color.FromArgb(60, 100, 200),
            GradientEnd = Color.FromArgb(30, 50, 120),
            ShinyGold = Color.FromArgb(255, 215, 0),
            LegalGreen = Color.FromArgb(100, 200, 150),
            IllegalRed = Color.FromArgb(255, 100, 120)
        },
        ["PKM Universe"] = new ThemeColors
        {
            Background = Color.FromArgb(15, 12, 25),
            BackgroundSecondary = Color.FromArgb(25, 20, 40),
            BackgroundTertiary = Color.FromArgb(35, 28, 55),
            Surface = Color.FromArgb(30, 25, 50),
            SurfaceHover = Color.FromArgb(45, 38, 70),
            Primary = Color.FromArgb(180, 100, 255),
            Secondary = Color.FromArgb(130, 120, 160),
            Text = Color.FromArgb(245, 240, 255),
            TextSecondary = Color.FromArgb(190, 180, 210),
            TextMuted = Color.FromArgb(130, 120, 150),
            Border = Color.FromArgb(70, 55, 100),
            BorderLight = Color.FromArgb(90, 75, 130),
            Success = Color.FromArgb(80, 220, 150),
            Warning = Color.FromArgb(255, 200, 80),
            Error = Color.FromArgb(255, 90, 110),
            Info = Color.FromArgb(80, 180, 255),
            Accent = Color.FromArgb(180, 100, 255),
            AccentHover = Color.FromArgb(200, 130, 255),
            AccentSecondary = Color.FromArgb(255, 100, 200),
            ButtonBackground = Color.FromArgb(45, 35, 70),
            ButtonHover = Color.FromArgb(180, 100, 255),
            InputBackground = Color.FromArgb(22, 18, 38),
            MenuBackground = Color.FromArgb(20, 15, 35),
            GradientStart = Color.FromArgb(180, 100, 255),
            GradientEnd = Color.FromArgb(100, 50, 180),
            ShinyGold = Color.FromArgb(255, 220, 80),
            LegalGreen = Color.FromArgb(80, 220, 150),
            IllegalRed = Color.FromArgb(255, 90, 110)
        },
        ["Neon"] = new ThemeColors
        {
            Background = Color.FromArgb(10, 10, 18),
            BackgroundSecondary = Color.FromArgb(18, 18, 28),
            BackgroundTertiary = Color.FromArgb(25, 25, 38),
            Surface = Color.FromArgb(22, 22, 35),
            SurfaceHover = Color.FromArgb(35, 35, 50),
            Primary = Color.FromArgb(0, 255, 255),
            Secondary = Color.FromArgb(150, 150, 180),
            Text = Color.FromArgb(240, 255, 255),
            TextSecondary = Color.FromArgb(180, 220, 220),
            TextMuted = Color.FromArgb(100, 140, 140),
            Border = Color.FromArgb(0, 150, 150),
            BorderLight = Color.FromArgb(0, 180, 180),
            Success = Color.FromArgb(0, 255, 150),
            Warning = Color.FromArgb(255, 255, 0),
            Error = Color.FromArgb(255, 50, 100),
            Info = Color.FromArgb(0, 200, 255),
            Accent = Color.FromArgb(0, 255, 255),
            AccentHover = Color.FromArgb(50, 255, 255),
            AccentSecondary = Color.FromArgb(255, 0, 200),
            ButtonBackground = Color.FromArgb(25, 35, 45),
            ButtonHover = Color.FromArgb(0, 200, 200),
            InputBackground = Color.FromArgb(15, 15, 25),
            MenuBackground = Color.FromArgb(12, 12, 22),
            GradientStart = Color.FromArgb(0, 255, 255),
            GradientEnd = Color.FromArgb(255, 0, 200),
            ShinyGold = Color.FromArgb(255, 255, 100),
            LegalGreen = Color.FromArgb(0, 255, 150),
            IllegalRed = Color.FromArgb(255, 50, 100)
        },
        ["Ocean"] = new ThemeColors
        {
            Background = Color.FromArgb(12, 20, 30),
            BackgroundSecondary = Color.FromArgb(18, 30, 45),
            BackgroundTertiary = Color.FromArgb(25, 40, 58),
            Surface = Color.FromArgb(20, 35, 52),
            SurfaceHover = Color.FromArgb(30, 50, 70),
            Primary = Color.FromArgb(0, 180, 200),
            Secondary = Color.FromArgb(100, 150, 180),
            Text = Color.FromArgb(230, 245, 255),
            TextSecondary = Color.FromArgb(170, 200, 220),
            TextMuted = Color.FromArgb(100, 130, 150),
            Border = Color.FromArgb(40, 80, 110),
            BorderLight = Color.FromArgb(60, 100, 135),
            Success = Color.FromArgb(0, 200, 150),
            Warning = Color.FromArgb(255, 200, 80),
            Error = Color.FromArgb(255, 100, 100),
            Info = Color.FromArgb(80, 180, 255),
            Accent = Color.FromArgb(0, 200, 220),
            AccentHover = Color.FromArgb(50, 220, 240),
            AccentSecondary = Color.FromArgb(0, 150, 200),
            ButtonBackground = Color.FromArgb(25, 45, 65),
            ButtonHover = Color.FromArgb(0, 180, 200),
            InputBackground = Color.FromArgb(15, 28, 42),
            MenuBackground = Color.FromArgb(14, 25, 38),
            GradientStart = Color.FromArgb(0, 180, 220),
            GradientEnd = Color.FromArgb(0, 100, 150),
            ShinyGold = Color.FromArgb(255, 215, 0),
            LegalGreen = Color.FromArgb(0, 200, 150),
            IllegalRed = Color.FromArgb(255, 100, 100)
        },
        ["Forest"] = new ThemeColors
        {
            Background = Color.FromArgb(15, 22, 15),
            BackgroundSecondary = Color.FromArgb(22, 32, 22),
            BackgroundTertiary = Color.FromArgb(30, 45, 30),
            Surface = Color.FromArgb(25, 38, 25),
            SurfaceHover = Color.FromArgb(38, 55, 38),
            Primary = Color.FromArgb(80, 180, 80),
            Secondary = Color.FromArgb(120, 150, 120),
            Text = Color.FromArgb(235, 250, 235),
            TextSecondary = Color.FromArgb(180, 210, 180),
            TextMuted = Color.FromArgb(110, 140, 110),
            Border = Color.FromArgb(50, 90, 50),
            BorderLight = Color.FromArgb(70, 110, 70),
            Success = Color.FromArgb(100, 220, 100),
            Warning = Color.FromArgb(220, 180, 50),
            Error = Color.FromArgb(220, 80, 80),
            Info = Color.FromArgb(80, 160, 200),
            Accent = Color.FromArgb(100, 200, 100),
            AccentHover = Color.FromArgb(130, 220, 130),
            AccentSecondary = Color.FromArgb(150, 200, 80),
            ButtonBackground = Color.FromArgb(35, 50, 35),
            ButtonHover = Color.FromArgb(80, 180, 80),
            InputBackground = Color.FromArgb(18, 28, 18),
            MenuBackground = Color.FromArgb(18, 26, 18),
            GradientStart = Color.FromArgb(80, 180, 80),
            GradientEnd = Color.FromArgb(40, 100, 40),
            ShinyGold = Color.FromArgb(255, 215, 0),
            LegalGreen = Color.FromArgb(100, 220, 100),
            IllegalRed = Color.FromArgb(220, 80, 80)
        },
        ["Sunset"] = new ThemeColors
        {
            Background = Color.FromArgb(25, 18, 18),
            BackgroundSecondary = Color.FromArgb(38, 28, 25),
            BackgroundTertiary = Color.FromArgb(52, 38, 32),
            Surface = Color.FromArgb(45, 32, 28),
            SurfaceHover = Color.FromArgb(60, 45, 38),
            Primary = Color.FromArgb(255, 140, 50),
            Secondary = Color.FromArgb(180, 140, 120),
            Text = Color.FromArgb(255, 245, 235),
            TextSecondary = Color.FromArgb(220, 190, 170),
            TextMuted = Color.FromArgb(160, 130, 110),
            Border = Color.FromArgb(100, 70, 55),
            BorderLight = Color.FromArgb(130, 95, 75),
            Success = Color.FromArgb(150, 220, 100),
            Warning = Color.FromArgb(255, 200, 50),
            Error = Color.FromArgb(255, 80, 80),
            Info = Color.FromArgb(100, 180, 220),
            Accent = Color.FromArgb(255, 120, 50),
            AccentHover = Color.FromArgb(255, 150, 80),
            AccentSecondary = Color.FromArgb(255, 80, 100),
            ButtonBackground = Color.FromArgb(55, 40, 35),
            ButtonHover = Color.FromArgb(255, 120, 50),
            InputBackground = Color.FromArgb(32, 24, 22),
            MenuBackground = Color.FromArgb(30, 22, 20),
            GradientStart = Color.FromArgb(255, 140, 50),
            GradientEnd = Color.FromArgb(200, 50, 80),
            ShinyGold = Color.FromArgb(255, 220, 80),
            LegalGreen = Color.FromArgb(150, 220, 100),
            IllegalRed = Color.FromArgb(255, 80, 80)
        }
    };

    public static ThemeColors Colors => Themes[CurrentTheme.ToString().Replace("_", " ")];

    public static void SetTheme(Theme theme)
    {
        CurrentTheme = theme;
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void SetTheme(string themeName)
    {
        themeName = themeName.Replace(" ", "_");
        if (Enum.TryParse<Theme>(themeName, out var theme))
            SetTheme(theme);
    }

    public static void ToggleTheme()
    {
        SetTheme(CurrentTheme == Theme.Dark ? Theme.Light : Theme.Dark);
    }

    public static string[] GetAvailableThemes()
    {
        return new[] { "Light", "Dark", "Midnight", "Pokemon Red", "Pokemon Blue", "PKM Universe", "Neon", "Ocean", "Forest", "Sunset" };
    }

    /// <summary>
    /// Apply theme to a form and all its controls
    /// </summary>
    public static void ApplyTheme(Form form)
    {
        var colors = Colors;

        form.BackColor = colors.Background;
        form.ForeColor = colors.Text;

        ApplyThemeToControls(form.Controls, colors);
    }

    /// <summary>
    /// Apply theme to a specific control and its children
    /// </summary>
    public static void ApplyTheme(Control control)
    {
        var colors = Colors;
        ApplyThemeToControl(control, colors);
        if (control.HasChildren)
            ApplyThemeToControls(control.Controls, colors);
    }

    private static void ApplyThemeToControls(Control.ControlCollection controls, ThemeColors colors)
    {
        foreach (Control control in controls)
        {
            ApplyThemeToControl(control, colors);

            if (control.HasChildren)
                ApplyThemeToControls(control.Controls, colors);
        }
    }

    private static void ApplyThemeToControl(Control control, ThemeColors colors)
    {
        switch (control)
        {
            case Button btn:
                StyleButton(btn, colors);
                break;

            case TextBox txt:
                txt.BackColor = colors.InputBackground;
                txt.ForeColor = colors.Text;
                txt.BorderStyle = BorderStyle.FixedSingle;
                break;

            case RichTextBox rtxt:
                rtxt.BackColor = colors.InputBackground;
                rtxt.ForeColor = colors.Text;
                break;

            case ComboBox cmb:
                cmb.BackColor = colors.InputBackground;
                cmb.ForeColor = colors.Text;
                cmb.FlatStyle = FlatStyle.Flat;
                break;

            case ListBox lst:
                lst.BackColor = colors.InputBackground;
                lst.ForeColor = colors.Text;
                break;

            case ListView lv:
                lv.BackColor = colors.InputBackground;
                lv.ForeColor = colors.Text;
                break;

            case DataGridView dgv:
                StyleDataGridView(dgv, colors);
                break;

            case TabControl tab:
                tab.BackColor = colors.Background;
                break;

            case TabPage tabPage:
                tabPage.BackColor = colors.BackgroundSecondary;
                tabPage.ForeColor = colors.Text;
                break;

            case Panel pnl:
                if (pnl.BorderStyle == BorderStyle.None)
                    pnl.BackColor = colors.BackgroundSecondary;
                else
                    pnl.BackColor = colors.Surface;
                break;

            case GroupBox grp:
                grp.BackColor = colors.BackgroundSecondary;
                grp.ForeColor = colors.Text;
                break;

            case Label lbl:
                lbl.ForeColor = colors.Text;
                if (lbl.BackColor != Color.Transparent)
                    lbl.BackColor = colors.BackgroundSecondary;
                break;

            case CheckBox chk:
                chk.ForeColor = colors.Text;
                chk.BackColor = Color.Transparent;
                break;

            case RadioButton rdo:
                rdo.ForeColor = colors.Text;
                rdo.BackColor = Color.Transparent;
                break;

            case NumericUpDown nud:
                nud.BackColor = colors.InputBackground;
                nud.ForeColor = colors.Text;
                break;

            case TrackBar:
                control.BackColor = colors.BackgroundSecondary;
                break;

            case ProgressBar:
                control.BackColor = colors.BackgroundSecondary;
                break;

            case MenuStrip menu:
                StyleMenuStrip(menu, colors);
                break;

            case ContextMenuStrip ctx:
                ctx.BackColor = colors.MenuBackground;
                ctx.ForeColor = colors.Text;
                ctx.Renderer = new ModernMenuRenderer(colors);
                break;

            case StatusStrip status:
                status.BackColor = colors.BackgroundTertiary;
                status.ForeColor = colors.TextSecondary;
                status.Renderer = new ModernToolStripRenderer(colors);
                break;

            case ToolStrip tools:
                tools.BackColor = colors.BackgroundSecondary;
                tools.ForeColor = colors.Text;
                tools.Renderer = new ModernToolStripRenderer(colors);
                break;

            case PictureBox pic:
                pic.BackColor = Color.Transparent;
                break;

            case SplitContainer split:
                split.BackColor = colors.Background;
                split.Panel1.BackColor = colors.BackgroundSecondary;
                split.Panel2.BackColor = colors.BackgroundSecondary;
                break;

            default:
                if (control.BackColor != Color.Transparent)
                {
                    control.BackColor = colors.BackgroundSecondary;
                    control.ForeColor = colors.Text;
                }
                break;
        }
    }

    public static void StyleButton(Button btn, ThemeColors? colors = null)
    {
        colors ??= Colors;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = colors.Border;
        btn.BackColor = colors.ButtonBackground;
        btn.ForeColor = colors.Text;
        btn.Cursor = Cursors.Hand;
        btn.FlatAppearance.MouseOverBackColor = colors.ButtonHover;
        btn.FlatAppearance.MouseDownBackColor = colors.AccentHover;
    }

    public static void StylePrimaryButton(Button btn, ThemeColors? colors = null)
    {
        colors ??= Colors;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = colors.Accent;
        btn.ForeColor = Color.White;
        btn.Cursor = Cursors.Hand;
        btn.Font = new Font(btn.Font.FontFamily, btn.Font.Size, FontStyle.Bold);
        btn.FlatAppearance.MouseOverBackColor = colors.AccentHover;
        btn.FlatAppearance.MouseDownBackColor = colors.Primary;
    }

    public static void StyleDataGridView(DataGridView dgv, ThemeColors? colors = null)
    {
        colors ??= Colors;
        dgv.BackgroundColor = colors.BackgroundSecondary;
        dgv.GridColor = colors.Border;
        dgv.DefaultCellStyle.BackColor = colors.Surface;
        dgv.DefaultCellStyle.ForeColor = colors.Text;
        dgv.DefaultCellStyle.SelectionBackColor = colors.Accent;
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = colors.BackgroundTertiary;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = colors.Text;
        dgv.RowHeadersDefaultCellStyle.BackColor = colors.BackgroundTertiary;
        dgv.RowHeadersDefaultCellStyle.ForeColor = colors.Text;
        dgv.EnableHeadersVisualStyles = false;
        dgv.BorderStyle = BorderStyle.None;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
    }

    public static void StyleMenuStrip(MenuStrip menu, ThemeColors? colors = null)
    {
        colors ??= Colors;
        menu.BackColor = colors.MenuBackground;
        menu.ForeColor = colors.Text;
        menu.Renderer = new ModernMenuRenderer(colors);
        foreach (ToolStripItem item in menu.Items)
            ApplyThemeToMenuItem(item, colors);
    }

    private static void ApplyThemeToMenuItem(ToolStripItem item, ThemeColors colors)
    {
        item.BackColor = colors.MenuBackground;
        item.ForeColor = colors.Text;

        if (item is ToolStripMenuItem menuItem)
        {
            foreach (ToolStripItem subItem in menuItem.DropDownItems)
                ApplyThemeToMenuItem(subItem, colors);
        }
    }

    /// <summary>
    /// Create a gradient brush for headers and special elements
    /// </summary>
    public static LinearGradientBrush CreateGradientBrush(Rectangle rect, bool horizontal = true)
    {
        var colors = Colors;
        if (rect.Width <= 0) rect.Width = 1;
        if (rect.Height <= 0) rect.Height = 1;
        return new LinearGradientBrush(
            rect,
            colors.GradientStart,
            colors.GradientEnd,
            horizontal ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical
        );
    }

    /// <summary>
    /// Draw a rounded rectangle path
    /// </summary>
    public static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var diameter = radius * 2;
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();

        return path;
    }

    /// <summary>
    /// Get color for legality status
    /// </summary>
    public static Color GetLegalityColor(bool isLegal)
    {
        return isLegal ? Colors.LegalGreen : Colors.IllegalRed;
    }

    /// <summary>
    /// Get color for shiny indicator
    /// </summary>
    public static Color GetShinyColor()
    {
        return Colors.ShinyGold;
    }
}

public enum Theme
{
    Light,
    Dark,
    Midnight,
    Pokemon_Red,
    Pokemon_Blue,
    PKM_Universe,
    Neon,
    Ocean,
    Forest,
    Sunset
}

public class ThemeColors
{
    public Color Background { get; set; }
    public Color BackgroundSecondary { get; set; }
    public Color BackgroundTertiary { get; set; }
    public Color Surface { get; set; }
    public Color SurfaceHover { get; set; }
    public Color Primary { get; set; }
    public Color Secondary { get; set; }
    public Color Text { get; set; }
    public Color TextSecondary { get; set; }
    public Color TextMuted { get; set; }
    public Color Border { get; set; }
    public Color BorderLight { get; set; }
    public Color Success { get; set; }
    public Color Warning { get; set; }
    public Color Error { get; set; }
    public Color Info { get; set; }
    public Color Accent { get; set; }
    public Color AccentHover { get; set; }
    public Color AccentSecondary { get; set; }
    public Color ButtonBackground { get; set; }
    public Color ButtonHover { get; set; }
    public Color InputBackground { get; set; }
    public Color MenuBackground { get; set; }
    public Color GradientStart { get; set; }
    public Color GradientEnd { get; set; }
    public Color ShinyGold { get; set; }
    public Color LegalGreen { get; set; }
    public Color IllegalRed { get; set; }
}

/// <summary>
/// Modern menu renderer with theme support
/// </summary>
public class ModernMenuRenderer : ToolStripProfessionalRenderer
{
    private readonly ThemeColors _colors;

    public ModernMenuRenderer(ThemeColors colors) : base(new ModernColorTable(colors))
    {
        _colors = colors;
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        var rect = new Rectangle(Point.Empty, e.Item.Size);
        if (e.Item.Selected || e.Item.Pressed)
        {
            using var brush = new SolidBrush(_colors.Accent);
            e.Graphics.FillRectangle(brush, rect);
        }
        else
        {
            using var brush = new SolidBrush(_colors.MenuBackground);
            e.Graphics.FillRectangle(brush, rect);
        }
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Selected || e.Item.Pressed ? Color.White : _colors.Text;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        var rect = new Rectangle(Point.Empty, e.Item.Size);
        using var pen = new Pen(_colors.Border);
        int y = rect.Height / 2;
        e.Graphics.DrawLine(pen, 4, y, rect.Width - 4, y);
    }
}

/// <summary>
/// Modern toolbar renderer
/// </summary>
public class ModernToolStripRenderer : ToolStripProfessionalRenderer
{
    private readonly ThemeColors _colors;

    public ModernToolStripRenderer(ThemeColors colors) : base(new ModernColorTable(colors))
    {
        _colors = colors;
    }

    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        var rect = new Rectangle(Point.Empty, e.Item.Size);
        if (e.Item.Selected || e.Item.Pressed)
        {
            using var brush = new SolidBrush(_colors.Accent);
            e.Graphics.FillRectangle(brush, rect);
        }
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        using var brush = new SolidBrush(_colors.BackgroundSecondary);
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Selected || e.Item.Pressed ? Color.White : _colors.Text;
        base.OnRenderItemText(e);
    }
}

/// <summary>
/// Color table for professional renderer
/// </summary>
public class ModernColorTable : ProfessionalColorTable
{
    private readonly ThemeColors _colors;

    public ModernColorTable(ThemeColors colors)
    {
        _colors = colors;
        UseSystemColors = false;
    }

    public override Color MenuBorder => _colors.Border;
    public override Color MenuItemBorder => _colors.Accent;
    public override Color MenuItemSelected => _colors.Accent;
    public override Color MenuItemSelectedGradientBegin => _colors.Accent;
    public override Color MenuItemSelectedGradientEnd => _colors.AccentHover;
    public override Color MenuStripGradientBegin => _colors.MenuBackground;
    public override Color MenuStripGradientEnd => _colors.MenuBackground;
    public override Color ToolStripDropDownBackground => _colors.MenuBackground;
    public override Color ImageMarginGradientBegin => _colors.BackgroundTertiary;
    public override Color ImageMarginGradientMiddle => _colors.BackgroundTertiary;
    public override Color ImageMarginGradientEnd => _colors.BackgroundTertiary;
    public override Color SeparatorDark => _colors.Border;
    public override Color SeparatorLight => _colors.BorderLight;
    public override Color StatusStripGradientBegin => _colors.BackgroundTertiary;
    public override Color StatusStripGradientEnd => _colors.BackgroundTertiary;
    public override Color ToolStripGradientBegin => _colors.BackgroundSecondary;
    public override Color ToolStripGradientMiddle => _colors.BackgroundSecondary;
    public override Color ToolStripGradientEnd => _colors.BackgroundSecondary;
    public override Color ButtonSelectedGradientBegin => _colors.Accent;
    public override Color ButtonSelectedGradientEnd => _colors.AccentHover;
    public override Color ButtonPressedGradientBegin => _colors.AccentHover;
    public override Color ButtonPressedGradientEnd => _colors.Accent;
}
