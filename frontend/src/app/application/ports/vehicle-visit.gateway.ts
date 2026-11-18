import { InjectionToken } from '@angular/core';
import {
  CreatedVehicleEntry,
  CreateVehicleEntry,
  QueueLookupResult,
  VehicleVisitSummary,
} from '../../domain/models/vehicle-visit';

export interface VehicleVisitGateway {
  list(): Promise<readonly VehicleVisitSummary[]>;
  getById(id: string): Promise<CreatedVehicleEntry | null>;
  lookupQueue(plateNumber: string): Promise<QueueLookupResult>;
  createEntry(command: CreateVehicleEntry): Promise<CreatedVehicleEntry>;
}

export const VEHICLE_VISIT_GATEWAY = new InjectionToken<VehicleVisitGateway>(
  'VEHICLE_VISIT_GATEWAY',
);
