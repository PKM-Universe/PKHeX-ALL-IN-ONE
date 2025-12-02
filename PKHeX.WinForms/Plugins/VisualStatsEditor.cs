using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Visual IV/EV Graph Editor - Interactive radar chart for stat editing
/// </summary>
public class VisualStatsEditor : Form
{
    private readonly SaveFile SAV;
    private PKM? CurrentPokemon;

    // UI Controls
    private readonly Panel radarPanel;
    private readonly NumericUpDown[] ivNuds = new NumericUpDown[6];
    private readonly NumericUpDown[] evNuds = new NumericUpDown[6];
    private readonly Label[] statLabels = new Label[6];
    private readonly TrackBar[] ivSliders = new TrackBar[6];
    private readonly TrackBar[] evSliders = new TrackBar[6];
    private readonly Label lblTotalEVs;
    private readonly Label lblIVTotal;
    private readonly Label lblHiddenPower;
    private readonly ComboBox cmbPresets;
    private readonly Button btnMaxIVs;
    private readonly Button btnMaxEVs;
    private readonly Button btnMinimize;
    private readonly Button btnBalanced;
    private readonly CheckBox chkShowIVs;
    private readonly CheckBox chkShowEVs;
    private readonly CheckBox chkShowBase;

    // Stat names
    private static readonly string[] StatNames = { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };

    // Colors
    private static readonly Color IVColor = Color.FromArgb(150, 66, 133, 244);
    private static readonly Color EVColor = Color.FromArgb(150, 219, 68, 55);
    private static readonly Color BaseColor = Color.FromArgb(100, 128, 128, 128);
    private static readonly Color GridColor = Color.FromArgb(60, 200, 200, 200);

    // EV Presets
    private static readonly Dictionary<string, int[]> EVPresets = new()
    {
        { "Sweeper (Physical)", new[] { 0, 252, 0, 0, 4, 252 } },
        { "Sweeper (Special)", new[] { 0, 0, 0, 252, 4, 252 } },
        { "Bulky Physical", new[] { 252, 0, 252, 0, 4, 0 } },
        { "Bulky Special", new[] { 252, 0, 4, 0, 252, 0 } },
        { "Mixed Attacker", new[] { 0, 128, 0, 128, 0, 252 } },
        { "Defensive Wall", new[] { 252, 0, 128, 0, 128, 0 } },
        { "Tank (Physical)", new[] { 252, 252, 0, 0, 4, 0 } },
        { "Tank (Special)", new[] { 252, 0, 0, 252, 4, 0 } },
        { "Speed Control", new[] { 252, 0, 0, 0, 4, 252 } },
        { "Balanced", new[] { 84, 84, 84, 84, 84, 84 } }
    };

    public VisualStatsEditor(SaveFile sav)
    {
        SAV = sav;

        Text = "Visual Stats Editor - PKM-Universe";
        Size = new Size(900, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(30, 30, 35);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9f);

        // Radar chart panel
        radarPanel = new Panel
        {
            Location = new Point(20, 20),
            Size = new Size(400, 400),
            BackColor = Color.FromArgb(40, 40, 45)
        };
        radarPanel.Paint += RadarPanel_Paint;
        Controls.Add(radarPanel);

        // Display options
        chkShowIVs = new CheckBox
        {
            Text = "Show IVs",
            Location = new Point(20, 430),
            Checked = true,
            ForeColor = IVColor,
            AutoSize = true
        };
        chkShowIVs.CheckedChanged += (s, e) => radarPanel.Invalidate();
        Controls.Add(chkShowIVs);

        chkShowEVs = new CheckBox
        {
            Text = "Show EVs",
            Location = new Point(120, 430),
            Checked = true,
            ForeColor = EVColor,
            AutoSize = true
        };
        chkShowEVs.CheckedChanged += (s, e) => radarPanel.Invalidate();
        Controls.Add(chkShowEVs);

        chkShowBase = new CheckBox
        {
            Text = "Show Base Stats",
            Location = new Point(220, 430),
            Checked = false,
            ForeColor = BaseColor,
            AutoSize = true
        };
        chkShowBase.CheckedChanged += (s, e) => radarPanel.Invalidate();
        Controls.Add(chkShowBase);

        // Stats info
        lblIVTotal = new Label
        {
            Location = new Point(20, 460),
            Size = new Size(180, 20),
            Text = "IV Total: 0/186",
            ForeColor = IVColor
        };
        Controls.Add(lblIVTotal);

        lblTotalEVs = new Label
        {
            Location = new Point(200, 460),
            Size = new Size(180, 20),
            Text = "EV Total: 0/510",
            ForeColor = EVColor
        };
        Controls.Add(lblTotalEVs);

        lblHiddenPower = new Label
        {
            Location = new Point(20, 485),
            Size = new Size(200, 20),
            Text = "Hidden Power: ---"
        };
        Controls.Add(lblHiddenPower);

        // Right panel - Stat controls
        var rightPanel = new Panel
        {
            Location = new Point(440, 20),
            Size = new Size(440, 600),
            BackColor = Color.FromArgb(35, 35, 40)
        };
        Controls.Add(rightPanel);

        // Create stat editors
        for (int i = 0; i < 6; i++)
        {
            int y = 20 + i * 70;
            int statIndex = i;

            // Stat name label
            statLabels[i] = new Label
            {
                Text = StatNames[i],
                Location = new Point(10, y),
                Size = new Size(40, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            rightPanel.Controls.Add(statLabels[i]);

            // IV controls
            var ivLabel = new Label
            {
                Text = "IV:",
                Location = new Point(60, y),
                Size = new Size(25, 20),
                ForeColor = IVColor
            };
            rightPanel.Controls.Add(ivLabel);

            ivNuds[i] = new NumericUpDown
            {
                Location = new Point(85, y - 2),
                Size = new Size(50, 24),
                Minimum = 0,
                Maximum = 31,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White
            };
            ivNuds[i].ValueChanged += (s, e) => UpdateFromNud(statIndex, true);
            rightPanel.Controls.Add(ivNuds[i]);

            ivSliders[i] = new TrackBar
            {
                Location = new Point(140, y - 5),
                Size = new Size(120, 30),
                Minimum = 0,
                Maximum = 31,
                TickFrequency = 5
            };
            ivSliders[i].ValueChanged += (s, e) => UpdateFromSlider(statIndex, true);
            rightPanel.Controls.Add(ivSliders[i]);

            // EV controls
            var evLabel = new Label
            {
                Text = "EV:",
                Location = new Point(60, y + 28),
                Size = new Size(25, 20),
                ForeColor = EVColor
            };
            rightPanel.Controls.Add(evLabel);

            evNuds[i] = new NumericUpDown
            {
                Location = new Point(85, y + 26),
                Size = new Size(50, 24),
                Minimum = 0,
                Maximum = 252,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White
            };
            evNuds[i].ValueChanged += (s, e) => UpdateFromNud(statIndex, false);
            rightPanel.Controls.Add(evNuds[i]);

            evSliders[i] = new TrackBar
            {
                Location = new Point(140, y + 23),
                Size = new Size(120, 30),
                Minimum = 0,
                Maximum = 252,
                TickFrequency = 42
            };
            evSliders[i].ValueChanged += (s, e) => UpdateFromSlider(statIndex, false);
            rightPanel.Controls.Add(evSliders[i]);

            // Final stat display
            var finalLabel = new Label
            {
                Location = new Point(280, y + 10),
                Size = new Size(80, 40),
                Text = "---",
                ForeColor = Color.LightGreen,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            rightPanel.Controls.Add(finalLabel);
        }

        // Preset combo
        var presetLabel = new Label
        {
            Text = "EV Preset:",
            Location = new Point(10, 450),
            Size = new Size(70, 20),
            ForeColor = Color.White
        };
        rightPanel.Controls.Add(presetLabel);

        cmbPresets = new ComboBox
        {
            Location = new Point(80, 447),
            Size = new Size(200, 24),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(50, 50, 55),
            ForeColor = Color.White
        };
        cmbPresets.Items.Add("-- Select Preset --");
        foreach (var preset in EVPresets.Keys)
            cmbPresets.Items.Add(preset);
        cmbPresets.SelectedIndex = 0;
        cmbPresets.SelectedIndexChanged += CmbPresets_SelectedIndexChanged;
        rightPanel.Controls.Add(cmbPresets);

        // Quick buttons
        btnMaxIVs = new Button
        {
            Text = "Max IVs (31)",
            Location = new Point(10, 490),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(60, 60, 65),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnMaxIVs.Click += BtnMaxIVs_Click;
        rightPanel.Controls.Add(btnMaxIVs);

        btnMaxEVs = new Button
        {
            Text = "Max EVs (252)",
            Location = new Point(115, 490),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(60, 60, 65),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnMaxEVs.Click += BtnMaxEVs_Click;
        rightPanel.Controls.Add(btnMaxEVs);

        btnMinimize = new Button
        {
            Text = "Zero EVs",
            Location = new Point(220, 490),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(60, 60, 65),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnMinimize.Click += BtnMinimize_Click;
        rightPanel.Controls.Add(btnMinimize);

        btnBalanced = new Button
        {
            Text = "Balanced EVs",
            Location = new Point(10, 530),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(60, 60, 65),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnBalanced.Click += BtnBalanced_Click;
        rightPanel.Controls.Add(btnBalanced);

        // Apply button
        var btnApply = new Button
        {
            Text = "Apply to Pokemon",
            Location = new Point(115, 530),
            Size = new Size(150, 30),
            BackColor = Color.FromArgb(66, 133, 244),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        btnApply.Click += BtnApply_Click;
        rightPanel.Controls.Add(btnApply);

        // Instructions
        var instructions = new Label
        {
            Text = "Load a Pokemon from your save to edit its IVs and EVs visually.\n" +
                   "The radar chart shows stat distribution at a glance.",
            Location = new Point(20, 520),
            Size = new Size(400, 50),
            ForeColor = Color.Gray
        };
        Controls.Add(instructions);
    }

    /// <summary>
    /// Load a Pokemon for editing
    /// </summary>
    public void LoadPokemon(PKM pk)
    {
        CurrentPokemon = pk;

        // Load IVs
        ivNuds[0].Value = pk.IV_HP;
        ivNuds[1].Value = pk.IV_ATK;
        ivNuds[2].Value = pk.IV_DEF;
        ivNuds[3].Value = pk.IV_SPA;
        ivNuds[4].Value = pk.IV_SPD;
        ivNuds[5].Value = pk.IV_SPE;

        // Load EVs
        evNuds[0].Value = pk.EV_HP;
        evNuds[1].Value = pk.EV_ATK;
        evNuds[2].Value = pk.EV_DEF;
        evNuds[3].Value = pk.EV_SPA;
        evNuds[4].Value = pk.EV_SPD;
        evNuds[5].Value = pk.EV_SPE;

        // Sync sliders
        for (int i = 0; i < 6; i++)
        {
            ivSliders[i].Value = (int)ivNuds[i].Value;
            evSliders[i].Value = (int)evNuds[i].Value;
        }

        UpdateStats();
        radarPanel.Invalidate();
    }

    private void UpdateFromNud(int statIndex, bool isIV)
    {
        if (isIV)
            ivSliders[statIndex].Value = (int)ivNuds[statIndex].Value;
        else
            evSliders[statIndex].Value = Math.Min((int)evNuds[statIndex].Value, 252);

        UpdateStats();
        radarPanel.Invalidate();
    }

    private void UpdateFromSlider(int statIndex, bool isIV)
    {
        if (isIV)
            ivNuds[statIndex].Value = ivSliders[statIndex].Value;
        else
            evNuds[statIndex].Value = evSliders[statIndex].Value;

        UpdateStats();
        radarPanel.Invalidate();
    }

    private void UpdateStats()
    {
        // Calculate totals
        int ivTotal = 0;
        int evTotal = 0;

        for (int i = 0; i < 6; i++)
        {
            ivTotal += (int)ivNuds[i].Value;
            evTotal += (int)evNuds[i].Value;
        }

        lblIVTotal.Text = $"IV Total: {ivTotal}/186";
        lblTotalEVs.Text = $"EV Total: {evTotal}/510";

        // Color code EV total
        if (evTotal > 510)
            lblTotalEVs.ForeColor = Color.Red;
        else if (evTotal == 510)
            lblTotalEVs.ForeColor = Color.LightGreen;
        else
            lblTotalEVs.ForeColor = EVColor;

        // Calculate Hidden Power type
        var hpType = CalculateHiddenPowerType();
        lblHiddenPower.Text = $"Hidden Power: {hpType}";
    }

    private string CalculateHiddenPowerType()
    {
        int a = (int)ivNuds[0].Value % 2;
        int b = (int)ivNuds[1].Value % 2;
        int c = (int)ivNuds[2].Value % 2;
        int d = (int)ivNuds[5].Value % 2;
        int e = (int)ivNuds[3].Value % 2;
        int f = (int)ivNuds[4].Value % 2;

        int typeNum = ((a + 2 * b + 4 * c + 8 * d + 16 * e + 32 * f) * 15) / 63;

        string[] types = { "Fighting", "Flying", "Poison", "Ground", "Rock", "Bug",
                          "Ghost", "Steel", "Fire", "Water", "Grass", "Electric",
                          "Psychic", "Ice", "Dragon", "Dark" };

        return types[typeNum];
    }

    private void RadarPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int centerX = radarPanel.Width / 2;
        int centerY = radarPanel.Height / 2;
        int radius = Math.Min(centerX, centerY) - 40;

        // Draw grid
        for (int level = 1; level <= 5; level++)
        {
            float levelRadius = radius * level / 5f;
            var gridPath = new GraphicsPath();

            for (int i = 0; i < 6; i++)
            {
                double angle = (i * 60 - 90) * Math.PI / 180;
                float x = centerX + (float)(levelRadius * Math.Cos(angle));
                float y = centerY + (float)(levelRadius * Math.Sin(angle));

                if (i == 0)
                    gridPath.StartFigure();

                if (gridPath.PointCount > 0)
                    gridPath.AddLine(gridPath.GetLastPoint(), new PointF(x, y));
                else
                    gridPath.AddLine(new PointF(x, y), new PointF(x, y));
            }
            gridPath.CloseFigure();

            using var gridPen = new Pen(GridColor, 1);
            g.DrawPath(gridPen, gridPath);
        }

        // Draw axis lines and labels
        using var axisPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1);
        using var labelBrush = new SolidBrush(Color.White);
        using var labelFont = new Font("Segoe UI", 10f, FontStyle.Bold);

        for (int i = 0; i < 6; i++)
        {
            double angle = (i * 60 - 90) * Math.PI / 180;
            float x = centerX + (float)(radius * Math.Cos(angle));
            float y = centerY + (float)(radius * Math.Sin(angle));

            g.DrawLine(axisPen, centerX, centerY, x, y);

            // Label
            float labelX = centerX + (float)((radius + 25) * Math.Cos(angle));
            float labelY = centerY + (float)((radius + 25) * Math.Sin(angle));

            var size = g.MeasureString(StatNames[i], labelFont);
            g.DrawString(StatNames[i], labelFont, labelBrush,
                labelX - size.Width / 2, labelY - size.Height / 2);
        }

        // Draw IV polygon
        if (chkShowIVs.Checked)
        {
            var ivPath = new GraphicsPath();
            for (int i = 0; i < 6; i++)
            {
                double angle = (i * 60 - 90) * Math.PI / 180;
                float value = (float)ivNuds[i].Value / 31f;
                float x = centerX + (float)(radius * value * Math.Cos(angle));
                float y = centerY + (float)(radius * value * Math.Sin(angle));

                if (i == 0)
                    ivPath.StartFigure();

                if (ivPath.PointCount > 0)
                    ivPath.AddLine(ivPath.GetLastPoint(), new PointF(x, y));
                else
                    ivPath.AddLine(new PointF(x, y), new PointF(x, y));
            }
            ivPath.CloseFigure();

            using var ivBrush = new SolidBrush(IVColor);
            using var ivPen = new Pen(Color.FromArgb(200, 66, 133, 244), 2);
            g.FillPath(ivBrush, ivPath);
            g.DrawPath(ivPen, ivPath);
        }

        // Draw EV polygon
        if (chkShowEVs.Checked)
        {
            var evPath = new GraphicsPath();
            for (int i = 0; i < 6; i++)
            {
                double angle = (i * 60 - 90) * Math.PI / 180;
                float value = (float)evNuds[i].Value / 252f;
                float x = centerX + (float)(radius * value * Math.Cos(angle));
                float y = centerY + (float)(radius * value * Math.Sin(angle));

                if (i == 0)
                    evPath.StartFigure();

                if (evPath.PointCount > 0)
                    evPath.AddLine(evPath.GetLastPoint(), new PointF(x, y));
                else
                    evPath.AddLine(new PointF(x, y), new PointF(x, y));
            }
            evPath.CloseFigure();

            using var evBrush = new SolidBrush(EVColor);
            using var evPen = new Pen(Color.FromArgb(200, 219, 68, 55), 2);
            g.FillPath(evBrush, evPath);
            g.DrawPath(evPen, evPath);
        }

        // Draw base stats polygon (if Pokemon loaded)
        if (chkShowBase.Checked && CurrentPokemon != null)
        {
            var pi = CurrentPokemon.PersonalInfo;
            int[] baseStats = { pi.HP, pi.ATK, pi.DEF, pi.SPA, pi.SPD, pi.SPE };
            int maxBase = 255; // Reasonable max for display

            var basePath = new GraphicsPath();
            for (int i = 0; i < 6; i++)
            {
                double angle = (i * 60 - 90) * Math.PI / 180;
                float value = (float)baseStats[i] / maxBase;
                float x = centerX + (float)(radius * value * Math.Cos(angle));
                float y = centerY + (float)(radius * value * Math.Sin(angle));

                if (i == 0)
                    basePath.StartFigure();

                if (basePath.PointCount > 0)
                    basePath.AddLine(basePath.GetLastPoint(), new PointF(x, y));
                else
                    basePath.AddLine(new PointF(x, y), new PointF(x, y));
            }
            basePath.CloseFigure();

            using var basePen = new Pen(BaseColor, 2) { DashStyle = DashStyle.Dash };
            g.DrawPath(basePen, basePath);
        }

        // Draw stat values in center
        for (int i = 0; i < 6; i++)
        {
            double angle = (i * 60 - 90) * Math.PI / 180;
            float labelRadius = radius * 0.5f;
            float x = centerX + (float)(labelRadius * Math.Cos(angle));
            float y = centerY + (float)(labelRadius * Math.Sin(angle));

            string ivText = ivNuds[i].Value.ToString();
            string evText = evNuds[i].Value.ToString();

            using var smallFont = new Font("Segoe UI", 8f);
            using var ivBrush = new SolidBrush(Color.FromArgb(200, 100, 150, 255));
            using var evBrush = new SolidBrush(Color.FromArgb(200, 255, 100, 100));

            g.DrawString($"IV:{ivText}", smallFont, ivBrush, x - 15, y - 8);
            g.DrawString($"EV:{evText}", smallFont, evBrush, x - 15, y + 4);
        }
    }

    private void CmbPresets_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbPresets.SelectedIndex <= 0) return;

        string preset = cmbPresets.SelectedItem?.ToString() ?? "";
        if (EVPresets.TryGetValue(preset, out int[]? evs))
        {
            for (int i = 0; i < 6; i++)
            {
                evNuds[i].Value = evs[i];
                evSliders[i].Value = evs[i];
            }
            UpdateStats();
            radarPanel.Invalidate();
        }
    }

    private void BtnMaxIVs_Click(object? sender, EventArgs e)
    {
        for (int i = 0; i < 6; i++)
        {
            ivNuds[i].Value = 31;
            ivSliders[i].Value = 31;
        }
        UpdateStats();
        radarPanel.Invalidate();
    }

    private void BtnMaxEVs_Click(object? sender, EventArgs e)
    {
        // Can't exceed 510 total, so set 252/252/4 pattern
        evNuds[0].Value = 4;
        evNuds[1].Value = 252;
        evNuds[2].Value = 0;
        evNuds[3].Value = 0;
        evNuds[4].Value = 0;
        evNuds[5].Value = 252;

        for (int i = 0; i < 6; i++)
            evSliders[i].Value = (int)evNuds[i].Value;

        UpdateStats();
        radarPanel.Invalidate();
    }

    private void BtnMinimize_Click(object? sender, EventArgs e)
    {
        for (int i = 0; i < 6; i++)
        {
            evNuds[i].Value = 0;
            evSliders[i].Value = 0;
        }
        UpdateStats();
        radarPanel.Invalidate();
    }

    private void BtnBalanced_Click(object? sender, EventArgs e)
    {
        // 510 / 6 = 85 per stat
        for (int i = 0; i < 6; i++)
        {
            evNuds[i].Value = 85;
            evSliders[i].Value = 85;
        }
        UpdateStats();
        radarPanel.Invalidate();
    }

    private void BtnApply_Click(object? sender, EventArgs e)
    {
        if (CurrentPokemon == null)
        {
            MessageBox.Show("No Pokemon loaded!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Check EV total
        int evTotal = 0;
        for (int i = 0; i < 6; i++)
            evTotal += (int)evNuds[i].Value;

        if (evTotal > 510)
        {
            MessageBox.Show($"EV total ({evTotal}) exceeds maximum of 510!", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Apply IVs
        CurrentPokemon.IV_HP = (int)ivNuds[0].Value;
        CurrentPokemon.IV_ATK = (int)ivNuds[1].Value;
        CurrentPokemon.IV_DEF = (int)ivNuds[2].Value;
        CurrentPokemon.IV_SPA = (int)ivNuds[3].Value;
        CurrentPokemon.IV_SPD = (int)ivNuds[4].Value;
        CurrentPokemon.IV_SPE = (int)ivNuds[5].Value;

        // Apply EVs
        CurrentPokemon.EV_HP = (int)evNuds[0].Value;
        CurrentPokemon.EV_ATK = (int)evNuds[1].Value;
        CurrentPokemon.EV_DEF = (int)evNuds[2].Value;
        CurrentPokemon.EV_SPA = (int)evNuds[3].Value;
        CurrentPokemon.EV_SPD = (int)evNuds[4].Value;
        CurrentPokemon.EV_SPE = (int)evNuds[5].Value;

        MessageBox.Show("Stats applied successfully!", "Success",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// Generate recommended EV spread based on base stats
    /// </summary>
    public int[] GenerateOptimalSpread(PKM pk, bool physical = true)
    {
        var pi = pk.PersonalInfo;
        int[] evs = new int[6];

        // Determine role based on base stats
        bool isFast = pi.SPE >= 100;
        bool isBulky = pi.HP >= 90 || pi.DEF >= 100 || pi.SPD >= 100;
        bool isPhysical = pi.ATK > pi.SPA;

        if (isFast)
        {
            evs[5] = 252; // Speed
            if (physical)
                evs[1] = 252; // Attack
            else
                evs[3] = 252; // Sp. Attack
            evs[0] = 4; // HP
        }
        else if (isBulky)
        {
            evs[0] = 252; // HP
            if (pi.DEF > pi.SPD)
                evs[2] = 252; // Defense
            else
                evs[4] = 252; // Sp. Defense
            evs[5] = 4; // Speed for tie-breaking
        }
        else
        {
            // Balanced
            evs[0] = 4;
            if (physical)
                evs[1] = 252;
            else
                evs[3] = 252;
            evs[5] = 252;
        }

        return evs;
    }

    /// <summary>
    /// Get IVs optimized for Hidden Power type
    /// </summary>
    public int[] GetHiddenPowerIVs(string type)
    {
        var hpIVs = new Dictionary<string, int[]>
        {
            { "Fire", new[] { 31, 30, 31, 30, 31, 30 } },
            { "Water", new[] { 31, 30, 30, 30, 31, 31 } },
            { "Grass", new[] { 31, 30, 31, 30, 31, 30 } },
            { "Electric", new[] { 31, 31, 31, 30, 31, 31 } },
            { "Ice", new[] { 31, 30, 30, 31, 31, 31 } },
            { "Fighting", new[] { 31, 31, 30, 30, 30, 30 } },
            { "Ground", new[] { 31, 31, 31, 30, 30, 31 } },
            { "Flying", new[] { 30, 30, 30, 30, 30, 31 } },
            { "Psychic", new[] { 31, 30, 31, 31, 31, 30 } },
            { "Bug", new[] { 31, 30, 31, 30, 30, 31 } },
            { "Rock", new[] { 31, 31, 30, 31, 30, 30 } },
            { "Ghost", new[] { 31, 30, 31, 31, 30, 31 } },
            { "Dragon", new[] { 31, 30, 31, 31, 31, 31 } },
            { "Dark", new[] { 31, 31, 31, 31, 31, 31 } },
            { "Steel", new[] { 31, 31, 31, 31, 30, 31 } },
            { "Poison", new[] { 31, 31, 30, 30, 30, 31 } }
        };

        return hpIVs.TryGetValue(type, out int[]? ivs) ? ivs : new[] { 31, 31, 31, 31, 31, 31 };
    }
}
