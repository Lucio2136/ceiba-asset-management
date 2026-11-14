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
