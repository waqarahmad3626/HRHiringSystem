namespace HRHiringSystem.Domain.Enums;

/// <summary>
/// Represents the status of a job application throughout the hiring process
/// </summary>
public enum ApplicationStatus
{
    /// <summary>
    /// Application submitted, waiting to be processed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// AI is currently evaluating the application
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Score >= 80: Automatically accepted, ready for interview scheduling
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// Score 65-80: Requires HR manual review and approval
    /// </summary>
    HRReview = 3,

    /// <summary>
    /// Score < 65: Automatically rejected
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Interview has been scheduled
    /// </summary>
    InterviewScheduled = 5,

    /// <summary>
    /// Candidate hired
    /// </summary>
    Hired = 6
}
