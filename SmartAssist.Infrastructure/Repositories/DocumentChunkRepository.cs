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
        var connectionString = ((DbConnectionFactory)_connectionFactory).ConnectionString;
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var vectorString = "[" + string.Join(",", embedding) + "]";

        var sql = $"""
            SELECT dc.id, dc.document_id AS DocumentId, dc.chunk_index AS ChunkIndex,
                dc.content, dc.token_count AS TokenCount, dc.created_at AS CreatedAt,
                d.title AS DocumentTitle,
                1 - (dc.embedding <=> '{vectorString}'::vector) AS SimilarityScore
            FROM document_chunks dc
            JOIN documents d ON d.id = dc.document_id
            WHERE dc.embedding IS NOT NULL
            ORDER BY dc.embedding <=> '{vectorString}'::vector
            LIMIT {topK};
            """;

        using var cmd = new NpgsqlCommand(sql, connection);

        var chunks = new List<DocumentChunk>();
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            chunks.Add(new DocumentChunk
            {
                Id = reader.GetGuid(0),
                DocumentId = reader.GetGuid(1),
                ChunkIndex = reader.GetInt32(2),
                Content = reader.GetString(3),
                TokenCount = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                CreatedAt = reader.GetDateTime(5),
                DocumentTitle = reader.IsDBNull(6) ? "" : reader.GetString(6),
                SimilarityScore = reader.GetDouble(7)
            });
        }

        return chunks;
    }
}