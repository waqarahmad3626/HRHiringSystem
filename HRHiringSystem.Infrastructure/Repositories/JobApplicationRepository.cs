using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRHiringSystem.Infrastructure.Repositories;

public class JobApplicationRepository : GenericRepository<JobApplication>, IJobApplicationRepository
{
    public JobApplicationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsAsync(Guid candidateId, Guid jobId)
    {
        return await _context.Set<JobApplication>()
            .Where(ja => !EF.Property<bool>(ja, "IsDeleted") && ja.CandidateId == candidateId && ja.JobId == jobId)
            .AnyAsync();
    }

    public async Task<JobApplication?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Set<JobApplication>()
            .Include(ja => ja.Candidate)
            .Include(ja => ja.Job)
            .Where(ja => !EF.Property<bool>(ja, "IsDeleted"))
            .FirstOrDefaultAsync(ja => ja.Id == id);
    }

    public async Task<IEnumerable<JobApplication>> GetAllWithDetailsAsync()
    {
        return await _context.Set<JobApplication>()
            .Include(ja => ja.Candidate)
            .Include(ja => ja.Job)
            .Where(ja => !EF.Property<bool>(ja, "IsDeleted"))
            .ToListAsync();
    }
}
