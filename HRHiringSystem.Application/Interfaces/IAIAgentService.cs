using HRHiringSystem.Domain.Models;

namespace HRHiringSystem.Application.Interfaces;

/// <summary>
/// Interface for communicating with the Python FastAPI AI Agent
/// </summary>
public interface IAIAgentService
{
    /// <summary>
    /// Parse CV to extract candidate information (first name, last name, email, phone)
    /// </summary>
    /// <param name="cvStream">The CV file stream</param>
    /// <param name="fileName">The original file name</param>
    /// <returns>Extracted candidate data</returns>
    Task<CvParseResult> ParseCvAsync(Stream cvStream, string fileName);

    /// <summary>
    /// Trigger full AI evaluation of a job application
    /// This is called by Azure Function for background processing
    /// </summary>
    /// <param name="request">Evaluation request with candidate, job, and CV details</param>
    /// <returns>Full evaluation report</returns>
    Task<EvaluationReport> EvaluateApplicationAsync(EvaluationRequest request);
}

/// <summary>
/// Result of CV parsing
/// </summary>
public class CvParseResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

/// <summary>
/// Request for full AI evaluation
/// </summary>
public class EvaluationRequest
{
    public Guid CandidateId { get; set; }
    public Guid JobId { get; set; }
    public Guid ApplicationId { get; set; }
    public required string CvUrl { get; set; }
    
    // Job details for evaluation
    public required string JobTitle { get; set; }
    public required string JobDescription { get; set; }
    public required string JobRequirements { get; set; }
    public required string JobRequiredSkills { get; set; }
    public int JobExperienceYears { get; set; }
    public string? JobEducation { get; set; }
}
