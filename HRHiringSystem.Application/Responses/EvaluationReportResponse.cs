using HRHiringSystem.Domain.Models;

namespace HRHiringSystem.Application.Responses;

/// <summary>
/// Response DTO for evaluation reports
/// </summary>
public class EvaluationReportResponse
{
    public string? ReportId { get; set; }
    public Guid CandidateId { get; set; }
    public Guid JobId { get; set; }
    public Guid ApplicationId { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public int Score { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public SkillsAnalysisResponse SkillsAnalysis { get; set; } = new();
    public EducationAnalysisResponse EducationAnalysis { get; set; } = new();
    public ExperienceAnalysisResponse ExperienceAnalysis { get; set; } = new();
    public EvaluationSummaryResponse Summary { get; set; } = new();
    public List<InterviewQuestionResponse> InterviewQuestions { get; set; } = new();
    public AIMetadataResponse AIMetadata { get; set; } = new();
    
    // Related entities (optional, populated when requested)
    public CandidateResponse? Candidate { get; set; }
    public JobResponse? Job { get; set; }
}

public class SkillsAnalysisResponse
{
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public int MatchPercentage { get; set; }
    public string Details { get; set; } = string.Empty;
}

public class EducationAnalysisResponse
{
    public string Required { get; set; } = string.Empty;
    public string Candidate { get; set; } = string.Empty;
    public bool Match { get; set; }
    public int Score { get; set; }
}

public class ExperienceAnalysisResponse
{
    public int RequiredYears { get; set; }
    public int CandidateYears { get; set; }
    public List<string> RelevantExperience { get; set; } = new();
    public int Score { get; set; }
}

public class EvaluationSummaryResponse
{
    public string Decision { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Concerns { get; set; } = new();
}

public class InterviewQuestionResponse
{
    public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string ExpectedAnswer { get; set; } = string.Empty;
}

public class AIMetadataResponse
{
    public string Model { get; set; } = string.Empty;
    public long ProcessingTimeMs { get; set; }
    public int ReflectionIterations { get; set; }
}

/// <summary>
/// Response for CV parsing
/// </summary>
public class CvParseResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
