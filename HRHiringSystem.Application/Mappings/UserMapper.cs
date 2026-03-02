using AutoMapper;
using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Mappings;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<User, UserResponse>();

        CreateMap<UserRequest, User>();
    }
}
