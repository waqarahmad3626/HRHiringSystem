namespace HRHiringSystem.Application.Interfaces;

public interface IPasswordHasherService
{
    string HashPassword(string plainPassword);
    bool VerifyPassword(string plainPassword, string passwordHash);
}