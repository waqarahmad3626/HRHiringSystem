using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Infrastructure.Data;

namespace HRHiringSystem.Infrastructure.Repositories;

public class JobRepository : GenericRepository<Job>, IJobRepository
{
    public JobRepository(AppDbContext context) : base(context)
    {
    }
}
