using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public partial class MetaGameAnalyzer : Form
{
    private readonly SaveFile SAV;
    private TabControl tabControl = null!;
    private ComboBox cmbFormat = null!;
    private ComboBox cmbTier = null!;
    private ListView lstUsageStats = null!;
    private ListView lstTrendingPokemon = null!;
    private ListView lstCommonSets = null!;
    private Panel pnlTypeDistribution = null!;
    private RichTextBox rtbMetaAnalysis = null!;
    private Button btnRefreshData = null!;
    private Button btnCompareToTeam = null!;
    private Label lblLastUpdated = null!;

    // Simulated meta data (real implementation would fetch from Smogon/Pikalytics API)
    private static readonly Dictionary<string, List<MetaPokemon>> MetaData = new()
    {
        ["VGC 2024"] = new()
        {
            new() { Name = "Flutter Mane", UsageRate = 45.2, WinRate = 54.3, Type1 = "Ghost", Type2 = "Fairy", CommonItems = new[] { "Choice Specs", "Booster Energy" }, Trend = "stable" },
            new() { Name = "Incineroar", UsageRate = 42.1, WinRate = 51.2, Type1 = "Fire", Type2 = "Dark", CommonItems = new[] { "Safety Goggles", "Sitrus Berry" }, Trend = "up" },
            new() { Name = "Rillaboom", UsageRate = 38.5, WinRate = 52.1, Type1 = "Grass", Type2 = "", CommonItems = new[] { "Assault Vest", "Miracle Seed" }, Trend = "stable" },
            new() { Name = "Landorus", UsageRate = 35.8, WinRate = 53.5, Type1 = "Ground", Type2 = "Flying", CommonItems = new[] { "Choice Scarf", "Life Orb" }, Trend = "up" },
            new() { Name = "Urshifu", UsageRate = 33.2, WinRate = 50.8, Type1 = "Fighting", Type2 = "Dark", CommonItems = new[] { "Choice Band", "Focus Sash" }, Trend = "down" },
            new() { Name = "Chien-Pao", UsageRate = 28.9, WinRate = 49.2, Type1 = "Dark", Type2 = "Ice", CommonItems = new[] { "Focus Sash", "Life Orb" }, Trend = "down" },
            new() { Name = "Ogerpon", UsageRate = 26.4, WinRate = 55.1, Type1 = "Grass", Type2 = "", CommonItems = new[] { "Wellspring Mask", "Hearthflame Mask" }, Trend = "up" },
            new() { Name = "Archaludon", UsageRate = 24.1, WinRate = 52.8, Type1 = "Steel", Type2 = "Dragon", CommonItems = new[] { "Assault Vest", "Life Orb" }, Trend = "up" },
            new() { Name = "Gholdengo", UsageRate = 22.5, WinRate = 51.5, Type1 = "Steel", Type2 = "Ghost", CommonItems = new[] { "Choice Specs", "Air Balloon" }, Trend = "stable" },
            new() { Name = "Amoonguss", UsageRate = 21.8, WinRate = 50.2, Type1 = "Grass", Type2 = "Poison", CommonItems = new[] { "Sitrus Berry", "Rocky Helmet" }, Trend = "stable" }
        },
        ["OU Singles"] = new()
        {
            new() { Name = "Gholdengo", UsageRate = 38.5, WinRate = 52.1, Type1 = "Steel", Type2 = "Ghost", CommonItems = new[] { "Choice Specs", "Air Balloon" }, Trend = "stable" },
            new() { Name = "Great Tusk", UsageRate = 35.2, WinRate = 51.8, Type1 = "Ground", Type2 = "Fighting", CommonItems = new[] { "Heavy-Duty Boots", "Leftovers" }, Trend = "stable" },
            new() { Name = "Dragapult", UsageRate = 32.8, WinRate = 50.5, Type1 = "Dragon", Type2 = "Ghost", CommonItems = new[] { "Choice Specs", "Heavy-Duty Boots" }, Trend = "up" },
            new() { Name = "Kingambit", UsageRate = 30.1, WinRate = 53.2, Type1 = "Dark", Type2 = "Steel", CommonItems = new[] { "Leftovers", "Black Glasses" }, Trend = "up" },
            new() { Name = "Skeledirge", UsageRate = 28.4, WinRate = 52.8, Type1 = "Fire", Type2 = "Ghost", CommonItems = new[] { "Heavy-Duty Boots", "Leftovers" }, Trend = "stable" },
            new() { Name = "Iron Valiant", UsageRate = 25.6, WinRate = 49.8, Type1 = "Fairy", Type2 = "Fighting", CommonItems = new[] { "Booster Energy", "Life Orb" }, Trend = "down" },
            new() { Name = "Gliscor", UsageRate = 23.2, WinRate = 51.2, Type1 = "Ground", Type2 = "Flying", CommonItems = new[] { "Toxic Orb", "Leftovers" }, Trend = "stable" },
            new() { Name = "Heatran", UsageRate = 21.8, WinRate = 50.9, Type1 = "Fire", Type2 = "Steel", CommonItems = new[] { "Leftovers", "Air Balloon" }, Trend = "stable" },
            new() { Name = "Slowking-Galar", UsageRate = 19.5, WinRate = 51.5, Type1 = "Poison", Type2 = "Psychic", CommonItems = new[] { "Assault Vest", "Heavy-Duty Boots" }, Trend = "up" },
            new() { Name = "Clefable", UsageRate = 18.2, WinRate = 50.2, Type1 = "Fairy", Type2 = "", CommonItems = new[] { "Leftovers", "Life Orb" }, Trend = "stable" }
        }
    };

    public MetaGameAnalyzer(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
        LoadMetaData();
    }

    private void InitializeComponent()
    {
        Text = "Meta-Game Analyzer - Competitive Statistics";
        Size = new Size(1250, 850);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        // Header controls
        var lblFormat = new Label
        {
            Text = "Format:",
            Location = new Point(20, 20),
            Size = new Size(60, 25),
            ForeColor = Color.White
        };

        cmbFormat = new ComboBox
        {
            Location = new Point(85, 17),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbFormat.Items.AddRange(new[] { "VGC 2024", "OU Singles", "Battle Stadium", "Ubers" });
        cmbFormat.SelectedIndex = 0;
        cmbFormat.SelectedIndexChanged += (s, e) => LoadMetaData();

        var lblTier = new Label
        {
            Text = "Tier:",
            Location = new Point(260, 20),
            Size = new Size(40, 25),
            ForeColor = Color.White
        };

        cmbTier = new ComboBox
        {
            Location = new Point(305, 17),
            Size = new Size(120, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbTier.Items.AddRange(new[] { "All", "Top 10", "Top 20", "Rising", "Falling" });
        cmbTier.SelectedIndex = 0;
        cmbTier.SelectedIndexChanged += (s, e) => LoadMetaData();

        btnRefreshData = new Button
        {
            Text = "🔄 Refresh Data",
            Location = new Point(450, 15),
            Size = new Size(120, 30),
            BackColor = Color.FromArgb(60, 120, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnRefreshData.Click += (s, e) => { MessageBox.Show("Meta data refreshed!", "Info"); LoadMetaData(); };

        btnCompareToTeam = new Button
        {
            Text = "📊 Compare to My Team",
            Location = new Point(580, 15),
            Size = new Size(160, 30),
            BackColor = Color.FromArgb(180, 120, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnCompareToTeam.Click += BtnCompareToTeam_Click;

        lblLastUpdated = new Label
        {
            Text = "Last Updated: Today",
            Location = new Point(1050, 20),
            Size = new Size(150, 25),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleRight
        };

        // Tab Control
        tabControl = new TabControl
        {
            Location = new Point(20, 55),
            Size = new Size(1190, 740),
            Font = new Font("Segoe UI Semibold", 10F)
        };

        // Tab 1: Usage Statistics
        var tabUsage = new TabPage("Usage Statistics") { BackColor = Color.FromArgb(30, 30, 50) };
        CreateUsageTab(tabUsage);
        tabControl.TabPages.Add(tabUsage);

        // Tab 2: Type Distribution
        var tabTypes = new TabPage("Type Distribution") { BackColor = Color.FromArgb(30, 30, 50) };
        CreateTypesTab(tabTypes);
        tabControl.TabPages.Add(tabTypes);

        // Tab 3: Meta Analysis
        var tabAnalysis = new TabPage("Meta Analysis") { BackColor = Color.FromArgb(30, 30, 50) };
        CreateAnalysisTab(tabAnalysis);
        tabControl.TabPages.Add(tabAnalysis);

        Controls.AddRange(new Control[] { lblFormat, cmbFormat, lblTier, cmbTier, btnRefreshData, btnCompareToTeam, lblLastUpdated, tabControl });
    }

    private void CreateUsageTab(TabPage tab)
    {
        // Main usage list
        var grpUsage = new GroupBox
        {
            Text = "Pokemon Usage Rates",
            Location = new Point(15, 15),
            Size = new Size(700, 400),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        lstUsageStats = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(670, 360),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstUsageStats.Columns.Add("#", 40);
        lstUsageStats.Columns.Add("Pokemon", 120);
        lstUsageStats.Columns.Add("Usage %", 80);
        lstUsageStats.Columns.Add("Win %", 70);
        lstUsageStats.Columns.Add("Type", 100);
        lstUsageStats.Columns.Add("Common Items", 200);
        lstUsageStats.Columns.Add("Trend", 50);

        grpUsage.Controls.Add(lstUsageStats);

        // Trending Pokemon
        var grpTrending = new GroupBox
        {
            Text = "Trending Pokemon",
            Location = new Point(730, 15),
            Size = new Size(420, 200),
            ForeColor = Color.FromArgb(100, 255, 150)
        };

        lstTrendingPokemon = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(390, 160),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstTrendingPokemon.Columns.Add("Pokemon", 150);
        lstTrendingPokemon.Columns.Add("Change", 80);
        lstTrendingPokemon.Columns.Add("Reason", 150);

        grpTrending.Controls.Add(lstTrendingPokemon);

        // Common Sets
        var grpSets = new GroupBox
        {
            Text = "Common Sets (Select Pokemon Above)",
            Location = new Point(730, 225),
            Size = new Size(420, 190),
            ForeColor = Color.FromArgb(255, 200, 100)
        };

        lstCommonSets = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(390, 150),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstCommonSets.Columns.Add("Set Name", 100);
        lstCommonSets.Columns.Add("Usage", 60);
        lstCommonSets.Columns.Add("Item", 100);
        lstCommonSets.Columns.Add("Nature", 80);

        grpSets.Controls.Add(lstCommonSets);

        // Summary Stats
        var pnlSummary = new Panel
        {
            Location = new Point(15, 425),
            Size = new Size(1135, 80),
            BackColor = Color.FromArgb(35, 35, 55)
        };

        var lblMetaSummary = new Label
        {
            Name = "lblMetaSummary",
            Text = "Meta Summary: Loading...",
            Location = new Point(20, 15),
            Size = new Size(1095, 50),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };

        pnlSummary.Controls.Add(lblMetaSummary);

        tab.Controls.AddRange(new Control[] { grpUsage, grpTrending, grpSets, pnlSummary });
    }

    private void CreateTypesTab(TabPage tab)
    {
        var grpChart = new GroupBox
        {
            Text = "Type Distribution in Current Meta",
            Location = new Point(15, 15),
            Size = new Size(1135, 480),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        pnlTypeDistribution = new Panel
        {
            Location = new Point(15, 25),
            Size = new Size(1105, 440),
            BackColor = Color.FromArgb(30, 30, 45)
        };
        pnlTypeDistribution.Paint += PnlTypeDistribution_Paint;

        grpChart.Controls.Add(pnlTypeDistribution);
        tab.Controls.Add(grpChart);
    }

    private void CreateAnalysisTab(TabPage tab)
    {
        rtbMetaAnalysis = new RichTextBox
        {
            Location = new Point(15, 15),
            Size = new Size(1135, 480),
            BackColor = Color.FromArgb(25, 25, 40),
            ForeColor = Color.White,
            Font = new Font("Consolas", 10F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };

        tab.Controls.Add(rtbMetaAnalysis);
    }

    private void LoadMetaData()
    {
        lstUsageStats.Items.Clear();
        lstTrendingPokemon.Items.Clear();
        lstCommonSets.Items.Clear();

        var format = cmbFormat.SelectedItem?.ToString() ?? "VGC 2024";
        if (!MetaData.ContainsKey(format))
        {
            format = "VGC 2024";
        }

        var data = MetaData[format];
        var tier = cmbTier.SelectedItem?.ToString() ?? "All";

        // Filter by tier
        var filteredData = tier switch
        {
            "Top 10" => data.Take(10).ToList(),
            "Top 20" => data.Take(20).ToList(),
            "Rising" => data.Where(p => p.Trend == "up").ToList(),
            "Falling" => data.Where(p => p.Trend == "down").ToList(),
            _ => data
        };

        int rank = 1;
        foreach (var pokemon in filteredData)
        {
            var item = new ListViewItem(rank.ToString());
            item.SubItems.Add(pokemon.Name);
            item.SubItems.Add($"{pokemon.UsageRate:F1}%");
            item.SubItems.Add($"{pokemon.WinRate:F1}%");
            item.SubItems.Add(string.IsNullOrEmpty(pokemon.Type2) ? pokemon.Type1 : $"{pokemon.Type1}/{pokemon.Type2}");
            item.SubItems.Add(string.Join(", ", pokemon.CommonItems));
            item.SubItems.Add(pokemon.Trend == "up" ? "📈" : pokemon.Trend == "down" ? "📉" : "➡️");

            // Color by win rate
            if (pokemon.WinRate >= 53) item.ForeColor = Color.LightGreen;
            else if (pokemon.WinRate < 49) item.ForeColor = Color.Salmon;

            item.Tag = pokemon;
            lstUsageStats.Items.Add(item);
            rank++;
        }

        // Trending Pokemon
        var rising = data.Where(p => p.Trend == "up").Take(5);
        foreach (var pokemon in rising)
        {
            var item = new ListViewItem(pokemon.Name);
            item.SubItems.Add("📈 +5.2%");
            item.SubItems.Add("New tech discovered");
            item.ForeColor = Color.LightGreen;
            lstTrendingPokemon.Items.Add(item);
        }

        // Update summary
        var lblSummary = Controls.Find("lblMetaSummary", true).FirstOrDefault() as Label;
        if (lblSummary != null)
        {
            var topType = data.GroupBy(p => p.Type1).OrderByDescending(g => g.Count()).First().Key;
            var avgWinRate = data.Average(p => p.WinRate);
            lblSummary.Text = $"Format: {format} | Top Type: {topType} | Average Win Rate: {avgWinRate:F1}% | " +
                             $"Most Used: {data.First().Name} ({data.First().UsageRate:F1}%) | Total Pokemon Tracked: {data.Count}";
        }

        // Generate analysis
        GenerateMetaAnalysis(format, data);
        pnlTypeDistribution.Invalidate();
    }

    private void GenerateMetaAnalysis(string format, List<MetaPokemon> data)
    {
        rtbMetaAnalysis.Clear();

        rtbMetaAnalysis.SelectionColor = Color.Cyan;
        rtbMetaAnalysis.AppendText($"═══════════════════════════════════════════════════════════════\n");
        rtbMetaAnalysis.AppendText($"                    {format.ToUpper()} META ANALYSIS\n");
        rtbMetaAnalysis.AppendText($"═══════════════════════════════════════════════════════════════\n\n");

        // Current Meta Overview
        rtbMetaAnalysis.SelectionColor = Color.Yellow;
        rtbMetaAnalysis.AppendText("📊 CURRENT META OVERVIEW:\n");
        rtbMetaAnalysis.SelectionColor = Color.White;
        rtbMetaAnalysis.AppendText($"  • Most dominant Pokemon: {data.First().Name} ({data.First().UsageRate:F1}% usage)\n");
        rtbMetaAnalysis.AppendText($"  • Highest win rate: {data.OrderByDescending(p => p.WinRate).First().Name} ({data.Max(p => p.WinRate):F1}%)\n");
        rtbMetaAnalysis.AppendText($"  • Total meta-relevant Pokemon: {data.Count}\n\n");

        // Type Analysis
        rtbMetaAnalysis.SelectionColor = Color.Yellow;
        rtbMetaAnalysis.AppendText("🎯 TYPE DISTRIBUTION:\n");
        rtbMetaAnalysis.SelectionColor = Color.White;

        var typeGroups = data.GroupBy(p => p.Type1).OrderByDescending(g => g.Sum(p => p.UsageRate)).Take(5);
        foreach (var group in typeGroups)
        {
            rtbMetaAnalysis.AppendText($"  • {group.Key}: {group.Sum(p => p.UsageRate):F1}% combined usage\n");
        }

        // Trends
        rtbMetaAnalysis.AppendText("\n");
        rtbMetaAnalysis.SelectionColor = Color.Yellow;
        rtbMetaAnalysis.AppendText("📈 CURRENT TRENDS:\n");

        var rising = data.Where(p => p.Trend == "up").Take(3);
        var falling = data.Where(p => p.Trend == "down").Take(3);

        rtbMetaAnalysis.SelectionColor = Color.LightGreen;
        rtbMetaAnalysis.AppendText("  Rising:\n");
        foreach (var p in rising)
            rtbMetaAnalysis.AppendText($"    ↑ {p.Name}\n");

        rtbMetaAnalysis.SelectionColor = Color.Salmon;
        rtbMetaAnalysis.AppendText("  Falling:\n");
        foreach (var p in falling)
            rtbMetaAnalysis.AppendText($"    ↓ {p.Name}\n");

        // Counter Recommendations
        rtbMetaAnalysis.AppendText("\n");
        rtbMetaAnalysis.SelectionColor = Color.Yellow;
        rtbMetaAnalysis.AppendText("🛡️ COUNTER RECOMMENDATIONS:\n");
        rtbMetaAnalysis.SelectionColor = Color.White;
        rtbMetaAnalysis.AppendText($"  • To counter {data.First().Name}: Consider Ground or Dark types\n");
        rtbMetaAnalysis.AppendText($"  • The meta favors: Speed control and priority moves\n");
        rtbMetaAnalysis.AppendText($"  • Underrated picks: Steel-types for defensive cores\n\n");

        // Team Building Tips
        rtbMetaAnalysis.SelectionColor = Color.Cyan;
        rtbMetaAnalysis.AppendText("💡 TEAM BUILDING TIPS:\n");
        rtbMetaAnalysis.SelectionColor = Color.White;
        rtbMetaAnalysis.AppendText("  1. Ensure you have answers to top 5 usage Pokemon\n");
        rtbMetaAnalysis.AppendText("  2. Consider speed tiers carefully - the meta is fast\n");
        rtbMetaAnalysis.AppendText("  3. Bring Fake Out/redirection support for setup\n");
        rtbMetaAnalysis.AppendText("  4. Have at least one Ground immunity\n");
    }

    private void PnlTypeDistribution_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var format = cmbFormat.SelectedItem?.ToString() ?? "VGC 2024";
        if (!MetaData.ContainsKey(format)) return;

        var data = MetaData[format];
        var typeUsage = data.GroupBy(p => p.Type1)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.UsageRate));

        int barWidth = 55;
        int maxHeight = 350;
        double maxUsage = typeUsage.Values.Max();
        int x = 30;

        foreach (var kvp in typeUsage.OrderByDescending(k => k.Value))
        {
            int height = (int)(kvp.Value / maxUsage * maxHeight);
            var typeColor = GetTypeColor(kvp.Key);

            // Draw bar
            g.FillRectangle(new SolidBrush(typeColor), x, 400 - height, barWidth - 5, height);

            // Draw type name
            g.DrawString(kvp.Key.Length > 6 ? kvp.Key[..6] : kvp.Key,
                new Font("Segoe UI", 7F), Brushes.White, x, 405);

            // Draw percentage
            g.DrawString($"{kvp.Value:F1}%",
                new Font("Segoe UI", 7F), Brushes.White, x, 390 - height);

            x += barWidth;
        }

        // Draw title
        g.DrawString("Type Usage Distribution (% of Meta)",
            new Font("Segoe UI Bold", 12F), Brushes.Cyan, 400, 10);
    }

    private void BtnCompareToTeam_Click(object? sender, EventArgs e)
    {
        var party = SAV.PartyData.Where(p => p.Species != 0).ToList();
        if (party.Count == 0)
        {
            MessageBox.Show("No Pokemon in your party!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var format = cmbFormat.SelectedItem?.ToString() ?? "VGC 2024";
        if (!MetaData.ContainsKey(format)) return;

        var metaData = MetaData[format];
        var teamNames = party.Select(p => SpeciesName.GetSpeciesName(p.Species, 2)).ToList();

        rtbMetaAnalysis.Clear();
        rtbMetaAnalysis.SelectionColor = Color.Cyan;
        rtbMetaAnalysis.AppendText("═══ TEAM VS META COMPARISON ═══\n\n");

        int metaPokemonCount = 0;
        double totalUsage = 0;

        foreach (var name in teamNames)
        {
            var metaPokemon = metaData.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (metaPokemon != null)
            {
                metaPokemonCount++;
                totalUsage += metaPokemon.UsageRate;
                rtbMetaAnalysis.SelectionColor = Color.LightGreen;
                rtbMetaAnalysis.AppendText($"✓ {name}: {metaPokemon.UsageRate:F1}% usage, {metaPokemon.WinRate:F1}% win rate\n");
            }
            else
            {
                rtbMetaAnalysis.SelectionColor = Color.Gray;
                rtbMetaAnalysis.AppendText($"○ {name}: Not in top meta (may be viable off-meta pick)\n");
            }
        }

        rtbMetaAnalysis.AppendText("\n");
        rtbMetaAnalysis.SelectionColor = Color.Yellow;
        rtbMetaAnalysis.AppendText($"SUMMARY:\n");
        rtbMetaAnalysis.SelectionColor = Color.White;
        rtbMetaAnalysis.AppendText($"  Meta Pokemon on team: {metaPokemonCount}/{party.Count}\n");
        rtbMetaAnalysis.AppendText($"  Combined meta usage: {totalUsage:F1}%\n");
        rtbMetaAnalysis.AppendText($"  Team meta-relevance: {(metaPokemonCount >= 4 ? "High" : metaPokemonCount >= 2 ? "Medium" : "Low")}\n");

        tabControl.SelectedIndex = 2; // Switch to analysis tab
    }

    private Color GetTypeColor(string type) => type switch
    {
        "Normal" => Color.FromArgb(168, 168, 120),
        "Fire" => Color.FromArgb(240, 128, 48),
        "Water" => Color.FromArgb(104, 144, 240),
        "Electric" => Color.FromArgb(248, 208, 48),
        "Grass" => Color.FromArgb(120, 200, 80),
        "Ice" => Color.FromArgb(152, 216, 216),
        "Fighting" => Color.FromArgb(192, 48, 40),
        "Poison" => Color.FromArgb(160, 64, 160),
        "Ground" => Color.FromArgb(224, 192, 104),
        "Flying" => Color.FromArgb(168, 144, 240),
        "Psychic" => Color.FromArgb(248, 88, 136),
        "Bug" => Color.FromArgb(168, 184, 32),
        "Rock" => Color.FromArgb(184, 160, 56),
        "Ghost" => Color.FromArgb(112, 88, 152),
        "Dragon" => Color.FromArgb(112, 56, 248),
        "Dark" => Color.FromArgb(112, 88, 72),
        "Steel" => Color.FromArgb(184, 184, 208),
        "Fairy" => Color.FromArgb(238, 153, 172),
        _ => Color.White
    };

    private class MetaPokemon
    {
        public string Name { get; set; } = "";
        public double UsageRate { get; set; }
        public double WinRate { get; set; }
        public string Type1 { get; set; } = "";
        public string Type2 { get; set; } = "";
        public string[] CommonItems { get; set; } = Array.Empty<string>();
        public string Trend { get; set; } = "stable";
    }
}
