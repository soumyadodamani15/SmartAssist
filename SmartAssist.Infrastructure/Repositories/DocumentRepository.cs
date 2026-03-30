using Dapper;
using SmartAssist.Core.Entities;
using SmartAssist.Core.Interfaces;
using SmartAssist.Infrastructure.Data;

namespace SmartAssist.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DocumentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Document> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT id, title, source, content_type AS ContentType,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM documents
            WHERE id = @Id
            """;
        var result = await connection.QueryFirstOrDefaultAsync<Document>(sql, new { Id = id });
        return result ?? throw new KeyNotFoundException($"Document {id} not found");
    }

    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT id, title, source, content_type AS ContentType,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM documents
            ORDER BY created_at DESC
            """;
        return await connection.QueryAsync<Document>(sql);
    }

    public async Task<Document> CreateAsync(Document document)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO documents (id, title, source, content_type, created_at, updated_at)
            VALUES (@Id, @Title, @Source, @ContentType, @CreatedAt, @UpdatedAt)
            RETURNING id, title, source, content_type AS ContentType,
                      created_at AS CreatedAt, updated_at AS UpdatedAt
            """;
        document.Id = Guid.NewGuid();
        document.CreatedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        return await connection.QueryFirstAsync<Document>(sql, document);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM documents WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}