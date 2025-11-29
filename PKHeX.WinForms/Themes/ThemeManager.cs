using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PKHeX.WinForms.Themes;

/// <summary>
/// Theme Manager - Handles Dark Mode and custom themes
/// </summary>
public static class ThemeManager
{
    public static Theme CurrentTheme { get; private set; } = Theme.Light;
    public static event EventHandler? ThemeChanged;

    private static readonly Dictionary<string, ThemeColors> Themes = new()
    {
        ["Light"] = new ThemeColors
        {
            Background = Color.FromArgb(240, 240, 240),
            Surface = Color.White,
            Primary = Color.FromArgb(0, 120, 215),
            Secondary = Color.FromArgb(100, 100, 100),
            Text = Color.Black,
            TextSecondary = Color.FromArgb(60, 60, 60),
            Border = Color.FromArgb(200, 200, 200),
            Success = Color.FromArgb(40, 167, 69),
            Warning = Color.FromArgb(255, 193, 7),
            Error = Color.FromArgb(220, 53, 69),
            Accent = Color.FromArgb(0, 120, 215),
            ButtonBackground = Color.FromArgb(225, 225, 225),
            ButtonHover = Color.FromArgb(200, 200, 200),
            InputBackground = Color.White,
            MenuBackground = Color.FromArgb(250, 250, 250)
        },
        ["Dark"] = new ThemeColors
        {
            Background = Color.FromArgb(30, 30, 30),
            Surface = Color.FromArgb(45, 45, 45),
            Primary = Color.FromArgb(100, 149, 237),
            Secondary = Color.FromArgb(150, 150, 150),
            Text = Color.FromArgb(240, 240, 240),
            TextSecondary = Color.FromArgb(180, 180, 180),
            Border = Color.FromArgb(70, 70, 70),
            Success = Color.FromArgb(75, 181, 67),
            Warning = Color.FromArgb(255, 183, 77),
            Error = Color.FromArgb(244, 67, 54),
            Accent = Color.FromArgb(138, 180, 248),
            ButtonBackground = Color.FromArgb(55, 55, 55),
            ButtonHover = Color.FromArgb(70, 70, 70),
            InputBackground = Color.FromArgb(40, 40, 40),
            MenuBackground = Color.FromArgb(35, 35, 35)
        },
        ["Midnight"] = new ThemeColors
        {
            Background = Color.FromArgb(15, 15, 25),
            Surface = Color.FromArgb(25, 25, 40),
            Primary = Color.FromArgb(130, 100, 200),
            Secondary = Color.FromArgb(120, 120, 140),
            Text = Color.FromArgb(230, 230, 240),
            TextSecondary = Color.FromArgb(170, 170, 190),
            Border = Color.FromArgb(50, 50, 70),
            Success = Color.FromArgb(100, 200, 100),
            Warning = Color.FromArgb(255, 200, 100),
            Error = Color.FromArgb(255, 100, 100),
            Accent = Color.FromArgb(180, 150, 255),
            ButtonBackground = Color.FromArgb(40, 40, 60),
            ButtonHover = Color.FromArgb(60, 60, 90),
            InputBackground = Color.FromArgb(20, 20, 35),
            MenuBackground = Color.FromArgb(20, 20, 30)
        },
        ["Pokemon Red"] = new ThemeColors
        {
            Background = Color.FromArgb(40, 25, 25),
            Surface = Color.FromArgb(55, 35, 35),
            Primary = Color.FromArgb(220, 60, 60),
            Secondary = Color.FromArgb(150, 100, 100),
            Text = Color.FromArgb(255, 240, 240),
            TextSecondary = Color.FromArgb(200, 180, 180),
            Border = Color.FromArgb(100, 60, 60),
            Success = Color.FromArgb(100, 180, 100),
            Warning = Color.FromArgb(255, 200, 100),
            Error = Color.FromArgb(255, 80, 80),
            Accent = Color.FromArgb(255, 100, 100),
            ButtonBackground = Color.FromArgb(80, 50, 50),
            ButtonHover = Color.FromArgb(100, 60, 60),
            InputBackground = Color.FromArgb(45, 30, 30),
            MenuBackground = Color.FromArgb(50, 30, 30)
        },
        ["Pokemon Blue"] = new ThemeColors
        {
            Background = Color.FromArgb(25, 30, 50),
            Surface = Color.FromArgb(35, 45, 70),
            Primary = Color.FromArgb(60, 120, 220),
            Secondary = Color.FromArgb(100, 120, 160),
            Text = Color.FromArgb(240, 245, 255),
            TextSecondary = Color.FromArgb(180, 190, 210),
            Border = Color.FromArgb(60, 80, 120),
            Success = Color.FromArgb(100, 180, 100),
            Warning = Color.FromArgb(255, 200, 100),
            Error = Color.FromArgb(255, 100, 100),
            Accent = Color.FromArgb(100, 150, 255),
            ButtonBackground = Color.FromArgb(50, 70, 110),
            ButtonHover = Color.FromArgb(70, 90, 140),
            InputBackground = Color.FromArgb(30, 40, 60),
            MenuBackground = Color.FromArgb(30, 40, 60)
        }
    };

    public static ThemeColors Colors => Themes[CurrentTheme.ToString()];

    public static void SetTheme(Theme theme)
    {
        CurrentTheme = theme;
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void SetTheme(string themeName)
    {
        if (Enum.TryParse<Theme>(themeName, out var theme))
            SetTheme(theme);
    }

    public static string[] GetAvailableThemes()
    {
        return Enum.GetNames(typeof(Theme));
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
                btn.BackColor = colors.ButtonBackground;
                btn.ForeColor = colors.Text;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = colors.Border;
                btn.FlatAppearance.MouseOverBackColor = colors.ButtonHover;
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
                dgv.BackgroundColor = colors.Surface;
                dgv.GridColor = colors.Border;
                dgv.DefaultCellStyle.BackColor = colors.InputBackground;
                dgv.DefaultCellStyle.ForeColor = colors.Text;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = colors.ButtonBackground;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = colors.Text;
                dgv.EnableHeadersVisualStyles = false;
                break;

            case TabControl tab:
                tab.BackColor = colors.Background;
                break;

            case TabPage tabPage:
                tabPage.BackColor = colors.Surface;
                tabPage.ForeColor = colors.Text;
                break;

            case Panel pnl:
                pnl.BackColor = colors.Surface;
                break;

            case GroupBox grp:
                grp.BackColor = colors.Surface;
                grp.ForeColor = colors.Text;
                break;

            case Label lbl:
                lbl.ForeColor = colors.Text;
                if (lbl.BackColor != Color.Transparent)
                    lbl.BackColor = colors.Surface;
                break;

            case CheckBox chk:
                chk.ForeColor = colors.Text;
                break;

            case RadioButton rdo:
                rdo.ForeColor = colors.Text;
                break;

            case NumericUpDown nud:
                nud.BackColor = colors.InputBackground;
                nud.ForeColor = colors.Text;
                break;

            case TrackBar:
                control.BackColor = colors.Surface;
                break;

            case ProgressBar:
                control.BackColor = colors.Surface;
                break;

            case MenuStrip menu:
                menu.BackColor = colors.MenuBackground;
                menu.ForeColor = colors.Text;
                foreach (ToolStripItem item in menu.Items)
                    ApplyThemeToMenuItem(item, colors);
                break;

            case ToolStrip tools:
                tools.BackColor = colors.MenuBackground;
                tools.ForeColor = colors.Text;
                break;

            case StatusStrip status:
                status.BackColor = colors.MenuBackground;
                status.ForeColor = colors.Text;
                break;

            default:
                control.BackColor = colors.Surface;
                control.ForeColor = colors.Text;
                break;
        }
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
    /// Create a themed message box
    /// </summary>
    public static DialogResult ShowThemedMessageBox(string message, string title,
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        // For now, use standard message box
        // A full implementation would create a custom form
        return MessageBox.Show(message, title, buttons, icon);
    }
}

public enum Theme
{
    Light,
    Dark,
    Midnight,
    PokemonRed,
    PokemonBlue
}

public class ThemeColors
{
    public Color Background { get; set; }
    public Color Surface { get; set; }
    public Color Primary { get; set; }
    public Color Secondary { get; set; }
    public Color Text { get; set; }
    public Color TextSecondary { get; set; }
    public Color Border { get; set; }
    public Color Success { get; set; }
    public Color Warning { get; set; }
    public Color Error { get; set; }
    public Color Accent { get; set; }
    public Color ButtonBackground { get; set; }
    public Color ButtonHover { get; set; }
    public Color InputBackground { get; set; }
    public Color MenuBackground { get; set; }
}
