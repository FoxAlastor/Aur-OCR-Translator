using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using OcrTranslator.Models;
using ModelOcrResult = OcrTranslator.Models.OcrResult;

namespace OcrTranslator.Core;

public sealed class ScreenCaptureService
{
    public byte[] Capture(CaptureRegion region)
    {
        using var bitmap = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
            graphics.CopyFromScreen(region.X, region.Y, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
}

public sealed class WindowsOcrProvider : IOcrProvider
{
    public async Task<ModelOcrResult> RecognizeAsync(byte[] image, string language, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        using var stream = new InMemoryRandomAccessStream();
        await stream.WriteAsync(image.AsBuffer());
        stream.Seek(0);
        var decoder = await BitmapDecoder.CreateAsync(stream);
        using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
        var windowsLanguage = language.ToUpperInvariant() switch
        {
            "UK" => "uk-UA",
            "RU" => "ru-RU",
            "EN" => "en-US",
            _ => language
        };
        var engine = string.IsNullOrWhiteSpace(windowsLanguage)
            ? OcrEngine.TryCreateFromUserProfileLanguages()
            : OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language(windowsLanguage));
        if (engine is null) throw new InvalidOperationException("Windows OCR недоступний. Перевірте MSIX package identity та мовний пакет OCR.");
        var result = await engine.RecognizeAsync(softwareBitmap);
        var text = string.Join(Environment.NewLine, result.Lines.Select(line => line.Text));
        return new ModelOcrResult(text, engine.RecognizerLanguage.LanguageTag, timer.Elapsed);
    }
}
