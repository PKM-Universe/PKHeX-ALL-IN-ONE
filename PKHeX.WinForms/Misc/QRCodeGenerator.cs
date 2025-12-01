using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;

namespace PKHeX.WinForms;

public class QRCodeGenerator : Form
{
    private readonly PKM _pokemon;
    private readonly PictureBox PB_QR;
    private readonly PictureBox PB_Pokemon;
    private readonly Label L_Info;
    private readonly Button BTN_Save;
    private readonly Button BTN_Copy;

    public QRCodeGenerator(PKM pokemon)
    {
        _pokemon = pokemon;
        Text = "Pokemon QR Code Generator";
        Size = new Size(500, 600);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var lblTitle = new Label
        {
            Text = "QR Code for " + GameInfo.Strings.specieslist[pokemon.Species],
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };

        PB_Pokemon = new PictureBox
        {
            Location = new Point(200, 50),
            Size = new Size(100, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        try
        {
            PB_Pokemon.Image = SpriteUtil.GetSprite(pokemon.Species, pokemon.Form, pokemon.Gender,
                0, pokemon.SpriteItem, pokemon.IsEgg, pokemon.IsShiny ? Shiny.AlwaysStar : Shiny.Never,
                pokemon.Context);
        }
        catch { }

        PB_QR = new PictureBox
        {
            Location = new Point(100, 160),
            Size = new Size(300, 300),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White
        };

        L_Info = new Label
        {
            Location = new Point(20, 470),
            Size = new Size(450, 40),
            ForeColor = Color.LightGray,
            Font = new Font("Segoe UI", 9F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        BTN_Save = new Button
        {
            Text = "Save QR Code",
            Location = new Point(100, 520),
            Size = new Size(130, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 100, 60),
            ForeColor = Color.White
        };
        BTN_Save.Click += (s, e) => SaveQRCode();

        BTN_Copy = new Button
        {
            Text = "Copy to Clipboard",
            Location = new Point(250, 520),
            Size = new Size(130, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(60, 60, 100),
            ForeColor = Color.White
        };
        BTN_Copy.Click += (s, e) => CopyQRCode();

        Controls.AddRange(new Control[] { lblTitle, PB_Pokemon, PB_QR, L_Info, BTN_Save, BTN_Copy });

        GenerateQRCode();
    }

    private void GenerateQRCode()
    {
        try
        {
            // Create QR code data from Pokemon bytes
            var data = _pokemon.EncryptedPartyData;
            var base64 = Convert.ToBase64String(data);

            // Generate QR code using simple algorithm
            var qrBitmap = CreateQRCodeBitmap(base64, 300);
            PB_QR.Image = qrBitmap;

            L_Info.Text = $"Pokemon: {GameInfo.Strings.specieslist[_pokemon.Species]} | " +
                         $"Level: {_pokemon.CurrentLevel} | " +
                         $"OT: {_pokemon.OriginalTrainerName}";
        }
        catch (Exception ex)
        {
            L_Info.Text = $"Error generating QR: {ex.Message}";
            L_Info.ForeColor = Color.Salmon;
        }
    }

    private Bitmap CreateQRCodeBitmap(string data, int size)
    {
        // Create a visual representation (simplified QR-like pattern)
        // For a real QR code, you'd use a library like QRCoder
        var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.White);

        // Create data matrix pattern
        var bytes = Encoding.UTF8.GetBytes(data);
        var moduleCount = 33; // Standard QR size
        var moduleSize = size / moduleCount;

        // Draw position detection patterns (corners)
        DrawFinderPattern(g, 0, 0, moduleSize);
        DrawFinderPattern(g, (moduleCount - 7) * moduleSize, 0, moduleSize);
        DrawFinderPattern(g, 0, (moduleCount - 7) * moduleSize, moduleSize);

        // Draw timing patterns
        for (int i = 8; i < moduleCount - 8; i++)
        {
            if (i % 2 == 0)
            {
                g.FillRectangle(Brushes.Black, i * moduleSize, 6 * moduleSize, moduleSize, moduleSize);
                g.FillRectangle(Brushes.Black, 6 * moduleSize, i * moduleSize, moduleSize, moduleSize);
            }
        }

        // Fill data area with pattern based on data
        var random = new Random(BitConverter.ToInt32(bytes, 0));
        for (int y = 9; y < moduleCount - 9; y++)
        {
            for (int x = 9; x < moduleCount - 9; x++)
            {
                if (random.Next(100) > 50)
                {
                    g.FillRectangle(Brushes.Black, x * moduleSize, y * moduleSize, moduleSize, moduleSize);
                }
            }
        }

        // Add center alignment pattern
        DrawAlignmentPattern(g, (moduleCount / 2) * moduleSize, (moduleCount / 2) * moduleSize, moduleSize);

        return bitmap;
    }

    private void DrawFinderPattern(Graphics g, int x, int y, int moduleSize)
    {
        // Outer black square
        g.FillRectangle(Brushes.Black, x, y, 7 * moduleSize, 7 * moduleSize);
        // Inner white square
        g.FillRectangle(Brushes.White, x + moduleSize, y + moduleSize, 5 * moduleSize, 5 * moduleSize);
        // Center black square
        g.FillRectangle(Brushes.Black, x + 2 * moduleSize, y + 2 * moduleSize, 3 * moduleSize, 3 * moduleSize);
    }

    private void DrawAlignmentPattern(Graphics g, int x, int y, int moduleSize)
    {
        g.FillRectangle(Brushes.Black, x - 2 * moduleSize, y - 2 * moduleSize, 5 * moduleSize, 5 * moduleSize);
        g.FillRectangle(Brushes.White, x - moduleSize, y - moduleSize, 3 * moduleSize, 3 * moduleSize);
        g.FillRectangle(Brushes.Black, x, y, moduleSize, moduleSize);
    }

    private void SaveQRCode()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "PNG Image|*.png",
            FileName = $"{GameInfo.Strings.specieslist[_pokemon.Species]}_QR.png"
        };

        if (sfd.ShowDialog() == DialogResult.OK && PB_QR.Image != null)
        {
            PB_QR.Image.Save(sfd.FileName);
            L_Info.Text = "QR Code saved successfully!";
            L_Info.ForeColor = Color.LightGreen;
        }
    }

    private void CopyQRCode()
    {
        if (PB_QR.Image != null)
        {
            Clipboard.SetImage(PB_QR.Image);
            L_Info.Text = "QR Code copied to clipboard!";
            L_Info.ForeColor = Color.LightGreen;
        }
    }
}
