using System;

namespace HRHiringSystem.Application.Responses;

public class RoleResponse
{
    public Guid RoleId { get; set; }
    public required string RoleName { get; set; }
    public required string RoleDescription { get; set; }
}
