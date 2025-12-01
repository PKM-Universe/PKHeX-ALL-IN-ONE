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
/// Represents a single animated sparkle particle
/// </summary>
public class SparkleParticle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public float Size { get; set; }
    public float Opacity { get; set; }
    public float Rotation { get; set; }
    public float RotationSpeed { get; set; }
    public int LifeTime { get; set; }
    public int MaxLifeTime { get; set; }
    public Color Color { get; set; }
    public SparkleType Type { get; set; }
}

public enum SparkleType
{
    Star,
    Diamond,
    Circle,
    Cross
}

/// <summary>
/// Enhanced Pokemon Preview Panel - Shows larger sprites with type badges and legality indicators
/// Features animated sparkle effects for shiny Pokemon
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

    // Sparkle animation fields
    private readonly List<SparkleParticle> _sparkles = new();
    private readonly Timer _sparkleTimer;
    private readonly Random _random = new();
    private int _sparkleSpawnCounter;
    private Rectangle _spriteArea;
    private bool _animationEnabled = true;

    // Sparkle colors for variety
    private static readonly Color[] SparkleColors = new[]
    {
        Color.FromArgb(255, 215, 0),    // Gold
        Color.FromArgb(255, 255, 200),  // Light yellow
        Color.FromArgb(255, 250, 250),  // Snow white
        Color.FromArgb(255, 223, 186),  // Peach
        Color.FromArgb(173, 216, 230),  // Light blue
        Color.FromArgb(255, 182, 193),  // Light pink
    };

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

        // Initialize sparkle animation timer
        _sparkleTimer = new Timer
        {
            Interval = 33 // ~30 FPS
        };
        _sparkleTimer.Tick += SparkleTimer_Tick;
    }

    /// <summary>
    /// Enable or disable the shiny sparkle animation
    /// </summary>
    public bool AnimationEnabled
    {
        get => _animationEnabled;
        set
        {
            _animationEnabled = value;
            if (_isShiny && value)
                _sparkleTimer.Start();
            else
                _sparkleTimer.Stop();
        }
    }

    private void SparkleTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isShiny || !_animationEnabled)
        {
            _sparkleTimer.Stop();
            _sparkles.Clear();
            return;
        }

        // Spawn new sparkles periodically
        _sparkleSpawnCounter++;
        if (_sparkleSpawnCounter >= 3) // Spawn every ~100ms
        {
            _sparkleSpawnCounter = 0;
            if (_sparkles.Count < 15) // Max 15 sparkles at once
            {
                SpawnSparkle();
            }
        }

        // Update existing sparkles
        for (int i = _sparkles.Count - 1; i >= 0; i--)
        {
            var sparkle = _sparkles[i];
            sparkle.X += sparkle.VelocityX;
            sparkle.Y += sparkle.VelocityY;
            sparkle.Rotation += sparkle.RotationSpeed;
            sparkle.LifeTime++;

            // Fade out as lifetime increases
            float lifeRatio = (float)sparkle.LifeTime / sparkle.MaxLifeTime;
            if (lifeRatio < 0.2f)
                sparkle.Opacity = lifeRatio * 5f; // Fade in
            else if (lifeRatio > 0.7f)
                sparkle.Opacity = (1f - lifeRatio) / 0.3f; // Fade out
            else
                sparkle.Opacity = 1f;

            // Pulse size
            sparkle.Size = sparkle.Size * (1f + 0.02f * (float)Math.Sin(sparkle.LifeTime * 0.2f));

            // Remove dead sparkles
            if (sparkle.LifeTime >= sparkle.MaxLifeTime)
            {
                _sparkles.RemoveAt(i);
            }
        }

        Invalidate();
    }

    private void SpawnSparkle()
    {
        if (_spriteArea.Width <= 0 || _spriteArea.Height <= 0)
            return;

        var sparkle = new SparkleParticle
        {
            X = _spriteArea.X + (float)_random.NextDouble() * _spriteArea.Width,
            Y = _spriteArea.Y + (float)_random.NextDouble() * _spriteArea.Height,
            VelocityX = (float)(_random.NextDouble() - 0.5) * 1.5f,
            VelocityY = (float)(_random.NextDouble() - 0.8) * 2f, // Bias upward
            Size = 4 + (float)_random.NextDouble() * 6,
            Opacity = 0f,
            Rotation = (float)_random.NextDouble() * 360f,
            RotationSpeed = (float)(_random.NextDouble() - 0.5) * 10f,
            LifeTime = 0,
            MaxLifeTime = 40 + _random.Next(30),
            Color = SparkleColors[_random.Next(SparkleColors.Length)],
            Type = (SparkleType)_random.Next(4)
        };

        _sparkles.Add(sparkle);
    }

    private void DrawSparkle(Graphics g, SparkleParticle sparkle)
    {
        if (sparkle.Opacity <= 0)
            return;

        var state = g.Save();
        g.TranslateTransform(sparkle.X, sparkle.Y);
        g.RotateTransform(sparkle.Rotation);

        var alpha = (int)(sparkle.Opacity * 255);
        var color = Color.FromArgb(alpha, sparkle.Color);
        var glowColor = Color.FromArgb(alpha / 3, sparkle.Color);
        var size = sparkle.Size;

        using var brush = new SolidBrush(color);
        using var glowBrush = new SolidBrush(glowColor);

        // Draw glow
        g.FillEllipse(glowBrush, -size * 1.5f, -size * 1.5f, size * 3f, size * 3f);

        switch (sparkle.Type)
        {
            case SparkleType.Star:
                DrawStar(g, brush, size);
                break;
            case SparkleType.Diamond:
                DrawDiamond(g, brush, size);
                break;
            case SparkleType.Circle:
                g.FillEllipse(brush, -size / 2, -size / 2, size, size);
                break;
            case SparkleType.Cross:
                DrawCross(g, brush, size);
                break;
        }

        g.Restore(state);
    }

    private void DrawStar(Graphics g, Brush brush, float size)
    {
        var points = new PointF[8];
        for (int i = 0; i < 8; i++)
        {
            var angle = i * Math.PI / 4;
            var radius = i % 2 == 0 ? size : size / 2.5f;
            points[i] = new PointF(
                (float)(Math.Cos(angle) * radius),
                (float)(Math.Sin(angle) * radius)
            );
        }
        g.FillPolygon(brush, points);
    }

    private void DrawDiamond(Graphics g, Brush brush, float size)
    {
        var points = new PointF[]
        {
            new(0, -size),
            new(size * 0.6f, 0),
            new(0, size),
            new(-size * 0.6f, 0)
        };
        g.FillPolygon(brush, points);
    }

    private void DrawCross(Graphics g, Brush brush, float size)
    {
        var thickness = size / 3;
        g.FillRectangle(brush, -size, -thickness / 2, size * 2, thickness);
        g.FillRectangle(brush, -thickness / 2, -size, thickness, size * 2);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sparkleTimer?.Stop();
            _sparkleTimer?.Dispose();
        }
        base.Dispose(disposing);
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
            _sparkleTimer.Stop();
            _sparkles.Clear();
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

        // Handle sparkle animation for shiny Pokemon
        if (_isShiny && _animationEnabled)
        {
            _sparkles.Clear();
            _sparkleSpawnCounter = 0;
            _sparkleTimer.Start();
        }
        else
        {
            _sparkleTimer.Stop();
            _sparkles.Clear();
        }

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

            // Store sprite area for sparkle spawning (expanded area)
            _spriteArea = new Rectangle(spriteX - 20, y - 10, spriteSize + 40, spriteSize + 20);

            g.DrawImage(_sprite, spriteX, y, spriteSize, spriteSize);

            // Draw shiny sparkle indicator (static star)
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

        // Draw animated sparkles for shiny Pokemon (drawn last so they appear on top)
        if (_isShiny && _sparkles.Count > 0)
        {
            foreach (var sparkle in _sparkles)
            {
                DrawSparkle(g, sparkle);
            }
        }
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
