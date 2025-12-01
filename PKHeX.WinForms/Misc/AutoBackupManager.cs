using System;
using System.IO;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class AutoBackupManager : IDisposable
{
    private readonly Timer _backupTimer;
    private SaveFile? _currentSave;
    private string? _currentPath;
    private readonly string _backupFolder;
    private int _intervalMinutes = 5;
    private bool _enabled = true;

    public event Action<string>? BackupCreated;

    public AutoBackupManager()
    {
        _backupFolder = Path.Combine(AppContext.BaseDirectory, "Backups");
        if (!Directory.Exists(_backupFolder)) Directory.CreateDirectory(_backupFolder);

        _backupTimer = new Timer { Interval = _intervalMinutes * 60 * 1000 };
        _backupTimer.Tick += (s, e) => CreateBackup();
    }

    public void SetSave(SaveFile sav, string path)
    {
        _currentSave = sav;
        _currentPath = path;
        if (_enabled) _backupTimer.Start();
    }

    public void SetInterval(int minutes)
    {
        _intervalMinutes = Math.Max(1, Math.Min(60, minutes));
        _backupTimer.Interval = _intervalMinutes * 60 * 1000;
    }

    public void Enable(bool enable)
    {
        _enabled = enable;
        if (_enabled && _currentSave != null) _backupTimer.Start();
        else _backupTimer.Stop();
    }

    public void CreateBackup()
    {
        if (_currentSave == null || string.IsNullOrEmpty(_currentPath)) return;

        try
        {
            var fileName = Path.GetFileNameWithoutExtension(_currentPath);
            var ext = Path.GetExtension(_currentPath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupName = $"{fileName}_backup_{timestamp}{ext}";
            var backupPath = Path.Combine(_backupFolder, backupName);

            File.WriteAllBytes(backupPath, _currentSave.Write().ToArray());
            BackupCreated?.Invoke(backupPath);
            CleanOldBackups();
        }
        catch { }
    }

    private void CleanOldBackups()
    {
        try
        {
            var files = Directory.GetFiles(_backupFolder, "*_backup_*");
            if (files.Length > 20)
            {
                Array.Sort(files);
                for (int i = 0; i < files.Length - 20; i++)
                    File.Delete(files[i]);
            }
        }
        catch { }
    }

    public string[] GetBackups()
    {
        try { return Directory.GetFiles(_backupFolder, "*_backup_*"); }
        catch { return Array.Empty<string>(); }
    }

    public void Dispose()
    {
        _backupTimer?.Stop();
        _backupTimer?.Dispose();
    }
}
