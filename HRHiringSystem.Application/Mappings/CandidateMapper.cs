using AutoMapper;
using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Mappings;

public class CandidateMapper : Profile
{
    public CandidateMapper()
    {
        // Use naming convention rules configured in AddApplicationMappings
        CreateMap<Candidate, CandidateResponse>();
    }
}
