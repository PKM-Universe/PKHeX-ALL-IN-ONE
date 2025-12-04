using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using PKHeX.Core;
using System.Linq;
using System.Collections.Generic;

namespace PKHeX.WinForms;

/// <summary>
/// VGC Team Importer - Import teams from Pokepaste, Victory Road, and other sources
/// </summary>
public partial class VGCTeamImporter : Form
{
    private readonly SaveFile SAV;
    private readonly IPKMView Editor;
    private static readonly HttpClient client = new();

    private TextBox txtPasteUrl;
    private TextBox txtShowdownText;
    private Button btnImportUrl;
    private Button btnImportText;
    private ListBox lstTeamPreview;
    private Label lblStatus;
    private ProgressBar progressBar;
    private Button btnAddToParty;
    private TabControl tabControl;

    public VGCTeamImporter(SaveFile sav, IPKMView editor)
    {
        SAV = sav;
        Editor = editor;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "VGC Team Importer";
        this.Size = new System.Drawing.Size(700, 550);
        this.StartPosition = FormStartPosition.CenterParent;

        // Title
        var lblTitle = new Label
        {
            Text = "⚔️ Import VGC Teams",
            Font = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Bold),
            Location = new System.Drawing.Point(20, 20),
            AutoSize = true
        };
        this.Controls.Add(lblTitle);

        // Tab Control
        tabControl = new TabControl
        {
            Location = new System.Drawing.Point(20, 60),
            Size = new System.Drawing.Size(640, 320)
        };

        // Tab 1: URL Import
        var tabUrl = new TabPage("From URL");
        var lblUrl = new Label
        {
            Text = "Paste URL (Pokepaste, Pastebin, Victory Road):",
            Location = new System.Drawing.Point(10, 20),
            AutoSize = true
        };
        tabUrl.Controls.Add(lblUrl);

        txtPasteUrl = new TextBox
        {
            Location = new System.Drawing.Point(10, 45),
            Size = new System.Drawing.Size(500, 25)
        };
        tabUrl.Controls.Add(txtPasteUrl);

        btnImportUrl = new Button
        {
            Text = "📥 Import from URL",
            Location = new System.Drawing.Point(520, 45),
            Size = new System.Drawing.Size(100, 25)
        };
        btnImportUrl.Click += async (s, e) => await ImportFromUrl();
        tabUrl.Controls.Add(btnImportUrl);

        var lblUrlExamples = new Label
        {
            Text = "Examples:\n• pokepaste.es/abc123\n• pastebin.com/raw/xyz789\n• victoryroadvgc.com/pokemon-rental/12345",
            Location = new System.Drawing.Point(10, 85),
            Size = new System.Drawing.Size(600, 60),
            ForeColor = System.Drawing.Color.Gray
        };
        tabUrl.Controls.Add(lblUrlExamples);

        tabControl.TabPages.Add(tabUrl);

        // Tab 2: Text Import
        var tabText = new TabPage("From Text");
        var lblText = new Label
        {
            Text = "Paste Showdown format team:",
            Location = new System.Drawing.Point(10, 20),
            AutoSize = true
        };
        tabText.Controls.Add(lblText);

        txtShowdownText = new TextBox
        {
            Location = new System.Drawing.Point(10, 45),
            Size = new System.Drawing.Size(600, 180),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new System.Drawing.Font("Consolas", 9)
        };
        tabText.Controls.Add(txtShowdownText);

        btnImportText = new Button
        {
            Text = "📥 Import Team",
            Location = new System.Drawing.Point(250, 235),
            Size = new System.Drawing.Size(120, 30)
        };
        btnImportText.Click += (s, e) => ImportFromText();
        tabText.Controls.Add(btnImportText);

        tabControl.TabPages.Add(tabText);

        this.Controls.Add(tabControl);

        // Team Preview
        var lblPreview = new Label
        {
            Text = "Team Preview:",
            Location = new System.Drawing.Point(20, 390),
            AutoSize = true
        };
        this.Controls.Add(lblPreview);

        lstTeamPreview = new ListBox
        {
            Location = new System.Drawing.Point(20, 415),
            Size = new System.Drawing.Size(500, 80)
        };
        this.Controls.Add(lstTeamPreview);

        btnAddToParty = new Button
        {
            Text = "✨ Add to Party",
            Location = new System.Drawing.Point(530, 415),
            Size = new System.Drawing.Size(130, 35),
            Enabled = false
        };
        btnAddToParty.Click += (s, e) => AddTeamToParty();
        this.Controls.Add(btnAddToParty);

        // Status
        lblStatus = new Label
        {
            Text = "Ready",
            Location = new System.Drawing.Point(530, 455),
            Size = new System.Drawing.Size(130, 40),
            ForeColor = System.Drawing.Color.Gray
        };
        this.Controls.Add(lblStatus);

        // Progress
        progressBar = new ProgressBar
        {
            Location = new System.Drawing.Point(20, 505),
            Size = new System.Drawing.Size(640, 20),
            Style = ProgressBarStyle.Marquee,
            Visible = false
        };
        this.Controls.Add(progressBar);
    }

    private async Task ImportFromUrl()
    {
        if (string.IsNullOrWhiteSpace(txtPasteUrl.Text))
        {
            MessageBox.Show("Please enter a URL!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            btnImportUrl.Enabled = false;
            progressBar.Visible = true;
            lblStatus.Text = "Fetching...";

            string url = txtPasteUrl.Text.Trim();

            // Handle different URL formats
            if (url.Contains("pokepaste.es"))
            {
                if (!url.Contains("/raw"))
                    url += "/raw";
            }
            else if (url.Contains("pastebin.com") && !url.Contains("/raw/"))
            {
                url = url.Replace("/", "/raw/");
            }

            var response = await client.GetStringAsync(url);
            txtShowdownText.Text = response;

            ImportFromText();
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Error!";
            lblStatus.ForeColor = System.Drawing.Color.Red;
            MessageBox.Show($"Error fetching URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnImportUrl.Enabled = true;
            progressBar.Visible = false;
        }
    }

    private void ImportFromText()
    {
        if (string.IsNullOrWhiteSpace(txtShowdownText.Text))
        {
            MessageBox.Show("Please paste a team first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            lstTeamPreview.Items.Clear();
            var team = ParseShowdownTeam(txtShowdownText.Text);

            if (team.Count == 0)
            {
                MessageBox.Show("No valid Pokemon found in the text!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var pk in team)
            {
                string display = $"{GameInfo.Strings.Species[pk.Species]} Lv.{pk.CurrentLevel}";
                if (pk.HeldItem > 0)
                    display += $" @ {GameInfo.Strings.Item[pk.HeldItem]}";
                lstTeamPreview.Items.Add(pk);
                lstTeamPreview.DisplayMember = "Species";
            }

            lblStatus.Text = $"{team.Count} Pokemon";
            lblStatus.ForeColor = System.Drawing.Color.Green;
            btnAddToParty.Enabled = true;

            MessageBox.Show($"Successfully imported {team.Count} Pokemon!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error parsing team: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private List<PKM> ParseShowdownTeam(string text)
    {
        var team = new List<PKM>();
        var sets = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var set in sets)
        {
            try
            {
                var showdownSet = new ShowdownSet(set);
                var pk = SAV.BlankPKM;
                pk.ApplySetDetails(showdownSet);
                team.Add(pk);
            }
            catch
            {
                // Skip invalid sets
            }
        }

        return team;
    }

    private void AddTeamToParty()
    {
        if (lstTeamPreview.Items.Count == 0)
        {
            MessageBox.Show("No Pokemon to add!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            int added = 0;
            for (int i = 0; i < Math.Min(lstTeamPreview.Items.Count, 6); i++)
            {
                if (i < SAV.PartyCount || SAV.PartyCount < 6)
                {
                    var pk = (PKM)lstTeamPreview.Items[i];
                    SAV.SetPartySlot(pk, i);
                    added++;
                }
            }

            MessageBox.Show($"Added {added} Pokemon to party!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding to party: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
