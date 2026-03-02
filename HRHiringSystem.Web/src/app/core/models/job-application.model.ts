export type ApplicationStatus = 
  | 'Pending' 
  | 'Processing' 
  | 'Accepted' 
  | 'HRReview' 
  | 'Rejected' 
  | 'InterviewScheduled' 
  | 'Hired';

export interface JobApplicationRequest {
  candidateFirstName: string;
  candidateLastName: string;
  candidateEmail: string;
  candidatePhone: string;
  jobId: string;
  cvFile?: File;
}

export interface CandidateSummary {
  id: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
}

export interface JobSummary {
  id: string;
  title?: string;
}

export interface JobApplication {
  id: string;
  candidateId: string;
  jobId: string;
  cvUrl: string;
  appliedDate: Date;
  status: ApplicationStatus;
  score?: number;
  mongoReportId?: string;
  evaluatedAt?: Date;
  interviewScheduledAt?: Date;
  hrNotes?: string;
  candidate?: CandidateSummary;
  job?: JobSummary;
}
