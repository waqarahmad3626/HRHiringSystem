using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Application.Responses;

namespace HRHiringSystem.Application.Handlers;

/// <summary>
/// Handler for CV operations - proxies to Python AI Agent for parsing
/// </summary>
public class CvHandler : ICvHandler
{
    private readonly IAIAgentService _aiAgentService;

    public CvHandler(IAIAgentService aiAgentService)
    {
        _aiAgentService = aiAgentService;
    }

    public async Task<ApiResponse<CvParseResponse>> ParseCvAsync(Stream cvStream, string fileName)
    {
        var result = await _aiAgentService.ParseCvAsync(cvStream, fileName);

        var response = new CvParseResponse
        {
            Success = result.Success,
            Error = result.Error,
            FirstName = result.FirstName,
            LastName = result.LastName,
            Email = result.Email,
            Phone = result.Phone
        };

        if (result.Success)
        {
            return ApiResponse<CvParseResponse>.Success(response, "CV parsed successfully");
        }
        else
        {
            return ApiResponse<CvParseResponse>.Failure("CV_PARSE_ERROR", result.Error ?? "Failed to parse CV");
        }
    }
}
