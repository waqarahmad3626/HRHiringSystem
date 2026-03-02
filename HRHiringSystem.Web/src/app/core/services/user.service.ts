import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, AppUser, UserRequest } from '../models';

interface UserResponseDto {
  userId: string;
  userName: string;
  userEmail: string;
  userRoleId: string;
  userIsActive: boolean;
  userRole?: {
    roleId: string;
    roleName: string;
    roleDescription: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<AppUser[]> {
    return this.http.get<ApiResponse<UserResponseDto[]>>(this.apiUrl).pipe(
      map(response => (response.data || []).map(this.mapUser))
    );
  }

  createUser(request: UserRequest): Observable<AppUser> {
    return this.http.post<ApiResponse<UserResponseDto>>(this.apiUrl, request).pipe(
      map(response => this.mapUser(response.data as UserResponseDto))
    );
  }

  updateUser(userId: string, request: UserRequest): Observable<AppUser> {
    return this.http.put<ApiResponse<UserResponseDto>>(`${this.apiUrl}/${userId}`, request).pipe(
      map(response => this.mapUser(response.data as UserResponseDto))
    );
  }

  deleteUser(userId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${userId}`);
  }

  private mapUser(dto: UserResponseDto): AppUser {
    return {
      id: dto.userId,
      name: dto.userName,
      email: dto.userEmail,
      roleId: dto.userRoleId,
      roleName: dto.userRole?.roleName,
      isActive: dto.userIsActive
    };
  }
}
