using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class TournamentTeamManager : Form
{
    private readonly SaveFile SAV;
    private readonly ListBox LB_Teams;
    private readonly TextBox TB_TeamName;
    private readonly Label L_Info;
    private readonly string _teamsPath;
    private Dictionary<string, List<byte[]>> _teams = new();

    public TournamentTeamManager(SaveFile sav)
    {
        SAV = sav;
        _teamsPath = Path.Combine(AppContext.BaseDirectory, "tournament_teams.json");
        LoadTeams();

        Text = "Tournament Team Manager";
        Size = new Size(500, 450);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblTitle = new Label { Text = "Saved Tournament Teams", Location = new Point(20, 15), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };

        LB_Teams = new ListBox { Location = new Point(20, 45), Size = new Size(280, 280), BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };
        LB_Teams.SelectedIndexChanged += (s, e) => UpdateInfo();
        RefreshList();

        var lblName = new Label { Text = "Team Name:", Location = new Point(20, 340), AutoSize = true, ForeColor = Color.White };
        TB_TeamName = new TextBox { Location = new Point(100, 337), Width = 200, BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.White };

        var btnSave = new Button { Text = "Save Party", Location = new Point(320, 45), Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 100, 60), ForeColor = Color.White };
        btnSave.Click += (s, e) => SaveTeam();

        var btnLoad = new Button { Text = "Load to Party", Location = new Point(320, 90), Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 100), ForeColor = Color.White };
        btnLoad.Click += (s, e) => LoadTeam();

        var btnDelete = new Button { Text = "Delete Team", Location = new Point(320, 200), Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(100, 50, 50), ForeColor = Color.White };
        btnDelete.Click += (s, e) => DeleteTeam();

        L_Info = new Label { Location = new Point(20, 375), Size = new Size(440, 30), ForeColor = Color.LightGray };

        Controls.AddRange(new Control[] { lblTitle, LB_Teams, lblName, TB_TeamName, btnSave, btnLoad, btnDelete, L_Info });
    }

    private void RefreshList()
    {
        LB_Teams.Items.Clear();
        foreach (var name in _teams.Keys) LB_Teams.Items.Add(name);
    }

    private void UpdateInfo()
    {
        if (LB_Teams.SelectedItem == null) { L_Info.Text = ""; return; }
        var name = LB_Teams.SelectedItem.ToString()!;
        if (_teams.TryGetValue(name, out var team))
            L_Info.Text = $"{team.Count} Pokemon in team";
    }

    private void SaveTeam()
    {
        var name = string.IsNullOrWhiteSpace(TB_TeamName.Text) ? $"Team {_teams.Count + 1}" : TB_TeamName.Text;
        if (!SAV.HasParty) { L_Info.Text = "No party available!"; return; }

        var team = new List<byte[]>();
        for (int i = 0; i < 6; i++)
        {
            var pk = SAV.GetPartySlotAtIndex(i);
            if (pk.Species != 0) team.Add(pk.Data.ToArray());
        }
        if (team.Count == 0) { L_Info.Text = "Party is empty!"; return; }

        _teams[name] = team;
        SaveTeamsToFile();
        RefreshList();
        L_Info.Text = $"Saved team with {team.Count} Pokemon!";
        L_Info.ForeColor = Color.LightGreen;
    }

    private void LoadTeam()
    {
        if (LB_Teams.SelectedItem == null) return;
        var name = LB_Teams.SelectedItem.ToString()!;
        if (!_teams.TryGetValue(name, out var team)) return;
        if (!SAV.HasParty) return;

        for (int i = 0; i < 6; i++)
        {
            if (i < team.Count)
            {
                var pk = EntityFormat.GetFromBytes(team[i], SAV.Context);
                if (pk != null) SAV.SetPartySlotAtIndex(pk, i);
            }
            else SAV.SetPartySlotAtIndex(SAV.BlankPKM, i);
        }
        L_Info.Text = "Loaded team to party!";
        L_Info.ForeColor = Color.LightGreen;
    }

    private void DeleteTeam()
    {
        if (LB_Teams.SelectedItem == null) return;
        var name = LB_Teams.SelectedItem.ToString()!;
        if (MessageBox.Show($"Delete team?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            _teams.Remove(name);
            SaveTeamsToFile();
            RefreshList();
            L_Info.Text = "Team deleted.";
        }
    }

    private void LoadTeams()
    {
        try
        {
            if (File.Exists(_teamsPath))
            {
                var json = File.ReadAllText(_teamsPath);
                var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                if (data != null)
                {
                    _teams = new Dictionary<string, List<byte[]>>();
                    foreach (var kvp in data)
                        _teams[kvp.Key] = kvp.Value.ConvertAll(b => Convert.FromBase64String(b));
                }
            }
        }
        catch { _teams = new Dictionary<string, List<byte[]>>(); }
    }

    private void SaveTeamsToFile()
    {
        try
        {
            var data = new Dictionary<string, List<string>>();
            foreach (var kvp in _teams)
                data[kvp.Key] = kvp.Value.ConvertAll(b => Convert.ToBase64String(b));
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(_teamsPath, json);
        }
        catch { }
    }
}
