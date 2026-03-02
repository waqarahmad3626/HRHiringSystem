import { Component, OnDestroy, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { finalize, timeout } from 'rxjs/operators';
import { JobService, JobApplicationService, CvParseService } from '../../../core/services';
import { Job, JobApplicationRequest, CvParseResponse } from '../../../core/models';

@Component({
  selector: 'app-job-detail',
  standalone: false,
  templateUrl: './job-detail.component.html',
  styleUrls: ['./job-detail.component.scss']
})
export class JobDetailComponent implements OnInit, OnDestroy {
  job: Job | null = null;
  loading = true;
  error = '';
  
  applicationForm: FormGroup;
  selectedFile: File | null = null;
  submitting = false;
  submitSuccess = false;
  submitError = '';
  submitMessage = '';
  
  // CV Parsing state
  parsing = false;
  parseError = '';
  parsedSkills: string[] = [];
  
  // Drag & Drop state
  isDragging = false;
  
  private loadingGuard: ReturnType<typeof setTimeout> | null = null;
  private routeParamSubscription: Subscription | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private jobService: JobService,
    private applicationService: JobApplicationService,
    private cvParseService: CvParseService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {
    this.applicationForm = this.fb.group({
      candidateFirstName: ['', [Validators.required, Validators.minLength(2)]],
      candidateLastName: ['', [Validators.required, Validators.minLength(2)]],
      candidateEmail: ['', [Validators.required, Validators.email]],
      candidatePhone: ['', [Validators.required, Validators.pattern(/^[0-9+\-\s()]+$/)]],
      cvFile: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    this.routeParamSubscription = this.route.paramMap.subscribe(params => {
      const jobId = params.get('id');
      if (jobId) {
        this.loadJob(jobId);
      } else {
        this.error = 'Invalid job selected.';
        this.loading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.stopLoadingGuard();
    this.routeParamSubscription?.unsubscribe();
  }

  loadJob(id: string): void {
    this.loading = true;
    this.error = '';
    this.startLoadingGuard('Job details are taking too long to load. Please go back and try again.');

    this.jobService.getJobById(id).pipe(
      timeout(15000),
      finalize(() => {
        this.ngZone.run(() => {
          this.stopLoadingGuard();
          this.loading = false;
          this.cdr.detectChanges();
        });
      })
    ).subscribe({
      next: (job: Job) => {
        this.ngZone.run(() => {
          this.job = job;
          this.cdr.detectChanges();
        });
      },
      error: (err: any) => {
        this.ngZone.run(() => {
          this.error = 'Failed to load job details.';
          console.error('Error loading job:', err);
          this.cdr.detectChanges();
        });
      }
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

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.handleFile(file);
    }
  }

  /**
   * Parse the CV using AI to extract candidate information
   * and auto-fill the form fields
   */
  private parseCV(file: File): void {
    this.parsing = true;
    this.parseError = '';
    this.parsedSkills = [];
    
    this.cvParseService.parse(file).subscribe({
      next: (result: CvParseResponse) => {
        this.parsing = false;
        
        if (result.success) {
          // Auto-fill form fields if values are found
          if (result.firstName) {
            this.applicationForm.patchValue({ candidateFirstName: result.firstName });
          }
          if (result.lastName) {
            this.applicationForm.patchValue({ candidateLastName: result.lastName });
          }
          if (result.email) {
            this.applicationForm.patchValue({ candidateEmail: result.email });
          }
          if (result.phone) {
            this.applicationForm.patchValue({ candidatePhone: result.phone });
          }
          
          // Store parsed skills for display
          this.parsedSkills = result.skills || [];
          
          this.cdr.detectChanges();
        } else {
          this.parseError = result.error || 'Unable to parse CV. Please fill in the form manually.';
        }
      },
      error: (err) => {
        this.parsing = false;
        this.parseError = 'CV parsing service unavailable. Please fill in the form manually.';
        console.error('CV parse error:', err);
        this.cdr.detectChanges();
      }
    });
  }

  // Drag & Drop event handlers
  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    if (!this.parsing) {
      this.isDragging = true;
    }
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
    
    if (this.parsing) return;
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFile(files[0]);
    }
  }

  private handleFile(file: File): void {
    const allowedTypes = ['application/pdf', 'application/msword', 
                         'application/vnd.openxmlformats-officedocument.wordprocessingml.document'];
    const maxSize = 10 * 1024 * 1024; // 10MB

    if (!allowedTypes.includes(file.type)) {
      this.applicationForm.get('cvFile')?.setErrors({ invalidType: true });
      this.applicationForm.get('cvFile')?.markAsTouched();
      this.selectedFile = null;
      return;
    }

    if (file.size > maxSize) {
      this.applicationForm.get('cvFile')?.setErrors({ tooLarge: true });
      this.applicationForm.get('cvFile')?.markAsTouched();
      this.selectedFile = null;
      return;
    }

    this.selectedFile = file;
    this.applicationForm.patchValue({ cvFile: file });
    this.applicationForm.get('cvFile')?.setErrors(null);
    
    // Auto-parse CV to extract candidate information
    this.parseCV(file);
  }

  removeFile(event: Event): void {
    event.stopPropagation();
    this.selectedFile = null;
    this.applicationForm.patchValue({ cvFile: null });
    this.parsedSkills = [];
    this.parseError = '';
  }

  getFileIcon(): string {
    if (!this.selectedFile) return 'fa-file';
    
    const type = this.selectedFile.type;
    if (type === 'application/pdf') {
      return 'fa-file-pdf';
    } else if (type === 'application/msword' || 
               type === 'application/vnd.openxmlformats-officedocument.wordprocessingml.document') {
      return 'fa-file-word';
    }
    return 'fa-file';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  submitApplication(): void {
    if (this.applicationForm.invalid || !this.selectedFile || !this.job) {
      Object.keys(this.applicationForm.controls).forEach(key => {
        this.applicationForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.submitting = true;
    this.submitError = '';

    const application: JobApplicationRequest = {
      candidateFirstName: this.applicationForm.value.candidateFirstName,
      candidateLastName: this.applicationForm.value.candidateLastName,
      candidateEmail: this.applicationForm.value.candidateEmail,
      candidatePhone: this.applicationForm.value.candidatePhone,
      jobId: this.job.id,
      cvFile: this.selectedFile
    };

    this.applicationService.submitApplication(application).subscribe({
      next: (response: any) => {
        this.submitSuccess = true;
        this.submitting = false;
        // Extract success message from server response
        this.submitMessage = response?.message || response?.Message || 'Application submitted successfully!';
        this.cdr.detectChanges();
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 3000);
      },
      error: (err: any) => {
        // Extract error message from server response
        const serverMessage = err.error?.message || err.error?.Message;
        if (serverMessage) {
          this.submitError = serverMessage;
        } else {
          this.submitError = 'Failed to submit application. Please try again.';
        }
        this.submitting = false;
        console.error('Error submitting application:', err);
        this.cdr.detectChanges();
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  get f() {
    return this.applicationForm.controls;
  }
}
