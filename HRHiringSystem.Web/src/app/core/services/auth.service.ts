import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { LoginRequest, ApiResponse, AuthResponse, User } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'auth_user';
  private currentUserSubject = new BehaviorSubject<User | null>(this.initializeUserFromStorage());

  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(credentials: LoginRequest): Observable<User> {
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => {
          if (!response.isSuccess || !response.data?.token || !response.data.userId) {
            throw new Error(response.message || 'Login failed');
          }

          const token = response.data.token;
          this.setToken(token);

          const claims = this.decodeTokenClaims(token);
          const role =
            claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
            claims['role'] ||
            '';
          const email =
            claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
            claims['email'] ||
            '';

          const user: User = {
            id: response.data.userId,
            name: response.data.userName || 'User',
            email,
            role
          };

          this.setUser(user);
          this.currentUserSubject.next(user);
        }),
        map(() => this.getCurrentUser() as User)
      );
  }

  logout(): Observable<any> {
    return this.http.post(`${environment.apiUrl}/auth/logout`, {})
      .pipe(
        tap(() => {
          this.clearAuth();
        })
      );
  }

  changePassword(oldPassword: string, newPassword: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${environment.apiUrl}/auth/change-password`, {
      oldPassword,
      newPassword
    });
  }

  private clearAuth(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  private setUser(user: User): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  private initializeUserFromStorage(): User | null {
    const token = this.getToken();
    if (!token) {
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.USER_KEY);
      return null;
    }

    const userFromStorage = this.getUserFromStorage();
    if (userFromStorage) {
      return userFromStorage;
    }

    const claims = this.decodeTokenClaims(token);
    const userId = claims['sub'] || '';
    if (!userId) {
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.USER_KEY);
      return null;
    }

    const role =
      claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      claims['role'] ||
      '';
    const email =
      claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
      claims['email'] ||
      '';

    const fallbackUser: User = {
      id: userId,
      name: email || 'User',
      email,
      role
    };

    this.setUser(fallbackUser);
    return fallbackUser;
  }

  private getUserFromStorage(): User | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    if (!userJson) {
      return null;
    }

    try {
      return JSON.parse(userJson) as User;
    } catch {
      localStorage.removeItem(this.USER_KEY);
      return null;
    }
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  forceLogout(): void {
    this.clearAuth();
  }

  private decodeTokenClaims(token: string): Record<string, string> {
    try {
      const payload = token.split('.')[1];
      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(
        atob(normalized)
          .split('')
          .map(c => `%${(`00${c.charCodeAt(0).toString(16)}`).slice(-2)}`)
          .join('')
      );
      return JSON.parse(json);
    } catch {
      return {};
    }
  }
}
