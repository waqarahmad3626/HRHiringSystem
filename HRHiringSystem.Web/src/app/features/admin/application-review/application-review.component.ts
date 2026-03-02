import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, forkJoin } from 'rxjs';
import { 
  JobApplicationService, 
  JobService, 
  EvaluationReportService 
} from '../../../core/services';
import { 
  Job, 
  JobApplication, 
  EvaluationReport, 
  ApplicationStatus 
} from '../../../core/models';

interface ApplicationWithReport extends JobApplication {
  report?: EvaluationReport;
}

@Component({
  selector: 'app-application-review',
  standalone: false,
  templateUrl: './application-review.component.html',
  styleUrls: ['./application-review.component.scss']
})
export class ApplicationReviewComponent implements OnInit, OnDestroy {
  job: Job | null = null;
  applications: ApplicationWithReport[] = [];
  selectedApplication: ApplicationWithReport | null = null;
  
  loading = true;
  loadingReport = false;
  error = '';
  
  // Filters
  statusFilter: ApplicationStatus | 'All' = 'All';
  scoreRange: 'All' | 'High' | 'Medium' | 'Low' = 'All';
  
  // HR Actions
  hrNotes = '';
  schedulingInterview = false;
  interviewDate: string = '';
  interviewTime: string = '';
  savingNotes = false;
  
  private routeSubscription: Subscription | null = null;

  statusOptions: Array<{value: ApplicationStatus | 'All', label: string}> = [
    { value: 'All', label: 'All Statuses' },
    { value: 'Pending', label: 'Pending' },
    { value: 'Processing', label: 'Processing' },
    { value: 'Accepted', label: 'Accepted' },
    { value: 'HRReview', label: 'HR Review' },
    { value: 'Rejected', label: 'Rejected' },
    { value: 'InterviewScheduled', label: 'Interview Scheduled' },
    { value: 'Hired', label: 'Hired' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private jobService: JobService,
    private applicationService: JobApplicationService,
    private reportService: EvaluationReportService
  ) {}

  ngOnInit(): void {
    this.routeSubscription = this.route.paramMap.subscribe(params => {
      const jobId = params.get('jobId');
      if (jobId) {
        this.loadJobAndApplications(jobId);
      } else {
        this.error = 'No job selected';
        this.loading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.routeSubscription?.unsubscribe();
  }

  loadJobAndApplications(jobId: string): void {
    this.loading = true;
    this.error = '';
    
    forkJoin({
      job: this.jobService.getJobById(jobId),
      applications: this.applicationService.getApplicationsByJob(jobId)
    }).subscribe({
      next: ({ job, applications }) => {
        this.job = job;
        this.applications = applications.map(app => ({ ...app }));
        this.loading = false;
        
        // Auto-select first application if any
        if (this.applications.length > 0) {
          this.selectApplication(this.applications[0]);
        }
      },
      error: (err) => {
        this.error = 'Failed to load job data';
        this.loading = false;
        console.error('Error loading job data:', err);
      }
    });
  }

  selectApplication(application: ApplicationWithReport): void {
    this.selectedApplication = application;
    this.hrNotes = application.hrNotes || '';
    
    // Load evaluation report if available
    if (application.mongoReportId && this.job) {
      this.loadReport(application.mongoReportId, this.job.id);
    }
  }

  loadReport(reportId: string, jobId: string): void {
    if (!this.selectedApplication) return;
    
    this.loadingReport = true;
    
    this.reportService.getReportById(reportId, jobId).subscribe({
      next: (report) => {
        if (this.selectedApplication) {
          this.selectedApplication.report = report;
        }
        this.loadingReport = false;
      },
      error: (err) => {
        console.error('Error loading report:', err);
        this.loadingReport = false;
      }
    });
  }

  get filteredApplications(): ApplicationWithReport[] {
    return this.applications.filter(app => {
      // Status filter
      if (this.statusFilter !== 'All' && app.status !== this.statusFilter) {
        return false;
      }
      
      // Score filter
      if (this.scoreRange !== 'All' && app.score !== undefined) {
        if (this.scoreRange === 'High' && app.score < 80) return false;
        if (this.scoreRange === 'Medium' && (app.score < 65 || app.score >= 80)) return false;
        if (this.scoreRange === 'Low' && app.score >= 65) return false;
      }
      
      return true;
    });
  }

  getStatusClass(status: ApplicationStatus): string {
    const classes: Record<ApplicationStatus, string> = {
      'Pending': 'bg-secondary',
      'Processing': 'bg-info',
      'Accepted': 'bg-success',
      'HRReview': 'bg-warning',
      'Rejected': 'bg-danger',
      'InterviewScheduled': 'bg-primary',
      'Hired': 'bg-success'
    };
    return classes[status] || 'bg-secondary';
  }

  getRowBackgroundClass(status: ApplicationStatus): string {
    if (status === 'Accepted' || status === 'Hired') {
      return 'list-group-item-success';
    }
    if (status === 'Rejected') {
      return 'list-group-item-danger';
    }
    // HRReview, Pending, Processing, InterviewScheduled - white background
    return '';
  }

  getScoreClass(score: number | undefined): string {
    if (score === undefined) return 'text-muted';
    if (score >= 80) return 'text-success';
    if (score >= 65) return 'text-warning';
    return 'text-danger';
  }

  saveHRNotes(): void {
    if (!this.selectedApplication) return;
    
    this.savingNotes = true;
    
    this.applicationService.updateHRNotes(this.selectedApplication.id, this.hrNotes).subscribe({
      next: () => {
        if (this.selectedApplication) {
          this.selectedApplication.hrNotes = this.hrNotes;
        }
        this.savingNotes = false;
      },
      error: (err) => {
        console.error('Error saving notes:', err);
        this.savingNotes = false;
      }
    });
  }

  scheduleInterview(): void {
    if (!this.selectedApplication || !this.interviewDate || !this.interviewTime) return;
    
    const scheduledAt = new Date(`${this.interviewDate}T${this.interviewTime}`);
    this.schedulingInterview = true;
    
    this.applicationService.scheduleInterview(this.selectedApplication.id, scheduledAt).subscribe({
      next: () => {
        if (this.selectedApplication) {
          this.selectedApplication.status = 'InterviewScheduled';
          this.selectedApplication.interviewScheduledAt = scheduledAt;
        }
        this.schedulingInterview = false;
        this.interviewDate = '';
        this.interviewTime = '';
      },
      error: (err) => {
        console.error('Error scheduling interview:', err);
        this.schedulingInterview = false;
      }
    });
  }

  downloadCV(): void {
    if (this.selectedApplication?.cvUrl) {
      window.open(this.selectedApplication.cvUrl, '_blank');
    }
  }

  goBack(): void {
    this.router.navigate(['/admin']);
  }
}
