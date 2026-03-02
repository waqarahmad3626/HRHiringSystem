using System.Net.Http.Json;
using System.Text.Json;
using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRHiringSystem.Infrastructure.Services;

/// <summary>
/// HTTP client service for communicating with Python FastAPI AI Agent
/// </summary>
public class AIAgentService : IAIAgentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIAgentService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AIAgentService(HttpClient httpClient, ILogger<AIAgentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<CvParseResult> ParseCvAsync(Stream cvStream, string fileName)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(cvStream);
            
            // Set content type based on file extension
            var contentType = GetContentType(fileName);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync("/api/cv/parse", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("CV parsing failed: {StatusCode} - {Error}", response.StatusCode, error);
                return new CvParseResult
                {
                    Success = false,
                    Error = $"AI Agent returned {response.StatusCode}: {error}"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<CvParseApiResponse>(_jsonOptions);
            
            return new CvParseResult
            {
                Success = true,
                FirstName = result?.FirstName,
                LastName = result?.LastName,
                Email = result?.Email,
                Phone = result?.Phone
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI Agent for CV parsing");
            return new CvParseResult
            {
                Success = false,
                Error = "Failed to connect to AI Agent service"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during CV parsing");
            return new CvParseResult
            {
                Success = false,
                Error = "Unexpected error during CV parsing"
            };
        }
    }

    public async Task<EvaluationReport> EvaluateApplicationAsync(EvaluationRequest request)
    {
        try
        {
            var apiRequest = new EvaluationApiRequest
            {
                CandidateId = request.CandidateId.ToString(),
                JobId = request.JobId.ToString(),
                ApplicationId = request.ApplicationId.ToString(),
                CvUrl = request.CvUrl,
                JobTitle = request.JobTitle,
                JobDescription = request.JobDescription,
                JobRequirements = request.JobRequirements,
                JobRequiredSkills = request.JobRequiredSkills,
                JobExperienceYears = request.JobExperienceYears,
                JobEducation = request.JobEducation
            };

            var response = await _httpClient.PostAsJsonAsync("/api/evaluate", apiRequest, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI evaluation failed: {StatusCode} - {Error}", response.StatusCode, error);
                throw new InvalidOperationException($"AI Agent evaluation failed: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<EvaluationApiResponse>(_jsonOptions);
            
            if (result == null)
            {
                throw new InvalidOperationException("AI Agent returned empty response");
            }

            return MapToEvaluationReport(result, request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI Agent for evaluation");
            throw new InvalidOperationException("Failed to connect to AI Agent service", ex);
        }
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private static EvaluationReport MapToEvaluationReport(EvaluationApiResponse response, EvaluationRequest request)
    {
        return new EvaluationReport
        {
            CandidateId = request.CandidateId,
            JobId = request.JobId,
            ApplicationId = request.ApplicationId,
            EvaluatedAt = DateTime.UtcNow,
            Score = response.Score,
            Status = response.Status,
            SkillsAnalysis = new SkillsAnalysis
            {
                MatchedSkills = response.SkillsAnalysis?.MatchedSkills ?? new List<string>(),
                MissingSkills = response.SkillsAnalysis?.MissingSkills ?? new List<string>(),
                MatchPercentage = response.SkillsAnalysis?.MatchPercentage ?? 0,
                Details = response.SkillsAnalysis?.Details ?? string.Empty
            },
            EducationAnalysis = new EducationAnalysis
            {
                Required = response.EducationAnalysis?.Required ?? string.Empty,
                Candidate = response.EducationAnalysis?.Candidate ?? string.Empty,
                Match = response.EducationAnalysis?.Match ?? false,
                Score = response.EducationAnalysis?.Score ?? 0
            },
            ExperienceAnalysis = new ExperienceAnalysis
            {
                RequiredYears = response.ExperienceAnalysis?.RequiredYears ?? 0,
                CandidateYears = response.ExperienceAnalysis?.CandidateYears ?? 0,
                RelevantExperience = response.ExperienceAnalysis?.RelevantExperience ?? new List<string>(),
                Score = response.ExperienceAnalysis?.Score ?? 0
            },
            Summary = new EvaluationSummary
            {
                Decision = response.Summary?.Decision ?? string.Empty,
                Reasoning = response.Summary?.Reasoning ?? string.Empty,
                Strengths = response.Summary?.Strengths ?? new List<string>(),
                Concerns = response.Summary?.Concerns ?? new List<string>()
            },
            InterviewQuestions = response.InterviewQuestions?.Select(q => new InterviewQuestion
            {
                Category = q.Category,
                Question = q.Question,
                Difficulty = q.Difficulty
            }).ToList() ?? new List<InterviewQuestion>(),
            AIMetadata = new AIMetadata
            {
                Model = response.AiMetadata?.Model ?? "unknown",
                ProcessingTimeMs = response.AiMetadata?.ProcessingTimeMs ?? 0,
                ReflectionIterations = response.AiMetadata?.ReflectionIterations ?? 0
            }
        };
    }
}

#region API DTOs

internal class CvParseApiResponse
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

internal class EvaluationApiRequest
{
    public string CandidateId { get; set; } = string.Empty;
    public string JobId { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public string CvUrl { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string JobRequirements { get; set; } = string.Empty;
    public string JobRequiredSkills { get; set; } = string.Empty;
    public int JobExperienceYears { get; set; }
    public string? JobEducation { get; set; }
}

internal class EvaluationApiResponse
{
    public int Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public SkillsAnalysisDto? SkillsAnalysis { get; set; }
    public EducationAnalysisDto? EducationAnalysis { get; set; }
    public ExperienceAnalysisDto? ExperienceAnalysis { get; set; }
    public SummaryDto? Summary { get; set; }
    public List<InterviewQuestionDto>? InterviewQuestions { get; set; }
    public AiMetadataDto? AiMetadata { get; set; }
}

internal class SkillsAnalysisDto
{
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public int MatchPercentage { get; set; }
    public string Details { get; set; } = string.Empty;
}

internal class EducationAnalysisDto
{
    public string Required { get; set; } = string.Empty;
    public string Candidate { get; set; } = string.Empty;
    public bool Match { get; set; }
    public int Score { get; set; }
}

internal class ExperienceAnalysisDto
{
    public int RequiredYears { get; set; }
    public int CandidateYears { get; set; }
    public List<string> RelevantExperience { get; set; } = new();
    public int Score { get; set; }
}

internal class SummaryDto
{
    public string Decision { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Concerns { get; set; } = new();
}

internal class InterviewQuestionDto
{
    public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
}

internal class AiMetadataDto
{
    public string Model { get; set; } = string.Empty;
    public long ProcessingTimeMs { get; set; }
    public int ReflectionIterations { get; set; }
}

#endregion
