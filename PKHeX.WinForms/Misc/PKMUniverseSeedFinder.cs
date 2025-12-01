using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

/// <summary>
/// PKM-Universe Seed Finder - Custom RNG tool for finding Pokemon seeds
/// Supports Sword/Shield Raids, Scarlet/Violet Tera Raids, and general RNG
/// </summary>
public class PKMUniverseSeedFinder : Form
{
    private readonly SaveFile SAV;
    private TabControl tabControl;
    private CancellationTokenSource? _cts;

    // SwSh Raid Tab Controls
    private NumericUpDown nudSwShSeed;
    private ComboBox cmbSwShSpecies;
    private NumericUpDown nudSwShStars;
    private CheckBox chkSwShShiny;
    private DataGridView dgvSwShResults;
    private Button btnSwShSearch;
    private Button btnSwShStop;
    private ProgressBar prgSwSh;
    private Label lblSwShStatus;

    // SV Tera Tab Controls
    private NumericUpDown nudSVSeed;
    private ComboBox cmbSVSpecies;
    private ComboBox cmbSVTeraType;
    private NumericUpDown nudSVStars;
    private CheckBox chkSVShiny;
    private DataGridView dgvSVResults;
    private Button btnSVSearch;
    private Button btnSVStop;
    private ProgressBar prgSV;
    private Label lblSVStatus;

    // General RNG Tab Controls
    private ComboBox cmbRNGGame;
    private ComboBox cmbRNGMethod;
    private NumericUpDown nudRNGSeed;
    private NumericUpDown nudRNGMinFrame;
    private NumericUpDown nudRNGMaxFrame;
    private CheckBox chkRNGShiny;
    private DataGridView dgvRNGResults;
    private Button btnRNGSearch;
    private Button btnRNGStop;
    private ProgressBar prgRNG;
    private Label lblRNGStatus;

    // PID/EC Calculator Tab Controls
    private TextBox txtCalcPID;
    private TextBox txtCalcEC;
    private TextBox txtCalcTID;
    private TextBox txtCalcSID;
    private Label lblCalcShinyType;
    private Label lblCalcPSV;
    private Label lblCalcTSV;
    private Button btnCalcAnalyze;

    public PKMUniverseSeedFinder(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
        LoadSpeciesList();
    }

    private void InitializeComponent()
    {
        Text = "PKM-Universe Seed Finder";
        Size = new Size(900, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        // Title
        var lblTitle = new Label
        {
            Text = "PKM-Universe Seed Finder",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold)
        };
        Controls.Add(lblTitle);

        // Tab Control
        tabControl = new TabControl
        {
            Location = new Point(20, 60),
            Size = new Size(845, 580),
            Font = new Font("Segoe UI", 10F)
        };

        // Create tabs
        var tabSwSh = CreateSwShRaidTab();
        var tabSV = CreateSVTeraTab();
        var tabRNG = CreateGeneralRNGTab();
        var tabCalc = CreatePIDCalculatorTab();

        tabControl.TabPages.Add(tabSwSh);
        tabControl.TabPages.Add(tabSV);
        tabControl.TabPages.Add(tabRNG);
        tabControl.TabPages.Add(tabCalc);

        Controls.Add(tabControl);
    }

    #region SwSh Raid Tab
    private TabPage CreateSwShRaidTab()
    {
        var tab = new TabPage("Sword/Shield Raids")
        {
            BackColor = Color.FromArgb(30, 30, 50)
        };

        // Seed Input
        var lblSeed = CreateLabel("Raid Seed (Hex):", 20, 20);
        nudSwShSeed = new NumericUpDown
        {
            Location = new Point(150, 17),
            Size = new Size(200, 25),
            Hexadecimal = true,
            Maximum = ulong.MaxValue,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        // Species Filter
        var lblSpecies = CreateLabel("Species:", 20, 55);
        cmbSwShSpecies = CreateComboBox(150, 52, 200);

        // Star Rating
        var lblStars = CreateLabel("Star Rating:", 20, 90);
        nudSwShStars = new NumericUpDown
        {
            Location = new Point(150, 87),
            Size = new Size(80, 25),
            Minimum = 1,
            Maximum = 5,
            Value = 5,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        // Shiny Filter
        chkSwShShiny = new CheckBox
        {
            Text = "Shiny Only",
            Location = new Point(250, 90),
            ForeColor = Color.Gold,
            AutoSize = true
        };

        // Search Buttons
        btnSwShSearch = CreateButton("Search", 380, 20, 100, Color.FromArgb(60, 120, 60));
        btnSwShSearch.Click += BtnSwShSearch_Click;

        btnSwShStop = CreateButton("Stop", 490, 20, 80, Color.FromArgb(120, 60, 60));
        btnSwShStop.Enabled = false;
        btnSwShStop.Click += BtnSwShStop_Click;

        // Progress
        prgSwSh = new ProgressBar
        {
            Location = new Point(380, 55),
            Size = new Size(400, 20),
            Style = ProgressBarStyle.Continuous
        };

        lblSwShStatus = CreateLabel("Ready", 380, 80);
        lblSwShStatus.Size = new Size(400, 20);

        // Results Grid
        dgvSwShResults = CreateResultsGrid(20, 130, 790, 380);
        dgvSwShResults.Columns.Add("Frame", "Frame");
        dgvSwShResults.Columns.Add("Seed", "Seed");
        dgvSwShResults.Columns.Add("Species", "Species");
        dgvSwShResults.Columns.Add("Shiny", "Shiny");
        dgvSwShResults.Columns.Add("Nature", "Nature");
        dgvSwShResults.Columns.Add("Ability", "Ability");
        dgvSwShResults.Columns.Add("IVs", "IVs");

        tab.Controls.AddRange(new Control[] { lblSeed, nudSwShSeed, lblSpecies, cmbSwShSpecies,
            lblStars, nudSwShStars, chkSwShShiny, btnSwShSearch, btnSwShStop,
            prgSwSh, lblSwShStatus, dgvSwShResults });

        return tab;
    }
    #endregion

    #region SV Tera Tab
    private TabPage CreateSVTeraTab()
    {
        var tab = new TabPage("Scarlet/Violet Tera Raids")
        {
            BackColor = Color.FromArgb(30, 30, 50)
        };

        // Seed Input
        var lblSeed = CreateLabel("Tera Seed (Hex):", 20, 20);
        nudSVSeed = new NumericUpDown
        {
            Location = new Point(150, 17),
            Size = new Size(200, 25),
            Hexadecimal = true,
            Maximum = uint.MaxValue,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        // Species Filter
        var lblSpecies = CreateLabel("Species:", 20, 55);
        cmbSVSpecies = CreateComboBox(150, 52, 200);

        // Tera Type
        var lblTera = CreateLabel("Tera Type:", 20, 90);
        cmbSVTeraType = CreateComboBox(150, 87, 150);
        cmbSVTeraType.Items.Add("Any");
        foreach (var type in Enum.GetNames(typeof(MoveType)))
            cmbSVTeraType.Items.Add(type);
        cmbSVTeraType.SelectedIndex = 0;

        // Star Rating
        var lblStars = CreateLabel("Star Rating:", 320, 55);
        nudSVStars = new NumericUpDown
        {
            Location = new Point(410, 52),
            Size = new Size(80, 25),
            Minimum = 1,
            Maximum = 7,
            Value = 6,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        // Shiny Filter
        chkSVShiny = new CheckBox
        {
            Text = "Shiny Only",
            Location = new Point(320, 90),
            ForeColor = Color.Gold,
            AutoSize = true
        };

        // Search Buttons
        btnSVSearch = CreateButton("Search", 550, 20, 100, Color.FromArgb(60, 120, 60));
        btnSVSearch.Click += BtnSVSearch_Click;

        btnSVStop = CreateButton("Stop", 660, 20, 80, Color.FromArgb(120, 60, 60));
        btnSVStop.Enabled = false;
        btnSVStop.Click += BtnSVStop_Click;

        // Progress
        prgSV = new ProgressBar
        {
            Location = new Point(550, 55),
            Size = new Size(230, 20),
            Style = ProgressBarStyle.Continuous
        };

        lblSVStatus = CreateLabel("Ready", 550, 80);
        lblSVStatus.Size = new Size(230, 20);

        // Results Grid
        dgvSVResults = CreateResultsGrid(20, 130, 790, 380);
        dgvSVResults.Columns.Add("Seed", "Seed");
        dgvSVResults.Columns.Add("Species", "Species");
        dgvSVResults.Columns.Add("TeraType", "Tera Type");
        dgvSVResults.Columns.Add("Shiny", "Shiny");
        dgvSVResults.Columns.Add("Nature", "Nature");
        dgvSVResults.Columns.Add("Ability", "Ability");
        dgvSVResults.Columns.Add("IVs", "IVs");
        dgvSVResults.Columns.Add("Gender", "Gender");

        tab.Controls.AddRange(new Control[] { lblSeed, nudSVSeed, lblSpecies, cmbSVSpecies,
            lblTera, cmbSVTeraType, lblStars, nudSVStars, chkSVShiny,
            btnSVSearch, btnSVStop, prgSV, lblSVStatus, dgvSVResults });

        return tab;
    }
    #endregion

    #region General RNG Tab
    private TabPage CreateGeneralRNGTab()
    {
        var tab = new TabPage("General RNG")
        {
            BackColor = Color.FromArgb(30, 30, 50)
        };

        // Game Selection
        var lblGame = CreateLabel("Game:", 20, 20);
        cmbRNGGame = CreateComboBox(100, 17, 180);
        cmbRNGGame.Items.AddRange(new object[] {
            "Red/Blue/Yellow", "Gold/Silver/Crystal",
            "Ruby/Sapphire/Emerald", "FireRed/LeafGreen",
            "Diamond/Pearl/Platinum", "HeartGold/SoulSilver",
            "Black/White", "Black 2/White 2",
            "X/Y", "Omega Ruby/Alpha Sapphire",
            "Sun/Moon", "Ultra Sun/Ultra Moon",
            "Let's Go Pikachu/Eevee",
            "Sword/Shield", "Brilliant Diamond/Shining Pearl",
            "Legends: Arceus", "Scarlet/Violet"
        });
        cmbRNGGame.SelectedIndex = 0;

        // RNG Method
        var lblMethod = CreateLabel("Method:", 300, 20);
        cmbRNGMethod = CreateComboBox(380, 17, 180);
        cmbRNGMethod.Items.AddRange(new object[] {
            "Wild Encounter", "Static Encounter", "Gift Pokemon",
            "Egg Generation", "Raid", "Mass Outbreak"
        });
        cmbRNGMethod.SelectedIndex = 0;

        // Seed Input
        var lblSeed = CreateLabel("Initial Seed:", 20, 55);
        nudRNGSeed = new NumericUpDown
        {
            Location = new Point(120, 52),
            Size = new Size(200, 25),
            Hexadecimal = true,
            Maximum = ulong.MaxValue,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        // Frame Range
        var lblMinFrame = CreateLabel("Min Frame:", 340, 55);
        nudRNGMinFrame = new NumericUpDown
        {
            Location = new Point(420, 52),
            Size = new Size(100, 25),
            Maximum = 999999999,
            Value = 0,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        var lblMaxFrame = CreateLabel("Max Frame:", 540, 55);
        nudRNGMaxFrame = new NumericUpDown
        {
            Location = new Point(630, 52),
            Size = new Size(100, 25),
            Maximum = 999999999,
            Value = 100000,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };

        // Shiny Filter
        chkRNGShiny = new CheckBox
        {
            Text = "Shiny Only",
            Location = new Point(20, 90),
            ForeColor = Color.Gold,
            AutoSize = true
        };

        // Search Buttons
        btnRNGSearch = CreateButton("Search", 580, 85, 100, Color.FromArgb(60, 120, 60));
        btnRNGSearch.Click += BtnRNGSearch_Click;

        btnRNGStop = CreateButton("Stop", 690, 85, 80, Color.FromArgb(120, 60, 60));
        btnRNGStop.Enabled = false;
        btnRNGStop.Click += BtnRNGStop_Click;

        // Progress
        prgRNG = new ProgressBar
        {
            Location = new Point(150, 90),
            Size = new Size(400, 20),
            Style = ProgressBarStyle.Continuous
        };

        lblRNGStatus = CreateLabel("Ready", 150, 115);
        lblRNGStatus.Size = new Size(400, 20);

        // Results Grid
        dgvRNGResults = CreateResultsGrid(20, 145, 790, 365);
        dgvRNGResults.Columns.Add("Frame", "Frame");
        dgvRNGResults.Columns.Add("PID", "PID");
        dgvRNGResults.Columns.Add("Shiny", "Shiny");
        dgvRNGResults.Columns.Add("Nature", "Nature");
        dgvRNGResults.Columns.Add("Ability", "Ability");
        dgvRNGResults.Columns.Add("IVs", "IVs");
        dgvRNGResults.Columns.Add("Gender", "Gender");

        tab.Controls.AddRange(new Control[] { lblGame, cmbRNGGame, lblMethod, cmbRNGMethod,
            lblSeed, nudRNGSeed, lblMinFrame, nudRNGMinFrame, lblMaxFrame, nudRNGMaxFrame,
            chkRNGShiny, btnRNGSearch, btnRNGStop, prgRNG, lblRNGStatus, dgvRNGResults });

        return tab;
    }
    #endregion

    #region PID Calculator Tab
    private TabPage CreatePIDCalculatorTab()
    {
        var tab = new TabPage("PID/EC Calculator")
        {
            BackColor = Color.FromArgb(30, 30, 50)
        };

        // Info Panel
        var pnlInfo = new Panel
        {
            Location = new Point(20, 20),
            Size = new Size(790, 200),
            BackColor = Color.FromArgb(40, 40, 60),
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblPIDTitle = CreateLabel("PID (Hex):", 20, 20);
        txtCalcPID = new TextBox
        {
            Location = new Point(150, 17),
            Size = new Size(200, 25),
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White,
            MaxLength = 8
        };
        txtCalcPID.TextChanged += TxtCalc_TextChanged;

        var lblECTitle = CreateLabel("EC (Hex):", 20, 55);
        txtCalcEC = new TextBox
        {
            Location = new Point(150, 52),
            Size = new Size(200, 25),
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White,
            MaxLength = 8
        };

        var lblTIDTitle = CreateLabel("TID:", 20, 90);
        txtCalcTID = new TextBox
        {
            Location = new Point(150, 87),
            Size = new Size(100, 25),
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White,
            Text = SAV.TID16.ToString()
        };
        txtCalcTID.TextChanged += TxtCalc_TextChanged;

        var lblSIDTitle = CreateLabel("SID:", 20, 125);
        txtCalcSID = new TextBox
        {
            Location = new Point(150, 122),
            Size = new Size(100, 25),
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White,
            Text = SAV.SID16.ToString()
        };
        txtCalcSID.TextChanged += TxtCalc_TextChanged;

        btnCalcAnalyze = CreateButton("Analyze", 150, 160, 100, Color.FromArgb(60, 100, 140));
        btnCalcAnalyze.Click += BtnCalcAnalyze_Click;

        // Results Section
        var lblResultsTitle = CreateLabel("Results:", 400, 20);
        lblResultsTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

        lblCalcShinyType = CreateLabel("Shiny Type: ---", 400, 55);
        lblCalcShinyType.Font = new Font("Segoe UI", 11F);
        lblCalcShinyType.Size = new Size(350, 25);

        lblCalcPSV = CreateLabel("PSV (PID Shiny Value): ---", 400, 85);
        lblCalcPSV.Size = new Size(350, 25);

        lblCalcTSV = CreateLabel("TSV (Trainer Shiny Value): ---", 400, 115);
        lblCalcTSV.Size = new Size(350, 25);

        pnlInfo.Controls.AddRange(new Control[] { lblPIDTitle, txtCalcPID, lblECTitle, txtCalcEC,
            lblTIDTitle, txtCalcTID, lblSIDTitle, txtCalcSID, btnCalcAnalyze,
            lblResultsTitle, lblCalcShinyType, lblCalcPSV, lblCalcTSV });

        // Instructions
        var lblInstructions = new Label
        {
            Text = "Instructions:\n\n" +
                   "• Enter a PID (Process ID) in hexadecimal format\n" +
                   "• Enter your TID and SID to check shiny status\n" +
                   "• PSV = ((PID >> 16) ^ (PID & 0xFFFF)) >> 4\n" +
                   "• TSV = (TID ^ SID) >> 4\n" +
                   "• A Pokemon is shiny if PSV == TSV\n" +
                   "• Square shiny: (PID ^ TID ^ SID) == 0\n" +
                   "• Star shiny: (PID ^ TID ^ SID) < 16 && != 0",
            Location = new Point(20, 240),
            Size = new Size(790, 200),
            ForeColor = Color.LightGray,
            Font = new Font("Segoe UI", 10F)
        };

        tab.Controls.AddRange(new Control[] { pnlInfo, lblInstructions });

        return tab;
    }
    #endregion

    #region Helper Methods
    private Label CreateLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true,
            ForeColor = Color.White
        };
    }

    private ComboBox CreateComboBox(int x, int y, int width)
    {
        return new ComboBox
        {
            Location = new Point(x, y),
            Size = new Size(width, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
    }

    private Button CreateButton(string text, int x, int y, int width, Color backColor)
    {
        return new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
    }

    private DataGridView CreateResultsGrid(int x, int y, int width, int height)
    {
        var dgv = new DataGridView
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackgroundColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            GridColor = Color.FromArgb(60, 60, 80),
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            EnableHeadersVisualStyles = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 70);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgv.DefaultCellStyle.BackColor = Color.FromArgb(35, 35, 55);
        dgv.DefaultCellStyle.ForeColor = Color.White;
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 100, 140);

        return dgv;
    }

    private void LoadSpeciesList()
    {
        var speciesList = new List<string> { "Any" };
        for (int i = 1; i <= SAV.MaxSpeciesID; i++)
        {
            var name = GameInfo.Strings.specieslist[i];
            if (!string.IsNullOrEmpty(name))
                speciesList.Add($"{i:000} - {name}");
        }

        cmbSwShSpecies.Items.AddRange(speciesList.ToArray());
        cmbSwShSpecies.SelectedIndex = 0;

        cmbSVSpecies.Items.AddRange(speciesList.ToArray());
        cmbSVSpecies.SelectedIndex = 0;
    }
    #endregion

    #region SwSh Search
    private async void BtnSwShSearch_Click(object? sender, EventArgs e)
    {
        btnSwShSearch.Enabled = false;
        btnSwShStop.Enabled = true;
        dgvSwShResults.Rows.Clear();
        _cts = new CancellationTokenSource();

        var seed = (ulong)nudSwShSeed.Value;
        var shinyOnly = chkSwShShiny.Checked;
        var maxFrames = 100000;

        try
        {
            await Task.Run(() => SearchSwShRaids(seed, shinyOnly, maxFrames, _cts.Token));
        }
        catch (OperationCanceledException)
        {
            lblSwShStatus.Text = "Search cancelled";
        }
        finally
        {
            btnSwShSearch.Enabled = true;
            btnSwShStop.Enabled = false;
        }
    }

    private void BtnSwShStop_Click(object? sender, EventArgs e)
    {
        _cts?.Cancel();
    }

    private void SearchSwShRaids(ulong seed, bool shinyOnly, int maxFrames, CancellationToken ct)
    {
        var xoro = new Xoroshiro128Plus(seed);
        var results = new List<RaidResult>();

        for (int frame = 0; frame < maxFrames; frame++)
        {
            ct.ThrowIfCancellationRequested();

            if (frame % 1000 == 0)
            {
                var progress = (int)((frame / (float)maxFrames) * 100);
                Invoke(() =>
                {
                    prgSwSh.Value = progress;
                    lblSwShStatus.Text = $"Searching frame {frame:N0}...";
                });
            }

            // Generate raid data from seed
            var pokemon = GenerateRaidPokemon(xoro.Next());

            if (!shinyOnly || pokemon.IsShiny)
            {
                results.Add(new RaidResult
                {
                    Frame = frame,
                    Seed = xoro.Next().ToString("X16"),
                    IsShiny = pokemon.IsShiny,
                    Nature = pokemon.Nature,
                    Ability = pokemon.Ability,
                    IVs = pokemon.IVs
                });

                if (results.Count >= 100)
                    break;
            }
        }

        Invoke(() =>
        {
            foreach (var r in results)
            {
                dgvSwShResults.Rows.Add(r.Frame, r.Seed, "---",
                    r.IsShiny ? "★ Shiny" : "No", r.Nature, r.Ability, r.IVs);
            }
            prgSwSh.Value = 100;
            lblSwShStatus.Text = $"Found {results.Count} results";
        });
    }
    #endregion

    #region SV Search
    private async void BtnSVSearch_Click(object? sender, EventArgs e)
    {
        btnSVSearch.Enabled = false;
        btnSVStop.Enabled = true;
        dgvSVResults.Rows.Clear();
        _cts = new CancellationTokenSource();

        var seed = (uint)nudSVSeed.Value;
        var shinyOnly = chkSVShiny.Checked;

        try
        {
            await Task.Run(() => SearchSVTeraRaids(seed, shinyOnly, _cts.Token));
        }
        catch (OperationCanceledException)
        {
            lblSVStatus.Text = "Search cancelled";
        }
        finally
        {
            btnSVSearch.Enabled = true;
            btnSVStop.Enabled = false;
        }
    }

    private void BtnSVStop_Click(object? sender, EventArgs e)
    {
        _cts?.Cancel();
    }

    private void SearchSVTeraRaids(uint seed, bool shinyOnly, CancellationToken ct)
    {
        var results = new List<TeraRaidResult>();
        var maxSeeds = 100000u;

        for (uint i = 0; i < maxSeeds; i++)
        {
            ct.ThrowIfCancellationRequested();

            if (i % 1000 == 0)
            {
                var progress = (int)((i / (float)maxSeeds) * 100);
                Invoke(() =>
                {
                    prgSV.Value = progress;
                    lblSVStatus.Text = $"Checking seed {seed + i:X8}...";
                });
            }

            var pokemon = GenerateTeraRaidPokemon(seed + i);

            if (!shinyOnly || pokemon.IsShiny)
            {
                results.Add(new TeraRaidResult
                {
                    Seed = (seed + i).ToString("X8"),
                    TeraType = pokemon.TeraType,
                    IsShiny = pokemon.IsShiny,
                    Nature = pokemon.Nature,
                    Ability = pokemon.Ability,
                    IVs = pokemon.IVs,
                    Gender = pokemon.Gender
                });

                if (results.Count >= 100)
                    break;
            }
        }

        Invoke(() =>
        {
            foreach (var r in results)
            {
                dgvSVResults.Rows.Add(r.Seed, "---", r.TeraType,
                    r.IsShiny ? "★ Shiny" : "No", r.Nature, r.Ability, r.IVs, r.Gender);
            }
            prgSV.Value = 100;
            lblSVStatus.Text = $"Found {results.Count} results";
        });
    }
    #endregion

    #region General RNG Search
    private async void BtnRNGSearch_Click(object? sender, EventArgs e)
    {
        btnRNGSearch.Enabled = false;
        btnRNGStop.Enabled = true;
        dgvRNGResults.Rows.Clear();
        _cts = new CancellationTokenSource();

        var seed = (ulong)nudRNGSeed.Value;
        var minFrame = (int)nudRNGMinFrame.Value;
        var maxFrame = (int)nudRNGMaxFrame.Value;
        var shinyOnly = chkRNGShiny.Checked;

        try
        {
            await Task.Run(() => SearchGeneralRNG(seed, minFrame, maxFrame, shinyOnly, _cts.Token));
        }
        catch (OperationCanceledException)
        {
            lblRNGStatus.Text = "Search cancelled";
        }
        finally
        {
            btnRNGSearch.Enabled = true;
            btnRNGStop.Enabled = false;
        }
    }

    private void BtnRNGStop_Click(object? sender, EventArgs e)
    {
        _cts?.Cancel();
    }

    private void SearchGeneralRNG(ulong seed, int minFrame, int maxFrame, bool shinyOnly, CancellationToken ct)
    {
        var results = new List<RNGResult>();
        var tid = SAV.TID16;
        var sid = SAV.SID16;

        for (int frame = minFrame; frame <= maxFrame; frame++)
        {
            ct.ThrowIfCancellationRequested();

            if (frame % 1000 == 0)
            {
                var progress = (int)(((frame - minFrame) / (float)(maxFrame - minFrame)) * 100);
                Invoke(() =>
                {
                    prgRNG.Value = Math.Min(progress, 100);
                    lblRNGStatus.Text = $"Searching frame {frame:N0}...";
                });
            }

            // Simple LCRNG for demonstration (actual implementation varies by game)
            var rngSeed = seed;
            for (int i = 0; i < frame; i++)
                rngSeed = LCRNG(rngSeed);

            var pid = (uint)(rngSeed >> 32);
            var isShiny = IsShiny(pid, tid, sid);

            if (!shinyOnly || isShiny)
            {
                var nature = (Nature)(pid % 25);
                var ability = (int)(pid & 1);

                results.Add(new RNGResult
                {
                    Frame = frame,
                    PID = pid.ToString("X8"),
                    IsShiny = isShiny,
                    Nature = nature.ToString(),
                    Ability = ability == 0 ? "1" : "2",
                    IVs = GenerateIVs(rngSeed),
                    Gender = DetermineGender(pid)
                });

                if (results.Count >= 500)
                    break;
            }
        }

        Invoke(() =>
        {
            foreach (var r in results)
            {
                dgvRNGResults.Rows.Add(r.Frame, r.PID,
                    r.IsShiny ? "★ Shiny" : "No", r.Nature, r.Ability, r.IVs, r.Gender);
            }
            prgRNG.Value = 100;
            lblRNGStatus.Text = $"Found {results.Count} results";
        });
    }

    private static ulong LCRNG(ulong seed)
    {
        return seed * 0x5D588B656C078965UL + 0x269EC3UL;
    }

    private static bool IsShiny(uint pid, ushort tid, ushort sid)
    {
        var psv = ((pid >> 16) ^ (pid & 0xFFFF)) >> 4;
        var tsv = (uint)((tid ^ sid) >> 4);
        return psv == tsv;
    }

    private static string GenerateIVs(ulong seed)
    {
        var ivs = new int[6];
        for (int i = 0; i < 6; i++)
        {
            seed = LCRNG(seed);
            ivs[i] = (int)((seed >> 48) & 0x1F);
        }
        return string.Join("/", ivs);
    }

    private static string DetermineGender(uint pid)
    {
        var genderValue = pid & 0xFF;
        if (genderValue < 31) return "♀";
        if (genderValue < 127) return "♀";
        return "♂";
    }
    #endregion

    #region PID Calculator
    private void TxtCalc_TextChanged(object? sender, EventArgs e)
    {
        // Auto-analyze on text change
    }

    private void BtnCalcAnalyze_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!uint.TryParse(txtCalcPID.Text, System.Globalization.NumberStyles.HexNumber, null, out var pid))
            {
                lblCalcShinyType.Text = "Shiny Type: Invalid PID";
                return;
            }

            if (!ushort.TryParse(txtCalcTID.Text, out var tid) ||
                !ushort.TryParse(txtCalcSID.Text, out var sid))
            {
                lblCalcShinyType.Text = "Shiny Type: Invalid TID/SID";
                return;
            }

            var psv = ((pid >> 16) ^ (pid & 0xFFFF)) >> 4;
            var tsv = (uint)((tid ^ sid) >> 4);
            var xor = (pid >> 16) ^ (pid & 0xFFFF) ^ (uint)tid ^ (uint)sid;

            lblCalcPSV.Text = $"PSV (PID Shiny Value): {psv}";
            lblCalcTSV.Text = $"TSV (Trainer Shiny Value): {tsv}";

            if (xor == 0)
            {
                lblCalcShinyType.Text = "Shiny Type: ■ SQUARE SHINY";
                lblCalcShinyType.ForeColor = Color.Gold;
            }
            else if (xor < 16)
            {
                lblCalcShinyType.Text = "Shiny Type: ★ STAR SHINY";
                lblCalcShinyType.ForeColor = Color.Yellow;
            }
            else
            {
                lblCalcShinyType.Text = "Shiny Type: Not Shiny";
                lblCalcShinyType.ForeColor = Color.Gray;
            }
        }
        catch (Exception ex)
        {
            lblCalcShinyType.Text = $"Error: {ex.Message}";
        }
    }
    #endregion

    #region Pokemon Generation Helpers
    private RaidPokemon GenerateRaidPokemon(ulong seed)
    {
        var xoro = new Xoroshiro128Plus(seed);

        // Simplified raid generation
        var ec = (uint)xoro.NextInt();
        var pid = (uint)xoro.NextInt();

        var ivs = new int[6];
        for (int i = 0; i < 6; i++)
            ivs[i] = (int)(xoro.NextInt() % 32);

        var nature = (Nature)(xoro.NextInt() % 25);
        var ability = (int)(xoro.NextInt() % 3);

        var isShiny = IsShiny(pid, SAV.TID16, SAV.SID16);

        return new RaidPokemon
        {
            EC = ec,
            PID = pid,
            IsShiny = isShiny,
            Nature = nature.ToString(),
            Ability = ability.ToString(),
            IVs = string.Join("/", ivs)
        };
    }

    private TeraRaidPokemon GenerateTeraRaidPokemon(uint seed)
    {
        var xoro = new Xoroshiro128Plus(seed);

        var ec = (uint)xoro.NextInt();
        var fakeTID = (uint)xoro.NextInt();
        var pid = (uint)xoro.NextInt();

        // Tera raids use a different shiny calculation
        var isShiny = ((pid >> 16) ^ (pid & 0xFFFF) ^ (fakeTID >> 16) ^ (fakeTID & 0xFFFF)) < 16;

        var ivs = new int[6];
        for (int i = 0; i < 6; i++)
            ivs[i] = (int)(xoro.NextInt() % 32);

        var nature = (Nature)(xoro.NextInt() % 25);
        var ability = (int)(xoro.NextInt() % 3);
        var gender = xoro.NextInt() % 2 == 0 ? "♂" : "♀";
        var teraType = ((MoveType)(xoro.NextInt() % 18)).ToString();

        return new TeraRaidPokemon
        {
            EC = ec,
            PID = pid,
            IsShiny = isShiny,
            Nature = nature.ToString(),
            Ability = ability.ToString(),
            IVs = string.Join("/", ivs),
            Gender = gender,
            TeraType = teraType
        };
    }
    #endregion

    #region Data Classes
    private class RaidResult
    {
        public int Frame { get; set; }
        public string Seed { get; set; } = "";
        public bool IsShiny { get; set; }
        public string Nature { get; set; } = "";
        public string Ability { get; set; } = "";
        public string IVs { get; set; } = "";
    }

    private class TeraRaidResult
    {
        public string Seed { get; set; } = "";
        public string TeraType { get; set; } = "";
        public bool IsShiny { get; set; }
        public string Nature { get; set; } = "";
        public string Ability { get; set; } = "";
        public string IVs { get; set; } = "";
        public string Gender { get; set; } = "";
    }

    private class RNGResult
    {
        public int Frame { get; set; }
        public string PID { get; set; } = "";
        public bool IsShiny { get; set; }
        public string Nature { get; set; } = "";
        public string Ability { get; set; } = "";
        public string IVs { get; set; } = "";
        public string Gender { get; set; } = "";
    }

    private class RaidPokemon
    {
        public uint EC { get; set; }
        public uint PID { get; set; }
        public bool IsShiny { get; set; }
        public string Nature { get; set; } = "";
        public string Ability { get; set; } = "";
        public string IVs { get; set; } = "";
    }

    private class TeraRaidPokemon
    {
        public uint EC { get; set; }
        public uint PID { get; set; }
        public bool IsShiny { get; set; }
        public string Nature { get; set; } = "";
        public string Ability { get; set; } = "";
        public string IVs { get; set; } = "";
        public string Gender { get; set; } = "";
        public string TeraType { get; set; } = "";
    }
    #endregion
}

/// <summary>
/// Xoroshiro128+ PRNG implementation for Pokemon RNG
/// </summary>
public class Xoroshiro128Plus
{
    private ulong s0, s1;

    public Xoroshiro128Plus(ulong seed)
    {
        s0 = seed;
        s1 = 0x82A2B175229D6A5B;
    }

    public Xoroshiro128Plus(ulong s0, ulong s1)
    {
        this.s0 = s0;
        this.s1 = s1;
    }

    public ulong Next()
    {
        var result = s0 + s1;
        s1 ^= s0;
        s0 = RotateLeft(s0, 24) ^ s1 ^ (s1 << 16);
        s1 = RotateLeft(s1, 37);
        return result;
    }

    public uint NextInt() => (uint)Next();

    public uint NextInt(uint max)
    {
        if (max == 0) return 0;
        return (uint)(Next() % max);
    }

    private static ulong RotateLeft(ulong x, int k)
    {
        return (x << k) | (x >> (64 - k));
    }
}
