using AutoMapper;
using FluentValidation;
using HRHiringSystem.Application.Constants;
using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;
using HRHiringSystem.Domain.Entities;

namespace HRHiringSystem.Application.Handlers;

public class RoleHandler : IRoleHandler
{
    private readonly IRoleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<RoleRequest> _validator;

    public RoleHandler(IRoleRepository repository, IMapper mapper, IValidator<RoleRequest> validator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ApiResponse<RoleResponse>> CreateAsync(RoleRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var msg = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return ApiResponse<RoleResponse>.Failure(ResponseConstants.ErrorCodeValidation, msg);
        }
        // Ensure role name is unique
        var existing = await _repository.GetByNameAsync(request.RoleName);
        if (existing != null)
        {
            return ApiResponse<RoleResponse>.Failure(ResponseConstants.ErrorCodeValidation, ResponseConstants.RoleNameAlreadyExists);
        }

        var entity = _mapper.Map<Role>(request);
        var created = await _repository.AddAsync(entity, null);
        var dto = _mapper.Map<RoleResponse>(created);
        return ApiResponse<RoleResponse>.Success(dto, ResponseConstants.RoleCreated);
    }

    public async Task<ApiResponse<RoleResponse>> GetByIdAsync(Guid roleId)
    {
        var entity = await _repository.GetByIdAsync(roleId);
        if (entity == null) return ApiResponse<RoleResponse>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.RoleNotFound);
        var dto = _mapper.Map<RoleResponse>(entity);
        return ApiResponse<RoleResponse>.Success(dto, ResponseConstants.RoleRetrieved);
    }

    public async Task<ApiResponse<IEnumerable<RoleResponse>>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        var dto = _mapper.Map<IEnumerable<RoleResponse>>(list);
        return ApiResponse<IEnumerable<RoleResponse>>.Success(dto, ResponseConstants.RolesRetrieved);
    }

    public async Task<ApiResponse<RoleResponse>> UpdateAsync(Guid roleId, RoleRequest request)
    {
        var entity = await _repository.GetByIdAsync(roleId);
        if (entity == null) return ApiResponse<RoleResponse>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.RoleNotFound);
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var msg = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return ApiResponse<RoleResponse>.Failure(ResponseConstants.ErrorCodeValidation, msg);
        }
        // If name changed, ensure uniqueness
        if (!string.Equals(entity.Name, request.RoleName, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _repository.GetByNameAsync(request.RoleName);
            if (existing != null && existing.Id != roleId)
            {
                return ApiResponse<RoleResponse>.Failure(ResponseConstants.ErrorCodeValidation, ResponseConstants.RoleNameAlreadyExists);
            }
        }

        _mapper.Map(request, entity);
        await _repository.UpdateAsync(entity, null);

        var dto = _mapper.Map<RoleResponse>(entity);
        return ApiResponse<RoleResponse>.Success(dto, ResponseConstants.RoleUpdated);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid roleId)
    {
        var entity = await _repository.GetByIdAsync(roleId);
        if (entity == null) return ApiResponse<bool>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.RoleNotFound);

        await _repository.DeleteAsync(roleId, null);
        return ApiResponse<bool>.Success(true, ResponseConstants.RoleDeleted);
    }
}
