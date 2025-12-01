using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms.Controls;

/// <summary>
/// Enhanced Pokemon Preview Panel - Shows larger sprites with type badges and legality indicators
/// </summary>
public class EnhancedPokemonPreview : Panel
{
    private PKM? _pokemon;
    private Image? _sprite;
    private bool _isShiny;
    private bool _isLegal;
    private string _species = "";
    private string _nickname = "";
    private byte _level;
    private string _type1 = "";
    private string _type2 = "";
    private string _ability = "";
    private string _nature = "";
    private string _heldItem = "";

    // Type colors for badges
    private static readonly Dictionary<string, Color> TypeColors = new()
    {
        ["Normal"] = Color.FromArgb(168, 167, 122),
        ["Fire"] = Color.FromArgb(238, 129, 48),
        ["Water"] = Color.FromArgb(99, 144, 240),
        ["Electric"] = Color.FromArgb(247, 208, 44),
        ["Grass"] = Color.FromArgb(122, 199, 76),
        ["Ice"] = Color.FromArgb(150, 217, 214),
        ["Fighting"] = Color.FromArgb(194, 46, 40),
        ["Poison"] = Color.FromArgb(163, 62, 161),
        ["Ground"] = Color.FromArgb(226, 191, 101),
        ["Flying"] = Color.FromArgb(169, 143, 243),
        ["Psychic"] = Color.FromArgb(249, 85, 135),
        ["Bug"] = Color.FromArgb(166, 185, 26),
        ["Rock"] = Color.FromArgb(182, 161, 54),
        ["Ghost"] = Color.FromArgb(115, 87, 151),
        ["Dragon"] = Color.FromArgb(111, 53, 252),
        ["Dark"] = Color.FromArgb(112, 87, 70),
        ["Steel"] = Color.FromArgb(183, 183, 206),
        ["Fairy"] = Color.FromArgb(214, 133, 173),
        ["???"] = Color.FromArgb(104, 160, 144)
    };

    public EnhancedPokemonPreview()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(280, 320);
        BackColor = Color.Transparent;
    }

    public void SetPokemon(PKM? pk)
    {
        _pokemon = pk;
        if (pk == null || pk.Species == 0)
        {
            _sprite = null;
            _species = "";
            _nickname = "";
            _isShiny = false;
            _isLegal = true;
            Invalidate();
            return;
        }

        // Get sprite
        _sprite = pk.Sprite();
        _isShiny = pk.IsShiny;
        _species = GameInfo.Strings.Species[pk.Species];
        _nickname = pk.Nickname;
        _level = pk.CurrentLevel;

        // Get types
        var pi = pk.PersonalInfo;
        _type1 = GetTypeName(pi.Type1);
        _type2 = pi.Type1 != pi.Type2 ? GetTypeName(pi.Type2) : "";

        // Get ability
        _ability = GameInfo.Strings.Ability[pk.Ability];

        // Get nature
        _nature = GameInfo.Strings.Natures[(int)pk.Nature];

        // Get held item
        _heldItem = pk.HeldItem > 0 ? GameInfo.Strings.Item[pk.HeldItem] : "None";

        // Check legality
        var la = new LegalityAnalysis(pk);
        _isLegal = la.Valid;

        Invalidate();
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

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var colors = ThemeManager.Colors;

        // Draw background card
        var cardRect = new Rectangle(5, 5, Width - 10, Height - 10);
        using (var path = ThemeManager.CreateRoundedRectangle(cardRect, 12))
        {
            using var bgBrush = new SolidBrush(colors.Surface);
            g.FillPath(bgBrush, path);

            using var borderPen = new Pen(colors.Border, 1);
            g.DrawPath(borderPen, path);
        }

        if (_pokemon == null || _pokemon.Species == 0)
        {
            // Draw empty state
            using var emptyFont = new Font("Segoe UI", 12, FontStyle.Italic);
            using var emptyBrush = new SolidBrush(colors.TextMuted);
            var emptyText = "No Pokemon Selected";
            var emptySize = g.MeasureString(emptyText, emptyFont);
            g.DrawString(emptyText, emptyFont, emptyBrush,
                (Width - emptySize.Width) / 2, (Height - emptySize.Height) / 2);
            return;
        }

        int y = 15;

        // Draw shiny indicator background
        if (_isShiny)
        {
            var shinyRect = new Rectangle(10, 10, Width - 20, 50);
            using var shinyBrush = new LinearGradientBrush(shinyRect,
                Color.FromArgb(40, colors.ShinyGold),
                Color.FromArgb(10, colors.ShinyGold),
                LinearGradientMode.Horizontal);
            g.FillRectangle(shinyBrush, shinyRect);
        }

        // Draw sprite (larger)
        if (_sprite != null)
        {
            var spriteSize = 96;
            var spriteX = (Width - spriteSize) / 2;
            g.DrawImage(_sprite, spriteX, y, spriteSize, spriteSize);

            // Draw shiny sparkle
            if (_isShiny)
            {
                using var sparkleFont = new Font("Segoe UI", 14);
                g.DrawString("★", sparkleFont, new SolidBrush(colors.ShinyGold),
                    spriteX + spriteSize - 5, y - 5);
            }

            // Draw legality indicator
            var legalColor = _isLegal ? colors.LegalGreen : colors.IllegalRed;
            var legalText = _isLegal ? "✓" : "✗";
            using var legalFont = new Font("Segoe UI", 12, FontStyle.Bold);
            g.DrawString(legalText, legalFont, new SolidBrush(legalColor),
                spriteX - 15, y + spriteSize - 20);

            y += spriteSize + 5;
        }

        // Draw nickname/species
        using var nameFont = new Font("Segoe UI", 14, FontStyle.Bold);
        var displayName = string.IsNullOrEmpty(_nickname) || _nickname == _species ? _species : _nickname;
        var nameSize = g.MeasureString(displayName, nameFont);
        g.DrawString(displayName, nameFont, new SolidBrush(colors.Text),
            (Width - nameSize.Width) / 2, y);
        y += (int)nameSize.Height + 2;

        // Draw level
        using var levelFont = new Font("Segoe UI", 10);
        var levelText = $"Lv. {_level}";
        var levelSize = g.MeasureString(levelText, levelFont);
        g.DrawString(levelText, levelFont, new SolidBrush(colors.TextSecondary),
            (Width - levelSize.Width) / 2, y);
        y += (int)levelSize.Height + 8;

        // Draw type badges
        DrawTypeBadges(g, y, _type1, _type2);
        y += 28;

        // Draw stats
        using var statFont = new Font("Segoe UI", 9);
        var statColor = new SolidBrush(colors.TextSecondary);

        // Ability
        var abilityText = $"Ability: {_ability}";
        g.DrawString(abilityText, statFont, statColor, 20, y);
        y += 18;

        // Nature
        var natureText = $"Nature: {_nature}";
        g.DrawString(natureText, statFont, statColor, 20, y);
        y += 18;

        // Held Item
        var itemText = $"Item: {_heldItem}";
        g.DrawString(itemText, statFont, statColor, 20, y);
    }

    private void DrawTypeBadges(Graphics g, int y, string type1, string type2)
    {
        var badgeWidth = 70;
        var badgeHeight = 22;
        var spacing = 8;
        var totalWidth = string.IsNullOrEmpty(type2) ? badgeWidth : badgeWidth * 2 + spacing;
        var startX = (Width - totalWidth) / 2;

        // Draw type 1
        DrawTypeBadge(g, startX, y, badgeWidth, badgeHeight, type1);

        // Draw type 2
        if (!string.IsNullOrEmpty(type2))
        {
            DrawTypeBadge(g, startX + badgeWidth + spacing, y, badgeWidth, badgeHeight, type2);
        }
    }

    private void DrawTypeBadge(Graphics g, int x, int y, int width, int height, string typeName)
    {
        var color = TypeColors.GetValueOrDefault(typeName, Color.Gray);
        var rect = new Rectangle(x, y, width, height);

        using (var path = ThemeManager.CreateRoundedRectangle(rect, 4))
        {
            using var brush = new SolidBrush(color);
            g.FillPath(brush, path);
        }

        using var font = new Font("Segoe UI", 9, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        var textSize = g.MeasureString(typeName, font);
        g.DrawString(typeName, font, textBrush,
            x + (width - textSize.Width) / 2,
            y + (height - textSize.Height) / 2);
    }
}
