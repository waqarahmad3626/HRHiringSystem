export interface Job {
  id: string;
  title: string;
  description: string;
  requirements: string;
  isActive: boolean;
  createdByHrId: string;
  applicationsCount?: number;
}
