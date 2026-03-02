import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../core/services';
import { User } from '../../core/models';

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  isAdminPage = false;
  activeAdminTab: 'activeJobs' | 'jobs' | 'users' | 'roles' = 'activeJobs';
  private routerEventsSubscription: Subscription | null = null;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });

    this.updateAdminNavState(this.router.url);
    this.routerEventsSubscription = this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.updateAdminNavState(event.urlAfterRedirects);
      }
    });
  }

  ngOnDestroy(): void {
    this.routerEventsSubscription?.unsubscribe();
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: () => {
        this.authService.forceLogout();
        this.router.navigate(['/']);
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  goToHome(): void {
    this.router.navigate(['/']);
  }

  goToDashboard(): void {
    this.router.navigate(['/admin'], { queryParams: { tab: 'activeJobs' } });
  }

  goToDashboardTab(tab: 'activeJobs' | 'jobs' | 'users' | 'roles'): void {
    this.router.navigate(['/admin'], { queryParams: { tab } });
  }

  isAdmin(): boolean {
    return (this.currentUser?.role || '').toLowerCase() === 'admin';
  }

  isHrOrAdmin(): boolean {
    const role = (this.currentUser?.role || '').toLowerCase();
    return role === 'admin' || role === 'hr';
  }

  goToChangePassword(): void {
    this.router.navigate(['/change-password']);
  }

  getInitials(): string {
    if (!this.currentUser?.name) {
      return 'U';
    }

    return this.currentUser.name
      .split(' ')
      .map(part => part[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  }

  private updateAdminNavState(url: string): void {
    this.isAdminPage = url.startsWith('/admin');
    if (!this.isAdminPage) {
      return;
    }

    const queryPart = url.split('?')[1] || '';
    const params = new URLSearchParams(queryPart);
    const tab = params.get('tab');
    this.activeAdminTab =
      tab === 'jobs' || tab === 'users' || tab === 'roles' || tab === 'activeJobs'
        ? tab
        : 'activeJobs';
  }
}
