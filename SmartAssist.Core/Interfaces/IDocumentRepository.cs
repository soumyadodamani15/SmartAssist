using SmartAssist.Core.Entities;
namespace SmartAssist.Core.Interfaces;

public interface IDocumentRepository
{
    Task<Document> GetByIdAsync(Guid id);
    Task<IEnumerable<Document>> GetAllAsync();
    Task<Document> CreateAsync(Document document);
    Task DeleteAsync(Guid id);
}