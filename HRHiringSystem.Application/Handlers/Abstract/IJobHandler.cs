using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Handlers.Abstract;

public interface IJobHandler
{
    Task<ApiResponse<JobResponse>> CreateAsync(JobRequest request);
    Task<ApiResponse<JobResponse>> GetByIdAsync(Guid userId);
    Task<ApiResponse<IEnumerable<JobResponse>>> GetAllAsync();
    Task<ApiResponse<JobResponse>> UpdateAsync(Guid userId, JobRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid userId);
}
