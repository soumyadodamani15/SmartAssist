using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SmartAssist.Core.DTOs;
using SmartAssist.Core.Interfaces;

namespace SmartAssist.Infrastructure.Services;

public class RagQueryService
{
    private readonly IDocumentChunkRepository _chunkRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly HttpClient _httpClient;
    private const string OllamaUrl = "http://localhost:11434/api/generate";
    private const string Model = "llama3.2";

    public RagQueryService(
        IDocumentChunkRepository chunkRepository,
        IEmbeddingService embeddingService,
        HttpClient httpClient)
    {
        _chunkRepository = chunkRepository;
        _embeddingService = embeddingService;
        _httpClient = httpClient;
    }

    public async Task<QueryResponse> QueryAsync(QueryRequest request)
    {
        var questionEmbedding = await _embeddingService
            .GenerateEmbeddingAsync(request.Question);

        var similarChunks = await _chunkRepository
            .SearchSimilarAsync(questionEmbedding, request.TopK);

        var chunks = similarChunks.ToList();

        if (!chunks.Any())
        {
            return new QueryResponse
            {
                Answer = "I could not find any relevant information to answer your question.",
                Sources = new List<SourceReference>()
            };
        }

        var contextBuilder = new StringBuilder();
        foreach (var chunk in chunks)
        {
            contextBuilder.AppendLine(chunk.Content);
            contextBuilder.AppendLine("---");
        }

        var prompt = $"""
            You are a helpful enterprise knowledge assistant.
            Answer the question based ONLY on the provided context.
            If the context does not contain enough information, say so.
            Do not make up information that is not in the context.

            Context:
            {contextBuilder}

            Question: {request.Question}

            Answer:
            """;

        var ollamaRequest = new
        {
            model = Model,
            prompt = prompt,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(OllamaUrl, ollamaRequest);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(json);
        var answer = result.RootElement
            .GetProperty("response").GetString() ?? "No answer generated.";

        var sources = chunks.Select(c => new SourceReference
        {
            DocumentTitle = c.DocumentTitle,
            ChunkContent = c.Content,
            SimilarityScore = Math.Round(c.SimilarityScore, 4)
        }).ToList();

        return new QueryResponse
        {
            Answer = answer.Trim(),
            Sources = sources
        };
    }
}