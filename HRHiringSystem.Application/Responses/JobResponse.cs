using System;

namespace HRHiringSystem.Application.Responses;

public class JobResponse
{
    public Guid JobId { get; set; }
    public required string JobTitle { get; set; }
    public required string JobDescription { get; set; }
    public required string JobRequirements { get; set; }
    public bool JobIsActive { get; set; }
    public Guid JobCreatedByHrId { get; set; }
    public DateTime JobCreatedAt { get; set; }
    
    // Nested related objects
    public UserResponse? JobCreatedByHr { get; set; }
}
