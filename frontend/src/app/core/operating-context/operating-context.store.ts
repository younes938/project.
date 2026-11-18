import { computed, inject, Injectable, signal } from '@angular/core';
import { OPERATING_CONTEXT_GATEWAY } from '../../application/ports/operating-context.gateway';
import {
  CheckpointOption,
  OperatingContextSelection,
  OperatingContextSummary,
} from '../../domain/models/operating-context';

export type OperatingContextStatus = 'idle' | 'loading' | 'ready' | 'saving' | 'error';

@Injectable({ providedIn: 'root' })
export class OperatingContextStore {
  private readonly gateway = inject(OPERATING_CONTEXT_GATEWAY);
  private readonly optionsState = signal<readonly CheckpointOption[]>([]);
  private readonly currentState = signal<OperatingContextSummary | null>(null);
  private readonly statusState = signal<OperatingContextStatus>('idle');
  private readonly errorState = signal<string | null>(null);

  readonly options = this.optionsState.asReadonly();
  readonly current = this.currentState.asReadonly();
  readonly status = this.statusState.asReadonly();
  readonly errorMessage = this.errorState.asReadonly();
  readonly hasContext = computed(() => this.currentState() !== null);
  readonly isBusy = computed(
    () => this.statusState() === 'loading' || this.statusState() === 'saving',
  );

  async refresh(): Promise<void> {
    this.statusState.set('loading');
    this.errorState.set(null);

    try {
      const [options, current] = await Promise.all([
        this.gateway.getOptions(),
        this.gateway.getCurrent(),
      ]);
      this.optionsState.set(options);
      this.currentState.set(current);
      this.statusState.set('ready');
    } catch {
      this.optionsState.set([]);
      this.currentState.set(null);
      this.statusState.set('error');
      this.errorState.set('Unable to load available operating modes.');
    }
  }

  async select(selection: OperatingContextSelection): Promise<boolean> {
    if (this.isBusy()) {
      return false;
    }

    this.statusState.set('saving');
    this.errorState.set(null);

    try {
      this.currentState.set(await this.gateway.select(selection));
      this.statusState.set('ready');
      return true;
    } catch {
      this.statusState.set('error');
      this.errorState.set('Unable to save the mode. Refresh the list and try again.');
      return false;
    }
  }
}
