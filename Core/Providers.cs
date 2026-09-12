using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using OcrTranslator.Models;

namespace OcrTranslator.Core;

public interface IOcrProvider
{
    Task<OcrResult> RecognizeAsync(byte[] image, string language, CancellationToken cancellationToken);
}

public interface ITranslationProvider
{
    Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage, CancellationToken cancellationToken);
}

public sealed class DeepLTranslationProvider(HttpClient http, Func<string?> getKey) : ITranslationProvider
{
    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage, CancellationToken cancellationToken)
    {
        var key = getKey();
        if (string.IsNullOrWhiteSpace(key)) return "Вкажіть DeepL API-ключ у Settings.";
        var body = new Dictionary<string, object?> { ["text"] = new[] { text }, ["target_lang"] = targetLanguage };
        if (!string.IsNullOrWhiteSpace(sourceLanguage)) body["source_lang"] = sourceLanguage;
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api-free.deepl.com/v2/translate")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.TryAddWithoutValidation("Authorization", $"DeepL-Auth-Key {key}");
        using var response = await http.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"DeepL {((int)response.StatusCode)}: {payload}");
        using var json = JsonDocument.Parse(payload);
        return json.RootElement.GetProperty("translations")[0].GetProperty("text").GetString() ?? "";
    }
}
