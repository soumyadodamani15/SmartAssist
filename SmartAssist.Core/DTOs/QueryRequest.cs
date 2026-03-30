namespace SmartAssist.Core.DTOs;

public class QueryRequest
{
    public string Question { get; set; } = string.Empty;
    public int TopK { get; set; } = 5;
}