using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms;

public partial class SplashScreen : Form
{
    private readonly Timer _animationTimer;
    private readonly Timer _progressTimer;
    private float _glowAngle = 0;
    private int _progressValue = 0;
    private readonly Random _random = new();

    // Particle system for visual effect
    private readonly Particle[] _particles = new Particle[20];

    public SplashScreen()
    {
        InitializeComponent();
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);

        // Initialize particles
        for (int i = 0; i < _particles.Length; i++)
        {
            _particles[i] = new Particle(_random, Width, Height);
        }

        // Animation timer for glow effect
        _animationTimer = new Timer { Interval = 30 };
        _animationTimer.Tick += (s, e) =>
        {
            _glowAngle += 2f;
            if (_glowAngle >= 360) _glowAngle = 0;

            // Update particles
            foreach (var p in _particles)
                p.Update(Height);

            Invalidate();
        };
        _animationTimer.Start();

        // Progress simulation timer
        _progressTimer = new Timer { Interval = 50 };
        _progressTimer.Tick += (s, e) =>
        {
            if (_progressValue < 100)
            {
                _progressValue += _random.Next(1, 4);
                if (_progressValue > 100) _progressValue = 100;
            }
        };
        _progressTimer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        var colors = ThemeManager.Colors;

        // Draw background gradient
        using (var bgBrush = new LinearGradientBrush(
            ClientRectangle,
            colors.Background,
            colors.BackgroundSecondary,
            LinearGradientMode.ForwardDiagonal))
        {
            g.FillRectangle(bgBrush, ClientRectangle);
        }

        // Draw particles
        foreach (var p in _particles)
        {
            using var particleBrush = new SolidBrush(Color.FromArgb((int)(p.Alpha * 255), colors.Accent));
            g.FillEllipse(particleBrush, p.X, p.Y, p.Size, p.Size);
        }

        // Draw glowing border
        DrawGlowingBorder(g, colors);

        // Draw accent line at top
        using (var accentBrush = new LinearGradientBrush(
            new Rectangle(0, 0, Width, 4),
            colors.GradientStart,
            colors.GradientEnd,
            LinearGradientMode.Horizontal))
        {
            g.FillRectangle(accentBrush, 0, 0, Width, 4);
        }

        // Draw progress bar
        DrawProgressBar(g, colors);

        // Draw logo glow effect
        DrawLogoGlow(g, colors);
    }

    private void DrawGlowingBorder(Graphics g, ThemeColors colors)
    {
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        // Animated glow
        var glowIntensity = (int)(50 + 30 * Math.Sin(_glowAngle * Math.PI / 180));
        using var glowPen = new Pen(Color.FromArgb(glowIntensity, colors.Accent), 2);
        g.DrawRectangle(glowPen, rect);

        // Inner border
        using var borderPen = new Pen(colors.Border, 1);
        g.DrawRectangle(borderPen, new Rectangle(1, 1, Width - 3, Height - 3));
    }

    private void DrawProgressBar(Graphics g, ThemeColors colors)
    {
        var barHeight = 3;
        var barY = Height - barHeight - 10;
        var barX = 20;
        var barWidth = Width - 40;

        // Background
        using (var bgBrush = new SolidBrush(colors.BackgroundTertiary))
        {
            g.FillRectangle(bgBrush, barX, barY, barWidth, barHeight);
        }

        // Progress fill with gradient
        var fillWidth = (int)(barWidth * (_progressValue / 100f));
        if (fillWidth > 0)
        {
            using var fillBrush = new LinearGradientBrush(
                new Rectangle(barX, barY, fillWidth, barHeight),
                colors.GradientStart,
                colors.AccentSecondary,
                LinearGradientMode.Horizontal);
            g.FillRectangle(fillBrush, barX, barY, fillWidth, barHeight);

            // Glow on progress tip
            if (fillWidth > 5)
            {
                var glowRect = new Rectangle(barX + fillWidth - 10, barY - 2, 15, barHeight + 4);
                using var glowBrush = new LinearGradientBrush(
                    glowRect,
                    Color.FromArgb(0, colors.Accent),
                    Color.FromArgb(100, colors.Accent),
                    LinearGradientMode.Horizontal);
                g.FillRectangle(glowBrush, glowRect);
            }
        }
    }

    private void DrawLogoGlow(Graphics g, ThemeColors colors)
    {
        // Glow behind the icon
        var iconCenter = new Point(PB_Icon.Left + PB_Icon.Width / 2, PB_Icon.Top + PB_Icon.Height / 2);
        var glowRadius = 40 + (int)(5 * Math.Sin(_glowAngle * Math.PI / 180));

        using var path = new GraphicsPath();
        path.AddEllipse(iconCenter.X - glowRadius, iconCenter.Y - glowRadius, glowRadius * 2, glowRadius * 2);

        using var glowBrush = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(60, colors.Accent),
            SurroundColors = new[] { Color.FromArgb(0, colors.Accent) }
        };
        g.FillPath(glowBrush, path);
    }

    private bool CanClose;

    private void SplashScreen_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!CanClose)
            e.Cancel = true;
    }

    public void ForceClose()
    {
        _animationTimer?.Stop();
        _animationTimer?.Dispose();
        _progressTimer?.Stop();
        _progressTimer?.Dispose();
        CanClose = true;
        Close();
    }

    public void SetStatus(string status)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetStatus(status));
            return;
        }
        L_Status.Text = status;
    }

    public void SetProgress(int value)
    {
        _progressValue = Math.Clamp(value, 0, 100);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            // Add drop shadow
            const int CS_DROPSHADOW = 0x00020000;
            var cp = base.CreateParams;
            cp.ClassStyle |= CS_DROPSHADOW;
            return cp;
        }
    }
}

/// <summary>
/// Simple particle for visual effects
/// </summary>
internal class Particle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Size { get; set; }
    public float Speed { get; set; }
    public float Alpha { get; set; }

    private readonly Random _random;
    private readonly int _maxWidth;

    public Particle(Random random, int maxWidth, int maxHeight)
    {
        _random = random;
        _maxWidth = maxWidth;
        Reset(maxHeight);
        Y = random.Next(0, maxHeight); // Start at random height initially
    }

    public void Reset(int maxHeight)
    {
        X = _random.Next(0, _maxWidth);
        Y = maxHeight + 10;
        Size = _random.Next(2, 6);
        Speed = (float)(_random.NextDouble() * 1.5 + 0.5);
        Alpha = (float)(_random.NextDouble() * 0.3 + 0.1);
    }

    public void Update(int maxHeight)
    {
        Y -= Speed;
        if (Y < -10)
            Reset(maxHeight);
    }
}
