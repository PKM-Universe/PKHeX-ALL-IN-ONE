using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// QR Code Generator - Generate QR codes for Pokemon sharing
/// Supports multiple formats: PKM data, Showdown sets, and custom URLs
/// </summary>
public class QRCodeGenerator
{
    private readonly SaveFile SAV;

    // QR code settings
    private const int DefaultModuleSize = 4;
    private const int DefaultMargin = 2;

    public QRCodeGenerator(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Generate QR code from Pokemon data (raw bytes)
    /// </summary>
    public Bitmap GenerateFromPokemon(PKM pk, int moduleSize = DefaultModuleSize)
    {
        var data = pk.EncryptedBoxData;
        var base64 = Convert.ToBase64String(data);
        return GenerateQRCode(base64, moduleSize);
    }

    /// <summary>
    /// Generate QR code from Showdown set text
    /// </summary>
    public Bitmap GenerateFromShowdown(PKM pk, int moduleSize = DefaultModuleSize)
    {
        var set = new ShowdownSet(pk);
        return GenerateQRCode(set.Text, moduleSize);
    }

    /// <summary>
    /// Generate QR code for a team (6 Pokemon)
    /// </summary>
    public Bitmap GenerateTeamQR(IList<PKM> team, int moduleSize = DefaultModuleSize)
    {
        var sb = new StringBuilder();
        foreach (var pk in team)
        {
            if (pk.Species == 0) continue;
            var set = new ShowdownSet(pk);
            sb.AppendLine(set.Text);
            sb.AppendLine();
        }
        return GenerateQRCode(sb.ToString(), moduleSize);
    }

    /// <summary>
    /// Generate QR code with custom data
    /// </summary>
    public Bitmap GenerateCustom(string data, int moduleSize = DefaultModuleSize)
    {
        return GenerateQRCode(data, moduleSize);
    }

    /// <summary>
    /// Save QR code to file
    /// </summary>
    public void SaveQRCode(Bitmap qrCode, string filePath, ImageFormat? format = null)
    {
        format ??= ImageFormat.Png;
        qrCode.Save(filePath, format);
    }

    /// <summary>
    /// Generate QR code with Pokemon info overlay
    /// </summary>
    public Bitmap GenerateWithOverlay(PKM pk, int moduleSize = DefaultModuleSize)
    {
        var qrCode = GenerateFromPokemon(pk, moduleSize);
        var result = new Bitmap(qrCode.Width, qrCode.Height + 60);

        using (var g = Graphics.FromImage(result))
        {
            g.Clear(Color.White);
            g.DrawImage(qrCode, 0, 0);

            // Add Pokemon info
            var speciesName = SpeciesName.GetSpeciesName(pk.Species, 2);
            var info = $"{speciesName} Lv.{pk.CurrentLevel}";
            if (pk.IsShiny) info += " ★";

            using var font = new Font("Arial", 12, FontStyle.Bold);
            var textSize = g.MeasureString(info, font);
            var x = (result.Width - textSize.Width) / 2;
            g.DrawString(info, font, Brushes.Black, x, qrCode.Height + 10);

            // Add stats summary
            using var smallFont = new Font("Arial", 8);
            var stats = $"HP:{pk.Stat_HPCurrent} ATK:{pk.Stat_ATK} DEF:{pk.Stat_DEF} SPA:{pk.Stat_SPA} SPD:{pk.Stat_SPD} SPE:{pk.Stat_SPE}";
            var statsSize = g.MeasureString(stats, smallFont);
            var sx = (result.Width - statsSize.Width) / 2;
            g.DrawString(stats, smallFont, Brushes.Gray, sx, qrCode.Height + 35);
        }

        return result;
    }

    /// <summary>
    /// Generate multiple QR codes for a box
    /// </summary>
    public Dictionary<int, Bitmap> GenerateBoxQRCodes(int box, int moduleSize = DefaultModuleSize)
    {
        var result = new Dictionary<int, Bitmap>();
        var pokemon = SAV.GetBoxData(box);

        for (int i = 0; i < pokemon.Length; i++)
        {
            if (pokemon[i].Species > 0)
            {
                result[i] = GenerateFromPokemon(pokemon[i], moduleSize);
            }
        }

        return result;
    }

    /// <summary>
    /// Generate a contact sheet with multiple QR codes
    /// </summary>
    public Bitmap GenerateContactSheet(IList<PKM> pokemon, int columns = 4, int moduleSize = 3)
    {
        var qrCodes = new List<Bitmap>();
        foreach (var pk in pokemon)
        {
            if (pk.Species > 0)
            {
                qrCodes.Add(GenerateWithOverlay(pk, moduleSize));
            }
        }

        if (qrCodes.Count == 0)
            return new Bitmap(100, 100);

        var qrWidth = qrCodes[0].Width;
        var qrHeight = qrCodes[0].Height;
        var rows = (int)Math.Ceiling(qrCodes.Count / (double)columns);

        var result = new Bitmap(columns * qrWidth, rows * qrHeight);
        using (var g = Graphics.FromImage(result))
        {
            g.Clear(Color.White);

            for (int i = 0; i < qrCodes.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;
                g.DrawImage(qrCodes[i], col * qrWidth, row * qrHeight);
            }
        }

        return result;
    }

    /// <summary>
    /// Parse QR code data back to Pokemon (for importing)
    /// </summary>
    public PKM? ParseQRData(string qrData)
    {
        try
        {
            // Try Base64 encoded PKM data first
            if (IsBase64(qrData))
            {
                var data = Convert.FromBase64String(qrData);
                return EntityFormat.GetFromBytes(data);
            }

            // Try Showdown format
            var set = new ShowdownSet(qrData);
            if (set.Species > 0)
            {
                var pk = SAV.BlankPKM;
                pk.Species = set.Species;
                pk.Form = set.Form;
                pk.CurrentLevel = set.Level;
                // ... additional set properties
                return pk;
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Generate QR code bitmap using simple implementation
    /// </summary>
    private Bitmap GenerateQRCode(string data, int moduleSize)
    {
        // Simple QR code generation using basic encoding
        // In production, would use a proper QR code library like QRCoder

        var encoder = new SimpleQREncoder();
        var modules = encoder.Encode(data);

        var size = modules.GetLength(0);
        var imageSize = (size + DefaultMargin * 2) * moduleSize;
        var bitmap = new Bitmap(imageSize, imageSize);

        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.White);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (modules[y, x])
                    {
                        g.FillRectangle(Brushes.Black,
                            (x + DefaultMargin) * moduleSize,
                            (y + DefaultMargin) * moduleSize,
                            moduleSize,
                            moduleSize);
                    }
                }
            }
        }

        return bitmap;
    }

    private bool IsBase64(string value)
    {
        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Simple QR code encoder - Creates a visual representation
/// For production use, integrate a proper QR library like QRCoder or ZXing
/// </summary>
public class SimpleQREncoder
{
    // This is a simplified visual representation generator
    // A real QR encoder would use Reed-Solomon error correction and proper encoding modes

    public bool[,] Encode(string data)
    {
        // Calculate size based on data length (simplified)
        int dataLen = data.Length;
        int size;

        if (dataLen <= 25) size = 21;      // Version 1
        else if (dataLen <= 47) size = 25;  // Version 2
        else if (dataLen <= 77) size = 29;  // Version 3
        else if (dataLen <= 114) size = 33; // Version 4
        else if (dataLen <= 154) size = 37; // Version 5
        else if (dataLen <= 195) size = 41; // Version 6
        else if (dataLen <= 367) size = 49; // Version 8
        else if (dataLen <= 652) size = 61; // Version 11
        else if (dataLen <= 1273) size = 77; // Version 15
        else size = 101; // Version 20

        var modules = new bool[size, size];

        // Add finder patterns (top-left, top-right, bottom-left)
        AddFinderPattern(modules, 0, 0);
        AddFinderPattern(modules, size - 7, 0);
        AddFinderPattern(modules, 0, size - 7);

        // Add timing patterns
        for (int i = 8; i < size - 8; i++)
        {
            modules[6, i] = i % 2 == 0;
            modules[i, 6] = i % 2 == 0;
        }

        // Add alignment pattern for versions > 1
        if (size > 21)
        {
            int alignPos = size - 9;
            AddAlignmentPattern(modules, alignPos, alignPos);
        }

        // Encode data into remaining space (simplified - creates visual pattern from data)
        var dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
        int byteIndex = 0;
        int bitIndex = 0;

        for (int y = size - 1; y >= 0; y -= 2)
        {
            if (y == 6) y--; // Skip timing pattern column

            for (int x = 0; x < size; x++)
            {
                for (int col = 0; col < 2; col++)
                {
                    int cx = y - col;
                    if (cx < 0) continue;

                    // Skip reserved areas
                    if (IsReserved(cx, x, size)) continue;

                    if (byteIndex < dataBytes.Length)
                    {
                        modules[x, cx] = ((dataBytes[byteIndex] >> (7 - bitIndex)) & 1) == 1;
                        bitIndex++;
                        if (bitIndex == 8)
                        {
                            bitIndex = 0;
                            byteIndex++;
                        }
                    }
                    else
                    {
                        // Fill remaining with pattern
                        modules[x, cx] = (x + cx) % 2 == 0;
                    }
                }
            }
        }

        // Apply mask pattern
        ApplyMask(modules, size);

        return modules;
    }

    private void AddFinderPattern(bool[,] modules, int x, int y)
    {
        for (int dy = 0; dy < 7; dy++)
        {
            for (int dx = 0; dx < 7; dx++)
            {
                bool isOuter = dx == 0 || dx == 6 || dy == 0 || dy == 6;
                bool isInner = dx >= 2 && dx <= 4 && dy >= 2 && dy <= 4;
                modules[y + dy, x + dx] = isOuter || isInner;
            }
        }

        // Add separator
        int size = modules.GetLength(0);
        for (int i = 0; i < 8; i++)
        {
            if (y + 7 < size && x + i < size) modules[y + 7, x + i] = false;
            if (y + i < size && x + 7 < size) modules[y + i, x + 7] = false;
        }
    }

    private void AddAlignmentPattern(bool[,] modules, int x, int y)
    {
        for (int dy = -2; dy <= 2; dy++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                bool isOuter = Math.Abs(dx) == 2 || Math.Abs(dy) == 2;
                bool isCenter = dx == 0 && dy == 0;
                modules[y + dy, x + dx] = isOuter || isCenter;
            }
        }
    }

    private bool IsReserved(int x, int y, int size)
    {
        // Finder patterns and separators
        if ((x < 9 && y < 9) ||
            (x < 9 && y >= size - 8) ||
            (x >= size - 8 && y < 9))
            return true;

        // Timing patterns
        if (x == 6 || y == 6)
            return true;

        // Alignment pattern area (simplified)
        if (size > 21 && x >= size - 11 && x <= size - 7 && y >= size - 11 && y <= size - 7)
            return true;

        return false;
    }

    private void ApplyMask(bool[,] modules, int size)
    {
        // Apply mask pattern 0: (x + y) % 2 == 0
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (!IsReserved(x, y, size))
                {
                    if ((x + y) % 2 == 0)
                        modules[y, x] = !modules[y, x];
                }
            }
        }
    }
}

/// <summary>
/// QR Code generation options
/// </summary>
public class QRCodeOptions
{
    public int ModuleSize { get; set; } = 4;
    public int Margin { get; set; } = 2;
    public Color ForegroundColor { get; set; } = Color.Black;
    public Color BackgroundColor { get; set; } = Color.White;
    public bool IncludeOverlay { get; set; } = false;
    public QRCodeFormat Format { get; set; } = QRCodeFormat.PKMData;
}

public enum QRCodeFormat
{
    PKMData,      // Raw encrypted PKM bytes
    Showdown,     // Showdown text format
    PokemonInfo   // Custom info string
}
