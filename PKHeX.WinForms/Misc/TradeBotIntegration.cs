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
/// Trade Bot Integration - Send Pokemon directly to PKM-Universe trade bots
/// </summary>
public partial class TradeBotIntegration : Form
{
    private readonly SaveFile SAV;
    private readonly PKM Pokemon;
    private static readonly HttpClient client = new();
    private const string API_BASE_URL = "https://home.pkmuniverseannouncements.com"; // Your production URL
    private const string STATUS_ENDPOINT = "/api/trade-bot-status";
    private const string QUEUE_ENDPOINT = "/api/queue-trade";

    private ComboBox cmbBotSelector;
    private Label lblBotStatus;
    private TextBox txtPokemonSummary;
    private Button btnSendToBot;
    private Button btnRefreshStatus;
    private Label lblQueueCount;
    private ProgressBar progressBar;
    private Panel pnlBotList;
    private CheckBox chkAutoSelect;

    public TradeBotIntegration(SaveFile sav, PKM pk)
    {
        SAV = sav;
        Pokemon = pk;
        InitializeComponent();
        LoadBotStatus();
    }

    private void InitializeComponent()
    {
        this.Text = "PKM-Universe Trade Bot Integration";
        this.Size = new System.Drawing.Size(600, 500);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Title
        var lblTitle = new Label
        {
            Text = "🤖 Send to Trade Bot",
            Font = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Bold),
            Location = new System.Drawing.Point(20, 20),
            AutoSize = true
        };
        this.Controls.Add(lblTitle);

        // Pokemon Summary
        var lblPokemon = new Label
        {
            Text = "Pokemon to Send:",
            Location = new System.Drawing.Point(20, 70),
            AutoSize = true
        };
        this.Controls.Add(lblPokemon);

        txtPokemonSummary = new TextBox
        {
            Location = new System.Drawing.Point(20, 95),
            Size = new System.Drawing.Size(540, 80),
            Multiline = true,
            ReadOnly = true,
            Text = GetPokemonSummary()
        };
        this.Controls.Add(txtPokemonSummary);

        // Bot Selector
        var lblBot = new Label
        {
            Text = "Select Trade Bot:",
            Location = new System.Drawing.Point(20, 190),
            AutoSize = true
        };
        this.Controls.Add(lblBot);

        cmbBotSelector = new ComboBox
        {
            Location = new System.Drawing.Point(20, 215),
            Size = new System.Drawing.Size(400, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        this.Controls.Add(cmbBotSelector);

        // Auto-select best bot
        chkAutoSelect = new CheckBox
        {
            Text = "Auto-select best available bot",
            Location = new System.Drawing.Point(430, 215),
            AutoSize = true,
            Checked = true
        };
        this.Controls.Add(chkAutoSelect);

        // Bot Status
        lblBotStatus = new Label
        {
            Text = "Status: Loading...",
            Location = new System.Drawing.Point(20, 250),
            AutoSize = true,
            ForeColor = System.Drawing.Color.Gray
        };
        this.Controls.Add(lblBotStatus);

        lblQueueCount = new Label
        {
            Text = "Queue: 0",
            Location = new System.Drawing.Point(20, 275),
            AutoSize = true
        };
        this.Controls.Add(lblQueueCount);

        // Progress Bar
        progressBar = new ProgressBar
        {
            Location = new System.Drawing.Point(20, 310),
            Size = new System.Drawing.Size(540, 25),
            Style = ProgressBarStyle.Marquee,
            Visible = false
        };
        this.Controls.Add(progressBar);

        // Buttons
        btnRefreshStatus = new Button
        {
            Text = "🔄 Refresh Status",
            Location = new System.Drawing.Point(20, 350),
            Size = new System.Drawing.Size(150, 35)
        };
        btnRefreshStatus.Click += async (s, e) => await LoadBotStatus();
        this.Controls.Add(btnRefreshStatus);

        btnSendToBot = new Button
        {
            Text = "✨ Send to Bot",
            Location = new System.Drawing.Point(410, 350),
            Size = new System.Drawing.Size(150, 35),
            BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSendToBot.Click += async (s, e) => await SendToBot();
        this.Controls.Add(btnSendToBot);

        // Info Label
        var lblInfo = new Label
        {
            Text = "💡 Your Pokemon will be queued to the selected trade bot.\nYou'll receive it in-game via link trade!",
            Location = new System.Drawing.Point(20, 400),
            Size = new System.Drawing.Size(540, 50),
            ForeColor = System.Drawing.Color.DarkGray
        };
        this.Controls.Add(lblInfo);
    }

    private string GetPokemonSummary()
    {
        if (Pokemon == null)
            return "No Pokemon selected";

        var summary = new StringBuilder();
        summary.AppendLine($"Species: {GameInfo.Strings.Species[Pokemon.Species]}");
        summary.AppendLine($"Level: {Pokemon.CurrentLevel} | Shiny: {(Pokemon.IsShiny ? "Yes" : "No")}");
        summary.AppendLine($"Nature: {GameInfo.Strings.Natures[(int)Pokemon.Nature]} | Ability: {GameInfo.Strings.Ability[Pokemon.Ability]}");
        summary.AppendLine($"Ball: {GameInfo.Strings.balllist[Pokemon.Ball]}");
        return summary.ToString();
    }

    private async Task LoadBotStatus()
    {
        try
        {
            lblBotStatus.Text = "Status: Loading...";
            lblBotStatus.ForeColor = System.Drawing.Color.Gray;
            btnRefreshStatus.Enabled = false;

            var response = await client.GetStringAsync($"{API_BASE_URL}{STATUS_ENDPOINT}");
            var statusData = JsonSerializer.Deserialize<TradeBotStatusResponse>(response);

            if (statusData?.network == null)
            {
                lblBotStatus.Text = "Status: Unable to reach servers";
                lblBotStatus.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // Populate bot selector
            cmbBotSelector.Items.Clear();
            var onlineBots = statusData.bots.Where(b => b.Value.status == "Online").ToList();

            foreach (var bot in onlineBots)
            {
                string displayText = $"{bot.Value.name} - {bot.Value.game} (Queue: {bot.Value.tradesQueued})";
                cmbBotSelector.Items.Add(new BotItem { Id = bot.Key, Name = bot.Value.name, Data = bot.Value, DisplayText = displayText });
            }

            if (cmbBotSelector.Items.Count > 0)
            {
                // Auto-select bot with lowest queue
                if (chkAutoSelect.Checked)
                {
                    var bestBot = onlineBots.OrderBy(b => b.Value.tradesQueued).First();
                    int index = cmbBotSelector.Items.Cast<BotItem>().ToList().FindIndex(b => b.Id == bestBot.Key);
                    cmbBotSelector.SelectedIndex = index;
                }
                else
                {
                    cmbBotSelector.SelectedIndex = 0;
                }
            }

            lblBotStatus.Text = $"Status: {statusData.network.status} - {statusData.network.activeBots}/{statusData.network.totalBots} bots online";
            lblBotStatus.ForeColor = System.Drawing.Color.Green;
            lblQueueCount.Text = $"Total Queued: {statusData.network.totalTradesQueued}";
        }
        catch (Exception ex)
        {
            lblBotStatus.Text = $"Status: Error - {ex.Message}";
            lblBotStatus.ForeColor = System.Drawing.Color.Red;
        }
        finally
        {
            btnRefreshStatus.Enabled = true;
        }
    }

    private async Task SendToBot()
    {
        if (Pokemon == null)
        {
            MessageBox.Show("No Pokemon to send!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (cmbBotSelector.SelectedItem == null)
        {
            MessageBox.Show("Please select a trade bot first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedBot = (BotItem)cmbBotSelector.SelectedItem;

        try
        {
            btnSendToBot.Enabled = false;
            progressBar.Visible = true;
            lblBotStatus.Text = "Sending to bot...";

            // Convert Pokemon to trade request format
            var tradeRequest = new
            {
                botId = selectedBot.Id,
                pokemon = new
                {
                    species = GameInfo.Strings.Species[Pokemon.Species],
                    nickname = Pokemon.Nickname,
                    level = Pokemon.CurrentLevel,
                    isShiny = Pokemon.IsShiny,
                    nature = GameInfo.Strings.Natures[(int)Pokemon.Nature],
                    ability = GameInfo.Strings.Ability[Pokemon.Ability],
                    ball = GameInfo.Strings.balllist[Pokemon.Ball],
                    item = Pokemon.HeldItem > 0 ? GameInfo.Strings.Item[Pokemon.HeldItem] : "None",
                    moves = new[]
                    {
                        GameInfo.Strings.Move[Pokemon.Move1],
                        GameInfo.Strings.Move[Pokemon.Move2],
                        GameInfo.Strings.Move[Pokemon.Move3],
                        GameInfo.Strings.Move[Pokemon.Move4]
                    },
                    ivs = new { hp = Pokemon.IV_HP, atk = Pokemon.IV_ATK, def = Pokemon.IV_DEF, spa = Pokemon.IV_SPA, spd = Pokemon.IV_SPD, spe = Pokemon.IV_SPE },
                    evs = new { hp = Pokemon.EV_HP, atk = Pokemon.EV_ATK, def = Pokemon.EV_DEF, spa = Pokemon.EV_SPA, spd = Pokemon.EV_SPD, spe = Pokemon.EV_SPE }
                },
                source = "PKHeX"
            };

            var json = JsonSerializer.Serialize(tradeRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{API_BASE_URL}{QUEUE_ENDPOINT}", content);

            if (response.IsSuccessStatusCode)
            {
                lblBotStatus.Text = "✅ Successfully queued to bot!";
                lblBotStatus.ForeColor = System.Drawing.Color.Green;

                MessageBox.Show(
                    $"Pokemon queued successfully!\n\nBot: {selectedBot.Name}\nPokemon: {GameInfo.Strings.Species[Pokemon.Species]}\n\nJoin the trade in your game!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblBotStatus.Text = "❌ Failed to queue";
                lblBotStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Failed to queue Pokemon: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            lblBotStatus.Text = $"Error: {ex.Message}";
            lblBotStatus.ForeColor = System.Drawing.Color.Red;
            MessageBox.Show($"Error sending to bot: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSendToBot.Enabled = true;
            progressBar.Visible = false;
        }
    }

    private class BotItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
        public BotData Data { get; set; } = new();
        public override string ToString() => DisplayText;
    }

    private class TradeBotStatusResponse
    {
        public NetworkData network { get; set; } = new();
        public Dictionary<string, BotData> bots { get; set; } = new();
    }

    private class NetworkData
    {
        public string status { get; set; } = string.Empty;
        public int totalBots { get; set; }
        public int activeBots { get; set; }
        public int totalTradesQueued { get; set; }
    }

    private class BotData
    {
        public string name { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string game { get; set; } = string.Empty;
        public int tradesQueued { get; set; }
        public int tradesCompleted { get; set; }
    }
}
