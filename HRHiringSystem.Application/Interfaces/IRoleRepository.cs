using HRHiringSystem.Domain.Entities;

namespace HRHiringSystem.Application.Interfaces;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
}
