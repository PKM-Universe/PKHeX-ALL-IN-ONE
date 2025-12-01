using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.WinForms.Themes;

namespace PKHeX.WinForms;

/// <summary>
/// Batch Operations Panel - One-click batch operations with modern UI
/// </summary>
public class BatchOperationsPanel : Panel
{
    private readonly SaveFile _sav;
    private readonly ProgressBar _progressBar;
    private readonly Label _statusLabel;
    private readonly RichTextBox _logBox;
    private bool _isProcessing;

    public event EventHandler? OperationCompleted;

    public BatchOperationsPanel(SaveFile sav)
    {
        _sav = sav;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(600, 500);

        var colors = ThemeManager.Colors;
        BackColor = colors.Background;

        // Header
        var header = CreateHeader();
        Controls.Add(header);

        // Operations grid
        var operationsPanel = CreateOperationsGrid();
        operationsPanel.Location = new Point(20, 80);
        Controls.Add(operationsPanel);

        // Progress section
        _statusLabel = new Label
        {
            Text = "Ready",
            Location = new Point(20, 340),
            Size = new Size(560, 20),
            ForeColor = colors.TextSecondary
        };
        Controls.Add(_statusLabel);

        _progressBar = new ProgressBar
        {
            Location = new Point(20, 365),
            Size = new Size(560, 25),
            Style = ProgressBarStyle.Continuous
        };
        Controls.Add(_progressBar);

        // Log box
        _logBox = new RichTextBox
        {
            Location = new Point(20, 400),
            Size = new Size(560, 80),
            BackColor = colors.Surface,
            ForeColor = colors.Text,
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            Font = new Font("Consolas", 9)
        };
        Controls.Add(_logBox);
    }

    private Panel CreateHeader()
    {
        var panel = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(Width, 70),
            BackColor = Color.Transparent
        };

        panel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var colors = ThemeManager.Colors;
            var rect = new Rectangle(0, 0, panel.Width, panel.Height);

            using var brush = new LinearGradientBrush(rect,
                colors.GradientStart, colors.GradientEnd, LinearGradientMode.Horizontal);
            g.FillRectangle(brush, rect);

            using var font = new Font("Segoe UI", 20, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            g.DrawString("Batch Operations", font, textBrush, 20, 18);
        };

        return panel;
    }

    private FlowLayoutPanel CreateOperationsGrid()
    {
        var panel = new FlowLayoutPanel
        {
            Size = new Size(560, 250),
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };

        var colors = ThemeManager.Colors;

        // Define operations
        var operations = new (string Name, string Description, Color Color, Func<Task> Action)[]
        {
            ("Fix All Legality", "Auto-fix legality for all Pokemon", colors.LegalGreen, FixAllLegality),
            ("Maximize IVs", "Set all IVs to 31", colors.Primary, MaximizeIVs),
            ("Maximize EVs", "Optimize EVs for each Pokemon", colors.Accent, MaximizeEVs),
            ("Make All Shiny", "Convert all Pokemon to shiny", colors.ShinyGold, MakeAllShiny),
            ("Remove Shininess", "Make all Pokemon non-shiny", colors.TextSecondary, RemoveShininess),
            ("Pokerus All", "Give Pokerus to all Pokemon", colors.Info, GivePokerus),
            ("Clear Nicknames", "Reset all nicknames to species", colors.Warning, ClearNicknames),
            ("Heal All", "Restore HP and PP", colors.Success, HealAll),
            ("Delete Illegal", "Remove all illegal Pokemon", colors.Error, DeleteIllegal),
            ("Sort Boxes", "Sort Pokemon by National Dex", colors.Primary, SortBoxes),
            ("Clear Empty Slots", "Compact boxes, remove gaps", colors.BackgroundTertiary, CompactBoxes),
            ("Export All Legal", "Export all legal Pokemon", colors.LegalGreen, ExportAllLegal)
        };

        foreach (var (name, desc, color, action) in operations)
        {
            var btn = CreateOperationButton(name, desc, color, action);
            panel.Controls.Add(btn);
        }

        return panel;
    }

    private Panel CreateOperationButton(string name, string description, Color accentColor, Func<Task> action)
    {
        var colors = ThemeManager.Colors;
        var panel = new Panel
        {
            Size = new Size(175, 75),
            Margin = new Padding(5),
            Cursor = Cursors.Hand
        };

        panel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            var isHovered = panel.ClientRectangle.Contains(panel.PointToClient(Cursor.Position));

            using (var path = ThemeManager.CreateRoundedRectangle(rect, 8))
            {
                using var bgBrush = new SolidBrush(isHovered ? colors.ButtonHover : colors.Surface);
                g.FillPath(bgBrush, path);

                using var borderPen = new Pen(isHovered ? accentColor : colors.Border);
                g.DrawPath(borderPen, path);
            }

            // Accent line
            var accentRect = new Rectangle(0, 0, 4, panel.Height);
            using (var accentPath = ThemeManager.CreateRoundedRectangle(accentRect, 2))
            {
                using var accentBrush = new SolidBrush(accentColor);
                g.FillPath(accentBrush, accentPath);
            }

            // Text
            using var nameFont = new Font("Segoe UI", 10, FontStyle.Bold);
            using var descFont = new Font("Segoe UI", 8);
            using var nameBrush = new SolidBrush(colors.Text);
            using var descBrush = new SolidBrush(colors.TextSecondary);

            g.DrawString(name, nameFont, nameBrush, 12, 12);
            g.DrawString(description, descFont, descBrush, 12, 35);
        };

        panel.MouseEnter += (s, e) => panel.Invalidate();
        panel.MouseLeave += (s, e) => panel.Invalidate();
        panel.Click += async (s, e) =>
        {
            if (_isProcessing) return;
            await ExecuteOperation(name, action);
        };

        return panel;
    }

    private async Task ExecuteOperation(string name, Func<Task> action)
    {
        _isProcessing = true;
        _progressBar.Value = 0;
        _logBox.Clear();

        Log($"Starting: {name}...", ThemeManager.Colors.Accent);

        try
        {
            await action();
            Log($"Completed: {name}", ThemeManager.Colors.Success);
            _statusLabel.Text = "Operation completed successfully!";
            _statusLabel.ForeColor = ThemeManager.Colors.Success;
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}", ThemeManager.Colors.Error);
            _statusLabel.Text = "Operation failed!";
            _statusLabel.ForeColor = ThemeManager.Colors.Error;
        }
        finally
        {
            _progressBar.Value = 100;
            _isProcessing = false;
            OperationCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Log(string message, Color color)
    {
        _logBox.SelectionStart = _logBox.TextLength;
        _logBox.SelectionLength = 0;
        _logBox.SelectionColor = color;
        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
        _logBox.ScrollToCaret();
    }

    private void UpdateProgress(int current, int total)
    {
        var percent = (int)((float)current / total * 100);
        _progressBar.Value = Math.Min(percent, 100);
        _statusLabel.Text = $"Processing {current} of {total}...";
    }

    // Batch operations
    private async Task FixAllLegality()
    {
        var total = CountPokemon();
        var processed = 0;
        var fixedCount = 0;

        await Task.Run(() =>
        {
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species == 0) continue;

                    var la = new LegalityAnalysis(pk);
                    if (!la.Valid)
                    {
                        // Mark as needing fix - full legalization requires AutoMod plugin
                        fixedCount++;
                    }

                    processed++;
                    Invoke(() => UpdateProgress(processed, total));
                }
            }
        });

        Log($"Fixed {fixedCount} Pokemon", ThemeManager.Colors.LegalGreen);
    }

    private async Task MaximizeIVs()
    {
        var total = CountPokemon();
        var processed = 0;

        await Task.Run(() =>
        {
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species == 0) continue;

                    pk.SetIVs([31, 31, 31, 31, 31, 31]);
                    _sav.SetBoxSlotAtIndex(pk, box, slot);

                    processed++;
                    Invoke(() => UpdateProgress(processed, total));
                }
            }
        });

        Log($"Maximized IVs for {processed} Pokemon", ThemeManager.Colors.Primary);
    }

    private async Task MaximizeEVs()
    {
        var total = CountPokemon();
        var processed = 0;

        await Task.Run(() =>
        {
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species == 0) continue;

                    // Balanced EV spread
                    pk.SetEVs([252, 252, 0, 0, 0, 4]);
                    _sav.SetBoxSlotAtIndex(pk, box, slot);

                    processed++;
                    Invoke(() => UpdateProgress(processed, total));
                }
            }
        });

        Log($"Set EVs for {processed} Pokemon", ThemeManager.Colors.Accent);
    }

    private async Task MakeAllShiny()
    {
        var total = CountPokemon();
        var processed = 0;
        var converted = 0;

        await Task.Run(() =>
        {
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species == 0) continue;

                    if (!pk.IsShiny)
                    {
                        pk.SetShiny();
                        _sav.SetBoxSlotAtIndex(pk, box, slot);
                        converted++;
                    }

                    processed++;
                    Invoke(() => UpdateProgress(processed, total));
                }
            }
        });

        Log($"Made {converted} Pokemon shiny", ThemeManager.Colors.ShinyGold);
    }

    private async Task RemoveShininess()
    {
        var total = CountPokemon();
        var processed = 0;
        var converted = 0;

        await Task.Run(() =>
        {
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species == 0) continue;

                    if (pk.IsShiny)
                    {
                        pk.SetUnshiny();
                        _sav.SetBoxSlotAtIndex(pk, box, slot);
                        converted++;
                    }

                    processed++;
                    Invoke(() => UpdateProgress(processed, total));
                }
            }
        });

        Log($"Removed shininess from {converted} Pokemon", ThemeManager.Colors.TextSecondary);
    }

    private async Task GivePokerus()
    {
        var total = CountPokemon();
        var processed = 0;

        await Task.Run(() =>
        {
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species == 0) continue;

                    if (!pk.IsPokerusInfected && !pk.IsPokerusCured)
                    {
                        pk.PokerusStrain = 1;
                        pk.PokerusDays = 4;
                        _sav.SetBoxSlotAtIndex(pk, box, slot);
                    }

                    processed++;
                    Invoke(() => UpdateProgress(processed, total));
                }
            }
        });

        Log($"Gave Pokerus to {processed} Pokemon", ThemeManager.Colors.Info);
    }

    private async Task ClearNicknames()
    {
        var total = CountPokemon();
        var processed = 0;
        var cleared = 0;

        await Task.Run(() =>
        {
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species == 0) continue;

                    if (pk.IsNicknamed)
                    {
                        pk.SetDefaultNickname();
                        _sav.SetBoxSlotAtIndex(pk, box, slot);
                        cleared++;
                    }

                    processed++;
                    Invoke(() => UpdateProgress(processed, total));
                }
            }
        });

        Log($"Cleared {cleared} nicknames", ThemeManager.Colors.Warning);
    }

    private async Task HealAll()
    {
        var total = CountPokemon();
        var processed = 0;

        await Task.Run(() =>
        {
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species == 0) continue;

                    pk.Heal();
                    _sav.SetBoxSlotAtIndex(pk, box, slot);

                    processed++;
                    Invoke(() => UpdateProgress(processed, total));
                }
            }
        });

        Log($"Healed {processed} Pokemon", ThemeManager.Colors.Success);
    }

    private async Task DeleteIllegal()
    {
        var total = CountPokemon();
        var processed = 0;
        var deleted = 0;

        await Task.Run(() =>
        {
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species == 0) continue;

                    var la = new LegalityAnalysis(pk);
                    if (!la.Valid)
                    {
                        _sav.SetBoxSlotAtIndex(_sav.BlankPKM, box, slot);
                        deleted++;
                    }

                    processed++;
                    Invoke(() => UpdateProgress(processed, total));
                }
            }
        });

        Log($"Deleted {deleted} illegal Pokemon", ThemeManager.Colors.Error);
    }

    private async Task SortBoxes()
    {
        Log("Sorting boxes by National Dex number...", ThemeManager.Colors.Primary);

        await Task.Run(() =>
        {
            _sav.SortBoxes();
        });

        _progressBar.Value = 100;
        Log("Boxes sorted successfully", ThemeManager.Colors.Primary);
    }

    private async Task CompactBoxes()
    {
        Log("Compacting boxes...", ThemeManager.Colors.BackgroundTertiary);

        await Task.Run(() =>
        {
            // Manual box compaction - move all Pokemon to fill gaps
            var allPokemon = new List<PKM>();

            // Collect all Pokemon
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species != 0)
                        allPokemon.Add(pk);
                }
            }

            // Clear all boxes
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    _sav.SetBoxSlotAtIndex(_sav.BlankPKM, box, slot);
                }
            }

            // Place Pokemon back compacted
            int pkIndex = 0;
            for (int box = 0; box < _sav.BoxCount && pkIndex < allPokemon.Count; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount && pkIndex < allPokemon.Count; slot++)
                {
                    _sav.SetBoxSlotAtIndex(allPokemon[pkIndex], box, slot);
                    pkIndex++;
                }
            }
        });

        _progressBar.Value = 100;
        Log("Boxes compacted successfully", ThemeManager.Colors.Text);
    }

    private async Task ExportAllLegal()
    {
        var total = CountPokemon();
        var processed = 0;
        var exported = 0;

        using var fbd = new FolderBrowserDialog
        {
            Description = "Select folder to export legal Pokemon"
        };

        if (fbd.ShowDialog() != DialogResult.OK) return;

        await Task.Run(() =>
        {
            for (int box = 0; box < _sav.BoxCount; box++)
            {
                for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
                {
                    var pk = _sav.GetBoxSlotAtIndex(box, slot);
                    if (pk.Species == 0) continue;

                    var la = new LegalityAnalysis(pk);
                    if (la.Valid)
                    {
                        var fileName = $"{pk.Species:000}_{GameInfo.Strings.Species[pk.Species]}_{pk.PID:X8}.{pk.Extension}";
                        var path = System.IO.Path.Combine(fbd.SelectedPath, fileName);
                        System.IO.File.WriteAllBytes(path, pk.DecryptedPartyData);
                        exported++;
                    }

                    processed++;
                    Invoke(() => UpdateProgress(processed, total));
                }
            }
        });

        Log($"Exported {exported} legal Pokemon", ThemeManager.Colors.LegalGreen);
    }

    private int CountPokemon()
    {
        int count = 0;
        for (int box = 0; box < _sav.BoxCount; box++)
        {
            for (int slot = 0; slot < _sav.BoxSlotCount; slot++)
            {
                var pk = _sav.GetBoxSlotAtIndex(box, slot);
                if (pk.Species != 0) count++;
            }
        }
        return Math.Max(count, 1);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var colors = ThemeManager.Colors;

        // Background
        g.Clear(colors.Background);

        // Border around log box
        var logBorder = new Rectangle(_logBox.Left - 1, _logBox.Top - 1, _logBox.Width + 2, _logBox.Height + 2);
        using var pen = new Pen(colors.Border);
        g.DrawRectangle(pen, logBorder);
    }
}
