using System.Net.Http.Json;
using System.Text.Json;
using SmartAssist.Core.Interfaces;

namespace SmartAssist.Infrastructure.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private const string Model = "nomic-embed-text";

    public EmbeddingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new
        {
            model = Model,
            prompt = text
        };

        var response = await _httpClient.PostAsJsonAsync(
            "http://localhost:11434/api/embeddings", request);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(json);

        var embeddingArray = result.RootElement
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(e => e.GetSingle())
            .ToArray();

        return embeddingArray;
    }
}