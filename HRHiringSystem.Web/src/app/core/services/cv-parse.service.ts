import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, CvParseResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CvParseService {
  private apiUrl = `${environment.apiUrl}/cv`;

  constructor(private http: HttpClient) {}

  /**
   * Parse a CV file and extract candidate information
   * @param cvFile The CV file to parse (PDF, DOC, or DOCX)
   * @returns Observable with parsed CV data
   */
  parse(cvFile: File): Observable<CvParseResponse> {
    const formData = new FormData();
    formData.append('file', cvFile, cvFile.name);

    return this.http.post<ApiResponse<CvParseResponse>>(`${this.apiUrl}/parse`, formData).pipe(
      map(response => {
        const data = response.data as CvParseResponse;
        return {
          success: data?.success ?? false,
          firstName: data?.firstName,
          lastName: data?.lastName,
          email: data?.email,
          phone: data?.phone,
          skills: data?.skills ?? [],
          education: data?.education,
          yearsOfExperience: data?.yearsOfExperience,
          rawText: data?.rawText,
          error: data?.error
        };
      })
    );
  }
}
