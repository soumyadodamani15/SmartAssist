using SmartAssist.Core.Entities;
namespace SmartAssist.Core.Interfaces;

public interface IIngestionJobRepository
{
    Task<IngestionJob> CreateAsync(IngestionJob job);
    Task<IngestionJob?> GetByIdAsync(Guid id);
    Task UpdateStatusAsync(Guid id, string status, string? errorMessage = null);
}