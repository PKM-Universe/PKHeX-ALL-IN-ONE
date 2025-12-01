using System;
using System.Drawing;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class DamageCalculator : Form
{
    private readonly ComboBox CB_AtkSpecies, CB_DefSpecies, CB_Move;
    private readonly NumericUpDown NUD_AtkLevel, NUD_DefLevel, NUD_AtkStat, NUD_DefStat, NUD_Power;
    private readonly CheckBox CHK_Stab, CHK_Crit, CHK_SuperEffective;
    private readonly Label L_Result;

    public DamageCalculator()
    {
        Text = "Damage Calculator";
        Size = new Size(500, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblAttacker = new Label { Text = "ATTACKER", Location = new Point(20, 15), AutoSize = true, ForeColor = Color.FromArgb(255, 100, 100), Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
        var lblAtkSpecies = new Label { Text = "Species:", Location = new Point(20, 45), AutoSize = true, ForeColor = Color.White };
        CB_AtkSpecies = new ComboBox { Location = new Point(100, 42), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        
        var lblAtkLevel = new Label { Text = "Level:", Location = new Point(20, 75), AutoSize = true, ForeColor = Color.White };
        NUD_AtkLevel = new NumericUpDown { Location = new Point(100, 72), Width = 60, Minimum = 1, Maximum = 100, Value = 50, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        
        var lblAtkStat = new Label { Text = "Atk/SpA:", Location = new Point(20, 105), AutoSize = true, ForeColor = Color.White };
        NUD_AtkStat = new NumericUpDown { Location = new Point(100, 102), Width = 80, Minimum = 1, Maximum = 999, Value = 100, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };

        var lblDefender = new Label { Text = "DEFENDER", Location = new Point(20, 145), AutoSize = true, ForeColor = Color.FromArgb(100, 100, 255), Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
        var lblDefSpecies = new Label { Text = "Species:", Location = new Point(20, 175), AutoSize = true, ForeColor = Color.White };
        CB_DefSpecies = new ComboBox { Location = new Point(100, 172), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        
        var lblDefLevel = new Label { Text = "Level:", Location = new Point(20, 205), AutoSize = true, ForeColor = Color.White };
        NUD_DefLevel = new NumericUpDown { Location = new Point(100, 202), Width = 60, Minimum = 1, Maximum = 100, Value = 50, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        
        var lblDefStat = new Label { Text = "Def/SpD:", Location = new Point(20, 235), AutoSize = true, ForeColor = Color.White };
        NUD_DefStat = new NumericUpDown { Location = new Point(100, 232), Width = 80, Minimum = 1, Maximum = 999, Value = 100, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };

        var lblMove = new Label { Text = "MOVE", Location = new Point(270, 15), AutoSize = true, ForeColor = Color.FromArgb(255, 200, 100), Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
        var lblMoveName = new Label { Text = "Move:", Location = new Point(270, 45), AutoSize = true, ForeColor = Color.White };
        CB_Move = new ComboBox { Location = new Point(320, 42), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        
        var lblPower = new Label { Text = "Power:", Location = new Point(270, 75), AutoSize = true, ForeColor = Color.White };
        NUD_Power = new NumericUpDown { Location = new Point(320, 72), Width = 60, Minimum = 0, Maximum = 250, Value = 80, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };

        CHK_Stab = new CheckBox { Text = "STAB (1.5x)", Location = new Point(270, 105), AutoSize = true, ForeColor = Color.LightGreen };
        CHK_Crit = new CheckBox { Text = "Critical Hit (1.5x)", Location = new Point(270, 130), AutoSize = true, ForeColor = Color.Gold };
        CHK_SuperEffective = new CheckBox { Text = "Super Effective (2x)", Location = new Point(270, 155), AutoSize = true, ForeColor = Color.Orange };

        var btnCalculate = new Button { Text = "Calculate Damage", Location = new Point(150, 280), Size = new Size(180, 45), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(80, 50, 120), ForeColor = Color.White, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
        btnCalculate.Click += (s, e) => Calculate();

        L_Result = new Label { Location = new Point(20, 340), Size = new Size(440, 100), ForeColor = Color.White, Font = new Font("Segoe UI", 12F), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.FromArgb(35, 35, 55) };

        for (int i = 1; i <= 1025; i++)
            if (i < GameInfo.Strings.specieslist.Length)
            {
                CB_AtkSpecies.Items.Add(GameInfo.Strings.specieslist[i]);
                CB_DefSpecies.Items.Add(GameInfo.Strings.specieslist[i]);
            }
        for (int i = 1; i < GameInfo.Strings.movelist.Length && i < 1000; i++)
            CB_Move.Items.Add(GameInfo.Strings.movelist[i]);

        if (CB_AtkSpecies.Items.Count > 0) CB_AtkSpecies.SelectedIndex = 0;
        if (CB_DefSpecies.Items.Count > 0) CB_DefSpecies.SelectedIndex = 0;
        if (CB_Move.Items.Count > 0) CB_Move.SelectedIndex = 0;

        Controls.AddRange(new Control[] { lblAttacker, lblAtkSpecies, CB_AtkSpecies, lblAtkLevel, NUD_AtkLevel, lblAtkStat, NUD_AtkStat, lblDefender, lblDefSpecies, CB_DefSpecies, lblDefLevel, NUD_DefLevel, lblDefStat, NUD_DefStat, lblMove, lblMoveName, CB_Move, lblPower, NUD_Power, CHK_Stab, CHK_Crit, CHK_SuperEffective, btnCalculate, L_Result });
    }

    private void Calculate()
    {
        int level = (int)NUD_AtkLevel.Value;
        int attack = (int)NUD_AtkStat.Value;
        int defense = (int)NUD_DefStat.Value;
        int power = (int)NUD_Power.Value;

        double baseDamage = ((2.0 * level / 5 + 2) * power * attack / defense) / 50 + 2;
        double modifier = 1.0;
        if (CHK_Stab.Checked) modifier *= 1.5;
        if (CHK_Crit.Checked) modifier *= 1.5;
        if (CHK_SuperEffective.Checked) modifier *= 2.0;

        int minDamage = (int)(baseDamage * modifier * 0.85);
        int maxDamage = (int)(baseDamage * modifier);
        int avgDamage = (minDamage + maxDamage) / 2;

        int defenderHP = 200;
        double minPercent = (minDamage * 100.0) / defenderHP;
        double maxPercent = (maxDamage * 100.0) / defenderHP;

        L_Result.Text = $"Damage: {minDamage} - {maxDamage}\n({minPercent:F1}% - {maxPercent:F1}% of ~200 HP)\n\nAverage: {avgDamage} damage";
    }
}
