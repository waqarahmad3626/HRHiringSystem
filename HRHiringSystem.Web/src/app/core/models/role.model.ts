export interface Role {
  id: string;
  name: string;
  description: string;
}

export interface RoleRequest {
  roleName: string;
  roleDescription: string;
}
