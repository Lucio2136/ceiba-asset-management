import { SortDirection } from './common.models';

/*
 * DTOs TEMPORALES alineados con CeibaAssets.Contracts.Assets / Assignments.
 */

export const ASSET_TYPES = [
  'Computer',
  'Laptop',
  'Monitor',
  'Webcam',
  'Peripheral',
  'CiscoPhone',
  'MobileDevice',
] as const;
export type AssetType = (typeof ASSET_TYPES)[number];

export const ASSET_STATUSES = ['Available', 'Assigned', 'InMaintenance', 'Retired'] as const;
export type AssetStatus = (typeof ASSET_STATUSES)[number];

export const ASSET_TYPE_LABELS: Record<AssetType, string> = {
  Computer: 'Equipo de escritorio',
  Laptop: 'Laptop',
  Monitor: 'Monitor',
  Webcam: 'Cámara web',
  Peripheral: 'Periférico',
  CiscoPhone: 'Teléfono Cisco',
  MobileDevice: 'Dispositivo móvil',
};

export const ASSET_STATUS_LABELS: Record<AssetStatus, string> = {
  Available: 'Disponible',
  Assigned: 'Asignado',
  InMaintenance: 'En mantenimiento',
  Retired: 'Dado de baja',
};

export const ASSET_IDENTIFIER_KINDS = [
  'SerialNumber',
  'AssetTag',
  'MacAddress',
  'Imei',
  'HostName',
  'PhoneExtension',
  'Other',
] as const;
export type AssetIdentifierKind = (typeof ASSET_IDENTIFIER_KINDS)[number];

export const ASSET_IDENTIFIER_KIND_LABELS: Record<AssetIdentifierKind, string> = {
  SerialNumber: 'Número de serie',
  AssetTag: 'Etiqueta de activo',
  MacAddress: 'Dirección MAC',
  Imei: 'IMEI',
  HostName: 'Nombre de equipo',
  PhoneExtension: 'Extensión telefónica',
  Other: 'Otro',
};

/** Fila del inventario (`AssetListItemDto`). */
export interface AssetSummary {
  id: string;
  name: string;
  assetType: AssetType;
  status: AssetStatus;
  manufacturer?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  locationId?: string | null;
  locationName?: string | null;
  locationPath?: string | null;
  assignedToId?: string | null;
  assignedToName?: string | null;
}

export interface AssetIdentifier {
  id: string;
  kind: AssetIdentifierKind;
  value: string;
  isPrimary: boolean;
}

export interface CurrentAssignment {
  id: string;
  assetId: string;
  responsiblePersonId: string;
  responsiblePersonName: string;
  assignedAtUtc: string;
  returnedAtUtc?: string | null;
  assignedByUserId?: string | null;
  notes?: string | null;
}

/** Detalle (`AssetDto`). No incluye assignedTo* de la lista. */
export interface AssetDetail {
  id: string;
  name: string;
  assetType: AssetType;
  status: AssetStatus;
  manufacturer?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  notes?: string | null;
  locationId?: string | null;
  locationName?: string | null;
  locationPath?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  rowVersion: string;
  identifiers: AssetIdentifier[];
  currentAssignment?: CurrentAssignment | null;
}

export interface AssetListQuery {
  page: number;
  pageSize: number;
  search?: string;
  type?: AssetType;
  status?: AssetStatus;
  sortBy?: string;
  sortDirection?: SortDirection;
}

export interface SaveAssetRequest {
  assetType: AssetType;
  name: string;
  manufacturer?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  notes?: string | null;
  /** Solo en alta. En edición la ubicación se cambia con Mover. */
  locationId?: string | null;
  status?: AssetStatus | null;
  rowVersion?: string;
}

export interface AssignAssetRequest {
  assetId: string;
  responsiblePersonId: string;
  notes?: string | null;
}

export interface ReturnAssetRequest {
  assetId: string;
  notes?: string | null;
}

export interface MoveAssetRequest {
  assetId: string;
  toLocationId: string;
  reason?: string | null;
}

export type AssignmentEventKind = 'Assigned' | 'Returned' | 'Moved';

export const ASSIGNMENT_EVENT_LABELS: Record<AssignmentEventKind, string> = {
  Assigned: 'Asignación',
  Returned: 'Devolución',
  Moved: 'Movimiento',
};

/** Historial unificado en la UI (asignaciones + movimientos). */
export interface AssignmentEvent {
  id: string;
  kind: AssignmentEventKind;
  occurredAtUtc: string;
  responsibleName?: string | null;
  locationName?: string | null;
  notes?: string | null;
  performedBy?: string | null;
}
