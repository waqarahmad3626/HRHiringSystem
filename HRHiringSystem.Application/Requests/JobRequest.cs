using System;

namespace HRHiringSystem.Application.Requests;

public class JobRequest
{
    public required string JobTitle { get; set; }
    public required string JobDescription { get; set; }
    public required string JobRequirements { get; set; }
    public Guid JobCreatedByHrId { get; set; }
    public bool JobIsActive { get; set; } = true;
}
