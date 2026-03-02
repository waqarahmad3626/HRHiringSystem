using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;
using HRHiringSystem.Application.Constants;

namespace HRHiringSystem.Application.Handlers;

public class AuthHandler : IAuthHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasherService _passwordHasherService;

    public AuthHandler(IUserRepository userRepository, ITokenService tokenService, IPasswordHasherService passwordHasherService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasherService = passwordHasherService;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) return ApiResponse<AuthResponse>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.UserNotFound);

        if (!_passwordHasherService.VerifyPassword(request.Password, user.PasswordHash))
            return ApiResponse<AuthResponse>.Failure(ResponseConstants.ErrorCodeValidation, "Invalid credentials");

        var roleName = user.Role?.Name;
        var token = await _tokenService.GenerateTokenAsync(user.Id, user.Email, roleName);
        await _tokenService.StoreTokenAsync(token, TimeSpan.FromMinutes(60), user.Id);

        var response = new AuthResponse
        {
            UserId = user.Id,
            UserName = user.Name,
            Token = token,
            ExpiresInSeconds = 3600
        };

        return ApiResponse<AuthResponse>.Success(response, "Login successful");
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string token)
    {
        await _tokenService.RevokeTokenAsync(token);
        return ApiResponse<bool>.Success(true, "Logged out");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return ApiResponse<bool>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.UserNotFound);

        if (!_passwordHasherService.VerifyPassword(request.OldPassword, user.PasswordHash))
            return ApiResponse<bool>.Failure(ResponseConstants.ErrorCodeValidation, "Old password is incorrect");

        user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user, null);
        return ApiResponse<bool>.Success(true, "Password changed successfully");
    }
}
