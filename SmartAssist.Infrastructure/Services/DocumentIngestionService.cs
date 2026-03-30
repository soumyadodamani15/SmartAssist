using SmartAssist.Core.Entities;
using SmartAssist.Core.Interfaces;

namespace SmartAssist.Infrastructure.Services;

public class DocumentIngestionService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentChunkRepository _chunkRepository;
    private readonly IIngestionJobRepository _jobRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly DocumentChunker _chunker;

    public DocumentIngestionService(
        IDocumentRepository documentRepository,
        IDocumentChunkRepository chunkRepository,
        IIngestionJobRepository jobRepository,
        IEmbeddingService embeddingService,
        DocumentChunker chunker)
    {
        _documentRepository = documentRepository;
        _chunkRepository = chunkRepository;
        _jobRepository = jobRepository;
        _embeddingService = embeddingService;
        _chunker = chunker;
    }

    public async Task<IngestionJob> IngestAsync(string title, string content, string contentType)
    {
        // Step 1 — create document record
        var document = await _documentRepository.CreateAsync(new Document
        {
            Title = title,
            Source = title,
            ContentType = contentType
        });

        // Step 2 — create ingestion job to track progress
        var job = await _jobRepository.CreateAsync(new IngestionJob
        {
            DocumentId = document.Id,
            Status = "processing"
        });

        try
        {
            // Step 3 — split content into chunks
            var textChunks = _chunker.Chunk(content);

            // Step 4 — embed each chunk and store it
            for (int i = 0; i < textChunks.Count; i++)
            {
                var embedding = await _embeddingService
                    .GenerateEmbeddingAsync(textChunks[i]);

                await _chunkRepository.CreateAsync(new DocumentChunk
                {
                    DocumentId = document.Id,
                    ChunkIndex = i,
                    Content = textChunks[i],
                    Embedding = embedding,
                    TokenCount = textChunks[i].Split(' ').Length
                });
            }

            // Step 5 — mark job as completed
            await _jobRepository.UpdateStatusAsync(job.Id, "completed");
            job.Status = "completed";
        }
        catch (Exception ex)
        {
            // If anything fails, record the error
            await _jobRepository.UpdateStatusAsync(job.Id, "failed", ex.Message);
            job.Status = "failed";
            job.ErrorMessage = ex.Message;
        }

        return job;
    }
}