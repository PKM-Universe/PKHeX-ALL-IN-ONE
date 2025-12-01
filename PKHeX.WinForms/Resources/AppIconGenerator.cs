using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace PKHeX.WinForms.ResourceGenerators;

/// <summary>
/// Generates a custom PKM Universe app icon programmatically
/// </summary>
public static class AppIconGenerator
{
    public static Icon GenerateAppIcon(int size = 256)
    {
        using var bitmap = GenerateIconBitmap(size);
        return Icon.FromHandle(bitmap.GetHicon());
    }

    public static Bitmap GenerateIconBitmap(int size = 256)
    {
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bitmap);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.CompositingQuality = CompositingQuality.HighQuality;

        var rect = new Rectangle(0, 0, size, size);
        var center = size / 2f;
        var radius = size / 2f - 4;

        // Background gradient (purple to blue - PKM Universe colors)
        using (var path = new GraphicsPath())
        {
            path.AddEllipse(2, 2, size - 4, size - 4);
            using var brush = new LinearGradientBrush(rect,
                Color.FromArgb(130, 50, 200),
                Color.FromArgb(50, 100, 200),
                LinearGradientMode.ForwardDiagonal);
            g.FillPath(brush, path);
        }

        // Outer ring glow
        using (var pen = new Pen(Color.FromArgb(100, 180, 130, 255), 4))
        {
            g.DrawEllipse(pen, 6, 6, size - 12, size - 12);
        }

        // Pokeball design elements
        var halfHeight = size / 2f;

        // Top half (darker shade)
        using (var topPath = new GraphicsPath())
        {
            topPath.AddArc(8, 8, size - 16, size - 16, 180, 180);
            topPath.CloseFigure();
            using var topBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0));
            g.FillPath(topBrush, topPath);
        }

        // Center line
        using (var linePen = new Pen(Color.FromArgb(200, 255, 255, 255), size / 25f))
        {
            g.DrawLine(linePen, 8, halfHeight, size - 8, halfHeight);
        }

        // Center button background
        var buttonSize = size / 3.5f;
        var buttonRect = new RectangleF(center - buttonSize / 2, center - buttonSize / 2, buttonSize, buttonSize);

        using (var buttonPath = new GraphicsPath())
        {
            buttonPath.AddEllipse(buttonRect);

            // White outer ring
            using (var outerBrush = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
            {
                g.FillPath(outerBrush, buttonPath);
            }

            // Inner button
            var innerSize = buttonSize * 0.7f;
            var innerRect = new RectangleF(center - innerSize / 2, center - innerSize / 2, innerSize, innerSize);

            using var innerPath = new GraphicsPath();
            innerPath.AddEllipse(innerRect);

            using var innerBrush = new LinearGradientBrush(innerRect,
                Color.FromArgb(180, 100, 255),
                Color.FromArgb(100, 80, 200),
                LinearGradientMode.ForwardDiagonal);
            g.FillPath(innerBrush, innerPath);

            // Shine on button
            var shineSize = innerSize * 0.4f;
            var shineRect = new RectangleF(center - shineSize / 2 - 2, center - shineSize / 2 - 2, shineSize, shineSize);
            using var shineBrush = new SolidBrush(Color.FromArgb(100, 255, 255, 255));
            g.FillEllipse(shineBrush, shineRect);
        }

        // "P" letter or star in center
        DrawCenterSymbol(g, center, size);

        // Highlight/shine on top
        using (var shinePath = new GraphicsPath())
        {
            var shineRect = new RectangleF(size * 0.2f, size * 0.1f, size * 0.35f, size * 0.2f);
            shinePath.AddEllipse(shineRect);
            using var shineBrush = new SolidBrush(Color.FromArgb(40, 255, 255, 255));
            g.FillPath(shineBrush, shinePath);
        }

        return bitmap;
    }

    private static void DrawCenterSymbol(Graphics g, float center, int size)
    {
        // Draw a stylized "U" for Universe or star
        var symbolSize = size / 8f;
        using var font = new Font("Segoe UI", symbolSize, FontStyle.Bold);
        using var brush = new SolidBrush(Color.White);

        // Unicode star
        var starText = "★";
        var starSize = g.MeasureString(starText, font);
        g.DrawString(starText, font, brush,
            center - starSize.Width / 2 + 1,
            center - starSize.Height / 2 + 1);
    }

    /// <summary>
    /// Saves the icon as ICO file with multiple sizes
    /// </summary>
    public static void SaveAsIcoFile(string path)
    {
        var sizes = new[] { 16, 24, 32, 48, 64, 128, 256 };

        using var stream = new FileStream(path, FileMode.Create);
        using var writer = new BinaryWriter(stream);

        // ICO header
        writer.Write((short)0);     // Reserved
        writer.Write((short)1);     // ICO type
        writer.Write((short)sizes.Length);  // Number of images

        var imageDataOffset = 6 + (sizes.Length * 16); // Header + directory entries
        var imageData = new byte[sizes.Length][];

        // Write directory entries
        for (int i = 0; i < sizes.Length; i++)
        {
            var size = sizes[i];
            using var bmp = GenerateIconBitmap(size);
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            imageData[i] = ms.ToArray();

            writer.Write((byte)(size < 256 ? size : 0));  // Width
            writer.Write((byte)(size < 256 ? size : 0));  // Height
            writer.Write((byte)0);      // Color palette
            writer.Write((byte)0);      // Reserved
            writer.Write((short)1);     // Color planes
            writer.Write((short)32);    // Bits per pixel
            writer.Write(imageData[i].Length);  // Size of image data
            writer.Write(imageDataOffset);      // Offset

            imageDataOffset += imageData[i].Length;
        }

        // Write image data
        foreach (var data in imageData)
        {
            writer.Write(data);
        }
    }

    /// <summary>
    /// Gets the app icon for use in forms
    /// </summary>
    public static Icon GetAppIcon()
    {
        try
        {
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pkmuni.ico");
            if (File.Exists(iconPath))
            {
                return new Icon(iconPath);
            }
        }
        catch { }

        return GenerateAppIcon(32);
    }

    /// <summary>
    /// Creates multiple size bitmaps for different uses
    /// </summary>
    public static Bitmap GetSplashLogo(int size = 128)
    {
        return GenerateIconBitmap(size);
    }

    public static Bitmap GetTaskbarIcon()
    {
        return GenerateIconBitmap(32);
    }

    public static Bitmap GetAboutLogo()
    {
        return GenerateIconBitmap(96);
    }
}

/// <summary>
/// Badge generator for various status indicators
/// </summary>
public static class StatusBadgeGenerator
{
    public static Bitmap CreateLegalBadge(int size = 24)
    {
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(Color.FromArgb(76, 175, 80));
        g.FillEllipse(brush, 1, 1, size - 2, size - 2);

        using var font = new Font("Segoe UI", size * 0.5f, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("✓", font, textBrush, new RectangleF(0, 0, size, size), sf);

        return bmp;
    }

    public static Bitmap CreateIllegalBadge(int size = 24)
    {
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(Color.FromArgb(244, 67, 54));
        g.FillEllipse(brush, 1, 1, size - 2, size - 2);

        using var font = new Font("Segoe UI", size * 0.5f, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("✗", font, textBrush, new RectangleF(0, 0, size, size), sf);

        return bmp;
    }

    public static Bitmap CreateShinyBadge(int size = 24)
    {
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Star shape for shiny
        var points = new PointF[10];
        var outerRadius = size / 2f - 1;
        var innerRadius = outerRadius * 0.4f;
        var cx = size / 2f;
        var cy = size / 2f;

        for (int i = 0; i < 10; i++)
        {
            var radius = i % 2 == 0 ? outerRadius : innerRadius;
            var angle = (i * 36 - 90) * Math.PI / 180;
            points[i] = new PointF(
                cx + radius * (float)Math.Cos(angle),
                cy + radius * (float)Math.Sin(angle)
            );
        }

        using var brush = new SolidBrush(Color.FromArgb(255, 215, 0));
        g.FillPolygon(brush, points);

        using var pen = new Pen(Color.FromArgb(200, 160, 0), 1);
        g.DrawPolygon(pen, points);

        return bmp;
    }

    public static Bitmap CreateEventBadge(int size = 24)
    {
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(Color.FromArgb(156, 39, 176));
        g.FillEllipse(brush, 1, 1, size - 2, size - 2);

        using var font = new Font("Segoe UI", size * 0.45f, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("E", font, textBrush, new RectangleF(0, 0, size, size), sf);

        return bmp;
    }
}
