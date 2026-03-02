using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Handlers.Abstract;

/// <summary>
/// Handler interface for evaluation report operations
/// </summary>
public interface IEvaluationReportHandler
{
    /// <summary>
    /// Save an evaluation report and update the job application status
    /// </summary>
    Task<ApiResponse<EvaluationReportResponse>> SaveReportAsync(Requests.SaveEvaluationReportRequest request);

    /// <summary>
    /// Get evaluation report by MongoDB ObjectId
    /// </summary>
    Task<ApiResponse<EvaluationReportResponse>> GetReportByIdAsync(Guid jobId, string reportId);

    /// <summary>
    /// Get evaluation report by application ID
    /// </summary>
    Task<ApiResponse<EvaluationReportResponse>> GetReportByApplicationIdAsync(Guid jobId, Guid applicationId);

    /// <summary>
    /// Get all evaluation reports for a job
    /// </summary>
    Task<ApiResponse<IEnumerable<EvaluationReportResponse>>> GetReportsByJobIdAsync(Guid jobId);
}
