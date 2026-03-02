using AutoMapper;
using FluentValidation;
using HRHiringSystem.Application.Constants;
using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;
using HRHiringSystem.Domain.Entities;

namespace HRHiringSystem.Application.Handlers;

public class UserHandler : IUserHandler
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<UserRequest> _validator;
    private readonly IPasswordHasherService _passwordHasherService;

    public UserHandler(IUserRepository repository, IMapper mapper, IValidator<UserRequest> validator, IPasswordHasherService passwordHasherService)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
        _passwordHasherService = passwordHasherService;
    }

    public async Task<ApiResponse<UserResponse>> CreateAsync(UserRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var msg = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return ApiResponse<UserResponse>.Failure(ResponseConstants.ErrorCodeValidation, msg);
        }
        // Ensure email is unique
        var existing = await _repository.GetByEmailAsync(request.UserEmail);
        if (existing != null)
        {
            return ApiResponse<UserResponse>.Failure(ResponseConstants.ErrorCodeValidation, ResponseConstants.EmailAlreadyExists);
        }

        var entity = _mapper.Map<User>(request);
    entity.PasswordHash = _passwordHasherService.HashPassword(request.UserPassword);
        var created = await _repository.AddAsync(entity, null);
        var dto = _mapper.Map<UserResponse>(created);
        return ApiResponse<UserResponse>.Success(dto, ResponseConstants.UserCreated);
    }

    public async Task<ApiResponse<UserResponse>> GetByIdAsync(Guid userId)
    {
        var entity = await _repository.GetByIdAsync(userId);
        if (entity == null) return ApiResponse<UserResponse>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.UserNotFound);
        var dto = _mapper.Map<UserResponse>(entity);
        return ApiResponse<UserResponse>.Success(dto, ResponseConstants.UserRetrieved);
    }

    public async Task<ApiResponse<IEnumerable<UserResponse>>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        var dto = _mapper.Map<IEnumerable<UserResponse>>(list);
        return ApiResponse<IEnumerable<UserResponse>>.Success(dto, ResponseConstants.UsersRetrieved);
    }

    public async Task<ApiResponse<UserResponse>> UpdateAsync(Guid userId, UserRequest request)
    {
        var entity = await _repository.GetByIdAsync(userId);
        if (entity == null) return ApiResponse<UserResponse>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.UserNotFound);
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var msg = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return ApiResponse<UserResponse>.Failure(ResponseConstants.ErrorCodeValidation, msg);
        }
        // If email changed, ensure uniqueness
        if (!string.Equals(entity.Email, request.UserEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _repository.GetByEmailAsync(request.UserEmail);
            if (existing != null && existing.Id != userId)
            {
                return ApiResponse<UserResponse>.Failure(ResponseConstants.ErrorCodeValidation, ResponseConstants.EmailAlreadyExists);
            }
        }

        _mapper.Map(request, entity);
    entity.PasswordHash = _passwordHasherService.HashPassword(request.UserPassword);
        await _repository.UpdateAsync(entity, null);

        var dto = _mapper.Map<UserResponse>(entity);
        return ApiResponse<UserResponse>.Success(dto, ResponseConstants.UserUpdated);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid userId)
    {
        var entity = await _repository.GetByIdAsync(userId);
        if (entity == null) return ApiResponse<bool>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.UserNotFound);

        await _repository.DeleteAsync(userId, null);
        return ApiResponse<bool>.Success(true, ResponseConstants.UserDeleted);
    }
}
