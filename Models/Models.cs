namespace OcrTranslator.Models;

public sealed class AppConfig
{
    public string Hotkey { get; set; } = "Ctrl+Shift+T";
    public string TargetLanguage { get; set; } = "UK";
    public string SourceLanguage { get; set; } = "";
    public string TranslationProvider { get; set; } = "DeepL";
    public double Opacity { get; set; } = 0.92;
    public bool DarkTheme { get; set; } = true;
    public string? DeepLApiKeyProtected { get; set; }
    public string? GoogleApiKeyProtected { get; set; }
}

public readonly record struct CaptureRegion(int X, int Y, int Width, int Height)
{
    public bool IsValid => Width > 2 && Height > 2;
}

public sealed record OcrResult(string Text, string? DetectedLanguage, TimeSpan Duration);
