using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Domain.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace HRHiringSystem.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation for evaluation reports
/// Each job gets its own database: job_{jobId}
/// Collection: evaluation_reports
/// </summary>
public class EvaluationReportRepository : IEvaluationReportRepository
{
    private readonly IMongoClient _mongoClient;
    private const string CollectionName = "evaluation_reports";

    public EvaluationReportRepository(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    private IMongoDatabase GetDatabase(Guid jobId)
    {
        return _mongoClient.GetDatabase($"job_{jobId}");
    }

    private IMongoCollection<EvaluationReportDocument> GetCollection(Guid jobId)
    {
        return GetDatabase(jobId).GetCollection<EvaluationReportDocument>(CollectionName);
    }

    public async Task<string> SaveReportAsync(EvaluationReport report)
    {
        var collection = GetCollection(report.JobId);
        
        var document = MapToDocument(report);
        
        await collection.InsertOneAsync(document);
        
        return document.Id.ToString();
    }

    public async Task<EvaluationReport?> GetReportByIdAsync(Guid jobId, string reportId)
    {
        if (!ObjectId.TryParse(reportId, out var objectId))
            return null;

        var collection = GetCollection(jobId);
        var filter = Builders<EvaluationReportDocument>.Filter.Eq(x => x.Id, objectId);
        
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        
        return document == null ? null : MapToModel(document);
    }

    public async Task<IEnumerable<EvaluationReport>> GetReportsByJobIdAsync(Guid jobId)
    {
        var collection = GetCollection(jobId);
        var documents = await collection.Find(_ => true).ToListAsync();
        
        return documents.Select(MapToModel);
    }

    public async Task<EvaluationReport?> GetReportByApplicationIdAsync(Guid jobId, Guid applicationId)
    {
        var collection = GetCollection(jobId);
        var filter = Builders<EvaluationReportDocument>.Filter.Eq(x => x.ApplicationId, applicationId);
        
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        
        return document == null ? null : MapToModel(document);
    }

    public async Task<bool> DeleteReportAsync(Guid jobId, string reportId)
    {
        if (!ObjectId.TryParse(reportId, out var objectId))
            return false;

        var collection = GetCollection(jobId);
        var filter = Builders<EvaluationReportDocument>.Filter.Eq(x => x.Id, objectId);
        
        var result = await collection.DeleteOneAsync(filter);
        
        return result.DeletedCount > 0;
    }

    private static EvaluationReportDocument MapToDocument(EvaluationReport report)
    {
        return new EvaluationReportDocument
        {
            CandidateId = report.CandidateId,
            JobId = report.JobId,
            ApplicationId = report.ApplicationId,
            EvaluatedAt = report.EvaluatedAt,
            Score = report.Score,
            Status = report.Status,
            SkillsAnalysis = new SkillsAnalysisDocument
            {
                MatchedSkills = report.SkillsAnalysis.MatchedSkills,
                MissingSkills = report.SkillsAnalysis.MissingSkills,
                MatchPercentage = report.SkillsAnalysis.MatchPercentage,
                Details = report.SkillsAnalysis.Details
            },
            EducationAnalysis = new EducationAnalysisDocument
            {
                Required = report.EducationAnalysis.Required,
                Candidate = report.EducationAnalysis.Candidate,
                Match = report.EducationAnalysis.Match,
                Score = report.EducationAnalysis.Score
            },
            ExperienceAnalysis = new ExperienceAnalysisDocument
            {
                RequiredYears = report.ExperienceAnalysis.RequiredYears,
                CandidateYears = report.ExperienceAnalysis.CandidateYears,
                RelevantExperience = report.ExperienceAnalysis.RelevantExperience,
                Score = report.ExperienceAnalysis.Score
            },
            Summary = new EvaluationSummaryDocument
            {
                Decision = report.Summary.Decision,
                Reasoning = report.Summary.Reasoning,
                Strengths = report.Summary.Strengths,
                Concerns = report.Summary.Concerns
            },
            InterviewQuestions = report.InterviewQuestions.Select(q => new InterviewQuestionDocument
            {
                Category = q.Category,
                Question = q.Question,
                Difficulty = q.Difficulty,
                ExpectedAnswer = q.ExpectedAnswer
            }).ToList(),
            AIMetadata = new AIMetadataDocument
            {
                Model = report.AIMetadata.Model,
                ProcessingTimeMs = report.AIMetadata.ProcessingTimeMs,
                ReflectionIterations = report.AIMetadata.ReflectionIterations
            }
        };
    }

    private static EvaluationReport MapToModel(EvaluationReportDocument document)
    {
        return new EvaluationReport
        {
            Id = document.Id.ToString(),
            CandidateId = document.CandidateId,
            JobId = document.JobId,
            ApplicationId = document.ApplicationId,
            EvaluatedAt = document.EvaluatedAt,
            Score = document.Score,
            Status = document.Status,
            SkillsAnalysis = new SkillsAnalysis
            {
                MatchedSkills = document.SkillsAnalysis.MatchedSkills,
                MissingSkills = document.SkillsAnalysis.MissingSkills,
                MatchPercentage = document.SkillsAnalysis.MatchPercentage,
                Details = document.SkillsAnalysis.Details
            },
            EducationAnalysis = new EducationAnalysis
            {
                Required = document.EducationAnalysis.Required,
                Candidate = document.EducationAnalysis.Candidate,
                Match = document.EducationAnalysis.Match,
                Score = document.EducationAnalysis.Score
            },
            ExperienceAnalysis = new ExperienceAnalysis
            {
                RequiredYears = document.ExperienceAnalysis.RequiredYears,
                CandidateYears = document.ExperienceAnalysis.CandidateYears,
                RelevantExperience = document.ExperienceAnalysis.RelevantExperience,
                Score = document.ExperienceAnalysis.Score
            },
            Summary = new EvaluationSummary
            {
                Decision = document.Summary.Decision,
                Reasoning = document.Summary.Reasoning,
                Strengths = document.Summary.Strengths,
                Concerns = document.Summary.Concerns
            },
            InterviewQuestions = document.InterviewQuestions.Select(q => new InterviewQuestion
            {
                Category = q.Category,
                Question = q.Question,
                Difficulty = q.Difficulty,
                ExpectedAnswer = q.ExpectedAnswer
            }).ToList(),
            AIMetadata = new AIMetadata
            {
                Model = document.AIMetadata.Model,
                ProcessingTimeMs = document.AIMetadata.ProcessingTimeMs,
                ReflectionIterations = document.AIMetadata.ReflectionIterations
            }
        };
    }
}

#region MongoDB Document Classes

internal class EvaluationReportDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("candidateId")]
    public Guid CandidateId { get; set; }

    [BsonElement("jobId")]
    public Guid JobId { get; set; }

    [BsonElement("applicationId")]
    public Guid ApplicationId { get; set; }

    [BsonElement("evaluatedAt")]
    public DateTime EvaluatedAt { get; set; }

    [BsonElement("score")]
    public int Score { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    [BsonElement("skillsAnalysis")]
    public SkillsAnalysisDocument SkillsAnalysis { get; set; } = new();

    [BsonElement("educationAnalysis")]
    public EducationAnalysisDocument EducationAnalysis { get; set; } = new();

    [BsonElement("experienceAnalysis")]
    public ExperienceAnalysisDocument ExperienceAnalysis { get; set; } = new();

    [BsonElement("summary")]
    public EvaluationSummaryDocument Summary { get; set; } = new();

    [BsonElement("interviewQuestions")]
    public List<InterviewQuestionDocument> InterviewQuestions { get; set; } = new();

    [BsonElement("aiMetadata")]
    public AIMetadataDocument AIMetadata { get; set; } = new();
}

internal class SkillsAnalysisDocument
{
    [BsonElement("matchedSkills")]
    public List<string> MatchedSkills { get; set; } = new();

    [BsonElement("missingSkills")]
    public List<string> MissingSkills { get; set; } = new();

    [BsonElement("matchPercentage")]
    public int MatchPercentage { get; set; }

    [BsonElement("details")]
    public string Details { get; set; } = string.Empty;
}

internal class EducationAnalysisDocument
{
    [BsonElement("required")]
    public string Required { get; set; } = string.Empty;

    [BsonElement("candidate")]
    public string Candidate { get; set; } = string.Empty;

    [BsonElement("match")]
    public bool Match { get; set; }

    [BsonElement("score")]
    public int Score { get; set; }
}

internal class ExperienceAnalysisDocument
{
    [BsonElement("requiredYears")]
    public int RequiredYears { get; set; }

    [BsonElement("candidateYears")]
    public int CandidateYears { get; set; }

    [BsonElement("relevantExperience")]
    public List<string> RelevantExperience { get; set; } = new();

    [BsonElement("score")]
    public int Score { get; set; }
}

internal class EvaluationSummaryDocument
{
    [BsonElement("decision")]
    public string Decision { get; set; } = string.Empty;

    [BsonElement("reasoning")]
    public string Reasoning { get; set; } = string.Empty;

    [BsonElement("strengths")]
    public List<string> Strengths { get; set; } = new();

    [BsonElement("concerns")]
    public List<string> Concerns { get; set; } = new();
}

internal class InterviewQuestionDocument
{
    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("question")]
    public string Question { get; set; } = string.Empty;

    [BsonElement("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [BsonElement("expectedAnswer")]
    public string ExpectedAnswer { get; set; } = string.Empty;
}

internal class AIMetadataDocument
{
    [BsonElement("model")]
    public string Model { get; set; } = string.Empty;

    [BsonElement("processingTimeMs")]
    public long ProcessingTimeMs { get; set; }

    [BsonElement("reflectionIterations")]
    public int ReflectionIterations { get; set; }
}

#endregion
