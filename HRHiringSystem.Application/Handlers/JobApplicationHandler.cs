using AutoMapper;
using HRHiringSystem.Application.Constants;
using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;
using HRHiringSystem.Domain.Entities;
using FluentValidation;

namespace HRHiringSystem.Application.Handlers;

public class JobApplicationHandler : IJobApplicationHandler
{
    private readonly IJobApplicationRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<JobApplicationRequest> _validator;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IBlobStorageService _blobStorage;
    private readonly IAzureFunctionService _azureFunctionService;

    public JobApplicationHandler(IJobApplicationRepository repository,
        IMapper mapper,
        IValidator<JobApplicationRequest> validator,
        ICandidateRepository candidateRepository,
        IBlobStorageService blobStorage,
        IAzureFunctionService azureFunctionService)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
        _candidateRepository = candidateRepository;
        _blobStorage = blobStorage;
        _azureFunctionService = azureFunctionService;
    }

    public async Task<ApiResponse<JobApplicationResponse>> CreateAsync(JobApplicationRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var msg = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return ApiResponse<JobApplicationResponse>.Failure(ResponseConstants.ErrorCodeValidation, msg);
        }

        // Find or create candidate by email
        var candidate = await _candidateRepository.GetByEmailAsync(request.CandidateEmail);
        if (candidate == null)
        {
            candidate = new Candidate
            {
                FirstName = request.CandidateFirstName,
                LastName = request.CandidateLastName,
                Email = request.CandidateEmail,
                Phone = request.CandidatePhone
            };

            candidate = await _candidateRepository.AddAsync(candidate, null);
        }

        // Check duplicate application
        var exists = await _repository.ExistsAsync(candidate.Id, request.JobId);
        if (exists)
        {
            return ApiResponse<JobApplicationResponse>.Failure(ResponseConstants.ErrorCodeValidation, ResponseConstants.AlreadyApplied);
        }

        // Upload CV to blob storage: blob path = "{jobId}/{candidateId}_CV{ext}"
        var cvFile = request.CvFile!;
        var ext = Path.GetExtension(cvFile.FileName);
        var blobPath = $"{request.JobId}/{candidate.Id}_CV{ext}";
        var contentType = cvFile.ContentType ?? "application/octet-stream";
        
        using var stream = cvFile.OpenReadStream();
        var url = await _blobStorage.UploadFileAsync(blobPath, stream, contentType);

        // Create job application
        var jobApplication = new JobApplication
        {
            CandidateId = candidate.Id,
            JobId = request.JobId,
            CVUrl = url
        };

        var created = await _repository.AddAsync(jobApplication, null);
        
        // Trigger Azure Function to process application in background
        await _azureFunctionService.TriggerApplicationProcessingAsync(
            candidate.Id, 
            request.JobId, 
            created.Id, 
            url);
        
        var dto = _mapper.Map<JobApplicationResponse>(created);
        return ApiResponse<JobApplicationResponse>.Success(dto, ResponseConstants.JobApplicationCreated);
    }

    public async Task<ApiResponse<JobApplicationResponse>> GetByIdAsync(Guid jobApplicationId)
    {
        var entity = await _repository.GetByIdWithDetailsAsync(jobApplicationId);
        if (entity == null) return ApiResponse<JobApplicationResponse>.Failure(ResponseConstants.ErrorCodeNotFound, ResponseConstants.JobApplicationNotFound);
        var dto = _mapper.Map<JobApplicationResponse>(entity);
        return ApiResponse<JobApplicationResponse>.Success(dto, ResponseConstants.JobApplicationRetrieved);
    }

    public async Task<ApiResponse<IEnumerable<JobApplicationResponse>>> GetAllAsync()
    {
        var list = await _repository.GetAllWithDetailsAsync();
        var dto = _mapper.Map<IEnumerable<JobApplicationResponse>>(list);
        return ApiResponse<IEnumerable<JobApplicationResponse>>.Success(dto, ResponseConstants.JobApplicationsRetrieved);
    }

    public Task<ApiResponse<JobApplicationResponse>> UpdateAsync(Guid jobApplicationId, JobApplicationRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> DeleteAsync(Guid jobApplicationId)
    {
        throw new NotImplementedException();
    }
}
