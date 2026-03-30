namespace SmartAssist.Infrastructure.Services;

public class DocumentChunker
{
    private readonly int _chunkSize;
    private readonly int _overlap;

    public DocumentChunker(int chunkSize = 500, int overlap = 50)
    {
        _chunkSize = chunkSize;
        _overlap = overlap;
    }

    public List<string> Chunk(string text)
    {
        var chunks = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        int start = 0;

        while (start < text.Length)
        {
            int end = Math.Min(start + _chunkSize, text.Length);
            string chunk = text[start..end].Trim();

            if (!string.IsNullOrWhiteSpace(chunk))
                chunks.Add(chunk);

            if (end == text.Length)
                break;

            start += _chunkSize - _overlap;
        }

        return chunks;
    }
}
