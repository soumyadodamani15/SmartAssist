using Dapper;
using SmartAssist.Core.Entities;
using SmartAssist.Core.Interfaces;
using SmartAssist.Infrastructure.Data;

namespace SmartAssist.Infrastructure.Repositories;

public class IngestionJobRepository : IIngestionJobRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public IngestionJobRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IngestionJob> CreateAsync(IngestionJob job)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO ingestion_jobs (id, document_id, status, created_at, updated_at)
            VALUES (@Id, @DocumentId, @Status, @CreatedAt, @UpdatedAt)
            RETURNING id, document_id AS DocumentId, status,
                      error_msg AS ErrorMessage,
                      created_at AS CreatedAt, updated_at AS UpdatedAt
            """;

        job.Id = Guid.NewGuid();
        job.CreatedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;

        return await connection.QueryFirstAsync<IngestionJob>(sql, job);
    }

    public async Task<IngestionJob?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT id, document_id AS DocumentId, status,
                   error_msg AS ErrorMessage,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM ingestion_jobs
            WHERE id = @Id
            """;
        return await connection.QueryFirstOrDefaultAsync<IngestionJob>(sql, new { Id = id });
    }

    public async Task UpdateStatusAsync(Guid id, string status, string? errorMessage = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            UPDATE ingestion_jobs
            SET status = @Status,
                error_msg = @ErrorMessage,
                updated_at = @UpdatedAt
            WHERE id = @Id
            """;
        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status,
            ErrorMessage = errorMessage,
            UpdatedAt = DateTime.UtcNow
        });
    }
}