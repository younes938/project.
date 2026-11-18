import { InjectionToken } from '@angular/core';
import {
  CheckpointOption,
  OperatingContextSelection,
  OperatingContextSummary,
} from '../../domain/models/operating-context';

export interface OperatingContextGateway {
  getOptions(): Promise<readonly CheckpointOption[]>;
  getCurrent(): Promise<OperatingContextSummary | null>;
  select(selection: OperatingContextSelection): Promise<OperatingContextSummary>;
}

export const OPERATING_CONTEXT_GATEWAY = new InjectionToken<OperatingContextGateway>(
  'OPERATING_CONTEXT_GATEWAY',
);
