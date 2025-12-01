using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class TradeHistoryLog : Form
{
    private readonly DataGridView DGV_History;
    private readonly Button BTN_Clear;
    private readonly Button BTN_Export;
    private readonly Label L_Stats;
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "TradeHistory.json");

    public TradeHistoryLog()
    {
        Text = "Trade History Log";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblTitle = new Label
        {
            Text = "Pokemon Trade History",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };

        DGV_History = new DataGridView
        {
            Location = new Point(20, 50),
            Size = new Size(845, 450),
            BackgroundColor = Color.FromArgb(40, 40, 60),
            ForeColor = Color.White,
            GridColor = Color.FromArgb(60, 60, 80),
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        DGV_History.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 60);
        DGV_History.DefaultCellStyle.ForeColor = Color.White;
        DGV_History.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 80, 100);
        DGV_History.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 70);
        DGV_History.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        DGV_History.EnableHeadersVisualStyles = false;

        DGV_History.Columns.Add("DateTime", "Date/Time");
        DGV_History.Columns.Add("Type", "Type");
        DGV_History.Columns.Add("Pokemon", "Pokemon");
        DGV_History.Columns.Add("Level", "Lv");
        DGV_History.Columns.Add("OT", "Original Trainer");
        DGV_History.Columns.Add("TID", "TID");
        DGV_History.Columns.Add("Game", "Game");
        DGV_History.Columns.Add("Notes", "Notes");

        L_Stats = new Label
        {
            Location = new Point(20, 510),
            Size = new Size(400, 25),
            ForeColor = Color.LightGray,
            Font = new Font("Segoe UI", 10F)
        };

        BTN_Clear = new Button
        {
            Text = "Clear History",
            Location = new Point(620, 510),
            Size = new Size(110, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 60, 60),
            ForeColor = Color.White
        };
        BTN_Clear.Click += (s, e) => ClearHistory();

        BTN_Export = new Button
        {
            Text = "Export CSV",
            Location = new Point(740, 510),
            Size = new Size(110, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 60),
            ForeColor = Color.White
        };
        BTN_Export.Click += (s, e) => ExportCSV();

        Controls.AddRange(new Control[] { lblTitle, DGV_History, L_Stats, BTN_Clear, BTN_Export });

        LoadHistory();
    }

    private void LoadHistory()
    {
        try
        {
            if (File.Exists(LogPath))
            {
                var json = File.ReadAllText(LogPath);
                var entries = JsonSerializer.Deserialize<List<TradeEntry>>(json);

                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        DGV_History.Rows.Add(
                            entry.DateTime,
                            entry.Type,
                            entry.Pokemon,
                            entry.Level,
                            entry.OriginalTrainer,
                            entry.TID,
                            entry.Game,
                            entry.Notes
                        );
                    }
                }

                UpdateStats(entries?.Count ?? 0);
            }
            else
            {
                UpdateStats(0);
            }
        }
        catch
        {
            UpdateStats(0);
        }
    }

    private void UpdateStats(int count)
    {
        L_Stats.Text = $"Total Trades: {count}";
    }

    private void ClearHistory()
    {
        var result = MessageBox.Show("Are you sure you want to clear all trade history?",
            "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            DGV_History.Rows.Clear();
            try { File.Delete(LogPath); } catch { }
            UpdateStats(0);
        }
    }

    private void ExportCSV()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "CSV File|*.csv",
            FileName = $"TradeHistory_{DateTime.Now:yyyyMMdd}.csv"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                using var sw = new StreamWriter(sfd.FileName);
                sw.WriteLine("DateTime,Type,Pokemon,Level,OriginalTrainer,TID,Game,Notes");

                foreach (DataGridViewRow row in DGV_History.Rows)
                {
                    var line = string.Join(",",
                        $"\"{row.Cells[0].Value}\"",
                        $"\"{row.Cells[1].Value}\"",
                        $"\"{row.Cells[2].Value}\"",
                        row.Cells[3].Value,
                        $"\"{row.Cells[4].Value}\"",
                        row.Cells[5].Value,
                        $"\"{row.Cells[6].Value}\"",
                        $"\"{row.Cells[7].Value}\""
                    );
                    sw.WriteLine(line);
                }

                MessageBox.Show("Trade history exported successfully!", "Export Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public static void LogTrade(PKM pokemon, string tradeType, string notes = "")
    {
        try
        {
            var entries = new List<TradeEntry>();

            if (File.Exists(LogPath))
            {
                var json = File.ReadAllText(LogPath);
                entries = JsonSerializer.Deserialize<List<TradeEntry>>(json) ?? new List<TradeEntry>();
            }

            entries.Add(new TradeEntry
            {
                DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Type = tradeType,
                Pokemon = GameInfo.Strings.specieslist[pokemon.Species],
                Level = pokemon.CurrentLevel,
                OriginalTrainer = pokemon.OriginalTrainerName,
                TID = pokemon.TID16,
                Game = pokemon.Version.ToString(),
                Notes = notes
            });

            // Keep last 1000 entries
            if (entries.Count > 1000)
                entries.RemoveRange(0, entries.Count - 1000);

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(LogPath, JsonSerializer.Serialize(entries, options));
        }
        catch { }
    }
}

public class TradeEntry
{
    public string DateTime { get; set; } = "";
    public string Type { get; set; } = "";
    public string Pokemon { get; set; } = "";
    public int Level { get; set; }
    public string OriginalTrainer { get; set; } = "";
    public int TID { get; set; }
    public string Game { get; set; } = "";
    public string Notes { get; set; } = "";
}
