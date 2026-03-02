import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, JobApplication, JobApplicationRequest, ApplicationStatus } from '../models';

interface JobApplicationResponseDto {
  jobApplicationId: string;
  jobApplicationCandidateId: string;
  jobApplicationJobId: string;
  jobApplicationCvUrl: string;
  jobApplicationAppliedAt: string;
  jobApplicationStatus: string | number;
  jobApplicationScore?: number;
  jobApplicationMongoReportId?: string;
  jobApplicationEvaluatedAt?: string;
  jobApplicationInterviewScheduledAt?: string;
  jobApplicationHRNotes?: string;
  jobApplicationCandidate?: {
    candidateId: string;
    firstName?: string;
    lastName?: string;
    email?: string;
    phone?: string;
  };
  jobApplicationJob?: {
    jobId: string;
    jobTitle?: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class JobApplicationService {
  private apiUrl = `${environment.apiUrl}/jobapplications`;

  constructor(private http: HttpClient) {}

  submitApplication(application: JobApplicationRequest): Observable<any> {
    const formData = new FormData();
    formData.append('candidateFirstName', application.candidateFirstName);
    formData.append('candidateLastName', application.candidateLastName);
    formData.append('candidateEmail', application.candidateEmail);
    formData.append('candidatePhone', application.candidatePhone);
    formData.append('jobId', application.jobId);
    
    if (application.cvFile) {
      formData.append('cvFile', application.cvFile, application.cvFile.name);
    }

    return this.http.post<ApiResponse<unknown>>(this.apiUrl, formData);
  }

  getAllApplications(): Observable<JobApplication[]> {
    return this.http.get<ApiResponse<JobApplicationResponseDto[]>>(this.apiUrl).pipe(
      map(response => (response.data || []).map(dto => this.mapApplication(dto)))
    );
  }

  getApplicationsByJob(jobId: string): Observable<JobApplication[]> {
    return this.http.get<ApiResponse<JobApplicationResponseDto[]>>(`${this.apiUrl}/job/${jobId}`).pipe(
      map(response => (response.data || []).map(dto => this.mapApplication(dto)))
    );
  }

  getApplicationById(id: string): Observable<JobApplication> {
    return this.http.get<ApiResponse<JobApplicationResponseDto>>(`${this.apiUrl}/${id}`).pipe(
      map(response => this.mapApplication(response.data as JobApplicationResponseDto))
    );
  }

  updateHRNotes(applicationId: string, notes: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${applicationId}/notes`, { hrNotes: notes });
  }

  scheduleInterview(applicationId: string, scheduledAt: Date): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${applicationId}/schedule-interview`, { 
      interviewScheduledAt: scheduledAt.toISOString() 
    });
  }

  private mapStatusToString(status: string | number | undefined): ApplicationStatus {
    // Handle numeric enum values from backend
    const statusMap: Record<number, ApplicationStatus> = {
      0: 'Pending',
      1: 'Processing',
      2: 'Accepted',
      3: 'HRReview',
      4: 'Rejected',
      5: 'InterviewScheduled',
      6: 'Hired'
    };

    if (status === undefined || status === null) {
      return 'Pending';
    }

    if (typeof status === 'number') {
      return statusMap[status] || 'Pending';
    }

    // If it's a string that looks like a number, convert it
    const numericStatus = parseInt(status, 10);
    if (!isNaN(numericStatus) && statusMap[numericStatus]) {
      return statusMap[numericStatus];
    }

    // Already a string status
    return (status || 'Pending') as ApplicationStatus;
  }

  private mapApplication(dto: JobApplicationResponseDto): JobApplication {
    return {
      id: dto.jobApplicationId,
      candidateId: dto.jobApplicationCandidateId,
      jobId: dto.jobApplicationJobId,
      cvUrl: dto.jobApplicationCvUrl,
      appliedDate: new Date(dto.jobApplicationAppliedAt),
      status: this.mapStatusToString(dto.jobApplicationStatus),
      score: dto.jobApplicationScore,
      mongoReportId: dto.jobApplicationMongoReportId,
      evaluatedAt: dto.jobApplicationEvaluatedAt ? new Date(dto.jobApplicationEvaluatedAt) : undefined,
      interviewScheduledAt: dto.jobApplicationInterviewScheduledAt ? new Date(dto.jobApplicationInterviewScheduledAt) : undefined,
      hrNotes: dto.jobApplicationHRNotes,
      candidate: dto.jobApplicationCandidate
        ? {
            id: dto.jobApplicationCandidate.candidateId,
            firstName: dto.jobApplicationCandidate.firstName,
            lastName: dto.jobApplicationCandidate.lastName,
            email: dto.jobApplicationCandidate.email,
            phone: dto.jobApplicationCandidate.phone
          }
        : undefined,
      job: dto.jobApplicationJob
        ? {
            id: dto.jobApplicationJob.jobId,
            title: dto.jobApplicationJob.jobTitle
          }
        : undefined
    };
  }
}
