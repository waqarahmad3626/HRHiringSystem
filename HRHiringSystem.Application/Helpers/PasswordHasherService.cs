using HRHiringSystem.Application.Interfaces;

namespace HRHiringSystem.Application.Helpers;

public class PasswordHasherService : IPasswordHasherService
{
    public string HashPassword(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    public bool VerifyPassword(string plainPassword, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
    }
}