using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OcrTranslator.Models;

namespace OcrTranslator.Core;

public sealed class ConfigService
{
    private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aur", "OcrTranslator", "config.json");
    public AppConfig Current { get; private set; } = new();
    public void Load()
    {
        try { if (File.Exists(_path)) Current = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(_path)) ?? new(); }
        catch { Current = new(); }
    }
    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
    }
    public string? GetApiKey()
    {
        if (string.IsNullOrWhiteSpace(Current.DeepLApiKeyProtected)) return null;
        try { return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(Current.DeepLApiKeyProtected), null, DataProtectionScope.CurrentUser)); }
        catch { return null; }
    }
    public string? GetGoogleApiKey()
    {
        if (string.IsNullOrWhiteSpace(Current.GoogleApiKeyProtected)) return null;
        try { return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(Current.GoogleApiKeyProtected), null, DataProtectionScope.CurrentUser)); }
        catch { return null; }
    }
    public void SetApiKey(string value)
    {
        var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser);
        Current.DeepLApiKeyProtected = Convert.ToBase64String(bytes);
    }
    public void SetGoogleApiKey(string value)
    {
        var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser);
        Current.GoogleApiKeyProtected = Convert.ToBase64String(bytes);
    }
}
