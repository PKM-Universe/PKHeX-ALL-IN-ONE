using System;
using System.Drawing;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class WonderCardManager : Form
{
    private readonly SaveFile SAV;

    public WonderCardManager(SaveFile sav)
    {
        SAV = sav;
        Text = "Wonder Card Manager";
        Size = new Size(700, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblTitle = new Label
        {
            Text = "Wonder Card Manager",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        var lblInfo = new Label
        {
            Text = $"Save File: {SAV.GetType().Name}\nGeneration: {SAV.Generation}\n\n" +
                   "Wonder Card management is available through:\n" +
                   "• Tools > Data > Mystery Gift Database\n\n" +
                   "This feature allows you to:\n" +
                   "• View wonder cards in your save\n" +
                   "• Import wonder cards from files\n" +
                   "• Export wonder cards to files\n\n" +
                   "For full wonder card editing, use the built-in\n" +
                   "Mystery Gift Database feature in the Tools menu.",
            Location = new Point(20, 60),
            Size = new Size(650, 300),
            ForeColor = Color.LightGray,
            Font = new Font("Segoe UI", 11F)
        };

        var btnOpenMGDB = new Button
        {
            Text = "Open Mystery Gift Database",
            Location = new Point(20, 380),
            Size = new Size(220, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };
        btnOpenMGDB.Click += (s, e) =>
        {
            WinFormsUtil.Alert("Please use Tools > Data > Mystery Gift Database\nto manage wonder cards.");
        };

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(560, 380),
            Size = new Size(100, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { lblTitle, lblInfo, btnOpenMGDB, btnClose });
    }
}
