import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, Job } from '../models';

interface JobResponseDto {
  jobId: string;
  jobTitle: string;
  jobDescription: string;
  jobRequirements: string;
  jobIsActive: boolean;
  jobCreatedByHrId: string;
}

interface JobRequestDto {
  jobTitle: string;
  jobDescription: string;
  jobRequirements: string;
  jobCreatedByHrId: string;
  jobIsActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private apiUrl = `${environment.apiUrl}/jobs`;

  constructor(private http: HttpClient) {}

  getAllJobs(): Observable<Job[]> {
    return this.http.get<ApiResponse<JobResponseDto[]>>(this.apiUrl).pipe(
      map(response => (response.data || []).map(this.mapJob))
    );
  }

  getJobById(id: string): Observable<Job> {
    return this.http.get<ApiResponse<JobResponseDto>>(`${this.apiUrl}/${id}`).pipe(
      map(response => this.mapJob(response.data as JobResponseDto))
    );
  }

  getActiveJobs(): Observable<Job[]> {
    return this.http.get<ApiResponse<JobResponseDto[]>>(`${this.apiUrl}/active`).pipe(
      map(response => (response.data || []).map(this.mapJob))
    );
  }

  createJob(job: Job): Observable<Job> {
    const request: JobRequestDto = {
      jobTitle: job.title,
      jobDescription: job.description,
      jobRequirements: job.requirements,
      jobCreatedByHrId: job.createdByHrId,
      jobIsActive: job.isActive
    };

    return this.http.post<ApiResponse<JobResponseDto>>(this.apiUrl, request).pipe(
      map(response => this.mapJob(response.data as JobResponseDto))
    );
  }

  updateJob(job: Job): Observable<Job> {
    const request: JobRequestDto = {
      jobTitle: job.title,
      jobDescription: job.description,
      jobRequirements: job.requirements,
      jobCreatedByHrId: job.createdByHrId,
      jobIsActive: job.isActive
    };

    return this.http.put<ApiResponse<JobResponseDto>>(`${this.apiUrl}/${job.id}`, request).pipe(
      map(response => this.mapJob(response.data as JobResponseDto))
    );
  }

  deleteJob(jobId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${jobId}`);
  }

  private mapJob(dto: JobResponseDto): Job {
    return {
      id: dto.jobId,
      title: dto.jobTitle,
      description: dto.jobDescription,
      requirements: dto.jobRequirements,
      isActive: dto.jobIsActive,
      createdByHrId: dto.jobCreatedByHrId
    };
  }
}
