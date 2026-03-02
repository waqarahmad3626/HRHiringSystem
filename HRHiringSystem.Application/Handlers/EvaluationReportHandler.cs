using HRHiringSystem.Application.Constants;
using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Responses;
using HRHiringSystem.Domain.Enums;
using HRHiringSystem.Domain.Models;

namespace HRHiringSystem.Application.Handlers;

/// <summary>
/// Handler for evaluation report operations
/// </summary>
public class EvaluationReportHandler : IEvaluationReportHandler
{
    private readonly IEvaluationReportRepository _reportRepository;
    private readonly IJobApplicationRepository _applicationRepository;

    public EvaluationReportHandler(
        IEvaluationReportRepository reportRepository,
        IJobApplicationRepository applicationRepository)
    {
        _reportRepository = reportRepository;
        _applicationRepository = applicationRepository;
    }

    public async Task<ApiResponse<EvaluationReportResponse>> SaveReportAsync(SaveEvaluationReportRequest request)
    {
        // Create the evaluation report model
        var report = new EvaluationReport
        {
            CandidateId = request.CandidateId,
            JobId = request.JobId,
            ApplicationId = request.ApplicationId,
            EvaluatedAt = DateTime.UtcNow,
            Score = request.Score,
            Status = request.Status,
            SkillsAnalysis = new SkillsAnalysis
            {
                MatchedSkills = request.SkillsAnalysis.MatchedSkills,
                MissingSkills = request.SkillsAnalysis.MissingSkills,
                MatchPercentage = request.SkillsAnalysis.MatchPercentage,
                Details = request.SkillsAnalysis.Details
            },
            EducationAnalysis = new EducationAnalysis
            {
                Required = request.EducationAnalysis.Required,
                Candidate = request.EducationAnalysis.Candidate,
                Match = request.EducationAnalysis.Match,
                Score = request.EducationAnalysis.Score
            },
            ExperienceAnalysis = new ExperienceAnalysis
            {
                RequiredYears = request.ExperienceAnalysis.RequiredYears,
                CandidateYears = request.ExperienceAnalysis.CandidateYears,
                RelevantExperience = request.ExperienceAnalysis.RelevantExperience,
                Score = request.ExperienceAnalysis.Score
            },
            Summary = new EvaluationSummary
            {
                Decision = request.Summary.Decision,
                Reasoning = request.Summary.Reasoning,
                Strengths = request.Summary.Strengths,
                Concerns = request.Summary.Concerns
            },
            InterviewQuestions = request.InterviewQuestions.Select(q => new InterviewQuestion
            {
                Category = q.Category,
                Question = q.Question,
                Difficulty = q.Difficulty,
                ExpectedAnswer = q.ExpectedAnswer
            }).ToList(),
            AIMetadata = new AIMetadata
            {
                Model = request.AIMetadata.Model,
                ProcessingTimeMs = request.AIMetadata.ProcessingTimeMs,
                ReflectionIterations = request.AIMetadata.ReflectionIterations
            }
        };

        // Save to MongoDB
        var mongoId = await _reportRepository.SaveReportAsync(report);
        report.Id = mongoId;

        // Update the job application with score, status, and mongo report ID
        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId);
        if (application != null)
        {
            application.Score = request.Score;
            application.MongoReportId = mongoId;
            application.EvaluatedAt = DateTime.UtcNow;
            
            // Map status string to enum
            application.Status = request.Status.ToLowerInvariant() switch
            {
                "accepted" => ApplicationStatus.Accepted,
                "hrreview" => ApplicationStatus.HRReview,
                "rejected" => ApplicationStatus.Rejected,
                _ => ApplicationStatus.Processing
            };

            await _applicationRepository.UpdateAsync(application, null);
        }

        var response = MapToResponse(report);
        return ApiResponse<EvaluationReportResponse>.Success(response, "Evaluation report saved successfully");
    }

    public async Task<ApiResponse<EvaluationReportResponse>> GetReportByIdAsync(Guid jobId, string reportId)
    {
        var report = await _reportRepository.GetReportByIdAsync(jobId, reportId);
        
        if (report == null)
        {
            return ApiResponse<EvaluationReportResponse>.Failure(
                ResponseConstants.ErrorCodeNotFound, 
                "Evaluation report not found");
        }

        var response = MapToResponse(report);
        return ApiResponse<EvaluationReportResponse>.Success(response, "Evaluation report retrieved successfully");
    }

    public async Task<ApiResponse<EvaluationReportResponse>> GetReportByApplicationIdAsync(Guid jobId, Guid applicationId)
    {
        var report = await _reportRepository.GetReportByApplicationIdAsync(jobId, applicationId);
        
        if (report == null)
        {
            return ApiResponse<EvaluationReportResponse>.Failure(
                ResponseConstants.ErrorCodeNotFound, 
                "Evaluation report not found for this application");
        }

        var response = MapToResponse(report);
        return ApiResponse<EvaluationReportResponse>.Success(response, "Evaluation report retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<EvaluationReportResponse>>> GetReportsByJobIdAsync(Guid jobId)
    {
        var reports = await _reportRepository.GetReportsByJobIdAsync(jobId);
        var responses = reports.Select(MapToResponse);
        
        return ApiResponse<IEnumerable<EvaluationReportResponse>>.Success(
            responses, 
            "Evaluation reports retrieved successfully");
    }

    private static EvaluationReportResponse MapToResponse(EvaluationReport report)
    {
        return new EvaluationReportResponse
        {
            ReportId = report.Id,
            CandidateId = report.CandidateId,
            JobId = report.JobId,
            ApplicationId = report.ApplicationId,
            EvaluatedAt = report.EvaluatedAt,
            Score = report.Score,
            Status = report.Status,
            SkillsAnalysis = new SkillsAnalysisResponse
            {
                MatchedSkills = report.SkillsAnalysis.MatchedSkills,
                MissingSkills = report.SkillsAnalysis.MissingSkills,
                MatchPercentage = report.SkillsAnalysis.MatchPercentage,
                Details = report.SkillsAnalysis.Details
            },
            EducationAnalysis = new EducationAnalysisResponse
            {
                Required = report.EducationAnalysis.Required,
                Candidate = report.EducationAnalysis.Candidate,
                Match = report.EducationAnalysis.Match,
                Score = report.EducationAnalysis.Score
            },
            ExperienceAnalysis = new ExperienceAnalysisResponse
            {
                RequiredYears = report.ExperienceAnalysis.RequiredYears,
                CandidateYears = report.ExperienceAnalysis.CandidateYears,
                RelevantExperience = report.ExperienceAnalysis.RelevantExperience,
                Score = report.ExperienceAnalysis.Score
            },
            Summary = new EvaluationSummaryResponse
            {
                Decision = report.Summary.Decision,
                Reasoning = report.Summary.Reasoning,
                Strengths = report.Summary.Strengths,
                Concerns = report.Summary.Concerns
            },
            InterviewQuestions = report.InterviewQuestions.Select(q => new InterviewQuestionResponse
            {
                Category = q.Category,
                Question = q.Question,
                Difficulty = q.Difficulty,
                ExpectedAnswer = q.ExpectedAnswer
            }).ToList(),
            AIMetadata = new AIMetadataResponse
            {
                Model = report.AIMetadata.Model,
                ProcessingTimeMs = report.AIMetadata.ProcessingTimeMs,
                ReflectionIterations = report.AIMetadata.ReflectionIterations
            }
        };
    }
}
