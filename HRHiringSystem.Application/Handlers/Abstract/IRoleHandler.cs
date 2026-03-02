using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Handlers.Abstract;

public interface IRoleHandler
{
    Task<ApiResponse<RoleResponse>> CreateAsync(RoleRequest request);
    Task<ApiResponse<RoleResponse>> GetByIdAsync(Guid roleId);
    Task<ApiResponse<IEnumerable<RoleResponse>>> GetAllAsync();
    Task<ApiResponse<RoleResponse>> UpdateAsync(Guid roleId, RoleRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid roleId);
}

