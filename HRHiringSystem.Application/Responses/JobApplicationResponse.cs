using System;
using HRHiringSystem.Domain.Enums;

namespace HRHiringSystem.Application.Responses;

public class JobApplicationResponse
{
    public Guid JobApplicationId { get; set; }
    public Guid JobApplicationCandidateId { get; set; }
    public Guid JobApplicationJobId { get; set; }
    public required string JobApplicationCvUrl { get; set; }
    public DateTime JobApplicationAppliedAt { get; set; }
    
    /// <summary>
    /// Current status of the application
    /// </summary>
    public ApplicationStatus JobApplicationStatus { get; set; }
    
    /// <summary>
    /// Status display name
    /// </summary>
    public string JobApplicationStatusName => JobApplicationStatus.ToString();
    
    /// <summary>
    /// AI-calculated score (0-100), null if not yet evaluated
    /// </summary>
    public int? JobApplicationScore { get; set; }
    
    /// <summary>
    /// MongoDB report ID for full evaluation details
    /// </summary>
    public string? JobApplicationMongoReportId { get; set; }
    
    /// <summary>
    /// Date when AI evaluation was completed
    /// </summary>
    public DateTime? JobApplicationEvaluatedAt { get; set; }
    
    /// <summary>
    /// Date when interview was scheduled
    /// </summary>
    public DateTime? JobApplicationInterviewScheduledAt { get; set; }
    
    /// <summary>
    /// HR notes/comments
    /// </summary>
    public string? JobApplicationHRNotes { get; set; }
    
    // Nested related objects
    public CandidateResponse? JobApplicationCandidate { get; set; }
    public JobResponse? JobApplicationJob { get; set; }
}
