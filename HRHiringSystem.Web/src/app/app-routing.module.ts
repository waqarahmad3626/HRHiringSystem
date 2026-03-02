import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { JobListComponent } from './features/jobs/job-list/job-list.component';
import { JobDetailComponent } from './features/jobs/job-detail/job-detail.component';
import { LoginComponent } from './features/auth/login/login.component';
import { ChangePasswordComponent } from './features/auth/change-password/change-password.component';
import { AdminDashboardComponent } from './features/admin/dashboard/admin-dashboard.component';
import { ApplicationReviewComponent } from './features/admin/application-review/application-review.component';
import { CandidateDetailComponent } from './features/candidates/candidate-detail/candidate-detail.component';
import { AuthGuard } from './core/guards';

const routes: Routes = [
  { path: '', component: JobListComponent },
  { path: 'jobs/:id', component: JobDetailComponent },
  { path: 'login', component: LoginComponent },
  { path: 'admin', component: AdminDashboardComponent, canActivate: [AuthGuard] },
  { path: 'admin/applications/:jobId', component: ApplicationReviewComponent, canActivate: [AuthGuard] },
  { path: 'change-password', component: ChangePasswordComponent, canActivate: [AuthGuard] },
  { path: 'candidates/:id', component: CandidateDetailComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
