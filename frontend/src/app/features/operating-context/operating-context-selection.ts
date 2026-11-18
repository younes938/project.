import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OperatingContextStore } from '../../core/operating-context/operating-context.store';

@Component({
  selector: 'et-operating-context-selection',
  templateUrl: './operating-context-selection.html',
  styleUrl: './operating-context-selection.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OperatingContextSelection implements OnInit {
  readonly store = inject(OperatingContextStore);
  readonly checkpointQuery = signal('');
  readonly selectedCheckpointId = signal<string | null>(null);
  readonly selectedGateId = signal<string | null>(null);

  readonly filteredCheckpoints = computed(() => {
    const query = this.checkpointQuery().trim().toLocaleLowerCase('uk-UA');
    if (!query) {
      return this.store.options();
    }

    return this.store
      .options()
      .filter((checkpoint) =>
        `${checkpoint.code} ${checkpoint.name}`.toLocaleLowerCase('uk-UA').includes(query),
      );
  });

  readonly selectedCheckpoint = computed(() =>
    this.store.options().find((item) => item.id === this.selectedCheckpointId()),
  );

  readonly canSave = computed(
    () =>
      this.selectedCheckpointId() !== null &&
      this.selectedGateId() !== null &&
      !this.store.isBusy(),
  );

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  async load(): Promise<void> {
    await this.store.refresh();

    const current = this.store.current();
    if (current) {
      this.selectedCheckpointId.set(current.checkpointId);
      this.selectedGateId.set(current.gateId);
      return;
    }

    const options = this.store.options();
    if (options.length === 1) {
      this.selectCheckpoint(options[0].id);
    }
  }

  updateQuery(value: string): void {
    this.checkpointQuery.set(value);
  }

  selectCheckpoint(checkpointId: string): void {
    if (this.selectedCheckpointId() === checkpointId) {
      return;
    }

    this.selectedCheckpointId.set(checkpointId);
    this.selectedGateId.set(null);
  }

  selectGate(gateId: string): void {
    this.selectedGateId.set(gateId);
  }

  async save(event: Event): Promise<void> {
    event.preventDefault();
    const checkpointId = this.selectedCheckpointId();
    const gateId = this.selectedGateId();
    if (!checkpointId || !gateId || !this.canSave()) {
      return;
    }

    const saved = await this.store.select({ checkpointId, gateId });
    if (!saved) {
      return;
    }

    await this.router.navigateByUrl(this.safeReturnUrl());
  }

  private safeReturnUrl(): string {
    const requestedUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    return requestedUrl?.startsWith('/') && !requestedUrl.startsWith('//') ? requestedUrl : '/';
  }
}
