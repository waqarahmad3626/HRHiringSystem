using System;

namespace HRHiringSystem.Application.Requests;

public class RoleRequest
{
    public required string RoleName { get; set; }
    public required string RoleDescription { get; set; }
}
