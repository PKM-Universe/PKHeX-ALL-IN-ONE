using System;
using System.Drawing;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;

namespace PKHeX.WinForms;

public class PokemonComparisonTool : Form
{
    private readonly SaveFile SAV;
    private PKM? _pokemon1;
    private PKM? _pokemon2;

    private readonly PictureBox PB_Pokemon1;
    private readonly PictureBox PB_Pokemon2;
    private readonly Panel PNL_Stats1;
    private readonly Panel PNL_Stats2;
    private readonly Label L_Name1;
    private readonly Label L_Name2;
    private readonly Button BTN_Load1;
    private readonly Button BTN_Load2;
    private readonly Button BTN_Swap;
    private readonly Panel PNL_Comparison;

    public PokemonComparisonTool(SaveFile sav)
    {
        SAV = sav;
        Text = "Pokemon Comparison Tool";
        Size = new Size(800, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var lblTitle = new Label
        {
            Text = "Compare Two Pokemon",
            Location = new Point(300, 10),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };

        // Pokemon 1 section
        var pnl1 = new Panel
        {
            Location = new Point(20, 50),
            Size = new Size(360, 280),
            BackColor = Color.FromArgb(40, 40, 60),
            BorderStyle = BorderStyle.FixedSingle
        };

        L_Name1 = new Label
        {
            Text = "Select Pokemon 1",
            Location = new Point(10, 10),
            Size = new Size(340, 25),
            ForeColor = Color.Cyan,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        PB_Pokemon1 = new PictureBox
        {
            Location = new Point(130, 40),
            Size = new Size(100, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        PNL_Stats1 = new Panel
        {
            Location = new Point(10, 150),
            Size = new Size(340, 120),
            BackColor = Color.Transparent
        };

        BTN_Load1 = new Button
        {
            Text = "Load from Editor",
            Location = new Point(115, 145),
            Size = new Size(130, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 60),
            ForeColor = Color.White
        };
        BTN_Load1.Click += (s, e) => LoadPokemon1();

        pnl1.Controls.AddRange(new Control[] { L_Name1, PB_Pokemon1, BTN_Load1, PNL_Stats1 });

        // Pokemon 2 section
        var pnl2 = new Panel
        {
            Location = new Point(420, 50),
            Size = new Size(360, 280),
            BackColor = Color.FromArgb(40, 40, 60),
            BorderStyle = BorderStyle.FixedSingle
        };

        L_Name2 = new Label
        {
            Text = "Select Pokemon 2",
            Location = new Point(10, 10),
            Size = new Size(340, 25),
            ForeColor = Color.Orange,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        PB_Pokemon2 = new PictureBox
        {
            Location = new Point(130, 40),
            Size = new Size(100, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        PNL_Stats2 = new Panel
        {
            Location = new Point(10, 150),
            Size = new Size(340, 120),
            BackColor = Color.Transparent
        };

        BTN_Load2 = new Button
        {
            Text = "Load from Box",
            Location = new Point(115, 145),
            Size = new Size(130, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 60, 100),
            ForeColor = Color.White
        };
        BTN_Load2.Click += (s, e) => LoadPokemon2();

        pnl2.Controls.AddRange(new Control[] { L_Name2, PB_Pokemon2, BTN_Load2, PNL_Stats2 });

        // Swap button
        BTN_Swap = new Button
        {
            Text = "⇄",
            Location = new Point(385, 150),
            Size = new Size(30, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F)
        };
        BTN_Swap.Click += (s, e) => SwapPokemon();

        // Comparison panel
        PNL_Comparison = new Panel
        {
            Location = new Point(20, 350),
            Size = new Size(760, 300),
            BackColor = Color.FromArgb(35, 35, 55),
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblCompare = new Label
        {
            Text = "Stat Comparison",
            Location = new Point(10, 10),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        PNL_Comparison.Controls.Add(lblCompare);

        Controls.AddRange(new Control[] { lblTitle, pnl1, pnl2, BTN_Swap, PNL_Comparison });

        CreateComparisonBars();
    }

    private void LoadPokemon1()
    {
        // This will be set from the editor
        // For now, get from first party slot
        if (SAV.HasParty)
        {
            _pokemon1 = SAV.GetPartySlotAtIndex(0);
            UpdatePokemon1Display();
        }
    }

    public void SetPokemon1(PKM pk)
    {
        _pokemon1 = pk;
        UpdatePokemon1Display();
    }

    private void LoadPokemon2()
    {
        // Show box selector
        using var form = new PokemonSelectorForm(SAV);
        if (form.ShowDialog() == DialogResult.OK && form.SelectedPokemon != null)
        {
            _pokemon2 = form.SelectedPokemon;
            UpdatePokemon2Display();
        }
    }

    private void UpdatePokemon1Display()
    {
        if (_pokemon1 == null || _pokemon1.Species == 0) return;

        L_Name1.Text = GameInfo.Strings.specieslist[_pokemon1.Species];
        try
        {
            PB_Pokemon1.Image = SpriteUtil.GetSprite(_pokemon1.Species, _pokemon1.Form, _pokemon1.Gender,
                0, _pokemon1.SpriteItem, _pokemon1.IsEgg, _pokemon1.IsShiny ? Shiny.AlwaysStar : Shiny.Never,
                _pokemon1.Context);
        }
        catch { }

        UpdateStats1();
        UpdateComparison();
    }

    private void UpdatePokemon2Display()
    {
        if (_pokemon2 == null || _pokemon2.Species == 0) return;

        L_Name2.Text = GameInfo.Strings.specieslist[_pokemon2.Species];
        try
        {
            PB_Pokemon2.Image = SpriteUtil.GetSprite(_pokemon2.Species, _pokemon2.Form, _pokemon2.Gender,
                0, _pokemon2.SpriteItem, _pokemon2.IsEgg, _pokemon2.IsShiny ? Shiny.AlwaysStar : Shiny.Never,
                _pokemon2.Context);
        }
        catch { }

        UpdateStats2();
        UpdateComparison();
    }

    private void UpdateStats1()
    {
        if (_pokemon1 == null) return;

        PNL_Stats1.Controls.Clear();
        var stats = new[] { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };
        var values = new[] { _pokemon1.Stat_HPCurrent, _pokemon1.Stat_ATK, _pokemon1.Stat_DEF,
                            _pokemon1.Stat_SPA, _pokemon1.Stat_SPD, _pokemon1.Stat_SPE };

        for (int i = 0; i < 6; i++)
        {
            var lbl = new Label
            {
                Text = $"{stats[i]}: {values[i]}",
                Location = new Point(10 + (i % 3) * 110, 5 + (i / 3) * 25),
                AutoSize = true,
                ForeColor = Color.White
            };
            PNL_Stats1.Controls.Add(lbl);
        }
    }

    private void UpdateStats2()
    {
        if (_pokemon2 == null) return;

        PNL_Stats2.Controls.Clear();
        var stats = new[] { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };
        var values = new[] { _pokemon2.Stat_HPCurrent, _pokemon2.Stat_ATK, _pokemon2.Stat_DEF,
                            _pokemon2.Stat_SPA, _pokemon2.Stat_SPD, _pokemon2.Stat_SPE };

        for (int i = 0; i < 6; i++)
        {
            var lbl = new Label
            {
                Text = $"{stats[i]}: {values[i]}",
                Location = new Point(10 + (i % 3) * 110, 5 + (i / 3) * 25),
                AutoSize = true,
                ForeColor = Color.White
            };
            PNL_Stats2.Controls.Add(lbl);
        }
    }

    private readonly ProgressBar[] _bars1 = new ProgressBar[6];
    private readonly ProgressBar[] _bars2 = new ProgressBar[6];
    private readonly Label[] _lblDiff = new Label[6];

    private void CreateComparisonBars()
    {
        var stats = new[] { "HP", "Attack", "Defense", "Sp. Atk", "Sp. Def", "Speed" };
        var colors1 = Color.Cyan;
        var colors2 = Color.Orange;

        for (int i = 0; i < 6; i++)
        {
            int y = 45 + i * 40;

            var lblStat = new Label
            {
                Text = stats[i],
                Location = new Point(10, y + 5),
                Size = new Size(70, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };

            _bars1[i] = new ProgressBar
            {
                Location = new Point(90, y),
                Size = new Size(250, 20),
                Maximum = 500,
                Style = ProgressBarStyle.Continuous
            };

            _bars2[i] = new ProgressBar
            {
                Location = new Point(400, y),
                Size = new Size(250, 20),
                Maximum = 500,
                Style = ProgressBarStyle.Continuous
            };

            _lblDiff[i] = new Label
            {
                Location = new Point(350, y + 2),
                Size = new Size(40, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            PNL_Comparison.Controls.AddRange(new Control[] { lblStat, _bars1[i], _bars2[i], _lblDiff[i] });
        }

        // Legend
        var legend1 = new Label
        {
            Text = "● Pokemon 1",
            Location = new Point(90, 290),
            AutoSize = true,
            ForeColor = colors1,
            Font = new Font("Segoe UI", 9F)
        };

        var legend2 = new Label
        {
            Text = "● Pokemon 2",
            Location = new Point(400, 290),
            AutoSize = true,
            ForeColor = colors2,
            Font = new Font("Segoe UI", 9F)
        };

        PNL_Comparison.Controls.AddRange(new Control[] { legend1, legend2 });
    }

    private void UpdateComparison()
    {
        if (_pokemon1 == null || _pokemon2 == null) return;

        var stats1 = new[] { _pokemon1.Stat_HPCurrent, _pokemon1.Stat_ATK, _pokemon1.Stat_DEF,
                            _pokemon1.Stat_SPA, _pokemon1.Stat_SPD, _pokemon1.Stat_SPE };
        var stats2 = new[] { _pokemon2.Stat_HPCurrent, _pokemon2.Stat_ATK, _pokemon2.Stat_DEF,
                            _pokemon2.Stat_SPA, _pokemon2.Stat_SPD, _pokemon2.Stat_SPE };

        for (int i = 0; i < 6; i++)
        {
            _bars1[i].Value = Math.Min(stats1[i], 500);
            _bars2[i].Value = Math.Min(stats2[i], 500);

            var diff = stats1[i] - stats2[i];
            _lblDiff[i].Text = diff > 0 ? $"+{diff}" : diff.ToString();
            _lblDiff[i].ForeColor = diff > 0 ? Color.LightGreen : (diff < 0 ? Color.Salmon : Color.White);
        }
    }

    private void SwapPokemon()
    {
        (_pokemon1, _pokemon2) = (_pokemon2, _pokemon1);
        UpdatePokemon1Display();
        UpdatePokemon2Display();
    }
}

public class PokemonSelectorForm : Form
{
    public PKM? SelectedPokemon { get; private set; }
    private readonly SaveFile SAV;
    private readonly FlowLayoutPanel FLP_Pokemon;
    private readonly NumericUpDown NUD_Box;

    public PokemonSelectorForm(SaveFile sav)
    {
        SAV = sav;
        Text = "Select Pokemon";
        Size = new Size(500, 400);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 30, 45);

        var lblBox = new Label
        {
            Text = "Box:",
            Location = new Point(10, 15),
            AutoSize = true,
            ForeColor = Color.White
        };

        NUD_Box = new NumericUpDown
        {
            Location = new Point(50, 12),
            Size = new Size(60, 25),
            Minimum = 1,
            Maximum = sav.BoxCount,
            Value = 1
        };
        NUD_Box.ValueChanged += (s, e) => LoadBox((int)NUD_Box.Value - 1);

        FLP_Pokemon = new FlowLayoutPanel
        {
            Location = new Point(10, 50),
            Size = new Size(465, 300),
            AutoScroll = true,
            BackColor = Color.FromArgb(40, 40, 60)
        };

        Controls.AddRange(new Control[] { lblBox, NUD_Box, FLP_Pokemon });

        LoadBox(0);
    }

    private void LoadBox(int box)
    {
        FLP_Pokemon.Controls.Clear();

        for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
        {
            var pk = SAV.GetBoxSlotAtIndex(box, slot);
            if (pk.Species == 0) continue;

            var btn = new Button
            {
                Size = new Size(70, 70),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 70),
                Tag = pk
            };

            try
            {
                btn.Image = SpriteUtil.GetSprite(pk.Species, pk.Form, pk.Gender,
                    0, pk.SpriteItem, pk.IsEgg, pk.IsShiny ? Shiny.AlwaysStar : Shiny.Never, pk.Context);
            }
            catch { }

            btn.Click += (s, e) =>
            {
                SelectedPokemon = (PKM)btn.Tag!;
                DialogResult = DialogResult.OK;
                Close();
            };

            FLP_Pokemon.Controls.Add(btn);
        }
    }
}
