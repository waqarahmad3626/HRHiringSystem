using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRHiringSystem.Domain.Enums;

namespace HRHiringSystem.Domain.Entities;

public class JobApplication : AuditableEntity
{
    [Key]
    [Required(ErrorMessage = "Job Application Id is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Candidate Id is required.")]
    public Guid CandidateId { get; set; }

    [ForeignKey("CandidateId")]
    public Candidate? Candidate { get; set; }

    [Required(ErrorMessage = "Job Id is required.")]
    public Guid JobId { get; set; }

    [ForeignKey("JobId")]
    public Job? Job { get; set; }

    [Required(ErrorMessage = "CV Url is required.")]
    public required string CVUrl { get; set; }

    /// <summary>
    /// Current status of the application
    /// </summary>
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    /// <summary>
    /// AI-calculated score (0-100)
    /// Null until AI processing is complete
    /// </summary>
    [Range(0, 100)]
    public int? Score { get; set; }

    /// <summary>
    /// MongoDB document ID for the full evaluation report
    /// Stored in MongoDB collection for the job
    /// </summary>
    [MaxLength(50)]
    public string? MongoReportId { get; set; }

    /// <summary>
    /// Date when AI evaluation was completed
    /// </summary>
    public DateTime? EvaluatedAt { get; set; }

    /// <summary>
    /// Date when interview was scheduled (if applicable)
    /// </summary>
    public DateTime? InterviewScheduledAt { get; set; }

    /// <summary>
    /// HR notes/comments on the application
    /// </summary>
    [MaxLength(2000)]
    public string? HRNotes { get; set; }
}