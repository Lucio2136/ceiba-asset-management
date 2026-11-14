/*
 * DTOs TEMPORALES alineados con CeibaAssets.Contracts.Locations.
 */

export interface Building {
  id: string;
  code: string;
  name: string;
  levelCount: number;
  isActive: boolean;
}

export interface Floor {
  id: string;
  buildingId: string;
  levelNumber: number;
  name: string;
  isActive: boolean;
}

export interface Location {
  id: string;
  floorId: string;
  floorName: string;
  levelNumber: number;
  buildingId: string;
  buildingName: string;
  code: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  rowVersion: string;
}

export interface LocationListQuery {
  buildingId?: string;
  floorId?: string;
  search?: string;
  includeInactive?: boolean;
}
