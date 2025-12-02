using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public partial class IVEVBatchOptimizer : Form
{
    private readonly SaveFile SAV;
    private CheckedListBox clbPokemon = null!;
    private ComboBox cmbRole = null!;
    private ComboBox cmbOptimizationTarget = null!;
    private NumericUpDown nudMinIVs = null!;
    private NumericUpDown nudMinPerfectIVs = null!;
    private CheckBox chkPreserveShiny = null!;
    private CheckBox chkMaxEVs = null!;
    private CheckBox chkOptimalNature = null!;
    private ListView lstResults = null!;
    private RichTextBox rtbLog = null!;
    private ProgressBar pbProgress = null!;
    private Button btnOptimize = null!;
    private Button btnApply = null!;
    private Button btnSelectAll = null!;

    private List<OptimizationResult> results = new();

    // EV spread presets by role
    private static readonly Dictionary<string, int[]> EVPresets = new()
    {
        ["Physical Sweeper"] = new[] { 0, 252, 0, 0, 4, 252 },
        ["Special Sweeper"] = new[] { 0, 0, 0, 252, 4, 252 },
        ["Mixed Attacker"] = new[] { 0, 128, 0, 128, 0, 252 },
        ["Physical Wall"] = new[] { 252, 0, 252, 0, 4, 0 },
        ["Special Wall"] = new[] { 252, 0, 4, 0, 252, 0 },
        ["Balanced Wall"] = new[] { 252, 0, 128, 0, 128, 0 },
        ["Bulky Attacker"] = new[] { 252, 252, 0, 0, 4, 0 },
        ["Bulky Special"] = new[] { 252, 0, 0, 252, 4, 0 },
        ["Speed Control"] = new[] { 252, 0, 4, 0, 0, 252 },
        ["Tank"] = new[] { 252, 128, 64, 0, 64, 0 },
        ["Trick Room"] = new[] { 252, 252, 4, 0, 0, 0 }, // 0 Speed IVs
        ["Custom"] = new[] { 0, 0, 0, 0, 0, 0 }
    };

    // Optimal natures by role
    private static readonly Dictionary<string, Nature> OptimalNatures = new()
    {
        ["Physical Sweeper"] = Nature.Jolly,
        ["Special Sweeper"] = Nature.Timid,
        ["Mixed Attacker"] = Nature.Naive,
        ["Physical Wall"] = Nature.Impish,
        ["Special Wall"] = Nature.Calm,
        ["Balanced Wall"] = Nature.Bold,
        ["Bulky Attacker"] = Nature.Adamant,
        ["Bulky Special"] = Nature.Modest,
        ["Speed Control"] = Nature.Jolly,
        ["Tank"] = Nature.Adamant,
        ["Trick Room"] = Nature.Brave
    };

    public IVEVBatchOptimizer(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
        LoadPokemonList();
    }

    private void InitializeComponent()
    {
        Text = "IV/EV Batch Optimizer with Constraints";
        Size = new Size(1150, 750);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        // Pokemon Selection
        var grpSelection = new GroupBox
        {
            Text = "Select Pokemon to Optimize",
            Location = new Point(20, 20),
            Size = new Size(350, 350),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        clbPokemon = new CheckedListBox
        {
            Location = new Point(15, 25),
            Size = new Size(320, 280),
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            CheckOnClick = true
        };

        btnSelectAll = new Button
        {
            Text = "Select All",
            Location = new Point(15, 310),
            Size = new Size(100, 28),
            BackColor = Color.FromArgb(60, 60, 90),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSelectAll.Click += (s, e) =>
        {
            for (int i = 0; i < clbPokemon.Items.Count; i++)
                clbPokemon.SetItemChecked(i, true);
        };

        var btnDeselectAll = new Button
        {
            Text = "Deselect All",
            Location = new Point(125, 310),
            Size = new Size(100, 28),
            BackColor = Color.FromArgb(60, 60, 90),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnDeselectAll.Click += (s, e) =>
        {
            for (int i = 0; i < clbPokemon.Items.Count; i++)
                clbPokemon.SetItemChecked(i, false);
        };

        grpSelection.Controls.AddRange(new Control[] { clbPokemon, btnSelectAll, btnDeselectAll });

        // Optimization Settings
        var grpSettings = new GroupBox
        {
            Text = "Optimization Settings",
            Location = new Point(390, 20),
            Size = new Size(350, 350),
            ForeColor = Color.FromArgb(255, 200, 100)
        };

        var lblRole = new Label
        {
            Text = "Target Role:",
            Location = new Point(15, 30),
            Size = new Size(100, 25),
            ForeColor = Color.White
        };

        cmbRole = new ComboBox
        {
            Location = new Point(120, 27),
            Size = new Size(210, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbRole.Items.AddRange(EVPresets.Keys.ToArray());
        cmbRole.SelectedIndex = 0;
        cmbRole.SelectedIndexChanged += CmbRole_SelectedIndexChanged;

        var lblTarget = new Label
        {
            Text = "Optimize For:",
            Location = new Point(15, 65),
            Size = new Size(100, 25),
            ForeColor = Color.White
        };

        cmbOptimizationTarget = new ComboBox
        {
            Location = new Point(120, 62),
            Size = new Size(210, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbOptimizationTarget.Items.AddRange(new[] { "Competitive (Max Stats)", "Speed Tier", "Bulk Optimization", "Damage Output", "Balanced", "Trick Room" });
        cmbOptimizationTarget.SelectedIndex = 0;

        var lblMinIVs = new Label
        {
            Text = "Minimum IV Value:",
            Location = new Point(15, 105),
            Size = new Size(130, 25),
            ForeColor = Color.White
        };

        nudMinIVs = new NumericUpDown
        {
            Location = new Point(150, 102),
            Size = new Size(80, 25),
            Minimum = 0,
            Maximum = 31,
            Value = 31,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        var lblPerfect = new Label
        {
            Text = "Min Perfect IVs (31):",
            Location = new Point(15, 140),
            Size = new Size(130, 25),
            ForeColor = Color.White
        };

        nudMinPerfectIVs = new NumericUpDown
        {
            Location = new Point(150, 137),
            Size = new Size(80, 25),
            Minimum = 0,
            Maximum = 6,
            Value = 5,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        chkPreserveShiny = new CheckBox
        {
            Text = "Preserve Shiny Status",
            Location = new Point(15, 180),
            Size = new Size(200, 25),
            ForeColor = Color.White,
            Checked = true
        };

        chkMaxEVs = new CheckBox
        {
            Text = "Apply Optimal EV Spread",
            Location = new Point(15, 210),
            Size = new Size(200, 25),
            ForeColor = Color.White,
            Checked = true
        };

        chkOptimalNature = new CheckBox
        {
            Text = "Set Optimal Nature",
            Location = new Point(15, 240),
            Size = new Size(200, 25),
            ForeColor = Color.White,
            Checked = true
        };

        // EV Preview
        var lblEVPreview = new Label
        {
            Text = "EV Spread Preview:",
            Location = new Point(15, 280),
            Size = new Size(320, 20),
            ForeColor = Color.Gray
        };

        var lblEVValues = new Label
        {
            Name = "lblEVValues",
            Text = "HP: 0 | ATK: 252 | DEF: 0 | SPA: 0 | SPD: 4 | SPE: 252",
            Location = new Point(15, 300),
            Size = new Size(320, 20),
            ForeColor = Color.Cyan
        };

        grpSettings.Controls.AddRange(new Control[] {
            lblRole, cmbRole, lblTarget, cmbOptimizationTarget,
            lblMinIVs, nudMinIVs, lblPerfect, nudMinPerfectIVs,
            chkPreserveShiny, chkMaxEVs, chkOptimalNature, lblEVPreview, lblEVValues
        });

        // Results
        var grpResults = new GroupBox
        {
            Text = "Optimization Results",
            Location = new Point(760, 20),
            Size = new Size(350, 350),
            ForeColor = Color.FromArgb(100, 255, 150)
        };

        lstResults = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(320, 310),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstResults.Columns.Add("Pokemon", 100);
        lstResults.Columns.Add("Changes", 200);

        grpResults.Controls.Add(lstResults);

        // Action Buttons
        btnOptimize = new Button
        {
            Text = "🔧 CALCULATE OPTIMIZATIONS",
            Location = new Point(20, 390),
            Size = new Size(250, 45),
            BackColor = Color.FromArgb(60, 120, 180),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 11F),
            FlatStyle = FlatStyle.Flat
        };
        btnOptimize.Click += BtnOptimize_Click;

        btnApply = new Button
        {
            Text = "✓ APPLY ALL CHANGES",
            Location = new Point(290, 390),
            Size = new Size(250, 45),
            BackColor = Color.FromArgb(60, 180, 80),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 11F),
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnApply.Click += BtnApply_Click;

        pbProgress = new ProgressBar
        {
            Location = new Point(560, 390),
            Size = new Size(550, 45),
            Style = ProgressBarStyle.Continuous
        };

        // Log
        var grpLog = new GroupBox
        {
            Text = "Optimization Log",
            Location = new Point(20, 450),
            Size = new Size(1090, 230),
            ForeColor = Color.FromArgb(200, 200, 200)
        };

        rtbLog = new RichTextBox
        {
            Location = new Point(15, 25),
            Size = new Size(1060, 190),
            BackColor = Color.FromArgb(20, 20, 35),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };

        grpLog.Controls.Add(rtbLog);

        Controls.AddRange(new Control[] { grpSelection, grpSettings, grpResults, btnOptimize, btnApply, pbProgress, grpLog });
    }

    private void LoadPokemonList()
    {
        clbPokemon.Items.Clear();

        var allPokemon = SAV.PartyData.Concat(SAV.BoxData).Where(p => p.Species != 0).ToList();

        foreach (var pk in allPokemon)
        {
            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            var shinyMark = pk.IsShiny ? "★ " : "";
            clbPokemon.Items.Add(new PokemonListItem { Pokemon = pk, Display = $"{shinyMark}{name}" });
        }
    }

    private void CmbRole_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var role = cmbRole.SelectedItem?.ToString() ?? "Physical Sweeper";
        if (EVPresets.TryGetValue(role, out var evs))
        {
            var lblEVValues = Controls.Find("lblEVValues", true).FirstOrDefault() as Label;
            if (lblEVValues != null)
            {
                lblEVValues.Text = $"HP: {evs[0]} | ATK: {evs[1]} | DEF: {evs[2]} | SPA: {evs[3]} | SPD: {evs[4]} | SPE: {evs[5]}";
            }
        }
    }

    private void BtnOptimize_Click(object? sender, EventArgs e)
    {
        var selected = clbPokemon.CheckedItems.Cast<PokemonListItem>().ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("Please select at least one Pokemon!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        results.Clear();
        lstResults.Items.Clear();
        rtbLog.Clear();

        pbProgress.Maximum = selected.Count;
        pbProgress.Value = 0;

        var role = cmbRole.SelectedItem?.ToString() ?? "Physical Sweeper";
        var evSpread = EVPresets.GetValueOrDefault(role, EVPresets["Physical Sweeper"]);
        var nature = OptimalNatures.GetValueOrDefault(role, Nature.Hardy);
        var minIV = (int)nudMinIVs.Value;
        var minPerfect = (int)nudMinPerfectIVs.Value;
        var target = cmbOptimizationTarget.SelectedItem?.ToString() ?? "Competitive";

        LogMessage($"Starting batch optimization for {selected.Count} Pokemon...", Color.Cyan);
        LogMessage($"Role: {role} | Target: {target}", Color.Yellow);
        LogMessage($"EV Spread: HP:{evSpread[0]} ATK:{evSpread[1]} DEF:{evSpread[2]} SPA:{evSpread[3]} SPD:{evSpread[4]} SPE:{evSpread[5]}", Color.White);
        LogMessage("", Color.White);

        foreach (var item in selected)
        {
            var pk = item.Pokemon;
            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            var result = new OptimizationResult { Pokemon = pk, Name = name };

            // Calculate optimal IVs
            var optimalIVs = CalculateOptimalIVs(pk, role, target, minIV, minPerfect);
            result.NewIVs = optimalIVs;
            result.OldIVs = new[] { pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE };

            // EV spread
            if (chkMaxEVs.Checked)
            {
                result.NewEVs = evSpread;
                result.OldEVs = new[] { pk.EV_HP, pk.EV_ATK, pk.EV_DEF, pk.EV_SPA, pk.EV_SPD, pk.EV_SPE };
            }

            // Nature
            if (chkOptimalNature.Checked)
            {
                result.NewNature = nature;
                result.OldNature = (Nature)pk.Nature;
            }

            // Build change summary
            var changes = new List<string>();
            if (!optimalIVs.SequenceEqual(result.OldIVs))
                changes.Add($"IVs: {SummarizeIVs(result.OldIVs)}→{SummarizeIVs(optimalIVs)}");
            if (chkMaxEVs.Checked && !evSpread.SequenceEqual(result.OldEVs!))
                changes.Add("EVs optimized");
            if (chkOptimalNature.Checked && result.NewNature != result.OldNature)
                changes.Add($"Nature: {result.OldNature}→{result.NewNature}");

            result.ChangesSummary = changes.Count > 0 ? string.Join(", ", changes) : "No changes needed";
            results.Add(result);

            var listItem = new ListViewItem(name);
            listItem.SubItems.Add(result.ChangesSummary);
            listItem.ForeColor = changes.Count > 0 ? Color.LightGreen : Color.Gray;
            lstResults.Items.Add(listItem);

            LogMessage($"  {name}: {result.ChangesSummary}", changes.Count > 0 ? Color.LightGreen : Color.Gray);

            pbProgress.Value++;
            Application.DoEvents();
        }

        var changesCount = results.Count(r => r.ChangesSummary != "No changes needed");
        LogMessage("", Color.White);
        LogMessage($"Optimization complete! {changesCount}/{selected.Count} Pokemon will be modified.", Color.Cyan);

        btnApply.Enabled = changesCount > 0;
    }

    private int[] CalculateOptimalIVs(PKM pk, string role, string target, int minIV, int minPerfect)
    {
        var ivs = new int[6];
        var perfectCount = 0;

        // Determine which stats need to be perfect based on role
        var priorityStats = role switch
        {
            "Physical Sweeper" => new[] { 1, 5 }, // ATK, SPE
            "Special Sweeper" => new[] { 3, 5 }, // SPA, SPE
            "Mixed Attacker" => new[] { 1, 3, 5 }, // ATK, SPA, SPE
            "Physical Wall" => new[] { 0, 2 }, // HP, DEF
            "Special Wall" => new[] { 0, 4 }, // HP, SPD
            "Balanced Wall" => new[] { 0, 2, 4 }, // HP, DEF, SPD
            "Trick Room" => new[] { 1, 0 }, // ATK, HP (0 SPE)
            _ => new[] { 0, 1, 2, 3, 4, 5 }
        };

        // Set priority stats to 31
        foreach (var stat in priorityStats)
        {
            if (perfectCount < minPerfect)
            {
                ivs[stat] = 31;
                perfectCount++;
            }
        }

        // Fill remaining with minimum IV or 31 up to limit
        for (int i = 0; i < 6; i++)
        {
            if (ivs[i] == 0)
            {
                if (perfectCount < minPerfect)
                {
                    ivs[i] = 31;
                    perfectCount++;
                }
                else
                {
                    ivs[i] = minIV;
                }
            }
        }

        // Special case: Trick Room wants 0 Speed
        if (role == "Trick Room")
        {
            ivs[5] = 0;
        }

        // Special case: Mixed attackers might want 0 in unused attack stat
        if (target == "Damage Output")
        {
            if (pk.PersonalInfo.ATK > pk.PersonalInfo.SPA)
                ivs[3] = 0; // Minimize SPA for physical
            else
                ivs[1] = 0; // Minimize ATK for special
        }

        return ivs;
    }

    private void BtnApply_Click(object? sender, EventArgs e)
    {
        var confirm = MessageBox.Show(
            $"This will permanently modify {results.Count(r => r.ChangesSummary != "No changes needed")} Pokemon.\n\nContinue?",
            "Confirm Changes",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        var applied = 0;
        foreach (var result in results.Where(r => r.ChangesSummary != "No changes needed"))
        {
            var pk = result.Pokemon;

            // Apply IVs
            pk.IV_HP = result.NewIVs[0];
            pk.IV_ATK = result.NewIVs[1];
            pk.IV_DEF = result.NewIVs[2];
            pk.IV_SPA = result.NewIVs[3];
            pk.IV_SPD = result.NewIVs[4];
            pk.IV_SPE = result.NewIVs[5];

            // Apply EVs
            if (result.NewEVs != null)
            {
                pk.EV_HP = result.NewEVs[0];
                pk.EV_ATK = result.NewEVs[1];
                pk.EV_DEF = result.NewEVs[2];
                pk.EV_SPA = result.NewEVs[3];
                pk.EV_SPD = result.NewEVs[4];
                pk.EV_SPE = result.NewEVs[5];
            }

            // Apply Nature
            if (result.NewNature.HasValue)
            {
                pk.Nature = result.NewNature.Value;
                pk.StatNature = result.NewNature.Value;
            }

            applied++;
        }

        LogMessage("", Color.White);
        LogMessage($"✓ Successfully applied changes to {applied} Pokemon!", Color.LightGreen);

        MessageBox.Show($"Successfully optimized {applied} Pokemon!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        btnApply.Enabled = false;
    }

    private void LogMessage(string message, Color color)
    {
        rtbLog.SelectionColor = color;
        rtbLog.AppendText(message + "\n");
        rtbLog.ScrollToCaret();
    }

    private string SummarizeIVs(int[] ivs)
    {
        var perfect = ivs.Count(v => v == 31);
        return $"{perfect}IV";
    }

    private class PokemonListItem
    {
        public PKM Pokemon { get; set; } = null!;
        public string Display { get; set; } = "";
        public override string ToString() => Display;
    }

    private class OptimizationResult
    {
        public PKM Pokemon { get; set; } = null!;
        public string Name { get; set; } = "";
        public int[] OldIVs { get; set; } = new int[6];
        public int[] NewIVs { get; set; } = new int[6];
        public int[]? OldEVs { get; set; }
        public int[]? NewEVs { get; set; }
        public Nature? OldNature { get; set; }
        public Nature? NewNature { get; set; }
        public string ChangesSummary { get; set; } = "";
    }
}
