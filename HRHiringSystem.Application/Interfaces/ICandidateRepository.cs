using HRHiringSystem.Domain.Entities;

namespace HRHiringSystem.Application.Interfaces;

public interface ICandidateRepository : IGenericRepository<Candidate>
{
    Task<Candidate?> GetByEmailAsync(string email);
}
