using HRHiringSystem.Domain.Models;

namespace HRHiringSystem.Application.Requests;

/// <summary>
/// Request to save an evaluation report (called by Python AI Agent via .NET API)
/// </summary>
public class SaveEvaluationReportRequest
{
    public Guid CandidateId { get; set; }
    public Guid JobId { get; set; }
    public Guid ApplicationId { get; set; }
    public int Score { get; set; }
    public required string Status { get; set; }
    
    public SkillsAnalysisRequest SkillsAnalysis { get; set; } = new();
    public EducationAnalysisRequest EducationAnalysis { get; set; } = new();
    public ExperienceAnalysisRequest ExperienceAnalysis { get; set; } = new();
    public EvaluationSummaryRequest Summary { get; set; } = new();
    public List<InterviewQuestionRequest> InterviewQuestions { get; set; } = new();
    public AIMetadataRequest AIMetadata { get; set; } = new();
}

public class SkillsAnalysisRequest
{
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public int MatchPercentage { get; set; }
    public string Details { get; set; } = string.Empty;
}

public class EducationAnalysisRequest
{
    public string Required { get; set; } = string.Empty;
    public string Candidate { get; set; } = string.Empty;
    public bool Match { get; set; }
    public int Score { get; set; }
}

public class ExperienceAnalysisRequest
{
    public int RequiredYears { get; set; }
    public int CandidateYears { get; set; }
    public List<string> RelevantExperience { get; set; } = new();
    public int Score { get; set; }
}

public class EvaluationSummaryRequest
{
    public string Decision { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Concerns { get; set; } = new();
}

public class InterviewQuestionRequest
{
    public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string ExpectedAnswer { get; set; } = string.Empty;
}

public class AIMetadataRequest
{
    public string Model { get; set; } = string.Empty;
    public long ProcessingTimeMs { get; set; }
    public int ReflectionIterations { get; set; }
}
