using System;

namespace HRHiringSystem.Application.Responses;

public class UserResponse
{
    public Guid UserId { get; set; }
    public required string UserName { get; set; }
    public required string UserEmail { get; set; }
    public Guid UserRoleId { get; set; }
    public bool UserIsActive { get; set; }
    // Nested related objects
    public RoleResponse? UserRole { get; set; }
}
