using AutoMapper;
using HRHiringSystem.Application.Constants;
using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;
using HRHiringSystem.Domain.Entities;
using FluentValidation;
using System.Linq;

namespace HRHiringSystem.Application.Handlers;

public class JobHandler : IJobHandler
{
    private readonly IJobRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<JobRequest> _validator;

    public JobHandler(IJobRepository repository, IMapper mapper, IValidator<JobRequest> validator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ApiResponse<JobResponse>> CreateAsync(JobRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var msg = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return ApiResponse<JobResponse>.Failure(ResponseConstants.ErrorCodeValidation, msg);
        }

        var entity = _mapper.Map<Job>(request);
        var created = await _repository.AddAsync(entity, null);
        var dto = _mapper.Map<JobResponse>(created);
        return ApiResponse<JobResponse>.Success(dto, ResponseConstants.JobCreated);
    }

    public async Task<ApiResponse<JobResponse>> GetByIdAsync(Guid jobId)
    {
        var entity = await _repository.GetByIdAsync(jobId);
        if (entity == null) return ApiResponse<JobResponse>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.JobNotFound);
        var dto = _mapper.Map<JobResponse>(entity);
        return ApiResponse<JobResponse>.Success(dto, ResponseConstants.JobRetrieved);
    }

    public async Task<ApiResponse<IEnumerable<JobResponse>>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        var dto = _mapper.Map<IEnumerable<JobResponse>>(list);
        return ApiResponse<IEnumerable<JobResponse>>.Success(dto, ResponseConstants.JobsRetrieved);
    }

    public async Task<ApiResponse<JobResponse>> UpdateAsync(Guid jobId, JobRequest request)
    {
        var entity = await _repository.GetByIdAsync(jobId);
        if (entity == null) return ApiResponse<JobResponse>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.JobNotFound);
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var msg = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return ApiResponse<JobResponse>.Failure(ResponseConstants.ErrorCodeValidation, msg);
        }

        _mapper.Map(request, entity);
        await _repository.UpdateAsync(entity, null);

        var dto = _mapper.Map<JobResponse>(entity);
        return ApiResponse<JobResponse>.Success(dto, ResponseConstants.JobUpdated);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid jobId)
    {
        var entity = await _repository.GetByIdAsync(jobId);
        if (entity == null) return ApiResponse<bool>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.JobNotFound);

        await _repository.DeleteAsync(jobId, null);
        return ApiResponse<bool>.Success(true, ResponseConstants.JobDeleted);
    }
}
