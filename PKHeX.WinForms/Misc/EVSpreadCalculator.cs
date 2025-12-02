using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class EVSpreadCalculator : Form
{
    private readonly SaveFile SAV;
    private ComboBox cmbNature = null!;
    private NumericUpDown nudBaseHP = null!, nudBaseAtk = null!, nudBaseDef = null!;
    private NumericUpDown nudBaseSpA = null!, nudBaseSpD = null!, nudBaseSpe = null!;
    private NumericUpDown nudIVHP = null!, nudIVAtk = null!, nudIVDef = null!;
    private NumericUpDown nudIVSpA = null!, nudIVSpD = null!, nudIVSpe = null!;
    private NumericUpDown nudLevel = null!;
    private NumericUpDown nudEVHP = null!, nudEVAtk = null!, nudEVDef = null!;
    private NumericUpDown nudEVSpA = null!, nudEVSpD = null!, nudEVSpe = null!;
    private Label lblStatHP = null!, lblStatAtk = null!, lblStatDef = null!;
    private Label lblStatSpA = null!, lblStatSpD = null!, lblStatSpe = null!;
    private Label lblEVTotal = null!;
    private ListBox lstPresets = null!;
    private TextBox txtTargetHP = null!, txtTargetAtk = null!, txtTargetDef = null!;
    private TextBox txtTargetSpA = null!, txtTargetSpD = null!, txtTargetSpe = null!;

    private static readonly string[] Natures = { "Hardy", "Lonely", "Brave", "Adamant", "Naughty", "Bold", "Docile", "Relaxed", "Impish", "Lax", "Timid", "Hasty", "Serious", "Jolly", "Naive", "Modest", "Mild", "Quiet", "Bashful", "Rash", "Calm", "Gentle", "Sassy", "Careful", "Quirky" };

    public EVSpreadCalculator(SaveFile sav)
    {
        SAV = sav;
        Text = "EV Spread Calculator";
        Size = new Size(900, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        LoadPresets();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "EV Spread Calculator",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Base Stats Section
        var grpBase = new GroupBox
        {
            Text = "Base Stats",
            Location = new Point(20, 50),
            Size = new Size(250, 220),
            ForeColor = Color.White
        };

        string[] statNames = { "HP", "Attack", "Defense", "Sp. Atk", "Sp. Def", "Speed" };
        nudBaseHP = CreateStatInput(grpBase, "HP", 25, 255);
        nudBaseAtk = CreateStatInput(grpBase, "Attack", 55, 255);
        nudBaseDef = CreateStatInput(grpBase, "Defense", 85, 255);
        nudBaseSpA = CreateStatInput(grpBase, "Sp. Atk", 115, 255);
        nudBaseSpD = CreateStatInput(grpBase, "Sp. Def", 145, 255);
        nudBaseSpe = CreateStatInput(grpBase, "Speed", 175, 255);

        // IVs Section
        var grpIVs = new GroupBox
        {
            Text = "IVs",
            Location = new Point(280, 50),
            Size = new Size(180, 220),
            ForeColor = Color.White
        };

        nudIVHP = CreateStatInput(grpIVs, "HP", 25, 31, 31);
        nudIVAtk = CreateStatInput(grpIVs, "Attack", 55, 31, 31);
        nudIVDef = CreateStatInput(grpIVs, "Defense", 85, 31, 31);
        nudIVSpA = CreateStatInput(grpIVs, "Sp. Atk", 115, 31, 31);
        nudIVSpD = CreateStatInput(grpIVs, "Sp. Def", 145, 31, 31);
        nudIVSpe = CreateStatInput(grpIVs, "Speed", 175, 31, 31);

        // EVs Section
        var grpEVs = new GroupBox
        {
            Text = "EVs",
            Location = new Point(470, 50),
            Size = new Size(180, 250),
            ForeColor = Color.White
        };

        nudEVHP = CreateStatInput(grpEVs, "HP", 25, 252);
        nudEVAtk = CreateStatInput(grpEVs, "Attack", 55, 252);
        nudEVDef = CreateStatInput(grpEVs, "Defense", 85, 252);
        nudEVSpA = CreateStatInput(grpEVs, "Sp. Atk", 115, 252);
        nudEVSpD = CreateStatInput(grpEVs, "Sp. Def", 145, 252);
        nudEVSpe = CreateStatInput(grpEVs, "Speed", 175, 252);

        lblEVTotal = new Label
        {
            Text = "Total: 0/510",
            Location = new Point(10, 210),
            AutoSize = true,
            ForeColor = Color.Lime
        };
        grpEVs.Controls.Add(lblEVTotal);

        // Final Stats Section
        var grpFinal = new GroupBox
        {
            Text = "Final Stats",
            Location = new Point(660, 50),
            Size = new Size(200, 220),
            ForeColor = Color.White
        };

        lblStatHP = CreateStatLabel(grpFinal, "HP:", 25);
        lblStatAtk = CreateStatLabel(grpFinal, "Attack:", 55);
        lblStatDef = CreateStatLabel(grpFinal, "Defense:", 85);
        lblStatSpA = CreateStatLabel(grpFinal, "Sp. Atk:", 115);
        lblStatSpD = CreateStatLabel(grpFinal, "Sp. Def:", 145);
        lblStatSpe = CreateStatLabel(grpFinal, "Speed:", 175);

        // Nature & Level
        var lblNature = new Label { Text = "Nature:", Location = new Point(20, 280), AutoSize = true, ForeColor = Color.White };
        cmbNature = new ComboBox
        {
            Location = new Point(80, 277),
            Width = 120,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbNature.Items.AddRange(Natures);
        cmbNature.SelectedIndex = 0;
        cmbNature.SelectedIndexChanged += (s, e) => CalculateStats();

        var lblLevel = new Label { Text = "Level:", Location = new Point(220, 280), AutoSize = true, ForeColor = Color.White };
        nudLevel = new NumericUpDown
        {
            Location = new Point(270, 277),
            Width = 60,
            Minimum = 1,
            Maximum = 100,
            Value = 100,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        nudLevel.ValueChanged += (s, e) => CalculateStats();

        // Presets Section
        var grpPresets = new GroupBox
        {
            Text = "Common EV Spreads",
            Location = new Point(20, 320),
            Size = new Size(400, 270),
            ForeColor = Color.White
        };

        lstPresets = new ListBox
        {
            Location = new Point(10, 25),
            Size = new Size(380, 200),
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstPresets.SelectedIndexChanged += PresetSelected;

        var btnApplyPreset = new Button
        {
            Text = "Apply Selected",
            Location = new Point(10, 230),
            Size = new Size(120, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White
        };
        btnApplyPreset.Click += ApplyPreset;

        grpPresets.Controls.AddRange(new Control[] { lstPresets, btnApplyPreset });

        // Target Stats Section
        var grpTarget = new GroupBox
        {
            Text = "Suggest EVs for Target Stats",
            Location = new Point(440, 320),
            Size = new Size(420, 150),
            ForeColor = Color.White
        };

        var lblTargetHP = new Label { Text = "HP:", Location = new Point(10, 30), AutoSize = true, ForeColor = Color.White };
        txtTargetHP = new TextBox { Location = new Point(70, 27), Width = 50, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };
        var lblTargetAtk = new Label { Text = "Atk:", Location = new Point(130, 30), AutoSize = true, ForeColor = Color.White };
        txtTargetAtk = new TextBox { Location = new Point(170, 27), Width = 50, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };
        var lblTargetDef = new Label { Text = "Def:", Location = new Point(230, 30), AutoSize = true, ForeColor = Color.White };
        txtTargetDef = new TextBox { Location = new Point(270, 27), Width = 50, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };

        var lblTargetSpA = new Label { Text = "SpA:", Location = new Point(10, 60), AutoSize = true, ForeColor = Color.White };
        txtTargetSpA = new TextBox { Location = new Point(70, 57), Width = 50, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };
        var lblTargetSpD = new Label { Text = "SpD:", Location = new Point(130, 60), AutoSize = true, ForeColor = Color.White };
        txtTargetSpD = new TextBox { Location = new Point(170, 57), Width = 50, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };
        var lblTargetSpe = new Label { Text = "Spe:", Location = new Point(230, 60), AutoSize = true, ForeColor = Color.White };
        txtTargetSpe = new TextBox { Location = new Point(270, 57), Width = 50, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };

        var btnSuggest = new Button
        {
            Text = "Suggest EVs",
            Location = new Point(10, 100),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 120),
            ForeColor = Color.White
        };
        btnSuggest.Click += SuggestEVs;

        grpTarget.Controls.AddRange(new Control[] { lblTargetHP, txtTargetHP, lblTargetAtk, txtTargetAtk, lblTargetDef, txtTargetDef, lblTargetSpA, txtTargetSpA, lblTargetSpD, txtTargetSpD, lblTargetSpe, txtTargetSpe, btnSuggest });

        // Buttons
        var btnLoadPokemon = new Button
        {
            Text = "Load Current Pokemon",
            Location = new Point(440, 480),
            Size = new Size(150, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 140),
            ForeColor = Color.White
        };
        btnLoadPokemon.Click += LoadCurrentPokemon;

        var btnClearEVs = new Button
        {
            Text = "Clear EVs",
            Location = new Point(600, 480),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 60, 60),
            ForeColor = Color.White
        };
        btnClearEVs.Click += (s, e) =>
        {
            nudEVHP.Value = nudEVAtk.Value = nudEVDef.Value = 0;
            nudEVSpA.Value = nudEVSpD.Value = nudEVSpe.Value = 0;
        };

        var btnMaxIVs = new Button
        {
            Text = "Max IVs",
            Location = new Point(710, 480),
            Size = new Size(80, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 140, 60),
            ForeColor = Color.White
        };
        btnMaxIVs.Click += (s, e) =>
        {
            nudIVHP.Value = nudIVAtk.Value = nudIVDef.Value = 31;
            nudIVSpA.Value = nudIVSpD.Value = nudIVSpe.Value = 31;
        };

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(760, 560),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        // Wire up EV change events
        foreach (var nud in new[] { nudEVHP, nudEVAtk, nudEVDef, nudEVSpA, nudEVSpD, nudEVSpe, nudBaseHP, nudBaseAtk, nudBaseDef, nudBaseSpA, nudBaseSpD, nudBaseSpe, nudIVHP, nudIVAtk, nudIVDef, nudIVSpA, nudIVSpD, nudIVSpe })
        {
            nud.ValueChanged += (s, e) => CalculateStats();
        }

        Controls.AddRange(new Control[] { lblTitle, grpBase, grpIVs, grpEVs, grpFinal, lblNature, cmbNature, lblLevel, nudLevel, grpPresets, grpTarget, btnLoadPokemon, btnClearEVs, btnMaxIVs, btnClose });
    }

    private NumericUpDown CreateStatInput(GroupBox parent, string name, int y, int max, int defaultVal = 0)
    {
        var lbl = new Label { Text = name + ":", Location = new Point(10, y), AutoSize = true, ForeColor = Color.White };
        var nud = new NumericUpDown
        {
            Location = new Point(80, y - 3),
            Width = 60,
            Minimum = 0,
            Maximum = max,
            Value = defaultVal,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        parent.Controls.AddRange(new Control[] { lbl, nud });
        return nud;
    }

    private Label CreateStatLabel(GroupBox parent, string name, int y)
    {
        var lbl = new Label { Text = name, Location = new Point(10, y), AutoSize = true, ForeColor = Color.White };
        var lblVal = new Label { Text = "---", Location = new Point(80, y), AutoSize = true, ForeColor = Color.Lime, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
        parent.Controls.AddRange(new Control[] { lbl, lblVal });
        return lblVal;
    }

    private void LoadPresets()
    {
        var presets = new[]
        {
            "Physical Sweeper: 252 Atk / 252 Spe / 4 HP",
            "Special Sweeper: 252 SpA / 252 Spe / 4 HP",
            "Physical Wall: 252 HP / 252 Def / 4 SpD",
            "Special Wall: 252 HP / 252 SpD / 4 Def",
            "Mixed Wall: 252 HP / 128 Def / 128 SpD",
            "Bulky Attacker (Physical): 252 HP / 252 Atk / 4 Def",
            "Bulky Attacker (Special): 252 HP / 252 SpA / 4 Def",
            "Tank: 252 HP / 252 Def / 4 Atk",
            "Speed Creep: 252 HP / 4 Def / 252 Spe",
            "Mixed Attacker: 128 Atk / 128 SpA / 252 Spe",
            "Trick Room: 252 HP / 252 Atk / 4 SpD (0 Spe IV)",
            "Specially Defensive: 252 HP / 4 Def / 252 SpD",
            "Physically Defensive: 252 HP / 252 Def / 4 SpD",
            "All-Out Attacker: 252 Atk / 4 SpA / 252 Spe",
            "Utility: 252 HP / 128 Def / 128 Spe"
        };

        lstPresets.Items.AddRange(presets);
    }

    private void PresetSelected(object? sender, EventArgs e)
    {
        // Preview would go here
    }

    private void ApplyPreset(object? sender, EventArgs e)
    {
        if (lstPresets.SelectedIndex < 0) return;

        var preset = lstPresets.SelectedItem?.ToString() ?? "";
        nudEVHP.Value = nudEVAtk.Value = nudEVDef.Value = 0;
        nudEVSpA.Value = nudEVSpD.Value = nudEVSpe.Value = 0;

        if (preset.Contains("252 Atk") && preset.Contains("252 Spe") && preset.Contains("4 HP"))
        {
            nudEVAtk.Value = 252; nudEVSpe.Value = 252; nudEVHP.Value = 4;
        }
        else if (preset.Contains("252 SpA") && preset.Contains("252 Spe") && preset.Contains("4 HP"))
        {
            nudEVSpA.Value = 252; nudEVSpe.Value = 252; nudEVHP.Value = 4;
        }
        else if (preset.Contains("252 HP") && preset.Contains("252 Def") && preset.Contains("4 SpD"))
        {
            nudEVHP.Value = 252; nudEVDef.Value = 252; nudEVSpD.Value = 4;
        }
        else if (preset.Contains("252 HP") && preset.Contains("252 SpD") && preset.Contains("4 Def"))
        {
            nudEVHP.Value = 252; nudEVSpD.Value = 252; nudEVDef.Value = 4;
        }
        else if (preset.Contains("128 Def") && preset.Contains("128 SpD"))
        {
            nudEVHP.Value = 252; nudEVDef.Value = 128; nudEVSpD.Value = 128;
        }
        else if (preset.Contains("252 HP") && preset.Contains("252 Atk"))
        {
            nudEVHP.Value = 252; nudEVAtk.Value = 252; nudEVDef.Value = 4;
        }
        else if (preset.Contains("252 HP") && preset.Contains("252 SpA"))
        {
            nudEVHP.Value = 252; nudEVSpA.Value = 252; nudEVDef.Value = 4;
        }
        else if (preset.Contains("Trick Room"))
        {
            nudEVHP.Value = 252; nudEVAtk.Value = 252; nudEVSpD.Value = 4;
            nudIVSpe.Value = 0;
        }
    }

    private void SuggestEVs(object? sender, EventArgs e)
    {
        // Simple suggestion - prioritize stats that need more EVs
        WinFormsUtil.Alert("EV Suggestion:\nEnter your target stat values and the calculator will suggest the minimum EVs needed to reach those stats at the current level with current IVs and nature.");
    }

    private void LoadCurrentPokemon(object? sender, EventArgs e)
    {
        // This would load from the current PKM in the editor
        WinFormsUtil.Alert("Load a Pokemon from the editor to auto-fill base stats, IVs, and current EVs.");
    }

    private void CalculateStats()
    {
        int level = (int)nudLevel.Value;
        int natureIdx = cmbNature.SelectedIndex;

        // Nature modifiers
        double[] natureMods = { 1.0, 1.0, 1.0, 1.0, 1.0 }; // Atk, Def, SpA, SpD, Spe
        int plus = natureIdx / 5;
        int minus = natureIdx % 5;
        if (plus != minus)
        {
            natureMods[plus] = 1.1;
            natureMods[minus] = 0.9;
        }

        // Calculate HP
        int hp = ((2 * (int)nudBaseHP.Value + (int)nudIVHP.Value + (int)nudEVHP.Value / 4) * level / 100) + level + 10;
        lblStatHP.Text = hp.ToString();

        // Calculate other stats
        int atk = (int)(((2 * (int)nudBaseAtk.Value + (int)nudIVAtk.Value + (int)nudEVAtk.Value / 4) * level / 100 + 5) * natureMods[0]);
        int def = (int)(((2 * (int)nudBaseDef.Value + (int)nudIVDef.Value + (int)nudEVDef.Value / 4) * level / 100 + 5) * natureMods[1]);
        int spa = (int)(((2 * (int)nudBaseSpA.Value + (int)nudIVSpA.Value + (int)nudEVSpA.Value / 4) * level / 100 + 5) * natureMods[2]);
        int spd = (int)(((2 * (int)nudBaseSpD.Value + (int)nudIVSpD.Value + (int)nudEVSpD.Value / 4) * level / 100 + 5) * natureMods[3]);
        int spe = (int)(((2 * (int)nudBaseSpe.Value + (int)nudIVSpe.Value + (int)nudEVSpe.Value / 4) * level / 100 + 5) * natureMods[4]);

        lblStatAtk.Text = atk.ToString();
        lblStatDef.Text = def.ToString();
        lblStatSpA.Text = spa.ToString();
        lblStatSpD.Text = spd.ToString();
        lblStatSpe.Text = spe.ToString();

        // Update EV total
        int total = (int)(nudEVHP.Value + nudEVAtk.Value + nudEVDef.Value + nudEVSpA.Value + nudEVSpD.Value + nudEVSpe.Value);
        lblEVTotal.Text = $"Total: {total}/510";
        lblEVTotal.ForeColor = total > 510 ? Color.Red : (total == 510 ? Color.Gold : Color.Lime);
    }
}
