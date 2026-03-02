using Microsoft.AspNetCore.Mvc;
using HRHiringSystem.Application.Handlers.Abstract;

namespace HRHiringSystem.API.Controllers;

/// <summary>
/// Controller for CV operations
/// Handles CV parsing (proxied to Python AI Agent)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CvController : ControllerBase
{
    private readonly ICvHandler _handler;

    public CvController(ICvHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Parse a CV file to extract candidate information
    /// This endpoint proxies the request to Python AI Agent
    /// Returns: FirstName, LastName, Email, Phone
    /// </summary>
    /// <param name="file">CV file (PDF, DOC, DOCX)</param>
    [HttpPost("parse")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
    public async Task<IActionResult> ParseCv(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        // Validate file type
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { error = "Invalid file type. Allowed: PDF, DOC, DOCX" });
        }

        using var stream = file.OpenReadStream();
        var result = await _handler.ParseCvAsync(stream, file.FileName);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
