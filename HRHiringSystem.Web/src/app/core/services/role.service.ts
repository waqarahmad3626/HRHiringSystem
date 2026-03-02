import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, Role, RoleRequest } from '../models';

interface RoleResponseDto {
  roleId: string;
  roleName: string;
  roleDescription: string;
}

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private apiUrl = `${environment.apiUrl}/roles`;

  constructor(private http: HttpClient) {}

  getAllRoles(): Observable<Role[]> {
    return this.http.get<ApiResponse<RoleResponseDto[]>>(this.apiUrl).pipe(
      map(response => (response.data || []).map(this.mapRole))
    );
  }

  createRole(request: RoleRequest): Observable<Role> {
    return this.http.post<ApiResponse<RoleResponseDto>>(this.apiUrl, request).pipe(
      map(response => this.mapRole(response.data as RoleResponseDto))
    );
  }

  updateRole(roleId: string, request: RoleRequest): Observable<Role> {
    return this.http.put<ApiResponse<RoleResponseDto>>(`${this.apiUrl}/${roleId}`, request).pipe(
      map(response => this.mapRole(response.data as RoleResponseDto))
    );
  }

  deleteRole(roleId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${roleId}`);
  }

  private mapRole(dto: RoleResponseDto): Role {
    return {
      id: dto.roleId,
      name: dto.roleName,
      description: dto.roleDescription
    };
  }
}
