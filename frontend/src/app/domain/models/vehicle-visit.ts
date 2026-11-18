export const vehicleVisitStatuses = [
  'entryDenied',
  'awaitingControls',
  'controlFailed',
  'awaitingExit',
  'passed',
  'return',
  'cancelled',
] as const;

export type VehicleVisitStatus = (typeof vehicleVisitStatuses)[number];

export const controlKinds = ['vehicleInspection', 'border', 'customs'] as const;
export type ControlKind = (typeof controlKinds)[number];

export const controlStatuses = ['pending', 'passed', 'failed', 'notRequired', 'cancelled'] as const;
export type ControlStatus = (typeof controlStatuses)[number];

export const gateModes = ['rearExit', 'borderExit', 'borderEntry', 'rearEntry'] as const;
export type GateMode = (typeof gateModes)[number];

export type MovementDirection = 'entry' | 'exit';
export type VehicleEntryDecision = 'allow' | 'deny';

export interface QueueEntrySummary {
  readonly id: string;
  readonly plateNumber: string;
  readonly externalStatus: string;
  readonly lastSynchronizedAt: string;
}

export interface QueueLookupResult {
  readonly plateNumber: string;
  readonly found: boolean;
  readonly entry: QueueEntrySummary | null;
}

export interface CreateVehicleEntry {
  readonly plateNumber: string;
  readonly brand: string;
  readonly type: string;
  readonly isServiceVehicle: boolean;
  readonly personsCount: number | null;
  readonly trailerPlateNumbers: readonly string[];
  readonly photoReference: string | null;
  readonly decision: VehicleEntryDecision;
  readonly idempotencyKey: string;
}

export interface CreatedVehicleEntry {
  readonly id: string;
  readonly vehicleId: string;
  readonly checkpointId: string;
  readonly gateId: string;
  readonly gateMode: GateMode;
  readonly direction: MovementDirection;
  readonly plateNumber: string;
  readonly brand: string;
  readonly type: string;
  readonly isServiceVehicle: boolean;
  readonly personsCount: number | null;
  readonly trailerPlateNumbers: readonly string[];
  readonly photoReference: string | null;
  readonly queueEntry: QueueEntrySummary | null;
  readonly status: VehicleVisitStatus;
  readonly enteredAt: string;
  readonly version: number;
  readonly wasCreated: boolean;
}

export interface ControlResultSummary {
  readonly kind: ControlKind;
  readonly status: ControlStatus;
  readonly decidedAt: string | null;
}

export interface VehicleVisitSummary {
  readonly id: string;
  readonly checkpointId: string;
  readonly plateNumber: string;
  readonly status: VehicleVisitStatus;
  readonly controls: readonly ControlResultSummary[];
  readonly enteredAt: string;
  readonly version: number;
}
