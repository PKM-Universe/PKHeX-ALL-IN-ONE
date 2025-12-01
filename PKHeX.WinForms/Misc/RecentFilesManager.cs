using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace PKHeX.WinForms;

public static class RecentFilesManager
{
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "recent_files.json");
    private static List<string> _recentFiles = new();
    private const int MaxRecentFiles = 10;

    static RecentFilesManager()
    {
        Load();
    }

    public static void AddRecentFile(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        _recentFiles.Remove(path);
        _recentFiles.Insert(0, path);
        if (_recentFiles.Count > MaxRecentFiles)
            _recentFiles.RemoveAt(_recentFiles.Count - 1);
        Save();
    }

    public static IReadOnlyList<string> GetRecentFiles() => _recentFiles;

    public static void ClearRecentFiles()
    {
        _recentFiles.Clear();
        Save();
    }

    private static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                _recentFiles = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                _recentFiles.RemoveAll(f => !File.Exists(f));
            }
        }
        catch { _recentFiles = new List<string>(); }
    }

    private static void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_recentFiles);
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }

    public static void PopulateMenu(ToolStripMenuItem menuItem, Action<string> onFileSelected)
    {
        menuItem.DropDownItems.Clear();
        var files = GetRecentFiles();
        if (files.Count == 0)
        {
            var empty = new ToolStripMenuItem("(No recent files)") { Enabled = false };
            menuItem.DropDownItems.Add(empty);
        }
        else
        {
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var name = Path.GetFileName(file);
                var item = new ToolStripMenuItem($"{i + 1}. {name}");
                item.ToolTipText = file;
                item.Click += (s, e) => onFileSelected(file);
                menuItem.DropDownItems.Add(item);
            }
            menuItem.DropDownItems.Add(new ToolStripSeparator());
            var clearItem = new ToolStripMenuItem("Clear Recent Files");
            clearItem.Click += (s, e) => { ClearRecentFiles(); PopulateMenu(menuItem, onFileSelected); };
            menuItem.DropDownItems.Add(clearItem);
        }
    }
}
