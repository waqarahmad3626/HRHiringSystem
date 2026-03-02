import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services';

@Component({
  selector: 'app-change-password',
  standalone: false,
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent {
  form: FormGroup;
  loading = false;
  error = '';
  success = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.form = this.fb.group({
      oldPassword: ['', [Validators.required, Validators.minLength(6)]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.form.value.newPassword !== this.form.value.confirmPassword) {
      this.error = 'New password and confirm password must match';
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = '';

    this.authService.changePassword(this.form.value.oldPassword, this.form.value.newPassword).subscribe({
      next: response => {
        this.loading = false;
        if (response.isSuccess) {
          this.success = response.message || 'Password changed successfully';
          this.form.reset();
        } else {
          this.error = response.message || 'Failed to change password';
        }
      },
      error: err => {
        this.loading = false;
        this.error = err.error?.message || 'Failed to change password';
      }
    });
  }

  goToDashboard(): void {
    this.router.navigate(['/admin']);
  }
}
