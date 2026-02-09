using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

var sizes = new[] { 16, 20, 24, 32, 48, 64, 128, 256 };
var bitmaps = new List<Bitmap>();

foreach (var size in sizes)
{
    bitmaps.Add(CreateIconBitmap(size));
}

var assetsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "VietIME.App", "Assets"));
if (!Directory.Exists(assetsDir))
{
    // Try relative from working directory
    assetsDir = Path.GetFullPath(Path.Combine(".", "..", "..", "src", "VietIME.App", "Assets"));
}

// Allow override via command line
if (args.Length > 0)
{
    assetsDir = args[0];
}

Console.WriteLine($"Output directory: {assetsDir}");
Directory.CreateDirectory(assetsDir);

var icoPath = Path.Combine(assetsDir, "icon.ico");
SaveAsIco(bitmaps, icoPath);
Console.WriteLine($"ICO saved: {icoPath}");

var pngPath = Path.Combine(assetsDir, "icon.png");
using (var png256 = CreateIconBitmap(256))
{
    png256.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);
}
Console.WriteLine($"PNG saved: {pngPath}");

foreach (var bmp in bitmaps) bmp.Dispose();
Console.WriteLine("Done!");

static Bitmap CreateIconBitmap(int size)
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
    float cornerRadius = size * 0.20f;

    // Background: deep dark with subtle warm undertone
    var bgColor = Color.FromArgb(20, 20, 28);
    using var bgPath = CreateRoundedRect(padding, padding, rectSize, rectSize, cornerRadius);
    using var bgBrush = new SolidBrush(bgColor);
    g.FillPath(bgBrush, bgPath);

    // Very subtle border for definition against dark backgrounds
    using var borderPen = new Pen(Color.FromArgb(35, 255, 255, 255), Math.Max(1f, size * 0.015f));
    g.DrawPath(borderPen, bgPath);

    // "V" character - bold red
    var vColor = Color.FromArgb(215, 40, 40); // Professional red

    float fontSize = size * 0.55f;
    using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);

    // Measure and center precisely
    var textSize = g.MeasureString("V", font);
    float x = (size - textSize.Width) / 2 + size * 0.015f;
    float y = (size - textSize.Height) / 2;

    // Subtle red glow for larger sizes
    if (size >= 32)
    {
        float glowRadius = size / 32f;
        for (int step = 3; step >= 1; step--)
        {
            float offset = glowRadius * step * 0.6f;
            int alpha = 15 / step;
            var glowColor = Color.FromArgb(alpha, 215, 40, 40);
            using var glowBrush = new SolidBrush(glowColor);
            g.DrawString("V", font, glowBrush, x - offset, y);
            g.DrawString("V", font, glowBrush, x + offset, y);
            g.DrawString("V", font, glowBrush, x, y - offset);
            g.DrawString("V", font, glowBrush, x, y + offset);
        }
    }

    using var textBrush = new SolidBrush(vColor);
    g.DrawString("V", font, textBrush, x, y);

    return bitmap;
}

static GraphicsPath CreateRoundedRect(float x, float y, float width, float height, float radius)
{
    var path = new GraphicsPath();
    float d = radius * 2;
    path.AddArc(x, y, d, d, 180, 90);
    path.AddArc(x + width - d, y, d, d, 270, 90);
    path.AddArc(x + width - d, y + height - d, d, d, 0, 90);
    path.AddArc(x, y + height - d, d, d, 90, 90);
    path.CloseFigure();
    return path;
}

static void SaveAsIco(List<Bitmap> bitmaps, string outputPath)
{
    using var ms = new MemoryStream();
    using var writer = new BinaryWriter(ms);

    writer.Write((short)0);
    writer.Write((short)1);
    writer.Write((short)bitmaps.Count);

    var pngDataList = new List<byte[]>();
    foreach (var bmp in bitmaps)
    {
        using var pngStream = new MemoryStream();
        bmp.Save(pngStream, System.Drawing.Imaging.ImageFormat.Png);
        pngDataList.Add(pngStream.ToArray());
    }

    int headerSize = 6 + (16 * bitmaps.Count);
    int currentOffset = headerSize;

    for (int i = 0; i < bitmaps.Count; i++)
    {
        var bmp = bitmaps[i];
        var pngData = pngDataList[i];
        writer.Write((byte)(bmp.Width >= 256 ? 0 : bmp.Width));
        writer.Write((byte)(bmp.Height >= 256 ? 0 : bmp.Height));
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((short)1);
        writer.Write((short)32);
        writer.Write(pngData.Length);
        writer.Write(currentOffset);
        currentOffset += pngData.Length;
    }

    foreach (var pngData in pngDataList)
    {
        writer.Write(pngData);
    }

    File.WriteAllBytes(outputPath, ms.ToArray());
}
