using SmartAssist.Core.Entities;
namespace SmartAssist.Core.Interfaces;

public interface IDocumentChunkRepository
{
    Task<DocumentChunk> CreateAsync(DocumentChunk chunk);
    Task CreateBatchAsync(IEnumerable<DocumentChunk> chunks);
    Task<IEnumerable<DocumentChunk>> GetByDocumentIdAsync(Guid documentId);
    Task<IEnumerable<DocumentChunk>> SearchSimilarAsync(float[] embedding, int topK = 5);
}