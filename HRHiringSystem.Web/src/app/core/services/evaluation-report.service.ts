import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, EvaluationReport } from '../models';

// DTO matching the actual .NET API response structure
interface EvaluationReportDto {
  reportId: string;
  candidateId: string;
  jobId: string;
  applicationId: string;
  evaluatedAt: string;
  score: number;
  status: string;
  skillsAnalysis: {
    matchedSkills: string[];
    missingSkills: string[];
    matchPercentage: number;
    details: string;
  };
  educationAnalysis: {
    required: string;
    candidate: string;
    match: boolean;
    score: number;
  };
  experienceAnalysis: {
    candidateYears: number;
    requiredYears: number;
    relevantExperience: string[];
    score: number;
  };
  summary: {
    decision: string;
    reasoning: string;
    strengths: string[];
    concerns: string[];
  };
  interviewQuestions: {
    question: string;
    category: string;
    difficulty: string;
    expectedAnswer: string;
  }[];
  aiMetadata: {
    model: string;
    processingTimeMs: number;
    reflectionIterations: number;
  };
}

@Injectable({
  providedIn: 'root'
})
export class EvaluationReportService {
  private apiUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  /**
   * Get all evaluation reports for a specific job
   * @param jobId The job ID
   * @returns Observable with array of evaluation reports
   */
  getReportsByJob(jobId: string): Observable<EvaluationReport[]> {
    return this.http.get<ApiResponse<EvaluationReportDto[]>>(`${this.apiUrl}/job/${jobId}`).pipe(
      map(response => (response.data || []).map(this.mapReport))
    );
  }

  /**
   * Get a specific evaluation report by ID
   * @param reportId The MongoDB report ID
   * @param jobId The job ID (needed for database selection)
   * @returns Observable with the evaluation report
   */
  getReportById(reportId: string, jobId: string): Observable<EvaluationReport> {
    return this.http.get<ApiResponse<EvaluationReportDto>>(`${this.apiUrl}/job/${jobId}/report/${reportId}`).pipe(
      map(response => this.mapReport(response.data as EvaluationReportDto))
    );
  }

  /**
   * Get a specific evaluation report by application ID
   * @param applicationId The application ID
   * @param jobId The job ID (needed for database selection)
   * @returns Observable with the evaluation report
   */
  getReportByApplicationId(applicationId: string, jobId: string): Observable<EvaluationReport> {
    return this.http.get<ApiResponse<EvaluationReportDto>>(`${this.apiUrl}/job/${jobId}/application/${applicationId}`).pipe(
      map(response => this.mapReport(response.data as EvaluationReportDto))
    );
  }

  private mapReport(dto: EvaluationReportDto): EvaluationReport {
    return {
      id: dto.reportId,
      candidateId: dto.candidateId,
      jobId: dto.jobId,
      applicationId: dto.applicationId,
      overallScore: dto.score,
      status: dto.status,
      skillsAnalysis: {
        matchedSkills: dto.skillsAnalysis?.matchedSkills || [],
        missingSkills: dto.skillsAnalysis?.missingSkills || [],
        additionalSkills: [],
        skillMatchPercentage: dto.skillsAnalysis?.matchPercentage || 0,
        skillsScore: dto.skillsAnalysis?.matchPercentage || 0,
        details: dto.skillsAnalysis?.details || ''
      },
      educationAnalysis: {
        candidateEducation: dto.educationAnalysis?.candidate || 'Not specified',
        requiredEducation: dto.educationAnalysis?.required || 'Not specified',
        educationMet: dto.educationAnalysis?.match ?? false,
        educationScore: dto.educationAnalysis?.score || 0,
        educationComments: ''
      },
      experienceAnalysis: {
        candidateYears: dto.experienceAnalysis?.candidateYears || 0,
        requiredYears: dto.experienceAnalysis?.requiredYears || 0,
        experienceMet: (dto.experienceAnalysis?.candidateYears || 0) >= (dto.experienceAnalysis?.requiredYears || 0),
        experienceScore: dto.experienceAnalysis?.score || 0,
        experienceComments: (dto.experienceAnalysis?.relevantExperience || []).join('; ')
      },
      summary: {
        strengths: dto.summary?.strengths || [],
        weaknesses: dto.summary?.concerns || [],
        recommendation: dto.summary?.decision || '',
        cultureFitAssessment: '',
        decision: dto.summary?.decision || '',
        reasoning: dto.summary?.reasoning || ''
      },
      interviewQuestions: (dto.interviewQuestions || []).map(q => ({
        question: q.question,
        category: q.category,
        difficulty: q.difficulty,
        expectedAnswer: q.expectedAnswer || ''
      })),
      aiMetadata: {
        modelUsed: dto.aiMetadata?.model || 'N/A',
        parsedAt: new Date(),
        evaluatedAt: dto.evaluatedAt ? new Date(dto.evaluatedAt) : new Date(),
        reflectionApplied: (dto.aiMetadata?.reflectionIterations || 0) > 0,
        totalProcessingTimeMs: dto.aiMetadata?.processingTimeMs || 0
      },
      createdAt: dto.evaluatedAt ? new Date(dto.evaluatedAt) : new Date()
    };
  }
}
