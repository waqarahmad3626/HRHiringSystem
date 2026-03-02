
namespace HRHiringSystem.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(System.Guid userId, string? email, string? role);
    Task StoreTokenAsync(string token, System.TimeSpan expiresIn, System.Guid userId);
    Task<bool> IsTokenValidAsync(string token);
    Task RevokeTokenAsync(string token);
}
