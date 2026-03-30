using Dapper;
using Npgsql;
using Pgvector;
using SmartAssist.Core.Entities;
using SmartAssist.Core.Interfaces;
using SmartAssist.Infrastructure.Data;

namespace SmartAssist.Infrastructure.Repositories;

public class DocumentChunkRepository : IDocumentChunkRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DocumentChunkRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DocumentChunk> CreateAsync(DocumentChunk chunk)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO document_chunks 
                (id, document_id, chunk_index, content, embedding, token_count, created_at)
            VALUES 
                (@Id, @DocumentId, @ChunkIndex, @Content, @Embedding::vector, @TokenCount, @CreatedAt)
            RETURNING id, document_id AS DocumentId, chunk_index AS ChunkIndex,
                      content, token_count AS TokenCount, created_at AS CreatedAt
            """;

        chunk.Id = Guid.NewGuid();
        chunk.CreatedAt = DateTime.UtcNow;

        return await connection.QueryFirstAsync<DocumentChunk>(sql, new
        {
            chunk.Id,
            chunk.DocumentId,
            chunk.ChunkIndex,
            chunk.Content,
            Embedding = chunk.Embedding != null
                ? "[" + string.Join(",", chunk.Embedding) + "]"
                : null,
            chunk.TokenCount,
            chunk.CreatedAt
        });
    }

    public async Task CreateBatchAsync(IEnumerable<DocumentChunk> chunks)
    {
        foreach (var chunk in chunks)
        {
            await CreateAsync(chunk);
        }
    }

    public async Task<IEnumerable<DocumentChunk>> GetByDocumentIdAsync(Guid documentId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT id, document_id AS DocumentId, chunk_index AS ChunkIndex,
                   content, token_count AS TokenCount, created_at AS CreatedAt
            FROM document_chunks
            WHERE document_id = @DocumentId
            ORDER BY chunk_index
            """;
        return await connection.QueryAsync<DocumentChunk>(sql, new { DocumentId = documentId });
    }

    public async Task<IEnumerable<DocumentChunk>> SearchSimilarAsync(float[] embedding, int topK = 5)
    {
        using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
        const string sql = """
            SELECT id, document_id AS DocumentId, chunk_index AS ChunkIndex,
                   content, token_count AS TokenCount, created_at AS CreatedAt,
                   1 - (embedding <=> @Embedding::vector) AS SimilarityScore
            FROM document_chunks
            ORDER BY embedding <=> @Embedding::vector
            LIMIT @TopK
            """;

        var vectorString = "[" + string.Join(",", embedding) + "]";

        return await connection.QueryAsync<DocumentChunk>(sql, new
        {
            Embedding = vectorString,
            TopK = topK
        });
    }
}