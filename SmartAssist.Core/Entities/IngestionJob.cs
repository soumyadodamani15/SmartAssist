namespace SmartAssist.Core.Entities;

public class IngestionJob
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Status { get; set; } = "pending";
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}