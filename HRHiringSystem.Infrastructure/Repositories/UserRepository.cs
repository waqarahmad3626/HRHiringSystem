using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRHiringSystem.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public new async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Set<User>()
            .Include(u => u.Role)
            .Where(u => !EF.Property<bool>(u, "IsDeleted"))
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public new async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Set<User>()
            .Include(u => u.Role)
            .Where(u => !EF.Property<bool>(u, "IsDeleted"))
            .ToListAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Set<User>()
            .Include(u => u.Role)
            .Where(u => !EF.Property<bool>(u, "IsDeleted") && u.Email == email)
            .FirstOrDefaultAsync();
    }
}
