using Microsoft.AspNetCore.Mvc;
using SmartAssist.Core.DTOs;
using SmartAssist.Infrastructure.Services;

namespace SmartAssist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentIngestionService _ingestionService;

    public DocumentsController(DocumentIngestionService ingestionService)
    {
        _ingestionService = ingestionService;
    }

    [HttpPost]
    public async Task<IActionResult> IngestDocument([FromBody] CreateDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Source))
            return BadRequest("Title and Source are required.");

        var job = await _ingestionService.IngestAsync(
            request.Title,
            request.Source,
            request.ContentType);

        return Ok(new
        {
            jobId = job.Id,
            status = job.Status,
            message = job.Status == "completed"
                ? "Document ingested successfully"
                : $"Ingestion failed: {job.ErrorMessage}"
        });
    }
}