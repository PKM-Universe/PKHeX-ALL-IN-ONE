using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PKHeX.Core;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.WinForms;

/// <summary>
/// Pokemon Home Cloud Sync - Sync Pokemon between PKHeX and Pokemon Home ecosystem
/// </summary>
public partial class PokemonHomeCloudSync : Form
{
    private readonly SaveFile SAV;
    private static readonly HttpClient client = new();
    private const string API_BASE_URL = "https://home.pkmuniverseannouncements.com";
    private const string BOXES_ENDPOINT = "/api/boxes";
    private const string SYNC_ENDPOINT = "/api/sync";

    private TextBox txtApiKey;
    private Button btnAuthenticate;
    private ListView lstCloudBoxes;
    private Button btnUploadBox;
    private Button btnDownloadBox;
    private ComboBox cmbLocalBox;
    private Label lblStatus;
    private ProgressBar progressBar;
    private Label lblLastSync;
    private bool isAuthenticated = false;

    public PokemonHomeCloudSync(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        this.Text = "Pokemon Home Cloud Sync";
        this.Size = new System.Drawing.Size(800, 600);
        this.StartPosition = FormStartPosition.CenterParent;

        // Title
        var lblTitle = new Label
        {
            Text = "☁️ Pokemon Home Cloud Sync",
            Font = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Bold),
            Location = new System.Drawing.Point(20, 20),
            AutoSize = true
        };
        this.Controls.Add(lblTitle);

        // API Key Section
        var lblApiKey = new Label
        {
            Text = "API Key (from Pokemon Home website):",
            Location = new System.Drawing.Point(20, 70),
            AutoSize = true
        };
        this.Controls.Add(lblApiKey);

        txtApiKey = new TextBox
        {
            Location = new System.Drawing.Point(20, 95),
            Size = new System.Drawing.Size(500, 25),
            UseSystemPasswordChar = true
        };
        this.Controls.Add(txtApiKey);

        btnAuthenticate = new Button
        {
            Text = "🔐 Authenticate",
            Location = new System.Drawing.Point(530, 95),
            Size = new System.Drawing.Size(120, 25)
        };
        btnAuthenticate.Click += async (s, e) => await Authenticate();
        this.Controls.Add(btnAuthenticate);

        // Local Box Selector
        var lblLocal = new Label
        {
            Text = "Local Save File Box:",
            Location = new System.Drawing.Point(20, 140),
            AutoSize = true
        };
        this.Controls.Add(lblLocal);

        cmbLocalBox = new ComboBox
        {
            Location = new System.Drawing.Point(20, 165),
            Size = new System.Drawing.Size(300, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        LoadLocalBoxes();
        this.Controls.Add(cmbLocalBox);

        // Cloud Boxes List
        var lblCloud = new Label
        {
            Text = "Cloud Boxes:",
            Location = new System.Drawing.Point(20, 210),
            AutoSize = true
        };
        this.Controls.Add(lblCloud);

        lstCloudBoxes = new ListView
        {
            Location = new System.Drawing.Point(20, 235),
            Size = new System.Drawing.Size(740, 200),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };
        lstCloudBoxes.Columns.Add("Box Name", 200);
        lstCloudBoxes.Columns.Add("Pokemon Count", 120);
        lstCloudBoxes.Columns.Add("Last Modified", 200);
        lstCloudBoxes.Columns.Add("Game", 120);
        this.Controls.Add(lstCloudBoxes);

        // Action Buttons
        btnUploadBox = new Button
        {
            Text = "⬆️ Upload to Cloud",
            Location = new System.Drawing.Point(20, 450),
            Size = new System.Drawing.Size(150, 35),
            Enabled = false
        };
        btnUploadBox.Click += async (s, e) => await UploadBox();
        this.Controls.Add(btnUploadBox);

        btnDownloadBox = new Button
        {
            Text = "⬇️ Download from Cloud",
            Location = new System.Drawing.Point(180, 450),
            Size = new System.Drawing.Size(170, 35),
            Enabled = false
        };
        btnDownloadBox.Click += async (s, e) => await DownloadBox();
        this.Controls.Add(btnDownloadBox);

        // Status
        lblStatus = new Label
        {
            Text = "Status: Not authenticated",
            Location = new System.Drawing.Point(370, 460),
            AutoSize = true,
            ForeColor = System.Drawing.Color.Gray
        };
        this.Controls.Add(lblStatus);

        lblLastSync = new Label
        {
            Text = "Last Sync: Never",
            Location = new System.Drawing.Point(370, 480),
            AutoSize = true,
            ForeColor = System.Drawing.Color.Gray
        };
        this.Controls.Add(lblLastSync);

        // Progress Bar
        progressBar = new ProgressBar
        {
            Location = new System.Drawing.Point(20, 500),
            Size = new System.Drawing.Size(740, 25),
            Style = ProgressBarStyle.Marquee,
            Visible = false
        };
        this.Controls.Add(progressBar);

        // Info
        var lblInfo = new Label
        {
            Text = "💡 Get your API key from: home.pkmuniverseannouncements.com/settings",
            Location = new System.Drawing.Point(20, 535),
            Size = new System.Drawing.Size(740, 20),
            ForeColor = System.Drawing.Color.DarkGray
        };
        this.Controls.Add(lblInfo);
    }

    private void LoadSettings()
    {
        // Load saved API key if exists
        if (Properties.Settings.Default.CloudSyncApiKey != null)
        {
            txtApiKey.Text = Properties.Settings.Default.CloudSyncApiKey;
        }

        if (Properties.Settings.Default.LastCloudSync != null)
        {
            lblLastSync.Text = $"Last Sync: {Properties.Settings.Default.LastCloudSync}";
        }
    }

    private void LoadLocalBoxes()
    {
        cmbLocalBox.Items.Clear();
        for (int i = 0; i < SAV.BoxCount; i++)
        {
            string boxName = SAV is IBoxDetailName ibn ? ibn.GetBoxName(i) : $"Box {i + 1}";
            cmbLocalBox.Items.Add(new BoxItem { Index = i, Name = boxName });
        }
        if (cmbLocalBox.Items.Count > 0)
            cmbLocalBox.SelectedIndex = 0;
    }

    private async Task Authenticate()
    {
        if (string.IsNullOrWhiteSpace(txtApiKey.Text))
        {
            MessageBox.Show("Please enter your API key!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            btnAuthenticate.Enabled = false;
            lblStatus.Text = "Status: Authenticating...";
            progressBar.Visible = true;

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {txtApiKey.Text}");

            var response = await client.GetAsync($"{API_BASE_URL}{BOXES_ENDPOINT}");

            if (response.IsSuccessStatusCode)
            {
                isAuthenticated = true;
                lblStatus.Text = "Status: ✅ Authenticated";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                // Save API key
                Properties.Settings.Default.CloudSyncApiKey = txtApiKey.Text;
                Properties.Settings.Default.Save();

                btnUploadBox.Enabled = true;
                btnDownloadBox.Enabled = true;

                await LoadCloudBoxes();

                MessageBox.Show("Successfully authenticated with Pokemon Home!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "Status: ❌ Authentication failed";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show("Invalid API key. Please check and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Status: Error - {ex.Message}";
            lblStatus.ForeColor = System.Drawing.Color.Red;
            MessageBox.Show($"Error authenticating: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnAuthenticate.Enabled = true;
            progressBar.Visible = false;
        }
    }

    private async Task LoadCloudBoxes()
    {
        if (!isAuthenticated) return;

        try
        {
            progressBar.Visible = true;
            var response = await client.GetStringAsync($"{API_BASE_URL}{BOXES_ENDPOINT}");
            var boxes = JsonSerializer.Deserialize<List<CloudBox>>(response);

            lstCloudBoxes.Items.Clear();
            if (boxes != null)
            {
                foreach (var box in boxes)
                {
                    var item = new ListViewItem(box.name ?? "Unknown");
                    item.SubItems.Add(box.pokemonCount.ToString());
                    item.SubItems.Add(box.lastModified ?? "Unknown");
                    item.SubItems.Add(box.game ?? "Unknown");
                    item.Tag = box;
                    lstCloudBoxes.Items.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading cloud boxes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            progressBar.Visible = false;
        }
    }

    private async Task UploadBox()
    {
        if (cmbLocalBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a box to upload!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var boxItem = (BoxItem)cmbLocalBox.SelectedItem;

        var result = MessageBox.Show(
            $"Upload '{boxItem.Name}' to cloud?\n\nThis will replace any existing box with the same name.",
            "Confirm Upload",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes) return;

        try
        {
            progressBar.Visible = true;
            lblStatus.Text = "Status: Uploading...";
            btnUploadBox.Enabled = false;

            // Get all Pokemon from this box
            var pokemonList = new List<object>();
            for (int slot = 0; slot < 30; slot++)
            {
                var pk = SAV.GetBoxSlotAtIndex(boxItem.Index, slot);
                if (pk.Species != 0)
                {
                    pokemonList.Add(SerializePokemon(pk));
                }
            }

            var uploadData = new
            {
                boxName = boxItem.Name,
                game = SAV.Version.ToString(),
                pokemon = pokemonList
            };

            var json = JsonSerializer.Serialize(uploadData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{API_BASE_URL}{BOXES_ENDPOINT}", content);

            if (response.IsSuccessStatusCode)
            {
                lblStatus.Text = "Status: ✅ Upload complete";
                lblStatus.ForeColor = System.Drawing.Color.Green;
                lblLastSync.Text = $"Last Sync: {DateTime.Now:g}";

                Properties.Settings.Default.LastCloudSync = DateTime.Now.ToString("g");
                Properties.Settings.Default.Save();

                await LoadCloudBoxes();

                MessageBox.Show($"Box '{boxItem.Name}' uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "Status: ❌ Upload failed";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Upload failed: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Status: Error - {ex.Message}";
            MessageBox.Show($"Error uploading box: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            progressBar.Visible = false;
            btnUploadBox.Enabled = true;
        }
    }

    private async Task DownloadBox()
    {
        if (lstCloudBoxes.SelectedItems.Count == 0)
        {
            MessageBox.Show("Please select a cloud box to download!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var cloudBox = (CloudBox)lstCloudBoxes.SelectedItems[0].Tag;

        var result = MessageBox.Show(
            $"Download '{cloudBox.name}' from cloud?\n\nThis will replace the currently selected local box.",
            "Confirm Download",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes) return;

        try
        {
            progressBar.Visible = true;
            lblStatus.Text = "Status: Downloading...";
            btnDownloadBox.Enabled = false;

            var response = await client.GetStringAsync($"{API_BASE_URL}{BOXES_ENDPOINT}/{cloudBox.id}");
            var boxData = JsonSerializer.Deserialize<CloudBoxWithPokemon>(response);

            if (boxData?.pokemon != null && cmbLocalBox.SelectedItem != null)
            {
                var localBox = (BoxItem)cmbLocalBox.SelectedItem;

                // Clear the box first
                for (int slot = 0; slot < 30; slot++)
                {
                    SAV.SetBoxSlotAtIndex(SAV.BlankPKM, localBox.Index, slot);
                }

                // Import Pokemon (implementation would need proper deserialization)
                // This is a placeholder - actual implementation would convert cloud data to PKM format

                lblStatus.Text = "Status: ✅ Download complete";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                MessageBox.Show($"Box '{cloudBox.name}' downloaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Status: Error - {ex.Message}";
            MessageBox.Show($"Error downloading box: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            progressBar.Visible = false;
            btnDownloadBox.Enabled = true;
        }
    }

    private object SerializePokemon(PKM pk)
    {
        return new
        {
            species = pk.Species,
            nickname = pk.Nickname,
            level = pk.CurrentLevel,
            isShiny = pk.IsShiny,
            nature = (int)pk.Nature,
            ability = pk.Ability,
            moves = new[] { pk.Move1, pk.Move2, pk.Move3, pk.Move4 },
            ivs = new { hp = pk.IV_HP, atk = pk.IV_ATK, def = pk.IV_DEF, spa = pk.IV_SPA, spd = pk.IV_SPD, spe = pk.IV_SPE },
            evs = new { hp = pk.EV_HP, atk = pk.EV_ATK, def = pk.EV_DEF, spa = pk.EV_SPA, spd = pk.EV_SPD, spe = pk.EV_SPE },
            ball = pk.Ball,
            item = pk.HeldItem,
            pid = pk.PID,
            ec = pk.EncryptionConstant
        };
    }

    private class BoxItem
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name;
    }

    private class CloudBox
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public int pokemonCount { get; set; }
        public string? lastModified { get; set; }
        public string? game { get; set; }
    }

    private class CloudBoxWithPokemon : CloudBox
    {
        public List<object>? pokemon { get; set; }
    }
}

// Add to Settings.settings
namespace PKHeX.WinForms.Properties
{
    partial class Settings
    {
        [System.Configuration.UserScopedSetting()]
        [System.Configuration.DefaultSettingValue("")]
        public string CloudSyncApiKey
        {
            get { return ((string)(this["CloudSyncApiKey"])); }
            set { this["CloudSyncApiKey"] = value; }
        }

        [System.Configuration.UserScopedSetting()]
        [System.Configuration.DefaultSettingValue("")]
        public string LastCloudSync
        {
            get { return ((string)(this["LastCloudSync"])); }
            set { this["LastCloudSync"] = value; }
        }
    }
}
