import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../services';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Only redirect to login if we're not already authenticated
          // This prevents redirecting when a background API call fails
          const currentUrl = this.router.url;
          const isOnPublicPage = currentUrl === '/' || 
                                  currentUrl === '/login' || 
                                  currentUrl.startsWith('/jobs/');
          
          if (isOnPublicPage || !this.authService.getToken()) {
            this.authService.forceLogout();
            this.router.navigate(['/login']);
          }
          // For authenticated pages, let the component handle the 401 error
        }
        return throwError(() => error);
      })
    );
  }
}
