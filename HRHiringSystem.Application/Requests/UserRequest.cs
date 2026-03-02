using System;

namespace HRHiringSystem.Application.Requests;

public class UserRequest
{
    public required string UserName { get; set; }
    public required string UserEmail { get; set; }
    public required string UserPassword { get; set; }
    public Guid UserRoleId { get; set; }
}
