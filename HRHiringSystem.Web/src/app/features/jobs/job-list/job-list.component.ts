import { Component, OnDestroy, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { of } from 'rxjs';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { JobService } from '../../../core/services';
import { Job } from '../../../core/models';

@Component({
  selector: 'app-job-list',
  standalone: false,
  templateUrl: './job-list.component.html',
  styleUrls: ['./job-list.component.scss']
})
export class JobListComponent implements OnInit, OnDestroy {
  jobs: Job[] = [];
  filteredJobs: Job[] = [];
  loading = true;
  error = '';
  searchTerm = '';
  private loadingGuard: ReturnType<typeof setTimeout> | null = null;
  private routeEventsSubscription: Subscription | null = null;

  constructor(
    private jobService: JobService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.loadJobs();

    this.routeEventsSubscription = this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd && event.urlAfterRedirects === '/') {
        this.loadJobs();
      }
    });
  }

  ngOnDestroy(): void {
    this.stopLoadingGuard();
    this.routeEventsSubscription?.unsubscribe();
  }

  loadJobs(): void {
    this.loading = true;
    this.error = '';
    this.startLoadingGuard('Job list is taking too long to load. Please refresh and try again.');

    this.jobService.getActiveJobs().pipe(
      timeout(15000),
      catchError((err: any) => {
        this.error = 'Failed to load jobs. Please try again later.';
        console.error('Error loading jobs:', err);
        return of([] as Job[]);
      }),
      finalize(() => {
        this.ngZone.run(() => {
          this.stopLoadingGuard();
          this.loading = false;
          this.cdr.detectChanges();
        });
      })
    ).subscribe((jobs: Job[]) => {
      this.ngZone.run(() => {
        this.jobs = jobs;
        this.filteredJobs = jobs;
        this.cdr.detectChanges();
      });
    });
  }

  private startLoadingGuard(message: string): void {
    this.stopLoadingGuard();
    this.loadingGuard = setTimeout(() => {
      if (this.loading) {
        this.loading = false;
        if (!this.error) {
          this.error = message;
        }
      }
    }, 20000);
  }

  private stopLoadingGuard(): void {
    if (this.loadingGuard) {
      clearTimeout(this.loadingGuard);
      this.loadingGuard = null;
    }
  }

  applyFilters(): void {
    this.filteredJobs = this.jobs.filter(job => {
      const matchesSearch = !this.searchTerm || 
        job.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        job.description.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        job.requirements.toLowerCase().includes(this.searchTerm.toLowerCase());

      return matchesSearch;
    });
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.applyFilters();
  }

  viewJobDetails(jobId: string): void {
    this.router.navigate(['/jobs', jobId]);
  }

}
