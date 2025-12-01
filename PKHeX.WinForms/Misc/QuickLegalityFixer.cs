using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.WinForms.Plugins;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms;

/// <summary>
/// Quick Legality Fixer - One-click batch legality fixing with modern UI
/// </summary>
public partial class QuickLegalityFixer : Form
{
    private readonly SaveFile SAV;
    private readonly AutoLegalityPlugin _plugin;
    private bool _isProcessing;

    public QuickLegalityFixer(SaveFile sav)
    {
        SAV = sav;
        _plugin = new AutoLegalityPlugin(sav);
        InitializeComponent();
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        ApplyTheme();
        PopulateBoxList();
    }

    private void ApplyTheme()
    {
        var colors = ThemeManager.Colors;
        BackColor = colors.Background;
        ForeColor = colors.Text;

        PNL_Header.BackColor = Color.Transparent;
        PNL_Content.BackColor = colors.BackgroundSecondary;
        PNL_Footer.BackColor = colors.BackgroundTertiary;

        L_Title.ForeColor = Color.White;
        L_Description.ForeColor = colors.TextSecondary;
        L_Status.ForeColor = colors.TextSecondary;
        L_Stats.ForeColor = colors.Text;

        // Style buttons
        ThemeManager.StylePrimaryButton(BTN_FixCurrentBox);
        ThemeManager.StylePrimaryButton(BTN_FixAllBoxes);
        ThemeManager.StyleButton(BTN_FixCurrentPokemon);
        ThemeManager.StyleButton(BTN_Close);

        // ComboBox
        CB_BoxSelect.BackColor = colors.InputBackground;
        CB_BoxSelect.ForeColor = colors.Text;
        CB_BoxSelect.FlatStyle = FlatStyle.Flat;

        // Progress bar will be custom painted
        PB_Progress.BackColor = colors.BackgroundTertiary;

        // Results box
        RTB_Results.BackColor = colors.Surface;
        RTB_Results.ForeColor = colors.Text;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var colors = ThemeManager.Colors;

        // Draw header gradient
        var headerRect = new Rectangle(0, 0, Width, 80);
        using (var brush = new LinearGradientBrush(
            headerRect,
            colors.GradientStart,
            colors.GradientEnd,
            LinearGradientMode.Horizontal))
        {
            g.FillRectangle(brush, headerRect);
        }

        // Decorative pattern
        using (var pen = new Pen(Color.FromArgb(15, 255, 255, 255)))
        {
            for (int i = 0; i < Width + 80; i += 15)
            {
                g.DrawLine(pen, i, 0, i - 80, 80);
            }
        }
    }

    private void PopulateBoxList()
    {
        CB_BoxSelect.Items.Clear();
        for (int i = 0; i < SAV.BoxCount; i++)
        {
            string boxName = SAV is IBoxDetailName bn ? bn.GetBoxName(i) : $"Box {i + 1}";
            CB_BoxSelect.Items.Add($"Box {i + 1}: {boxName}");
        }
        if (CB_BoxSelect.Items.Count > 0)
            CB_BoxSelect.SelectedIndex = 0;
    }

    private async void BTN_FixCurrentPokemon_Click(object sender, EventArgs e)
    {
        if (_isProcessing) return;
        _isProcessing = true;
        UpdateUI(true);

        try
        {
            L_Status.Text = "Fixing current Pokemon...";

            await Task.Run(() =>
            {
                // This would need to get the current Pokemon from the editor
                // For now, show a message
            });

            AppendResult("Use the Fix Legality option from the Pokemon editor context menu.", ThemeManager.Colors.Info);
            L_Status.Text = "Ready";
        }
        finally
        {
            _isProcessing = false;
            UpdateUI(false);
        }
    }

    private async void BTN_FixCurrentBox_Click(object sender, EventArgs e)
    {
        if (_isProcessing) return;
        _isProcessing = true;
        UpdateUI(true);

        try
        {
            var box = CB_BoxSelect.SelectedIndex;
            if (box < 0) return;

            L_Status.Text = $"Fixing Box {box + 1}...";
            PB_Progress.Value = 0;
            RTB_Results.Clear();

            var result = await Task.Run(() => _plugin.FixBox(box));

            PB_Progress.Value = 100;
            DisplayResults(result, $"Box {box + 1}");
            L_Status.Text = "Complete!";
        }
        catch (Exception ex)
        {
            AppendResult($"Error: {ex.Message}", ThemeManager.Colors.Error);
            L_Status.Text = "Error occurred";
        }
        finally
        {
            _isProcessing = false;
            UpdateUI(false);
        }
    }

    private async void BTN_FixAllBoxes_Click(object sender, EventArgs e)
    {
        if (_isProcessing) return;

        var confirmResult = MessageBox.Show(
            "This will attempt to fix legality for ALL Pokemon in ALL boxes.\n\nContinue?",
            "Confirm Batch Fix",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmResult != DialogResult.Yes) return;

        _isProcessing = true;
        UpdateUI(true);

        try
        {
            L_Status.Text = "Fixing all boxes...";
            PB_Progress.Value = 0;
            RTB_Results.Clear();

            var totalResult = new BatchFixResult();
            int totalBoxes = SAV.BoxCount;

            for (int box = 0; box < totalBoxes; box++)
            {
                L_Status.Text = $"Fixing Box {box + 1} of {totalBoxes}...";
                PB_Progress.Value = (box * 100) / totalBoxes;

                var boxResult = await Task.Run(() => _plugin.FixBox(box));
                totalResult.TotalProcessed += boxResult.TotalProcessed;
                totalResult.AlreadyLegal += boxResult.AlreadyLegal;
                totalResult.Fixed += boxResult.Fixed;
                totalResult.CouldNotFix += boxResult.CouldNotFix;
            }

            PB_Progress.Value = 100;
            DisplayResults(totalResult, "All Boxes");
            L_Status.Text = "Complete!";
        }
        catch (Exception ex)
        {
            AppendResult($"Error: {ex.Message}", ThemeManager.Colors.Error);
            L_Status.Text = "Error occurred";
        }
        finally
        {
            _isProcessing = false;
            UpdateUI(false);
        }
    }

    private void DisplayResults(BatchFixResult result, string scope)
    {
        var colors = ThemeManager.Colors;

        AppendResult($"=== Legality Fix Results: {scope} ===\n", colors.Accent);
        AppendResult($"Total Processed: {result.TotalProcessed}\n", colors.Text);
        AppendResult($"Already Legal: {result.AlreadyLegal}\n", colors.LegalGreen);
        AppendResult($"Successfully Fixed: {result.Fixed}\n", colors.Success);

        if (result.CouldNotFix > 0)
            AppendResult($"Could Not Fix: {result.CouldNotFix}\n", colors.Warning);
        else
            AppendResult($"Could Not Fix: 0\n", colors.TextMuted);

        // Summary
        int successRate = result.TotalProcessed > 0
            ? ((result.AlreadyLegal + result.Fixed) * 100) / result.TotalProcessed
            : 0;

        AppendResult($"\nSuccess Rate: {successRate}%\n", successRate >= 80 ? colors.LegalGreen : colors.Warning);

        // Update stats label
        L_Stats.Text = $"Processed: {result.TotalProcessed} | Legal: {result.AlreadyLegal + result.Fixed} | Issues: {result.CouldNotFix}";
    }

    private void AppendResult(string text, Color color)
    {
        RTB_Results.SelectionStart = RTB_Results.TextLength;
        RTB_Results.SelectionLength = 0;
        RTB_Results.SelectionColor = color;
        RTB_Results.AppendText(text);
        RTB_Results.ScrollToCaret();
    }

    private void UpdateUI(bool processing)
    {
        BTN_FixCurrentPokemon.Enabled = !processing;
        BTN_FixCurrentBox.Enabled = !processing;
        BTN_FixAllBoxes.Enabled = !processing;
        CB_BoxSelect.Enabled = !processing;
        Cursor = processing ? Cursors.WaitCursor : Cursors.Default;
    }

    private void BTN_Close_Click(object sender, EventArgs e)
    {
        Close();
    }
}
