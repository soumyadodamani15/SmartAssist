namespace SmartAssist.Core.Entities;

public class Document
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation — one document has many chunks
    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}