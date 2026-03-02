namespace HRHiringSystem.Application.Interfaces;

/// <summary>
/// Interface for calling Azure Functions for background processing
/// </summary>
public interface IAzureFunctionService
{
    /// <summary>
    /// Trigger the Azure Function to process a job application (AI evaluation)
    /// This runs in background and does not block the API response
    /// </summary>
    /// <param name="candidateId">The candidate ID</param>
    /// <param name="jobId">The job ID</param>
    /// <param name="applicationId">The job application ID</param>
    /// <param name="cvUrl">The URL to the uploaded CV</param>
    Task TriggerApplicationProcessingAsync(Guid candidateId, Guid jobId, Guid applicationId, string cvUrl);
}
