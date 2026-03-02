using AutoMapper;
using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Mappings;

public class RoleMapper : Profile
{
    public RoleMapper()
    {
        // Use naming convention rules configured in AddApplicationMappings
        CreateMap<Role, RoleResponse>();
        CreateMap<RoleRequest, Role>();
    }
}
