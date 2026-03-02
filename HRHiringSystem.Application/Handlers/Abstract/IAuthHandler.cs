using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Handlers.Abstract;

public interface IAuthHandler
{
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<bool>> LogoutAsync(string token);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
