using AutoMapper;
using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Mappings;

public class JobMapper : Profile
{
    public JobMapper()
    {
        CreateMap<Job, JobResponse>()
            .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.JobDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.JobRequirements, opt => opt.MapFrom(src => src.Requirements))
            .ForMember(dest => dest.JobIsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.JobCreatedByHrId, opt => opt.MapFrom(src => src.CreatedByHrId))
            .ForMember(dest => dest.JobCreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.JobCreatedByHr, opt => opt.MapFrom(src => src.CreatedByHr));

        CreateMap<JobRequest, Job>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.JobTitle))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.JobDescription))
            .ForMember(dest => dest.Requirements, opt => opt.MapFrom(src => src.JobRequirements))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.JobIsActive))
            .ForMember(dest => dest.CreatedByHrId, opt => opt.MapFrom(src => src.JobCreatedByHrId));
    }
}
