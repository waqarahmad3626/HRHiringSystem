export interface AppUser {
  id: string;
  name: string;
  email: string;
  roleId: string;
  roleName?: string;
  isActive: boolean;
}

export interface UserRequest {
  userName: string;
  userEmail: string;
  userPassword: string;
  userRoleId: string;
}
