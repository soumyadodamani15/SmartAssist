namespace SmartAssist.Core.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public float[]? Embedding { get; set; }
    public int TokenCount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation — back to parent document
    public Document? Document { get; set; }
}