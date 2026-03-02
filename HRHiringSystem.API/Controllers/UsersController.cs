using Microsoft.AspNetCore.Mvc;
using HRHiringSystem.Application.Requests;
using System;
using System.Threading.Tasks;
using HRHiringSystem.Application.Handlers.Abstract;

namespace HRHiringSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserHandler _handler;

    public UsersController(IUserHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserRequest request)
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserRequest request)
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
