using HRHiringSystem.Domain.Models;

namespace HRHiringSystem.Application.Interfaces;

/// <summary>
/// Interface for MongoDB operations related to evaluation reports
/// </summary>
public interface IEvaluationReportRepository
{
    /// <summary>
    /// Save an evaluation report to MongoDB
    /// Creates database/collection if not exists
    /// </summary>
    /// <param name="report">The evaluation report to save</param>
    /// <returns>The MongoDB ObjectId of the saved document</returns>
    Task<string> SaveReportAsync(EvaluationReport report);

    /// <summary>
    /// Get an evaluation report by its MongoDB ObjectId
    /// </summary>
    /// <param name="jobId">The job ID (used to determine the database)</param>
    /// <param name="reportId">The MongoDB ObjectId</param>
    /// <returns>The evaluation report or null if not found</returns>
    Task<EvaluationReport?> GetReportByIdAsync(Guid jobId, string reportId);

    /// <summary>
    /// Get all evaluation reports for a specific job
    /// </summary>
    /// <param name="jobId">The job ID</param>
    /// <returns>List of evaluation reports</returns>
    Task<IEnumerable<EvaluationReport>> GetReportsByJobIdAsync(Guid jobId);

    /// <summary>
    /// Get evaluation report by application ID
    /// </summary>
    /// <param name="jobId">The job ID</param>
    /// <param name="applicationId">The application ID</param>
    /// <returns>The evaluation report or null if not found</returns>
    Task<EvaluationReport?> GetReportByApplicationIdAsync(Guid jobId, Guid applicationId);

    /// <summary>
    /// Delete an evaluation report
    /// </summary>
    /// <param name="jobId">The job ID</param>
    /// <param name="reportId">The MongoDB ObjectId</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteReportAsync(Guid jobId, string reportId);
}
