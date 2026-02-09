// C# Script to generate VietIME icon
// Run with: dotnet-script GenerateIcon.csx
// Or compile as console app

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Collections.Generic;

// Generate icon at multiple sizes
var sizes = new[] { 16, 20, 24, 32, 48, 64, 128, 256 };
var bitmaps = new List<Bitmap>();

foreach (var size in sizes)
{
    bitmaps.Add(CreateIconBitmap(size, true));
}

// Save as .ico
var outputPath = Path.Combine("..", "src", "VietIME.App", "Assets", "icon.ico");
SaveAsIco(bitmaps, outputPath);
Console.WriteLine($"Icon saved to: {Path.GetFullPath(outputPath)}");

// Also save PNG for reference
var pngPath = Path.Combine("..", "src", "VietIME.App", "Assets", "icon.png");
var png256 = CreateIconBitmap(256, true);
png256.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);
Console.WriteLine($"PNG saved to: {Path.GetFullPath(pngPath)}");

// Cleanup
foreach (var bmp in bitmaps) bmp.Dispose();
png256.Dispose();

Console.WriteLine("Done!");

static Bitmap CreateIconBitmap(int size, bool enabled)
{
    var bitmap = new Bitmap(size, size);
    using var g = Graphics.FromImage(bitmap);

    g.SmoothingMode = SmoothingMode.HighQuality;
    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    g.Clear(Color.Transparent);

    float padding = size * 0.03f;
    float rectSize = size - padding * 2;
    float cornerRadius = size * 0.22f;

    // Background: dark rounded square
    var bgColor = Color.FromArgb(24, 24, 32); // Near-black with slight blue tint
    using var bgPath = CreateRoundedRect(padding, padding, rectSize, rectSize, cornerRadius);
    using var bgBrush = new SolidBrush(bgColor);
    g.FillPath(bgBrush, bgPath);

    // Subtle border for definition
    using var borderPen = new Pen(Color.FromArgb(50, 255, 255, 255), size * 0.02f);
    g.DrawPath(borderPen, bgPath);

    // Draw "V" character
    var vColor = enabled
        ? Color.FromArgb(220, 38, 38)   // Rich red for enabled
        : Color.FromArgb(100, 100, 110); // Gray for disabled

    float fontSize = size * 0.52f;
    using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
    using var textBrush = new SolidBrush(vColor);

    // Measure and center
    var textSize = g.MeasureString("V", font);
    float x = (size - textSize.Width) / 2 + size * 0.02f;
    float y = (size - textSize.Height) / 2 + size * 0.01f;

    // Subtle glow effect for the V (only for larger sizes)
    if (size >= 32 && enabled)
    {
        var glowColor = Color.FromArgb(40, 220, 38, 38);
        using var glowBrush = new SolidBrush(glowColor);
        for (float offset = 2f * size / 32f; offset >= 0.5f; offset -= 0.5f)
        {
            g.DrawString("V", font, glowBrush, x - offset, y);
            g.DrawString("V", font, glowBrush, x + offset, y);
            g.DrawString("V", font, glowBrush, x, y - offset);
            g.DrawString("V", font, glowBrush, x, y + offset);
        }
    }

    g.DrawString("V", font, textBrush, x, y);

    return bitmap;
}

static GraphicsPath CreateRoundedRect(float x, float y, float width, float height, float radius)
{
    var path = new GraphicsPath();
    float diameter = radius * 2;

    path.AddArc(x, y, diameter, diameter, 180, 90);
    path.AddArc(x + width - diameter, y, diameter, diameter, 270, 90);
    path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90);
    path.AddArc(x, y + height - diameter, diameter, diameter, 90, 90);
    path.CloseFigure();

    return path;
}

static void SaveAsIco(List<Bitmap> bitmaps, string outputPath)
{
    using var ms = new MemoryStream();
    using var writer = new BinaryWriter(ms);

    // ICO header
    writer.Write((short)0);     // Reserved
    writer.Write((short)1);     // Type: ICO
    writer.Write((short)bitmaps.Count); // Number of images

    // Calculate offsets
    int headerSize = 6 + (16 * bitmaps.Count);
    var pngDataList = new List<byte[]>();

    foreach (var bmp in bitmaps)
    {
        using var pngStream = new MemoryStream();
        bmp.Save(pngStream, System.Drawing.Imaging.ImageFormat.Png);
        pngDataList.Add(pngStream.ToArray());
    }

    int currentOffset = headerSize;

    for (int i = 0; i < bitmaps.Count; i++)
    {
        var bmp = bitmaps[i];
        var pngData = pngDataList[i];

        writer.Write((byte)(bmp.Width >= 256 ? 0 : bmp.Width));   // Width
        writer.Write((byte)(bmp.Height >= 256 ? 0 : bmp.Height)); // Height
        writer.Write((byte)0);      // Color palette
        writer.Write((byte)0);      // Reserved
        writer.Write((short)1);     // Color planes
        writer.Write((short)32);    // Bits per pixel
        writer.Write(pngData.Length); // Size of image data
        writer.Write(currentOffset);  // Offset to image data

        currentOffset += pngData.Length;
    }

    // Write PNG data
    foreach (var pngData in pngDataList)
    {
        writer.Write(pngData);
    }

    // Save to file
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    File.WriteAllBytes(outputPath, ms.ToArray());
}
