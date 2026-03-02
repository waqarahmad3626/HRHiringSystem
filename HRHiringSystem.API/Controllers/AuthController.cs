using Microsoft.AspNetCore.Mvc;
using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;
using Microsoft.AspNetCore.Authorization;

namespace HRHiringSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthHandler _handler;

    public AuthController(IAuthHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _handler.LoginAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var auth = Request.Headers["Authorization"].FirstOrDefault();
        var token = auth?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token)) return BadRequest(ApiResponse<bool>.Failure("Invalid", "No token provided"));
        var result = await _handler.LogoutAsync(token);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest(ApiResponse<bool>.Failure("Invalid", "Invalid user"));

        var result = await _handler.ChangePasswordAsync(userId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
