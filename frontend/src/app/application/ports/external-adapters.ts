import { InjectionToken } from '@angular/core';
import { ControlResultSummary, GateMode } from '../../domain/models/vehicle-visit';
import { Permission } from '../../domain/permissions/permission';

export interface AuthenticatedUserSummary {
  readonly id: string;
  readonly displayName: string;
  readonly roles: readonly string[];
  readonly permissions: readonly Permission[];
  readonly checkpointIds: readonly string[];
  readonly authenticationMethod: string;
  readonly idleTimeoutMinutes: number;
}

export interface CameraRecognitionSummary {
  readonly id: string;
  readonly cameraId: string;
  readonly location: string;
  readonly capturedAt: string;
  readonly plateNumber: string;
  readonly photoReference: string | null;
  readonly expiresAt: string;
}

export interface QueueEntrySummary {
  readonly id: string;
  readonly checkpointId: string;
  readonly plateNumber: string;
  readonly externalStatus: string;
  readonly lastSynchronizedAt: string;
}

export interface AuthAdapter {
  getCurrentSession(): Promise<AuthenticatedUserSummary | null>;
  signIn(): Promise<AuthenticatedUserSummary>;
  signOut(): Promise<void>;
  recordActivity(): Promise<void>;
}

export interface VmsAdapter {
  listRecognitions(mode: GateMode): Promise<readonly CameraRecognitionSummary[]>;
  markProcessed(recognitionId: string): Promise<void>;
}

export interface EQueueAdapter {
  findByPlate(checkpointId: string, plateNumber: string): Promise<QueueEntrySummary | null>;
  setPaused(paused: boolean): Promise<void>;
}

export interface CustomsAdapter {
  getControlResult(visitId: string): Promise<ControlResultSummary | null>;
}

export interface BorderControlAdapter {
  getControlResult(visitId: string): Promise<ControlResultSummary | null>;
}

export const AUTH_ADAPTER = new InjectionToken<AuthAdapter>('AUTH_ADAPTER');
export const VMS_ADAPTER = new InjectionToken<VmsAdapter>('VMS_ADAPTER');
export const EQUEUE_ADAPTER = new InjectionToken<EQueueAdapter>('EQUEUE_ADAPTER');
export const CUSTOMS_ADAPTER = new InjectionToken<CustomsAdapter>('CUSTOMS_ADAPTER');
export const BORDER_CONTROL_ADAPTER = new InjectionToken<BorderControlAdapter>(
  'BORDER_CONTROL_ADAPTER',
);
