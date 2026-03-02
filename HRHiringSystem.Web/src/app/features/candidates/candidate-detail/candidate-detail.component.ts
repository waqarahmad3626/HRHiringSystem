import { Component, OnInit, OnDestroy, ChangeDetectorRef, NgZone } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { JobApplication, EvaluationReport } from '../../../core/models';
import { JobApplicationService, EvaluationReportService } from '../../../core/services';
import { Subject } from 'rxjs';
import { takeUntil, finalize, timeout } from 'rxjs/operators';

@Component({
  selector: 'app-candidate-detail',
  standalone: false,
  templateUrl: './candidate-detail.component.html',
  styleUrls: ['./candidate-detail.component.scss']
})
export class CandidateDetailComponent implements OnInit, OnDestroy {
  candidateId = '';
  selectedJobId = '';
  candidateApplication: JobApplication | null = null;
  report: EvaluationReport | null = null;
  loading = false;
  loadingReport = false;
  error = '';
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private jobApplicationService: JobApplicationService,
    private reportService: EvaluationReportService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    // Subscribe to route parameter changes (fixes caching issue when switching candidates)
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const candidateId = params.get('id');
        const jobId = params.get('jobId');
        
        if (!candidateId) {
          this.error = 'Candidate id is missing.';
          return;
        }

        if (!jobId) {
          this.error = 'Job id is missing.';
          return;
        }

        // Reset component state for new candidate/job combination
        this.candidateId = candidateId;
        this.selectedJobId = jobId;
        this.candidateApplication = null;
        this.report = null;
        this.error = '';
        this.loadCandidateDetail();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCandidateDetail(): void {
    this.loading = true;
    this.error = '';

    this.jobApplicationService.getAllApplications().pipe(
      timeout(15000),
      finalize(() => {
        this.ngZone.run(() => {
          this.loading = false;
          this.cdr.detectChanges();
        });
      })
    ).subscribe({
      next: applications => {
        this.ngZone.run(() => {
          // Find application for this candidate AND this specific job (not just any job)
          this.candidateApplication = applications.find(
            app => app.candidateId === this.candidateId && app.jobId === this.selectedJobId
          ) || null;

          if (!this.candidateApplication) {
            this.error = 'Application not found for this candidate and job.';
          } else if (this.candidateApplication.mongoReportId && this.candidateApplication.jobId) {
            this.loadReport(this.candidateApplication.mongoReportId, this.candidateApplication.jobId);
          }
          this.cdr.detectChanges();
        });
      },
      error: err => {
        this.ngZone.run(() => {
          this.error = err.error?.message || 'Failed to load candidate detail.';
          this.cdr.detectChanges();
        });
      }
    });
  }

  loadReport(reportId: string, jobId: string): void {
    this.loadingReport = true;
    this.reportService.getReportById(reportId, jobId).pipe(
      timeout(15000),
      finalize(() => {
        this.ngZone.run(() => {
          this.loadingReport = false;
          this.cdr.detectChanges();
        });
      })
    ).subscribe({
      next: report => {
        this.ngZone.run(() => {
          this.report = report;
          this.cdr.detectChanges();
        });
      },
      error: err => {
        console.error('Failed to load evaluation report:', err);
      }
    });
  }

  getScoreClass(score: number | undefined): string {
    if (score === undefined) return 'text-muted';
    if (score >= 80) return 'text-success';
    if (score >= 65) return 'text-warning';
    return 'text-danger';
  }

  getStatusClass(status: string): string {
    const classes: Record<string, string> = {
      'Pending': 'bg-secondary',
      'Processing': 'bg-info',
      'Accepted': 'bg-success',
      'HRReview': 'bg-warning text-dark',
      'Rejected': 'bg-danger',
      'InterviewScheduled': 'bg-primary',
      'Hired': 'bg-success'
    };
    return classes[status] || 'bg-secondary';
  }

  getScoreCircleClass(score: number): string {
    if (score >= 80) return 'score-circle-success';
    if (score >= 65) return 'score-circle-warning';
    return 'score-circle-danger';
  }

  getScoreCardClass(score: number): string {
    if (score >= 80) return 'bg-success-light';
    if (score >= 65) return 'bg-warning-light';
    return 'bg-danger-light';
  }

  getDecisionCardClass(status: string): string {
    switch (status) {
      case 'Accepted': return 'bg-success-light';
      case 'HRReview': return 'bg-warning-light';
      case 'Rejected': return 'bg-danger-light';
      default: return 'bg-light';
    }
  }

  getDecisionIcon(status: string): string {
    switch (status) {
      case 'Accepted': return 'fa-check-circle text-success';
      case 'HRReview': return 'fa-user-clock text-warning';
      case 'Rejected': return 'fa-times-circle text-danger';
      default: return 'fa-hourglass-half text-secondary';
    }
  }

  getDifficultyClass(difficulty: string): string {
    switch (difficulty) {
      case 'Easy': return 'bg-success';
      case 'Medium': return 'bg-primary';
      case 'Hard': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }

  getExperienceProgress(): number {
    if (!this.report?.experienceAnalysis) return 0;
    const { candidateYears, requiredYears } = this.report.experienceAnalysis;
    if (requiredYears === 0) return 100;
    return Math.min(100, (candidateYears / requiredYears) * 100);
  }

  backToDashboard(): void {
    this.router.navigate(['/admin']);
  }
}
