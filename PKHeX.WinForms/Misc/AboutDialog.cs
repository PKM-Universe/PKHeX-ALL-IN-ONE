using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PKHeX.WinForms;

public class AboutDialog : Form
{
    public AboutDialog()
    {
        Text = "About PKM-Universe";
        Size = new Size(500, 450);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(25, 25, 40);

        var pnlHeader = new Panel { Location = new Point(0, 0), Size = new Size(500, 120), BackColor = Color.FromArgb(35, 25, 55) };
        pnlHeader.Paint += (s, e) => {
            using var brush = new LinearGradientBrush(pnlHeader.ClientRectangle, Color.FromArgb(45, 25, 65), Color.FromArgb(25, 25, 45), LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(brush, pnlHeader.ClientRectangle);
            using var titleFont = new Font("Segoe UI", 28F, FontStyle.Bold);
            using var titleBrush = new LinearGradientBrush(new Rectangle(30, 25, 300, 50), Color.FromArgb(200, 150, 255), Color.FromArgb(138, 43, 226), LinearGradientMode.Horizontal);
            e.Graphics.DrawString("PKM-Universe", titleFont, titleBrush, 30, 25);
            using var subFont = new Font("Segoe UI", 10F, FontStyle.Italic);
            e.Graphics.DrawString("The Ultimate Pokemon Save Editor", subFont, Brushes.LightGray, 32, 70);
        };
        Controls.Add(pnlHeader);

        var lblVersion = new Label { Text = $"Version {Program.CurrentVersion}", Location = new Point(30, 135), AutoSize = true, ForeColor = Color.FromArgb(180, 180, 200), Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
        Controls.Add(lblVersion);

        var lblBuild = new Label { Text = $"Build Date: {DateTime.Now:MMMM yyyy}", Location = new Point(30, 160), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9F) };
        Controls.Add(lblBuild);

        var lblDesc = new Label { Text = "PKM-Universe is a powerful fork of PKHeX with enhanced features for Pokemon save file editing, team building, and competitive Pokemon management.", Location = new Point(30, 195), Size = new Size(420, 50), ForeColor = Color.FromArgb(200, 200, 220), Font = new Font("Segoe UI", 9F) };
        Controls.Add(lblDesc);

        var btnWebsite = CreateLinkButton("Visit Website", new Point(30, 260), "https://pkmuniverseannouncements.org");
        var btnDiscord = CreateLinkButton("Join Discord", new Point(170, 260), "https://discord.gg/pkmuniverseannouncements");
        var btnGitHub = CreateLinkButton("GitHub", new Point(310, 260), "https://github.com/pkmuniverseannouncements");
        Controls.AddRange(new Control[] { btnWebsite, btnDiscord, btnGitHub });

        var lblCredits = new Label { Text = "Credits:", Location = new Point(30, 310), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
        var lblCreditsText = new Label { Text = "Based on PKHeX by Kaphotics\nPKM-Universe Team - Eric & Contributors\nSprites from Project Pokemon", Location = new Point(30, 335), Size = new Size(420, 50), ForeColor = Color.Gray, Font = new Font("Segoe UI", 9F) };
        Controls.AddRange(new Control[] { lblCredits, lblCreditsText });

        var btnClose = new Button { Text = "Close", Location = new Point(380, 380), Size = new Size(90, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 40, 80), ForeColor = Color.White };
        btnClose.Click += (s, e) => Close();
        Controls.Add(btnClose);
    }

    private Button CreateLinkButton(string text, Point location, string url)
    {
        var btn = new Button { Text = text, Location = location, Size = new Size(120, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 30, 70), ForeColor = Color.FromArgb(180, 150, 220), Cursor = Cursors.Hand };
        btn.FlatAppearance.BorderColor = Color.FromArgb(100, 70, 130);
        btn.Click += (s, e) => { try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { } };
        return btn;
    }
}
