using HRHiringSystem.Domain.Entities;

namespace HRHiringSystem.Application.Interfaces;

public interface IJobApplicationRepository : IGenericRepository<JobApplication>
{
    Task<bool> ExistsAsync(Guid candidateId, Guid jobId);
    Task<JobApplication?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<JobApplication>> GetAllWithDetailsAsync();
}
