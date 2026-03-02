using Microsoft.AspNetCore.Mvc;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Handlers.Abstract;

namespace HRHiringSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobApplicationsController : ControllerBase
{
    private readonly IJobApplicationHandler _handler;

    public JobApplicationsController(IJobApplicationHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] JobApplicationRequest request)
    {
        var result = await _handler.CreateAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _handler.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _handler.GetAllAsync();
        return Ok(result);
    }

    
}
