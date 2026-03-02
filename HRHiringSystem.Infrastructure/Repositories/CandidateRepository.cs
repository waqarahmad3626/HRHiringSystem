using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRHiringSystem.Infrastructure.Repositories;

public class CandidateRepository : GenericRepository<Candidate>, ICandidateRepository
{
    public CandidateRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Candidate?> GetByEmailAsync(string email)
    {
        return await _context.Set<Candidate>()
            .Where(c => !EF.Property<bool>(c, "IsDeleted") && c.Email == email)
            .FirstOrDefaultAsync();
    }
}
