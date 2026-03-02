using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Requests;

namespace HRHiringSystem.API.Controllers;

/// <summary>
/// Controller for managing AI evaluation reports
/// Reports are stored in MongoDB, one database per job
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IEvaluationReportHandler _handler;

    public ReportsController(IEvaluationReportHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Save an evaluation report (called by Python AI Agent)
    /// This endpoint updates both MongoDB (full report) and SQL Server (application status)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SaveReport([FromBody] SaveEvaluationReportRequest request)
    {
        var result = await _handler.SaveReportAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Get evaluation report by MongoDB ObjectId
    /// </summary>
    [HttpGet("job/{jobId}/report/{reportId}")]
    public async Task<IActionResult> GetReportById(Guid jobId, string reportId)
    {
        var result = await _handler.GetReportByIdAsync(jobId, reportId);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Get evaluation report by application ID
    /// </summary>
    [HttpGet("job/{jobId}/application/{applicationId}")]
    public async Task<IActionResult> GetReportByApplicationId(Guid jobId, Guid applicationId)
    {
        var result = await _handler.GetReportByApplicationIdAsync(jobId, applicationId);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Get all evaluation reports for a specific job
    /// </summary>
    [HttpGet("job/{jobId}")]
    public async Task<IActionResult> GetReportsByJobId(Guid jobId)
    {
        var result = await _handler.GetReportsByJobIdAsync(jobId);
        return Ok(result);
    }
}
