namespace SmartAssist.Core.DTOs;

public class QueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<SourceReference> Sources { get; set; } = new();
}

public class SourceReference
{
    public string DocumentTitle { get; set; } = string.Empty;
    public string ChunkContent { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
}