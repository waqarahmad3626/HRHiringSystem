using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobHandler _handler;

    public JobsController(IJobHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JobRequest request)
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

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _handler.GetAllAsync();
        if (!result.IsSuccess || result.Data == null)
        {
            return Ok(result);
        }

        var activeJobs = result.Data.Where(job => job.JobIsActive);
        var activeResult = ApiResponse<IEnumerable<JobResponse>>.Success(activeJobs, "Active jobs retrieved");
        return Ok(activeResult);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] JobRequest request)
    {
        var result = await _handler.UpdateAsync(id, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _handler.DeleteAsync(id);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}