/*
 * DTO TEMPORAL alineado con ResponsiblePersonDto.
 */

export interface Responsible {
  id: string;
  employeeNumber: string;
  displayName: string;
  email: string;
  department?: string | null;
  isActive: boolean;
}

export interface ResponsibleListQuery {
  search?: string;
  includeInactive?: boolean;
  page?: number;
  pageSize?: number;
}
