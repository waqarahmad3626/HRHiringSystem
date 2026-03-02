using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Handlers.Abstract;

public interface IUserHandler
{
    Task<ApiResponse<UserResponse>> CreateAsync(UserRequest request);
    Task<ApiResponse<UserResponse>> GetByIdAsync(Guid userId);
    Task<ApiResponse<IEnumerable<UserResponse>>> GetAllAsync();
    Task<ApiResponse<UserResponse>> UpdateAsync(Guid userId, UserRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid userId);
}
