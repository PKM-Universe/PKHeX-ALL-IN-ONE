using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public partial class ProfessionalReportExporter : Form
{
    private readonly SaveFile SAV;
    private CheckedListBox clbReportTypes = null!;
    private ComboBox cmbFormat = null!;
    private TextBox txtOutputPath = null!;
    private CheckBox chkIncludeStats = null!;
    private CheckBox chkIncludeMoves = null!;
    private CheckBox chkIncludeIVsEVs = null!;
    private CheckBox chkIncludeOrigin = null!;
    private CheckBox chkDetailedAnalysis = null!;
    private RichTextBox rtbPreview = null!;
    private ProgressBar pbExport = null!;
    private Button btnExport = null!;
    private Button btnPreview = null!;

    public ProfessionalReportExporter(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Professional Report Exporter";
        Size = new Size(1100, 750);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        // Report Type Selection
        var grpReportTypes = new GroupBox
        {
            Text = "Select Report Types",
            Location = new Point(20, 20),
            Size = new Size(300, 250),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        clbReportTypes = new CheckedListBox
        {
            Location = new Point(15, 25),
            Size = new Size(270, 210),
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            CheckOnClick = true
        };
        clbReportTypes.Items.AddRange(new object[]
        {
            "Team Analysis Report",
            "Full Collection Inventory",
            "Competitive Viability Report",
            "Breeding Records",
            "Shiny Collection Catalog",
            "Living Dex Progress",
            "Trade History Summary",
            "Tournament Results",
            "Training Progress Report"
        });
        clbReportTypes.SetItemChecked(0, true);

        grpReportTypes.Controls.Add(clbReportTypes);

        // Export Options
        var grpOptions = new GroupBox
        {
            Text = "Export Options",
            Location = new Point(340, 20),
            Size = new Size(300, 250),
            ForeColor = Color.FromArgb(255, 200, 100)
        };

        var lblFormat = new Label
        {
            Text = "Export Format:",
            Location = new Point(15, 30),
            Size = new Size(100, 25),
            ForeColor = Color.White
        };

        cmbFormat = new ComboBox
        {
            Location = new Point(120, 27),
            Size = new Size(160, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbFormat.Items.AddRange(new[] { "HTML Report", "CSV Spreadsheet", "JSON Data", "Plain Text", "Markdown" });
        cmbFormat.SelectedIndex = 0;

        chkIncludeStats = new CheckBox
        {
            Text = "Include Base Stats",
            Location = new Point(15, 70),
            Size = new Size(200, 25),
            ForeColor = Color.White,
            Checked = true
        };

        chkIncludeMoves = new CheckBox
        {
            Text = "Include Movesets",
            Location = new Point(15, 100),
            Size = new Size(200, 25),
            ForeColor = Color.White,
            Checked = true
        };

        chkIncludeIVsEVs = new CheckBox
        {
            Text = "Include IVs/EVs",
            Location = new Point(15, 130),
            Size = new Size(200, 25),
            ForeColor = Color.White,
            Checked = true
        };

        chkIncludeOrigin = new CheckBox
        {
            Text = "Include Origin Info",
            Location = new Point(15, 160),
            Size = new Size(200, 25),
            ForeColor = Color.White,
            Checked = true
        };

        chkDetailedAnalysis = new CheckBox
        {
            Text = "Include Detailed Analysis",
            Location = new Point(15, 190),
            Size = new Size(200, 25),
            ForeColor = Color.White,
            Checked = true
        };

        grpOptions.Controls.AddRange(new Control[] { lblFormat, cmbFormat, chkIncludeStats, chkIncludeMoves, chkIncludeIVsEVs, chkIncludeOrigin, chkDetailedAnalysis });

        // Output Path
        var grpOutput = new GroupBox
        {
            Text = "Output Location",
            Location = new Point(660, 20),
            Size = new Size(400, 100),
            ForeColor = Color.FromArgb(100, 255, 150)
        };

        txtOutputPath = new TextBox
        {
            Location = new Point(15, 30),
            Size = new Size(300, 25),
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White,
            Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PKM_Report")
        };

        var btnBrowse = new Button
        {
            Text = "...",
            Location = new Point(320, 28),
            Size = new Size(60, 28),
            BackColor = Color.FromArgb(60, 60, 90),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnBrowse.Click += (s, e) =>
        {
            using var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
                txtOutputPath.Text = fbd.SelectedPath;
        };

        grpOutput.Controls.AddRange(new Control[] { txtOutputPath, btnBrowse });

        // Action Buttons
        btnPreview = new Button
        {
            Text = "👁️ Preview Report",
            Location = new Point(660, 140),
            Size = new Size(180, 45),
            BackColor = Color.FromArgb(60, 120, 180),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 11F),
            FlatStyle = FlatStyle.Flat
        };
        btnPreview.Click += BtnPreview_Click;

        btnExport = new Button
        {
            Text = "📥 Export Reports",
            Location = new Point(860, 140),
            Size = new Size(180, 45),
            BackColor = Color.FromArgb(60, 180, 80),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 11F),
            FlatStyle = FlatStyle.Flat
        };
        btnExport.Click += BtnExport_Click;

        pbExport = new ProgressBar
        {
            Location = new Point(660, 200),
            Size = new Size(380, 25),
            Style = ProgressBarStyle.Continuous
        };

        // Preview Panel
        var grpPreview = new GroupBox
        {
            Text = "Report Preview",
            Location = new Point(20, 280),
            Size = new Size(1040, 400),
            ForeColor = Color.FromArgb(200, 200, 200)
        };

        rtbPreview = new RichTextBox
        {
            Location = new Point(15, 25),
            Size = new Size(1010, 360),
            BackColor = Color.FromArgb(20, 20, 35),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };

        grpPreview.Controls.Add(rtbPreview);

        Controls.AddRange(new Control[] { grpReportTypes, grpOptions, grpOutput, btnPreview, btnExport, pbExport, grpPreview });
    }

    private void BtnPreview_Click(object? sender, EventArgs e)
    {
        var selectedReports = clbReportTypes.CheckedItems.Cast<string>().ToList();
        if (selectedReports.Count == 0)
        {
            MessageBox.Show("Please select at least one report type!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        GeneratePreview(selectedReports);
    }

    private void GeneratePreview(List<string> reportTypes)
    {
        rtbPreview.Clear();

        foreach (var reportType in reportTypes)
        {
            switch (reportType)
            {
                case "Team Analysis Report":
                    GenerateTeamAnalysisPreview();
                    break;
                case "Full Collection Inventory":
                    GenerateInventoryPreview();
                    break;
                case "Competitive Viability Report":
                    GenerateViabilityPreview();
                    break;
                case "Shiny Collection Catalog":
                    GenerateShinyCatalogPreview();
                    break;
                case "Living Dex Progress":
                    GenerateLivingDexPreview();
                    break;
                default:
                    AppendPreviewHeader(reportType);
                    rtbPreview.AppendText("  [Report data will be generated on export]\n\n");
                    break;
            }
        }
    }

    private void AppendPreviewHeader(string title)
    {
        rtbPreview.SelectionColor = Color.Cyan;
        rtbPreview.AppendText($"═══════════════════════════════════════════════════════════════\n");
        rtbPreview.AppendText($"  {title.ToUpper()}\n");
        rtbPreview.AppendText($"═══════════════════════════════════════════════════════════════\n\n");
    }

    private void GenerateTeamAnalysisPreview()
    {
        AppendPreviewHeader("Team Analysis Report");

        var party = SAV.PartyData.Where(p => p.Species != 0).ToList();
        if (party.Count == 0)
        {
            rtbPreview.SelectionColor = Color.Gray;
            rtbPreview.AppendText("  No Pokemon in party.\n\n");
            return;
        }

        rtbPreview.SelectionColor = Color.Yellow;
        rtbPreview.AppendText($"  Team Size: {party.Count} Pokemon\n");
        rtbPreview.AppendText($"  Total BST: {party.Sum(p => p.PersonalInfo.GetBaseStatTotal())}\n\n");

        rtbPreview.SelectionColor = Color.White;
        foreach (var pk in party)
        {
            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            var shiny = pk.IsShiny ? "★ " : "";
            rtbPreview.AppendText($"  {shiny}{name}\n");

            if (chkIncludeStats.Checked)
            {
                rtbPreview.SelectionColor = Color.Gray;
                rtbPreview.AppendText($"    BST: {pk.PersonalInfo.GetBaseStatTotal()} | ");
                rtbPreview.AppendText($"HP:{pk.PersonalInfo.HP} ATK:{pk.PersonalInfo.ATK} DEF:{pk.PersonalInfo.DEF} ");
                rtbPreview.AppendText($"SPA:{pk.PersonalInfo.SPA} SPD:{pk.PersonalInfo.SPD} SPE:{pk.PersonalInfo.SPE}\n");
            }

            if (chkIncludeIVsEVs.Checked)
            {
                rtbPreview.SelectionColor = Color.LightGreen;
                rtbPreview.AppendText($"    IVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}\n");
                rtbPreview.AppendText($"    EVs: {pk.EV_HP}/{pk.EV_ATK}/{pk.EV_DEF}/{pk.EV_SPA}/{pk.EV_SPD}/{pk.EV_SPE}\n");
            }

            if (chkIncludeMoves.Checked)
            {
                rtbPreview.SelectionColor = Color.Cyan;
                var moves = new[] { pk.Move1, pk.Move2, pk.Move3, pk.Move4 }
                    .Where(m => m != 0)
                    .Select(m => MoveNameHelper.GetMoveName(m));
                rtbPreview.AppendText($"    Moves: {string.Join(", ", moves)}\n");
            }

            rtbPreview.SelectionColor = Color.White;
            rtbPreview.AppendText("\n");
        }

        // Type coverage analysis
        if (chkDetailedAnalysis.Checked)
        {
            rtbPreview.SelectionColor = Color.Yellow;
            rtbPreview.AppendText("  TYPE COVERAGE ANALYSIS:\n");
            rtbPreview.SelectionColor = Color.White;

            var types = party.SelectMany(p => new[] {
                GetTypeName((int)p.PersonalInfo.Type1),
                p.PersonalInfo.Type2 != p.PersonalInfo.Type1 ? GetTypeName((int)p.PersonalInfo.Type2) : null
            }).Where(t => t != null).Distinct().ToList();

            rtbPreview.AppendText($"    Types represented: {string.Join(", ", types)}\n");
            rtbPreview.AppendText($"    Coverage: {types.Count}/18 types\n\n");
        }
    }

    private void GenerateInventoryPreview()
    {
        AppendPreviewHeader("Full Collection Inventory");

        var allPokemon = SAV.BoxData.Where(p => p.Species != 0).ToList();
        var party = SAV.PartyData.Where(p => p.Species != 0).ToList();

        rtbPreview.SelectionColor = Color.Yellow;
        rtbPreview.AppendText($"  SUMMARY:\n");
        rtbPreview.SelectionColor = Color.White;
        rtbPreview.AppendText($"    Party: {party.Count} Pokemon\n");
        rtbPreview.AppendText($"    Boxes: {allPokemon.Count} Pokemon\n");
        rtbPreview.AppendText($"    Total: {party.Count + allPokemon.Count} Pokemon\n\n");

        // Unique species
        var uniqueSpecies = allPokemon.Concat(party).Select(p => p.Species).Distinct().Count();
        rtbPreview.AppendText($"    Unique Species: {uniqueSpecies}\n");

        // Shinies
        var shinies = allPokemon.Concat(party).Count(p => p.IsShiny);
        rtbPreview.AppendText($"    Shiny Pokemon: {shinies}\n\n");

        // Sample entries
        rtbPreview.SelectionColor = Color.Gray;
        rtbPreview.AppendText("  [First 10 entries shown in preview]\n\n");
        rtbPreview.SelectionColor = Color.White;

        foreach (var pk in allPokemon.Take(10))
        {
            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            var shiny = pk.IsShiny ? "★" : " ";
            rtbPreview.AppendText($"    {shiny} {name,-15} Lv.{pk.CurrentLevel,-3} ");

            if (chkIncludeOrigin.Checked)
            {
                rtbPreview.SelectionColor = Color.Gray;
                rtbPreview.AppendText($"OT: {pk.OriginalTrainerName}");
            }
            rtbPreview.SelectionColor = Color.White;
            rtbPreview.AppendText("\n");
        }
        rtbPreview.AppendText("\n");
    }

    private void GenerateViabilityPreview()
    {
        AppendPreviewHeader("Competitive Viability Report");

        var allPokemon = SAV.PartyData.Concat(SAV.BoxData).Where(p => p.Species != 0).ToList();

        // Calculate viability scores
        var scoredPokemon = allPokemon.Select(pk => new
        {
            Pokemon = pk,
            Name = SpeciesName.GetSpeciesName(pk.Species, 2),
            Score = CalculateViabilityScore(pk)
        }).OrderByDescending(x => x.Score).ToList();

        rtbPreview.SelectionColor = Color.Yellow;
        rtbPreview.AppendText("  TOP COMPETITIVE POKEMON:\n\n");
        rtbPreview.SelectionColor = Color.White;

        foreach (var entry in scoredPokemon.Take(10))
        {
            var scoreColor = entry.Score >= 80 ? Color.LightGreen : entry.Score >= 60 ? Color.Yellow : Color.Salmon;
            rtbPreview.SelectionColor = scoreColor;
            rtbPreview.AppendText($"    {entry.Score,3:F0}");
            rtbPreview.SelectionColor = Color.White;
            rtbPreview.AppendText($"  {entry.Name}\n");
        }

        rtbPreview.AppendText("\n");
        rtbPreview.SelectionColor = Color.Gray;
        rtbPreview.AppendText($"  Average Team Viability: {scoredPokemon.Take(6).Average(x => x.Score):F1}/100\n\n");
    }

    private void GenerateShinyCatalogPreview()
    {
        AppendPreviewHeader("Shiny Collection Catalog");

        var shinies = SAV.PartyData.Concat(SAV.BoxData).Where(p => p.Species != 0 && p.IsShiny).ToList();

        rtbPreview.SelectionColor = Color.Yellow;
        rtbPreview.AppendText($"  Total Shinies: {shinies.Count}\n\n");

        if (shinies.Count == 0)
        {
            rtbPreview.SelectionColor = Color.Gray;
            rtbPreview.AppendText("  No shiny Pokemon found.\n\n");
            return;
        }

        rtbPreview.SelectionColor = Color.White;
        foreach (var pk in shinies.Take(15))
        {
            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            rtbPreview.SelectionColor = Color.Gold;
            rtbPreview.AppendText("  ★ ");
            rtbPreview.SelectionColor = Color.White;
            rtbPreview.AppendText($"{name,-20} Lv.{pk.CurrentLevel,-3}");

            if (chkIncludeOrigin.Checked)
            {
                rtbPreview.SelectionColor = Color.Gray;
                rtbPreview.AppendText($" OT: {pk.OriginalTrainerName}");
            }
            rtbPreview.SelectionColor = Color.White;
            rtbPreview.AppendText("\n");
        }

        if (shinies.Count > 15)
        {
            rtbPreview.SelectionColor = Color.Gray;
            rtbPreview.AppendText($"\n  ... and {shinies.Count - 15} more\n");
        }
        rtbPreview.AppendText("\n");
    }

    private void GenerateLivingDexPreview()
    {
        AppendPreviewHeader("Living Dex Progress");

        var uniqueSpecies = SAV.PartyData.Concat(SAV.BoxData)
            .Where(p => p.Species != 0)
            .Select(p => p.Species)
            .Distinct()
            .ToList();

        int totalNationalDex = 1025; // Approximate current National Dex size
        double progress = (double)uniqueSpecies.Count / totalNationalDex * 100;

        rtbPreview.SelectionColor = Color.Yellow;
        rtbPreview.AppendText($"  LIVING DEX PROGRESS:\n\n");
        rtbPreview.SelectionColor = Color.White;
        rtbPreview.AppendText($"    Species Collected: {uniqueSpecies.Count}/{totalNationalDex}\n");
        rtbPreview.AppendText($"    Completion: {progress:F1}%\n\n");

        // Progress bar visualization
        int barLength = 50;
        int filledLength = (int)(progress / 100 * barLength);
        rtbPreview.SelectionColor = Color.LightGreen;
        rtbPreview.AppendText($"    [");
        rtbPreview.AppendText(new string('█', filledLength));
        rtbPreview.SelectionColor = Color.Gray;
        rtbPreview.AppendText(new string('░', barLength - filledLength));
        rtbPreview.SelectionColor = Color.LightGreen;
        rtbPreview.AppendText($"] {progress:F1}%\n\n");

        rtbPreview.SelectionColor = Color.White;
    }

    private void BtnExport_Click(object? sender, EventArgs e)
    {
        var selectedReports = clbReportTypes.CheckedItems.Cast<string>().ToList();
        if (selectedReports.Count == 0)
        {
            MessageBox.Show("Please select at least one report type!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var format = cmbFormat.SelectedItem?.ToString() ?? "HTML Report";
        var outputPath = txtOutputPath.Text;

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        pbExport.Maximum = selectedReports.Count;
        pbExport.Value = 0;

        var exportedFiles = new List<string>();

        foreach (var reportType in selectedReports)
        {
            var fileName = GenerateFileName(reportType, format);
            var filePath = Path.Combine(outputPath, fileName);

            var content = GenerateReportContent(reportType, format);
            File.WriteAllText(filePath, content);

            exportedFiles.Add(filePath);
            pbExport.Value++;
            Application.DoEvents();
        }

        MessageBox.Show(
            $"Successfully exported {exportedFiles.Count} report(s) to:\n{outputPath}",
            "Export Complete",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private string GenerateFileName(string reportType, string format)
    {
        var baseName = reportType.Replace(" ", "_").ToLower();
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var extension = format switch
        {
            "HTML Report" => "html",
            "CSV Spreadsheet" => "csv",
            "JSON Data" => "json",
            "Markdown" => "md",
            _ => "txt"
        };

        return $"{baseName}_{timestamp}.{extension}";
    }

    private string GenerateReportContent(string reportType, string format)
    {
        return format switch
        {
            "HTML Report" => GenerateHTMLReport(reportType),
            "CSV Spreadsheet" => GenerateCSVReport(reportType),
            "JSON Data" => GenerateJSONReport(reportType),
            "Markdown" => GenerateMarkdownReport(reportType),
            _ => GeneratePlainTextReport(reportType)
        };
    }

    private string GenerateHTMLReport(string reportType)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head>");
        sb.AppendLine("<meta charset='UTF-8'>");
        sb.AppendLine($"<title>{reportType} - PKM Universe Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; background: #1a1a2e; color: #eee; padding: 20px; }");
        sb.AppendLine("h1 { color: #00d4ff; border-bottom: 2px solid #00d4ff; padding-bottom: 10px; }");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
        sb.AppendLine("th, td { border: 1px solid #444; padding: 10px; text-align: left; }");
        sb.AppendLine("th { background: #2a2a4e; color: #00d4ff; }");
        sb.AppendLine("tr:nth-child(even) { background: #252540; }");
        sb.AppendLine(".shiny { color: gold; }");
        sb.AppendLine(".stat-bar { background: #333; height: 20px; border-radius: 3px; }");
        sb.AppendLine(".stat-fill { background: linear-gradient(90deg, #00d4ff, #00ff88); height: 100%; border-radius: 3px; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>{reportType}</h1>");
        sb.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

        // Add report-specific content
        switch (reportType)
        {
            case "Team Analysis Report":
            case "Full Collection Inventory":
                sb.AppendLine("<table><tr><th>Pokemon</th><th>Level</th><th>Nature</th>");
                if (chkIncludeIVsEVs.Checked) sb.AppendLine("<th>IVs</th><th>EVs</th>");
                if (chkIncludeMoves.Checked) sb.AppendLine("<th>Moves</th>");
                sb.AppendLine("</tr>");

                var pokemon = reportType.Contains("Team") ?
                    SAV.PartyData.Where(p => p.Species != 0) :
                    SAV.BoxData.Where(p => p.Species != 0);

                foreach (var pk in pokemon)
                {
                    var name = SpeciesName.GetSpeciesName(pk.Species, 2);
                    var shinyClass = pk.IsShiny ? " class='shiny'" : "";
                    sb.AppendLine($"<tr><td{shinyClass}>{(pk.IsShiny ? "★ " : "")}{name}</td>");
                    sb.AppendLine($"<td>{pk.CurrentLevel}</td>");
                    sb.AppendLine($"<td>{(Nature)pk.Nature}</td>");

                    if (chkIncludeIVsEVs.Checked)
                    {
                        sb.AppendLine($"<td>{pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}</td>");
                        sb.AppendLine($"<td>{pk.EV_HP}/{pk.EV_ATK}/{pk.EV_DEF}/{pk.EV_SPA}/{pk.EV_SPD}/{pk.EV_SPE}</td>");
                    }

                    if (chkIncludeMoves.Checked)
                    {
                        var moves = new[] { pk.Move1, pk.Move2, pk.Move3, pk.Move4 }
                            .Where(m => m != 0)
                            .Select(m => MoveNameHelper.GetMoveName(m));
                        sb.AppendLine($"<td>{string.Join(", ", moves)}</td>");
                    }

                    sb.AppendLine("</tr>");
                }
                sb.AppendLine("</table>");
                break;
        }

        sb.AppendLine("<footer><p>Generated by PKM Universe - Professional Pokemon Management</p></footer>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private string GenerateCSVReport(string reportType)
    {
        var sb = new StringBuilder();

        // Header
        sb.Append("Species,Level,Nature,Shiny");
        if (chkIncludeIVsEVs.Checked) sb.Append(",IV_HP,IV_ATK,IV_DEF,IV_SPA,IV_SPD,IV_SPE,EV_HP,EV_ATK,EV_DEF,EV_SPA,EV_SPD,EV_SPE");
        if (chkIncludeMoves.Checked) sb.Append(",Move1,Move2,Move3,Move4");
        if (chkIncludeOrigin.Checked) sb.Append(",OT,TID");
        sb.AppendLine();

        var pokemon = SAV.PartyData.Concat(SAV.BoxData).Where(p => p.Species != 0);

        foreach (var pk in pokemon)
        {
            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            sb.Append($"\"{name}\",{pk.CurrentLevel},{(Nature)pk.Nature},{pk.IsShiny}");

            if (chkIncludeIVsEVs.Checked)
                sb.Append($",{pk.IV_HP},{pk.IV_ATK},{pk.IV_DEF},{pk.IV_SPA},{pk.IV_SPD},{pk.IV_SPE},{pk.EV_HP},{pk.EV_ATK},{pk.EV_DEF},{pk.EV_SPA},{pk.EV_SPD},{pk.EV_SPE}");

            if (chkIncludeMoves.Checked)
            {
                sb.Append($",\"{MoveNameHelper.GetMoveName(pk.Move1)}\",\"{MoveNameHelper.GetMoveName(pk.Move2)}\",\"{MoveNameHelper.GetMoveName(pk.Move3)}\",\"{MoveNameHelper.GetMoveName(pk.Move4)}\"");
            }

            if (chkIncludeOrigin.Checked)
                sb.Append($",\"{pk.OriginalTrainerName}\",{pk.TID16}");

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GenerateJSONReport(string reportType)
    {
        var pokemon = SAV.PartyData.Concat(SAV.BoxData).Where(p => p.Species != 0);

        var data = pokemon.Select(pk => new
        {
            species = SpeciesName.GetSpeciesName(pk.Species, 2),
            speciesId = pk.Species,
            level = pk.CurrentLevel,
            nature = ((Nature)pk.Nature).ToString(),
            isShiny = pk.IsShiny,
            ivs = chkIncludeIVsEVs.Checked ? new { hp = pk.IV_HP, atk = pk.IV_ATK, def = pk.IV_DEF, spa = pk.IV_SPA, spd = pk.IV_SPD, spe = pk.IV_SPE } : null,
            evs = chkIncludeIVsEVs.Checked ? new { hp = pk.EV_HP, atk = pk.EV_ATK, def = pk.EV_DEF, spa = pk.EV_SPA, spd = pk.EV_SPD, spe = pk.EV_SPE } : null,
            moves = chkIncludeMoves.Checked ? new[] { MoveNameHelper.GetMoveName(pk.Move1), MoveNameHelper.GetMoveName(pk.Move2), MoveNameHelper.GetMoveName(pk.Move3), MoveNameHelper.GetMoveName(pk.Move4) }.Where(m => m != "(None)").ToArray() : null,
            ot = chkIncludeOrigin.Checked ? pk.OriginalTrainerName : null
        });

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            reportType,
            generatedAt = DateTime.Now.ToString("O"),
            pokemon = data
        }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    private string GenerateMarkdownReport(string reportType)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {reportType}");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        var pokemon = SAV.PartyData.Concat(SAV.BoxData).Where(p => p.Species != 0);

        sb.AppendLine("| Pokemon | Level | Nature | Shiny |");
        sb.AppendLine("|---------|-------|--------|-------|");

        foreach (var pk in pokemon.Take(50))
        {
            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            sb.AppendLine($"| {(pk.IsShiny ? "★ " : "")}{name} | {pk.CurrentLevel} | {(Nature)pk.Nature} | {(pk.IsShiny ? "Yes" : "No")} |");
        }

        return sb.ToString();
    }

    private string GeneratePlainTextReport(string reportType)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== {reportType.ToUpper()} ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine(new string('=', 60));
        sb.AppendLine();

        var pokemon = SAV.PartyData.Concat(SAV.BoxData).Where(p => p.Species != 0);

        foreach (var pk in pokemon)
        {
            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            sb.AppendLine($"{(pk.IsShiny ? "★ " : "  ")}{name} (Lv. {pk.CurrentLevel})");

            if (chkIncludeIVsEVs.Checked)
            {
                sb.AppendLine($"   IVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}");
                sb.AppendLine($"   EVs: {pk.EV_HP}/{pk.EV_ATK}/{pk.EV_DEF}/{pk.EV_SPA}/{pk.EV_SPD}/{pk.EV_SPE}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private double CalculateViabilityScore(PKM pk)
    {
        var bst = pk.PersonalInfo.GetBaseStatTotal();
        return Math.Min(100, bst / 6.5);
    }

    private string GetTypeName(int typeId) => typeId switch
    {
        0 => "Normal", 1 => "Fighting", 2 => "Flying", 3 => "Poison", 4 => "Ground",
        5 => "Rock", 6 => "Bug", 7 => "Ghost", 8 => "Steel", 9 => "Fire",
        10 => "Water", 11 => "Grass", 12 => "Electric", 13 => "Psychic", 14 => "Ice",
        15 => "Dragon", 16 => "Dark", 17 => "Fairy", _ => "Normal"
    };
}

public static class MoveNameHelper
{
    public static string GetMoveName(ushort moveId)
    {
        // This would normally pull from game data, simplified here
        if (moveId == 0) return "(None)";
        return $"Move_{moveId}"; // In real implementation, use game's move names
    }
}
