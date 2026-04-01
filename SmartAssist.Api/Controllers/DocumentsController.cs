using Microsoft.AspNetCore.Mvc;
using SmartAssist.Core.DTOs;
using SmartAssist.Core.Models;
using SmartAssist.Infrastructure.Services;

namespace SmartAssist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IngestionMessageProducer _producer;

    public DocumentsController(IngestionMessageProducer producer)
    {
        _producer = producer;
    }

    [HttpPost]
    public async Task<IActionResult> IngestDocument([FromBody] CreateDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Source))
            return BadRequest("Title and Source are required.");

        var message = new IngestionMessage
        {
            DocumentId = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Source,
            ContentType = request.ContentType
        };

        await _producer.PublishAsync(message);

        return Accepted(new
        {
            message = "Document queued for ingestion",
            documentTitle = request.Title
        });
    }
}