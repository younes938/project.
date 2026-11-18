import { GateMode } from './vehicle-visit';

export interface GateOption {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly mode: GateMode;
}

export interface CheckpointOption {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly gates: readonly GateOption[];
}

export interface OperatingContextSummary {
  readonly checkpointId: string;
  readonly checkpointCode: string;
  readonly checkpointName: string;
  readonly gateId: string;
  readonly gateCode: string;
  readonly gateName: string;
  readonly mode: GateMode;
  readonly updatedAt: string;
}

export interface OperatingContextSelection {
  readonly checkpointId: string;
  readonly gateId: string;
}
