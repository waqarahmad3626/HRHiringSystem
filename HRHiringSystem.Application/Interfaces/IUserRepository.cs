
using HRHiringSystem.Domain.Entities;

namespace HRHiringSystem.Application.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
