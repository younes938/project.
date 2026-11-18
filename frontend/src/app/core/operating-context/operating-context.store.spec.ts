import { TestBed } from '@angular/core/testing';
import {
  OPERATING_CONTEXT_GATEWAY,
  OperatingContextGateway,
} from '../../application/ports/operating-context.gateway';
import {
  CheckpointOption,
  OperatingContextSelection,
  OperatingContextSummary,
} from '../../domain/models/operating-context';
import { OperatingContextStore } from './operating-context.store';

const options: readonly CheckpointOption[] = [
  {
    id: 'checkpoint-1',
    code: 'CP-1',
    name: 'Checkpoint 1',
    gates: [{ id: 'gate-1', code: 'G-1', name: 'Gate 1', mode: 'rearEntry' }],
  },
];

const selectedContext: OperatingContextSummary = {
  checkpointId: 'checkpoint-1',
  checkpointCode: 'CP-1',
  checkpointName: 'Checkpoint 1',
  gateId: 'gate-1',
  gateCode: 'G-1',
  gateName: 'Gate 1',
  mode: 'rearEntry',
  updatedAt: '2026-09-07T12:00:00Z',
};

class FakeOperatingContextGateway implements OperatingContextGateway {
  current: OperatingContextSummary | null = null;
  selectCalls = 0;
  pendingSelection: Promise<OperatingContextSummary> | null = null;

  async getOptions(): Promise<readonly CheckpointOption[]> {
    return options;
  }

  async getCurrent(): Promise<OperatingContextSummary | null> {
    return this.current;
  }

  async select(selection: OperatingContextSelection): Promise<OperatingContextSummary> {
    void selection;
    this.selectCalls += 1;
    if (this.pendingSelection) {
      return this.pendingSelection;
    }

    this.current = selectedContext;
    return selectedContext;
  }
}

describe('OperatingContextStore', () => {
  let gateway: FakeOperatingContextGateway;
  let store: OperatingContextStore;

  beforeEach(() => {
    gateway = new FakeOperatingContextGateway();
    TestBed.configureTestingModule({
      providers: [OperatingContextStore, { provide: OPERATING_CONTEXT_GATEWAY, useValue: gateway }],
    });
    store = TestBed.inject(OperatingContextStore);
  });

  it('loads available options and the current context together', async () => {
    gateway.current = selectedContext;

    await store.refresh();

    expect(store.options()).toEqual(options);
    expect(store.current()).toEqual(selectedContext);
    expect(store.status()).toBe('ready');
  });

  it('stores the backend-confirmed selection', async () => {
    const result = await store.select({ checkpointId: 'checkpoint-1', gateId: 'gate-1' });

    expect(result).toBe(true);
    expect(store.current()).toEqual(selectedContext);
  });

  it('rejects a duplicate selection while the first request is pending', async () => {
    let resolveSelection: ((value: OperatingContextSummary) => void) | undefined;
    gateway.pendingSelection = new Promise((resolve) => {
      resolveSelection = resolve;
    });

    const first = store.select({ checkpointId: 'checkpoint-1', gateId: 'gate-1' });
    const duplicate = await store.select({ checkpointId: 'checkpoint-1', gateId: 'gate-1' });
    resolveSelection?.(selectedContext);

    expect(duplicate).toBe(false);
    expect(await first).toBe(true);
    expect(gateway.selectCalls).toBe(1);
  });
});
