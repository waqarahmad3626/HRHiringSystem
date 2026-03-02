export interface LoginRequest {
  email: string;
  password: string;
}

export interface ApiResponse<T> {
  isSuccess: boolean;
  errorCode?: string;
  message?: string;
  data?: T;
}

export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
}

export interface AuthResponse {
  userId: string;
  userName?: string;
  token?: string;
  expiresInSeconds: number;
}
