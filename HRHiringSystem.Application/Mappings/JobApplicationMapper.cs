using AutoMapper;
using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Mappings;

public class JobApplicationMapper : Profile
{
    public JobApplicationMapper()
    {
        CreateMap<JobApplication, JobApplicationResponse>()
            .ForMember(dest => dest.JobApplicationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.JobApplicationCandidateId, opt => opt.MapFrom(src => src.CandidateId))
            .ForMember(dest => dest.JobApplicationJobId, opt => opt.MapFrom(src => src.JobId))
            .ForMember(dest => dest.JobApplicationCvUrl, opt => opt.MapFrom(src => src.CVUrl))
            .ForMember(dest => dest.JobApplicationAppliedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.JobApplicationStatus, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.JobApplicationScore, opt => opt.MapFrom(src => src.Score))
            .ForMember(dest => dest.JobApplicationMongoReportId, opt => opt.MapFrom(src => src.MongoReportId))
            .ForMember(dest => dest.JobApplicationEvaluatedAt, opt => opt.MapFrom(src => src.EvaluatedAt))
            .ForMember(dest => dest.JobApplicationInterviewScheduledAt, opt => opt.MapFrom(src => src.InterviewScheduledAt))
            .ForMember(dest => dest.JobApplicationHRNotes, opt => opt.MapFrom(src => src.HRNotes))
            .ForMember(dest => dest.JobApplicationCandidate, opt => opt.MapFrom(src => src.Candidate))
            .ForMember(dest => dest.JobApplicationJob, opt => opt.MapFrom(src => src.Job));

        CreateMap<JobApplicationRequest, JobApplication>()
            .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId));
    }
}
