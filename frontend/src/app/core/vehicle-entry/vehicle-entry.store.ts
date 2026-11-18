import { computed, inject, Injectable, signal } from '@angular/core';
import { VEHICLE_VISIT_GATEWAY } from '../../application/ports/vehicle-visit.gateway';
import {
  CreatedVehicleEntry,
  CreateVehicleEntry,
  QueueLookupResult,
} from '../../domain/models/vehicle-visit';

export type VehicleEntryStatus =
  'idle' | 'lookingUp' | 'ready' | 'submitting' | 'success' | 'error';

@Injectable({ providedIn: 'root' })
export class VehicleEntryStore {
  private readonly gateway = inject(VEHICLE_VISIT_GATEWAY);
  private readonly statusState = signal<VehicleEntryStatus>('idle');
  private readonly queueState = signal<QueueLookupResult | null>(null);
  private readonly createdState = signal<CreatedVehicleEntry | null>(null);
  private readonly errorState = signal<string | null>(null);

  readonly status = this.statusState.asReadonly();
  readonly queueLookup = this.queueState.asReadonly();
  readonly created = this.createdState.asReadonly();
  readonly errorMessage = this.errorState.asReadonly();
  readonly isBusy = computed(
    () => this.statusState() === 'lookingUp' || this.statusState() === 'submitting',
  );

  async lookupQueue(plateNumber: string): Promise<boolean> {
    if (this.isBusy()) {
      return false;
    }

    this.statusState.set('lookingUp');
    this.queueState.set(null);
    this.errorState.set(null);

    try {
      this.queueState.set(await this.gateway.lookupQueue(plateNumber));
      this.statusState.set('ready');
      return true;
    } catch {
      this.statusState.set('error');
      this.errorState.set('Unable to verify the vehicle in the e-Queue.');
      return false;
    }
  }

  async create(command: CreateVehicleEntry): Promise<boolean> {
    if (this.isBusy() || this.createdState()) {
      return false;
    }

    this.statusState.set('submitting');
    this.errorState.set(null);

    try {
      this.createdState.set(await this.gateway.createEntry(command));
      this.statusState.set('success');
      return true;
    } catch {
      this.statusState.set('error');
      this.errorState.set('Unable to save the decision. Check the data and try again.');
      return false;
    }
  }

  clearQueueLookup(): void {
    this.queueState.set(null);
    if (!this.isBusy() && !this.createdState()) {
      this.statusState.set('idle');
      this.errorState.set(null);
    }
  }

  reset(): void {
    this.statusState.set('idle');
    this.queueState.set(null);
    this.createdState.set(null);
    this.errorState.set(null);
  }
}
