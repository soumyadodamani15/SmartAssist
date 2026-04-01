namespace SmartAssist.Core.Models;

public class IngestionMessage
{
    public Guid DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}