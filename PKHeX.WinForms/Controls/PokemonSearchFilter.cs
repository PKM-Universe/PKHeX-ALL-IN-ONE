using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms.Controls;

/// <summary>
/// Advanced Pokemon Search and Filter Panel with modern UI
/// </summary>
public class PokemonSearchFilter : Panel
{
    private readonly TextBox _txtSearch;
    private readonly ComboBox _cmbType;
    private readonly ComboBox _cmbGeneration;
    private readonly CheckBox _chkShiny;
    private readonly CheckBox _chkLegal;
    private readonly CheckBox _chkIllegal;
    private readonly Button _btnSearch;
    private readonly Button _btnClear;
    private readonly Label _lblResults;

    public event EventHandler<SearchCriteria>? SearchRequested;
    public event EventHandler? ClearRequested;

    public PokemonSearchFilter()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = 120;
        Dock = DockStyle.Top;
        Padding = new Padding(12);

        var colors = ThemeManager.Colors;

        // Search box
        var lblSearch = new Label
        {
            Text = "Search:",
            Location = new Point(12, 18),
            AutoSize = true,
            ForeColor = colors.Text
        };

        _txtSearch = new TextBox
        {
            Location = new Point(70, 15),
            Size = new Size(200, 24),
            BackColor = colors.InputBackground,
            ForeColor = colors.Text,
            BorderStyle = BorderStyle.FixedSingle
        };
        _txtSearch.KeyPress += (s, e) =>
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ExecuteSearch();
                e.Handled = true;
            }
        };

        // Type filter
        var lblType = new Label
        {
            Text = "Type:",
            Location = new Point(290, 18),
            AutoSize = true,
            ForeColor = colors.Text
        };

        _cmbType = new ComboBox
        {
            Location = new Point(330, 15),
            Size = new Size(120, 24),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = colors.InputBackground,
            ForeColor = colors.Text,
            FlatStyle = FlatStyle.Flat
        };
        _cmbType.Items.AddRange(new object[] { "Any", "Normal", "Fire", "Water", "Electric", "Grass", "Ice",
            "Fighting", "Poison", "Ground", "Flying", "Psychic", "Bug", "Rock", "Ghost", "Dragon", "Dark", "Steel", "Fairy" });
        _cmbType.SelectedIndex = 0;

        // Generation filter
        var lblGen = new Label
        {
            Text = "Gen:",
            Location = new Point(470, 18),
            AutoSize = true,
            ForeColor = colors.Text
        };

        _cmbGeneration = new ComboBox
        {
            Location = new Point(505, 15),
            Size = new Size(80, 24),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = colors.InputBackground,
            ForeColor = colors.Text,
            FlatStyle = FlatStyle.Flat
        };
        _cmbGeneration.Items.AddRange(new object[] { "Any", "1", "2", "3", "4", "5", "6", "7", "8", "9", "L:A", "Z-A" });
        _cmbGeneration.SelectedIndex = 0;

        // Checkboxes row
        _chkShiny = new CheckBox
        {
            Text = "Shiny Only",
            Location = new Point(70, 50),
            AutoSize = true,
            ForeColor = colors.ShinyGold,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        _chkLegal = new CheckBox
        {
            Text = "Legal",
            Location = new Point(170, 50),
            AutoSize = true,
            ForeColor = colors.LegalGreen,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        _chkIllegal = new CheckBox
        {
            Text = "Illegal",
            Location = new Point(250, 50),
            AutoSize = true,
            ForeColor = colors.IllegalRed,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        // Search button
        _btnSearch = new Button
        {
            Text = "Search",
            Location = new Point(70, 80),
            Size = new Size(100, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = colors.Primary,
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        _btnSearch.FlatAppearance.BorderSize = 0;
        _btnSearch.Click += (s, e) => ExecuteSearch();

        // Clear button
        _btnClear = new Button
        {
            Text = "Clear",
            Location = new Point(180, 80),
            Size = new Size(80, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = colors.BackgroundTertiary,
            ForeColor = colors.Text,
            Cursor = Cursors.Hand
        };
        _btnClear.FlatAppearance.BorderColor = colors.Border;
        _btnClear.Click += (s, e) => ClearFilters();

        // Results label
        _lblResults = new Label
        {
            Text = "",
            Location = new Point(280, 85),
            AutoSize = true,
            ForeColor = colors.TextSecondary
        };

        Controls.AddRange(new Control[] {
            lblSearch, _txtSearch,
            lblType, _cmbType,
            lblGen, _cmbGeneration,
            _chkShiny, _chkLegal, _chkIllegal,
            _btnSearch, _btnClear, _lblResults
        });
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var colors = ThemeManager.Colors;

        // Background
        using (var bgBrush = new SolidBrush(colors.Surface))
        {
            var rect = new Rectangle(4, 4, Width - 8, Height - 8);
            using var path = ThemeManager.CreateRoundedRectangle(rect, 8);
            g.FillPath(bgBrush, path);

            using var borderPen = new Pen(colors.Border);
            g.DrawPath(borderPen, path);
        }

        // Title
        using var titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
        using var titleBrush = new SolidBrush(colors.Accent);
        g.DrawString("Search Pokemon", titleFont, titleBrush, 12, 12);
    }

    private void ExecuteSearch()
    {
        var criteria = new SearchCriteria
        {
            SearchText = _txtSearch.Text.Trim(),
            TypeFilter = _cmbType.SelectedIndex > 0 ? _cmbType.SelectedItem?.ToString() : null,
            Generation = _cmbGeneration.SelectedIndex > 0 ? _cmbGeneration.SelectedItem?.ToString() : null,
            ShinyOnly = _chkShiny.Checked,
            LegalOnly = _chkLegal.Checked,
            IllegalOnly = _chkIllegal.Checked
        };

        SearchRequested?.Invoke(this, criteria);
    }

    private void ClearFilters()
    {
        _txtSearch.Clear();
        _cmbType.SelectedIndex = 0;
        _cmbGeneration.SelectedIndex = 0;
        _chkShiny.Checked = false;
        _chkLegal.Checked = false;
        _chkIllegal.Checked = false;
        _lblResults.Text = "";

        ClearRequested?.Invoke(this, EventArgs.Empty);
    }

    public void SetResultCount(int count, int total)
    {
        var colors = ThemeManager.Colors;
        _lblResults.Text = $"Found {count} of {total} Pokemon";
        _lblResults.ForeColor = count > 0 ? colors.Success : colors.Warning;
    }
}

/// <summary>
/// Search criteria for Pokemon filtering
/// </summary>
public class SearchCriteria
{
    public string? SearchText { get; set; }
    public string? TypeFilter { get; set; }
    public string? Generation { get; set; }
    public bool ShinyOnly { get; set; }
    public bool LegalOnly { get; set; }
    public bool IllegalOnly { get; set; }

    public bool Matches(PKM pk)
    {
        // Text search (species, nickname, OT)
        if (!string.IsNullOrEmpty(SearchText))
        {
            var search = SearchText.ToLowerInvariant();
            var species = GameInfo.Strings.Species[pk.Species].ToLowerInvariant();
            var nickname = pk.Nickname.ToLowerInvariant();
            var ot = pk.OriginalTrainerName.ToLowerInvariant();

            if (!species.Contains(search) && !nickname.Contains(search) && !ot.Contains(search))
                return false;
        }

        // Type filter
        if (!string.IsNullOrEmpty(TypeFilter))
        {
            var pi = pk.PersonalInfo;
            var type1 = GetTypeName(pi.Type1);
            var type2 = GetTypeName(pi.Type2);

            if (!type1.Equals(TypeFilter, StringComparison.OrdinalIgnoreCase) &&
                !type2.Equals(TypeFilter, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Shiny filter
        if (ShinyOnly && !pk.IsShiny)
            return false;

        // Legality filters
        if (LegalOnly || IllegalOnly)
        {
            var la = new LegalityAnalysis(pk);
            if (LegalOnly && !la.Valid)
                return false;
            if (IllegalOnly && la.Valid)
                return false;
        }

        return true;
    }

    private static string GetTypeName(int typeId)
    {
        return typeId switch
        {
            0 => "Normal", 1 => "Fighting", 2 => "Flying", 3 => "Poison",
            4 => "Ground", 5 => "Rock", 6 => "Bug", 7 => "Ghost",
            8 => "Steel", 9 => "Fire", 10 => "Water", 11 => "Grass",
            12 => "Electric", 13 => "Psychic", 14 => "Ice", 15 => "Dragon",
            16 => "Dark", 17 => "Fairy", _ => "???"
        };
    }
}

/// <summary>
/// Quick search box for inline searching
/// </summary>
public class QuickSearchBox : Panel
{
    private readonly TextBox _txtSearch;
    private readonly Button _btnClear;

    public event EventHandler<string>? SearchTextChanged;

    public string SearchText => _txtSearch.Text;

    public QuickSearchBox()
    {
        Height = 32;
        var colors = ThemeManager.Colors;
        BackColor = colors.InputBackground;

        _txtSearch = new TextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackColor = colors.InputBackground,
            ForeColor = colors.Text,
            Font = new Font("Segoe UI", 10)
        };
        _txtSearch.TextChanged += (s, e) => SearchTextChanged?.Invoke(this, _txtSearch.Text);

        _btnClear = new Button
        {
            Text = "X",
            Dock = DockStyle.Right,
            Width = 28,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = colors.TextMuted,
            Cursor = Cursors.Hand
        };
        _btnClear.FlatAppearance.BorderSize = 0;
        _btnClear.Click += (s, e) => _txtSearch.Clear();

        var searchIcon = new Label
        {
            Text = "🔍",
            Dock = DockStyle.Left,
            Width = 28,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = colors.TextMuted
        };

        Controls.Add(_txtSearch);
        Controls.Add(_btnClear);
        Controls.Add(searchIcon);

        // Border
        Padding = new Padding(1);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var colors = ThemeManager.Colors;
        using var pen = new Pen(colors.Border);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    public void Clear()
    {
        _txtSearch.Clear();
    }
}
