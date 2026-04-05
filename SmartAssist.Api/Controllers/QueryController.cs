using Microsoft.AspNetCore.Mvc;
using SmartAssist.Core.DTOs;
using SmartAssist.Infrastructure.Services;

namespace SmartAssist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly RagQueryService _ragQueryService;

    public QueryController(RagQueryService ragQueryService)
    {
        _ragQueryService = ragQueryService;
    }

    [HttpPost]
    public async Task<IActionResult> Query([FromBody] QueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question is required.");

        var response = await _ragQueryService.QueryAsync(request);
        return Ok(response);
    }

}
