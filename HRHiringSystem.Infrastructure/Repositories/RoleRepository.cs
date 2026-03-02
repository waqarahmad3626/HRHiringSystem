using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRHiringSystem.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Set<Role>()
            .Where(r => !EF.Property<bool>(r, "IsDeleted") && r.Name == name)
            .FirstOrDefaultAsync();
    }
}
