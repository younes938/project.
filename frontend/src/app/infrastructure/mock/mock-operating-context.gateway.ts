import { OperatingContextGateway } from '../../application/ports/operating-context.gateway';
import {
  CheckpointOption,
  OperatingContextSelection,
  OperatingContextSummary,
} from '../../domain/models/operating-context';

const storageKey = 'eticket.mock.operating-context.8fd9706f-a25e-4dbd-85cc-14d61a93106d';

const checkpoints: readonly CheckpointOption[] = [
  {
    id: '19fb5569-2fcf-4d4e-9535-2d2798702af8',
    code: 'UA-TEST-01',
    name: 'Demo Vehicle Border Crossing',
    gates: [
      {
        id: 'f833e8e2-b8bf-43e8-a39e-8b652e429713',
        code: 'BORDER-EXIT',
        name: 'Border Gate Exit',
        mode: 'borderExit',
      },
      {
        id: 'b7ef357d-7052-49f1-9219-23b991113654',
        code: 'BORDER-ENTRY',
        name: 'Border Gate Entry',
        mode: 'borderEntry',
      },
      {
        id: '6adb7fb0-4a48-49ae-ab75-d91775205120',
        code: 'REAR-EXIT',
        name: 'Rear Gate Exit',
        mode: 'rearExit',
      },
      {
        id: 'c34ee5f3-ced7-43ae-9949-e3f579fdf832',
        code: 'REAR-ENTRY',
        name: 'Rear Gate Entry',
        mode: 'rearEntry',
      },
    ],
  },
];

export class MockOperatingContextGateway implements OperatingContextGateway {
  private current: OperatingContextSummary | null = null;

  constructor(private readonly storage: Storage | null = getBrowserStorage()) {}

  async getOptions(): Promise<readonly CheckpointOption[]> {
    return checkpoints;
  }

  async getCurrent(): Promise<OperatingContextSummary | null> {
    if (this.current) {
      return this.current;
    }

    const stored = this.readStoredSelection();
    if (!stored) {
      return null;
    }

    const restored = createSummary(stored);
    if (!restored) {
      this.removeStoredSelection();
      return null;
    }

    this.current = restored;
    return restored;
  }

  async select(selection: OperatingContextSelection): Promise<OperatingContextSummary> {
    const selected = createSummary({
      ...selection,
      updatedAt: new Date().toISOString(),
    });
    if (!selected) {
      throw new Error('The selected operating context is not available.');
    }

    this.current = selected;
    this.writeStoredSelection(selected);
    return selected;
  }

  private readStoredSelection(): StoredSelection | null {
    try {
      const raw = this.storage?.getItem(storageKey);
      if (!raw) {
        return null;
      }

      const value: unknown = JSON.parse(raw);
      if (!isStoredSelection(value)) {
        this.removeStoredSelection();
        return null;
      }

      return value;
    } catch {
      this.removeStoredSelection();
      return null;
    }
  }

  private writeStoredSelection(context: OperatingContextSummary): void {
    try {
      this.storage?.setItem(
        storageKey,
        JSON.stringify({
          checkpointId: context.checkpointId,
          gateId: context.gateId,
          updatedAt: context.updatedAt,
        }),
      );
    } catch {
      // Storage is a convenience for mock mode, never an authorization boundary.
    }
  }

  private removeStoredSelection(): void {
    try {
      this.storage?.removeItem(storageKey);
    } catch {
      // Ignore unavailable browser storage and continue without persistence.
    }
  }
}

interface StoredSelection extends OperatingContextSelection {
  readonly updatedAt: string;
}

function createSummary(selection: StoredSelection): OperatingContextSummary | null {
  const checkpoint = checkpoints.find((item) => item.id === selection.checkpointId);
  const gate = checkpoint?.gates.find((item) => item.id === selection.gateId);
  if (!checkpoint || !gate) {
    return null;
  }

  return {
    checkpointId: checkpoint.id,
    checkpointCode: checkpoint.code,
    checkpointName: checkpoint.name,
    gateId: gate.id,
    gateCode: gate.code,
    gateName: gate.name,
    mode: gate.mode,
    updatedAt: selection.updatedAt,
  };
}

function isStoredSelection(value: unknown): value is StoredSelection {
  if (typeof value !== 'object' || value === null) {
    return false;
  }

  const candidate = value as Record<string, unknown>;
  return (
    typeof candidate['checkpointId'] === 'string' &&
    typeof candidate['gateId'] === 'string' &&
    typeof candidate['updatedAt'] === 'string'
  );
}

function getBrowserStorage(): Storage | null {
  try {
    return globalThis.localStorage;
  } catch {
    return null;
  }
}
