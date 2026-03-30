namespace SmartAssist.Core.DTOs;

public class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}