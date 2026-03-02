using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Handlers.Abstract;

/// <summary>
/// Handler interface for CV operations
/// </summary>
public interface ICvHandler
{
    /// <summary>
    /// Parse CV to extract candidate information
    /// </summary>
    /// <param name="cvStream">The CV file stream</param>
    /// <param name="fileName">Original file name</param>
    /// <returns>Parsed candidate information</returns>
    Task<ApiResponse<CvParseResponse>> ParseCvAsync(Stream cvStream, string fileName);
}
