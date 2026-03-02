using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Handlers.Abstract;

public interface IJobApplicationHandler
{
    Task<ApiResponse<JobApplicationResponse>> CreateAsync(JobApplicationRequest request);
    Task<ApiResponse<JobApplicationResponse>> GetByIdAsync(Guid jobApplicationId);
    Task<ApiResponse<IEnumerable<JobApplicationResponse>>> GetAllAsync();
    Task<ApiResponse<JobApplicationResponse>> UpdateAsync(Guid jobApplicationId, JobApplicationRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid jobApplicationId);
}
