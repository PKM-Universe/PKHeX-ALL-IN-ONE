using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PKHeX.WinForms;

public class QuickStartTutorial : Form
{
    private int _currentStep = 0;
    private readonly Panel _contentPanel;
    private readonly Label _stepLabel;
    private readonly Button _btnNext;
    private readonly Button _btnPrev;
    private readonly Button _btnSkip;

    private readonly (string Title, string Content, string Tip)[] _steps = new[]
    {
        ("Welcome to PKM-Universe!", "PKM-Universe is your all-in-one Pokemon save editor. This quick tutorial will show you the basics.", "You can access this tutorial anytime from Help > Quick Start Tutorial"),
        ("Loading a Save File", "To get started:\n\n1. Go to File > Open\n2. Select your save file (.sav, .dsv, .bin, etc.)\n3. The save will load automatically\n\nYou can also drag and drop save files!", "PKM-Universe supports saves from all main Pokemon games."),
        ("Editing Pokemon", "Click any Pokemon in your boxes or party to load it into the editor.\n\nYou can modify:\n- Species, Level, Nature\n- IVs, EVs, Moves\n- Shiny status, Ball type\n- And much more!", "Use Ctrl+Click to quick-edit multiple Pokemon."),
        ("Legality Checking", "PKM-Universe checks if your Pokemon are legal.\n\n- Green checkmark = Legal\n- Red X = Illegal\n\nClick the legality icon for details.", "Use Tools > Auto-Legality Mod to automatically fix illegal Pokemon."),
        ("PKM-Universe Features", "Exclusive features in the PKM-Universe menu:\n\n- Shiny Living Dex Generator\n- Quick Competitive Builder\n- Box Wallpapers\n- Team Coverage Analyzer\n- And more!", "Check out all the tools under PKM-Universe menu!"),
        ("Saving Your Changes", "When you are done editing:\n\n1. Go to File > Export SAV\n2. Choose where to save\n3. Copy back to your device/emulator\n\nAlways keep backups!", "Enable Auto-Backup in settings for automatic save snapshots."),
        ("You are Ready!", "You now know the basics of PKM-Universe!\n\nExplore the menus and have fun editing your Pokemon!\n\nNeed help? Visit our Discord or check the documentation.", "Press F1 anytime for context-sensitive help.")
    };

    public QuickStartTutorial()
    {
        Text = "Quick Start Tutorial";
        Size = new Size(600, 450);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(25, 25, 40);

        _stepLabel = new Label { Location = new Point(20, 20), AutoSize = true, ForeColor = Color.FromArgb(138, 43, 226), Font = new Font("Segoe UI", 9F) };
        Controls.Add(_stepLabel);

        _contentPanel = new Panel { Location = new Point(20, 50), Size = new Size(540, 300), BackColor = Color.FromArgb(35, 35, 55) };
        _contentPanel.Paint += ContentPanel_Paint;
        Controls.Add(_contentPanel);

        _btnPrev = new Button { Text = "< Previous", Location = new Point(20, 370), Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 50, 70), ForeColor = Color.White };
        _btnPrev.Click += (s, e) => { if (_currentStep > 0) { _currentStep--; UpdateContent(); } };
        Controls.Add(_btnPrev);

        _btnNext = new Button { Text = "Next >", Location = new Point(360, 370), Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(100, 60, 140), ForeColor = Color.White };
        _btnNext.Click += (s, e) => { if (_currentStep < _steps.Length - 1) { _currentStep++; UpdateContent(); } else { Close(); } };
        Controls.Add(_btnNext);

        _btnSkip = new Button { Text = "Skip Tutorial", Location = new Point(470, 370), Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 50, 70), ForeColor = Color.Gray };
        _btnSkip.Click += (s, e) => Close();
        Controls.Add(_btnSkip);

        UpdateContent();
    }

    private void UpdateContent()
    {
        _stepLabel.Text = $"Step {_currentStep + 1} of {_steps.Length}";
        _btnPrev.Enabled = _currentStep > 0;
        _btnNext.Text = _currentStep == _steps.Length - 1 ? "Finish" : "Next >";
        _contentPanel.Invalidate();
    }

    private void ContentPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var step = _steps[_currentStep];

        using var titleFont = new Font("Segoe UI", 18F, FontStyle.Bold);
        using var contentFont = new Font("Segoe UI", 11F);
        using var tipFont = new Font("Segoe UI", 9F, FontStyle.Italic);

        g.DrawString(step.Title, titleFont, Brushes.White, 20, 20);
        g.DrawString(step.Content, contentFont, Brushes.LightGray, new RectangleF(20, 70, 500, 150));

        using var tipBrush = new SolidBrush(Color.FromArgb(100, 138, 43, 226));
        g.FillRectangle(tipBrush, 20, 230, 500, 50);
        g.DrawString("Tip: " + step.Tip, tipFont, Brushes.White, new RectangleF(30, 240, 480, 40));
    }
}
