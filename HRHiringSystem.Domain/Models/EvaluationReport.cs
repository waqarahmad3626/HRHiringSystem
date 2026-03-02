namespace HRHiringSystem.Domain.Models;

/// <summary>
/// MongoDB document model for AI evaluation reports
/// Stored in collection: evaluation_reports within job-specific database
/// </summary>
public class EvaluationReport
{
    /// <summary>
    /// MongoDB ObjectId as string
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Reference to SQL Server Candidate.Id
    /// </summary>
    public Guid CandidateId { get; set; }

    /// <summary>
    /// Reference to SQL Server Job.Id
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// Reference to SQL Server JobApplication.Id
    /// </summary>
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// When the AI evaluation was performed
    /// </summary>
    public DateTime EvaluatedAt { get; set; }

    /// <summary>
    /// Overall score (0-100)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Decision status: Accepted, HRReview, Rejected
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Skills analysis details
    /// </summary>
    public SkillsAnalysis SkillsAnalysis { get; set; } = new();

    /// <summary>
    /// Education analysis details
    /// </summary>
    public EducationAnalysis EducationAnalysis { get; set; } = new();

    /// <summary>
    /// Experience analysis details
    /// </summary>
    public ExperienceAnalysis ExperienceAnalysis { get; set; } = new();

    /// <summary>
    /// Summary and decision reasoning
    /// </summary>
    public EvaluationSummary Summary { get; set; } = new();

    /// <summary>
    /// Generated interview questions (if score >= 65)
    /// </summary>
    public List<InterviewQuestion> InterviewQuestions { get; set; } = new();

    /// <summary>
    /// AI processing metadata
    /// </summary>
    public AIMetadata AIMetadata { get; set; } = new();
}

public class SkillsAnalysis
{
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public int MatchPercentage { get; set; }
    public string Details { get; set; } = string.Empty;
}

public class EducationAnalysis
{
    public string Required { get; set; } = string.Empty;
    public string Candidate { get; set; } = string.Empty;
    public bool Match { get; set; }
    public int Score { get; set; }
}

public class ExperienceAnalysis
{
    public int RequiredYears { get; set; }
    public int CandidateYears { get; set; }
    public List<string> RelevantExperience { get; set; } = new();
    public int Score { get; set; }
}

public class EvaluationSummary
{
    public string Decision { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Concerns { get; set; } = new();
}

public class InterviewQuestion
{
    public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string ExpectedAnswer { get; set; } = string.Empty;
}

public class AIMetadata
{
    public string Model { get; set; } = string.Empty;
    public long ProcessingTimeMs { get; set; }
    public int ReflectionIterations { get; set; }
}
