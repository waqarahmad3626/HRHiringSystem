import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize, timeout } from 'rxjs/operators';
import {
  AuthService,
  JobApplicationService,
  JobService,
  RoleService,
  UserService
} from '../../../core/services';
import { AppUser, Job, JobApplication, Role, User } from '../../../core/models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: false,
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  currentUser: User | null = null;
  activeMenu: 'activeJobs' | 'jobs' | 'users' | 'roles' = 'activeJobs';

  jobs: Job[] = [];
  applications: JobApplication[] = [];
  users: AppUser[] = [];
  roles: Role[] = [];

  selectedActiveJobId: string | null = null;

  loadingJobs = false;
  loadingUsers = false;
  loadingRoles = false;
  savingUser = false;
  private loadingJobsGuard: ReturnType<typeof setTimeout> | null = null;

  jobForm: FormGroup;
  userForm: FormGroup;
  roleForm: FormGroup;

  editingJobId: string | null = null;
  editingUserId: string | null = null;
  editingRoleId: string | null = null;

  message = '';
  error = '';

  constructor(
    private authService: AuthService,
    private jobService: JobService,
    private jobApplicationService: JobApplicationService,
    private userService: UserService,
    private roleService: RoleService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {
    this.jobForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      requirements: ['', [Validators.required, Validators.minLength(10)]],
      isActive: [true]
    });

    this.userForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      roleId: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
    }, { validators: this.passwordsMatchValidator() });

    this.roleForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: ['', [Validators.required, Validators.minLength(3)]]
    });
  }

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();

    this.route.queryParamMap.subscribe(params => {
      const tab = params.get('tab');
      const nextMenu: 'activeJobs' | 'jobs' | 'users' | 'roles' =
        tab === 'jobs' || tab === 'users' || tab === 'roles' || tab === 'activeJobs'
          ? tab
          : 'activeJobs';

      this.activeMenu = nextMenu;

      if (!this.roles.length && !this.loadingRoles) {
        this.loadRoles();
      }

      if (nextMenu === 'activeJobs') {
        this.loadJobsWithApplicationCounts();
      }

      if (nextMenu === 'jobs' && !this.jobs.length && !this.loadingJobs) {
        this.loadJobsWithApplicationCounts();
      }

      if (this.isAdmin() && nextMenu === 'users' && !this.users.length && !this.loadingUsers) {
        this.loadUsers();
      }

      if (nextMenu === 'activeJobs' && !this.selectedActiveJobId && this.activeJobs.length > 0) {
        this.selectedActiveJobId = this.activeJobs[0].id;
      }
    });
  }

  isAdmin(): boolean {
    return (this.currentUser?.role || '').toLowerCase() === 'admin';
  }

  isHrOrAdmin(): boolean {
    const role = (this.currentUser?.role || '').toLowerCase();
    return role === 'admin' || role === 'hr';
  }

  selectMenu(menu: 'activeJobs' | 'jobs' | 'users' | 'roles'): void {
    this.message = '';
    this.error = '';

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: menu },
      queryParamsHandling: 'merge'
    });
  }

  loadJobsWithApplicationCounts(): void {
    this.loadingJobs = true;
    this.startJobsLoadingGuard();

    forkJoin({
      jobs: this.jobService.getAllJobs().pipe(
        timeout(15000),
        catchError(err => {
          console.error('DEBUG - Jobs error:', err);
          this.error = err.error?.message || 'Failed to load jobs';
          return of([] as Job[]);
        })
      ),
      applications: this.jobApplicationService.getAllApplications().pipe(
        timeout(15000),
        catchError(err => {
          console.error('DEBUG - Applications error:', err);
          return of([] as JobApplication[]);
        })
      )
    }).pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.stopJobsLoadingGuard();
          this.loadingJobs = false;
          this.cdr.detectChanges();
        });
      })
    ).subscribe(({ jobs, applications }) => {
      this.ngZone.run(() => {
        this.applications = applications;

        console.log('DEBUG - Jobs:', jobs.map(j => ({ id: j.id, title: j.title })));
        console.log('DEBUG - Applications:', applications.map(a => ({ id: a.id, jobId: a.jobId })));

        const counts = applications.reduce((acc, application) => {
          acc[application.jobId] = (acc[application.jobId] || 0) + 1;
          return acc;
        }, {} as Record<string, number>);

        console.log('DEBUG - Counts by jobId:', counts);

        this.jobs = jobs.map(job => ({
          ...job,
          applicationsCount: counts[job.id] || 0
        }));

        if (!this.selectedActiveJobId || !this.activeJobs.some(job => job.id === this.selectedActiveJobId)) {
          this.selectedActiveJobId = this.activeJobs.length > 0 ? this.activeJobs[0].id : null;
        }
        this.cdr.detectChanges();
      });
    });
  }

  private startJobsLoadingGuard(): void {
    this.stopJobsLoadingGuard();
    this.loadingJobsGuard = setTimeout(() => {
      if (this.loadingJobs) {
        this.loadingJobs = false;
        if (!this.error) {
          this.error = 'Jobs are taking too long to load. Please refresh and try again.';
        }
      }
    }, 20000);
  }

  private stopJobsLoadingGuard(): void {
    if (this.loadingJobsGuard) {
      clearTimeout(this.loadingJobsGuard);
      this.loadingJobsGuard = null;
    }
  }

  saveJob(): void {
    this.message = '';
    this.error = '';

    if (this.jobForm.invalid || !this.currentUser) {
      this.jobForm.markAllAsTouched();
      this.error = this.currentUser ? 'Please fix the highlighted validation errors.' : 'User session is missing. Please login again.';
      return;
    }

    const payload: Job = {
      id: this.editingJobId || '',
      title: this.jobForm.value.title,
      description: this.jobForm.value.description,
      requirements: this.jobForm.value.requirements,
      isActive: this.jobForm.value.isActive,
      createdByHrId: this.currentUser.id
    };

    const request$ = this.editingJobId
      ? this.jobService.updateJob(payload)
      : this.jobService.createJob(payload);

    request$.subscribe({
      next: () => {
        this.message = this.editingJobId ? 'Job updated successfully' : 'Job created successfully';
        this.resetJobForm();
        this.loadJobsWithApplicationCounts();
      },
      error: err => {
        this.error = err.error?.message || 'Failed to save job';
      }
    });
  }

  editJob(job: Job): void {
    this.editingJobId = job.id;
    this.jobForm.patchValue({
      title: job.title,
      description: job.description,
      requirements: job.requirements,
      isActive: job.isActive
    });
  }

  toggleJobStatus(job: Job): void {
    const updated: Job = {
      ...job,
      isActive: !job.isActive
    };

    this.jobService.updateJob(updated).subscribe({
      next: () => this.loadJobsWithApplicationCounts(),
      error: err => {
        this.error = err.error?.message || 'Failed to update job status';
      }
    });
  }

  deleteJob(jobId: string): void {
    this.jobService.deleteJob(jobId).subscribe({
      next: () => {
        this.message = 'Job deleted successfully';
        this.loadJobsWithApplicationCounts();
      },
      error: err => {
        this.error = err.error?.message || 'Failed to delete job';
      }
    });
  }

  get activeJobs(): Job[] {
    return this.jobs.filter(job => job.isActive);
  }

  get selectedActiveJob(): Job | null {
    if (!this.selectedActiveJobId) {
      return null;
    }

    return this.activeJobs.find(job => job.id === this.selectedActiveJobId) || null;
  }

  get selectedActiveJobApplications(): JobApplication[] {
    if (!this.selectedActiveJobId) {
      return [];
    }

    return this.applications.filter(application => application.jobId === this.selectedActiveJobId);
  }

  selectActiveJob(jobId: string): void {
    this.selectedActiveJobId = jobId;
  }

  viewCandidateDetails(application: JobApplication): void {
    if (!application.candidateId) {
      return;
    }

    this.router.navigate(['/candidates', application.candidateId]);
  }

  getCandidateDisplayName(application: JobApplication): string {
    const firstName = application.candidate?.firstName?.trim() || '';
    const lastName = application.candidate?.lastName?.trim() || '';
    const fullName = `${firstName} ${lastName}`.trim();

    return fullName || application.candidateId;
  }

  getApplicationRowClass(status: string): string {
    switch (status) {
      case 'Accepted':
        return 'table-success';
      case 'HRReview':
        return 'table-warning';
      case 'Rejected':
        return 'table-danger';
      default:
        return '';
    }
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Accepted':
        return 'bg-success';
      case 'HRReview':
        return 'bg-warning text-dark';
      case 'Rejected':
        return 'bg-danger';
      case 'Processing':
        return 'bg-info';
      default:
        return 'bg-secondary';
    }
  }

  getScoreTextClass(score: number): string {
    if (score >= 80) return 'text-success';
    if (score >= 65) return 'text-warning';
    return 'text-danger';
  }

  resetJobForm(): void {
    this.editingJobId = null;
    this.jobForm.reset({
      title: '',
      description: '',
      requirements: '',
      isActive: true
    });
  }

  loadUsers(): void {
    this.loadingUsers = true;
    this.error = '';
    this.userService.getAllUsers().pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.loadingUsers = false;
          this.cdr.detectChanges();
        });
      })
    ).subscribe({
      next: users => {
        this.ngZone.run(() => {
          this.users = users;
          this.cdr.detectChanges();
        });
      },
      error: err => {
        this.ngZone.run(() => {
          this.error = err.error?.message || 'Failed to load users';
          this.cdr.detectChanges();
        });
      }
    });
  }

  saveUser(): void {
    this.message = '';
    this.error = '';

    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      this.error = 'Please fix the highlighted validation errors.';
      return;
    }

    const request = {
      userName: this.userForm.value.name,
      userEmail: this.userForm.value.email,
      userPassword: this.userForm.value.password,
      userRoleId: this.userForm.value.roleId
    };

    const request$ = this.editingUserId
      ? this.userService.updateUser(this.editingUserId, request)
      : this.userService.createUser(request);

    this.savingUser = true;
    request$.pipe(
      finalize(() => {
        this.savingUser = false;
      })
    ).subscribe({
      next: () => {
        this.message = this.editingUserId ? 'User updated successfully' : 'User created successfully';
        this.resetUserForm();
        this.loadUsers();
      },
      error: err => {
        this.error = err.error?.message || 'Failed to save user';
      }
    });
  }

  editUser(user: AppUser): void {
    this.editingUserId = user.id;
    this.userForm.patchValue({
      name: user.name,
      email: user.email,
      roleId: user.roleId,
      password: '',
      confirmPassword: ''
    });
  }

  deleteUser(userId: string): void {
    this.userService.deleteUser(userId).subscribe({
      next: () => {
        this.message = 'User deleted successfully';
        this.loadUsers();
      },
      error: err => {
        this.error = err.error?.message || 'Failed to delete user';
      }
    });
  }

  resetUserForm(): void {
    this.editingUserId = null;
    this.userForm.reset({
      name: '',
      email: '',
      roleId: '',
      password: '',
      confirmPassword: ''
    });
    this.userForm.markAsPristine();
    this.userForm.markAsUntouched();
  }

  hasUserError(controlName: string, errorName?: string): boolean {
    const control = this.userForm.get(controlName);
    if (!control) return false;

    const touched = control.touched || control.dirty;
    if (!touched) return false;

    return errorName ? !!control.getError(errorName) : control.invalid;
  }

  hasJobError(controlName: string, errorName?: string): boolean {
    const control = this.jobForm.get(controlName);
    if (!control) return false;

    const touched = control.touched || control.dirty;
    if (!touched) return false;

    return errorName ? !!control.getError(errorName) : control.invalid;
  }

  hasPasswordMismatchError(): boolean {
    const passwordControl = this.userForm.get('password');
    const confirmPasswordControl = this.userForm.get('confirmPassword');
    const interacted = !!passwordControl && !!confirmPasswordControl &&
      (passwordControl.touched || passwordControl.dirty || confirmPasswordControl.touched || confirmPasswordControl.dirty);

    return interacted && this.userForm.hasError('passwordMismatch');
  }

  private passwordsMatchValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const password = control.get('password')?.value;
      const confirmPassword = control.get('confirmPassword')?.value;

      if (!password || !confirmPassword) {
        return null;
      }

      return password === confirmPassword ? null : { passwordMismatch: true };
    };
  }

  loadRoles(): void {
    this.loadingRoles = true;
    this.roleService.getAllRoles().subscribe({
      next: roles => {
        this.ngZone.run(() => {
          this.roles = roles;
          this.loadingRoles = false;
          this.cdr.detectChanges();
        });
      },
      error: err => {
        this.ngZone.run(() => {
          this.error = err.error?.message || 'Failed to load roles';
          this.loadingRoles = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  saveRole(): void {
    this.message = '';
    this.error = '';

    if (this.roleForm.invalid) {
      this.roleForm.markAllAsTouched();
      this.error = 'Please fix the highlighted validation errors.';
      return;
    }

    const request = {
      roleName: this.roleForm.value.name,
      roleDescription: this.roleForm.value.description
    };

    const request$ = this.editingRoleId
      ? this.roleService.updateRole(this.editingRoleId, request)
      : this.roleService.createRole(request);

    request$.subscribe({
      next: () => {
        this.message = this.editingRoleId ? 'Role updated successfully' : 'Role created successfully';
        this.resetRoleForm();
        this.loadRoles();
        if (this.isAdmin()) {
          this.loadUsers();
        }
      },
      error: err => {
        this.error = err.error?.message || 'Failed to save role';
      }
    });
  }

  editRole(role: Role): void {
    this.editingRoleId = role.id;
    this.roleForm.patchValue({
      name: role.name,
      description: role.description
    });
  }

  deleteRole(roleId: string): void {
    this.roleService.deleteRole(roleId).subscribe({
      next: () => {
        this.message = 'Role deleted successfully';
        this.loadRoles();
      },
      error: err => {
        this.error = err.error?.message || 'Failed to delete role';
      }
    });
  }

  resetRoleForm(): void {
    this.editingRoleId = null;
    this.roleForm.reset({
      name: '',
      description: ''
    });
  }

  hasRoleError(controlName: string, errorName?: string): boolean {
    const control = this.roleForm.get(controlName);
    if (!control) return false;

    const touched = control.touched || control.dirty;
    if (!touched) return false;

    return errorName ? !!control.getError(errorName) : control.invalid;
  }
}
