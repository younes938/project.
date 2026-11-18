import { TestBed } from '@angular/core/testing';
import {
  VEHICLE_VISIT_GATEWAY,
  VehicleVisitGateway,
} from '../../application/ports/vehicle-visit.gateway';
import {
  CreatedVehicleEntry,
  CreateVehicleEntry,
  QueueLookupResult,
  VehicleVisitSummary,
} from '../../domain/models/vehicle-visit';
import { VehicleEntryStore } from './vehicle-entry.store';

const command: CreateVehicleEntry = {
  plateNumber: 'AA1234BB',
  brand: 'Volvo',
  type: 'Truck',
  isServiceVehicle: false,
  personsCount: 1,
  trailerPlateNumbers: [],
  photoReference: null,
  decision: 'allow',
  idempotencyKey: 'request-0001',
};

const created: CreatedVehicleEntry = {
  id: 'visit-1',
  vehicleId: 'vehicle-1',
  checkpointId: 'checkpoint-1',
  gateId: 'gate-1',
  gateMode: 'rearExit',
  direction: 'exit',
  plateNumber: 'AA1234BB',
  brand: 'Volvo',
  type: 'Truck',
  isServiceVehicle: false,
  personsCount: 1,
  trailerPlateNumbers: [],
  photoReference: null,
  queueEntry: null,
  status: 'awaitingControls',
  enteredAt: '2026-09-07T12:00:00Z',
  version: 1,
  wasCreated: true,
};

class FakeVehicleVisitGateway implements VehicleVisitGateway {
  createCalls = 0;
  pendingCreate: Promise<CreatedVehicleEntry> | null = null;

  async list(): Promise<readonly VehicleVisitSummary[]> {
    return [];
  }

  async getById(): Promise<CreatedVehicleEntry | null> {
    return null;
  }

  async lookupQueue(plateNumber: string): Promise<QueueLookupResult> {
    return { plateNumber, found: false, entry: null };
  }

  async createEntry(): Promise<CreatedVehicleEntry> {
    this.createCalls += 1;
    return this.pendingCreate ?? created;
  }
}

describe('VehicleEntryStore', () => {
  let gateway: FakeVehicleVisitGateway;
  let store: VehicleEntryStore;

  beforeEach(() => {
    gateway = new FakeVehicleVisitGateway();
    TestBed.configureTestingModule({
      providers: [VehicleEntryStore, { provide: VEHICLE_VISIT_GATEWAY, useValue: gateway }],
    });
    store = TestBed.inject(VehicleEntryStore);
  });

  it('stores an e-Queue lookup result', async () => {
    await store.lookupQueue('AA1234BB');

    expect(store.queueLookup()).toEqual({
      plateNumber: 'AA1234BB',
      found: false,
      entry: null,
    });
    expect(store.status()).toBe('ready');
  });

  it('stores a created vehicle entry', async () => {
    const result = await store.create(command);

    expect(result).toBe(true);
    expect(store.created()).toEqual(created);
    expect(store.status()).toBe('success');
  });

  it('prevents a duplicate submit while the command is pending', async () => {
    let resolveCreate: ((value: CreatedVehicleEntry) => void) | undefined;
    gateway.pendingCreate = new Promise((resolve) => {
      resolveCreate = resolve;
    });

    const first = store.create(command);
    const duplicate = await store.create(command);
    resolveCreate?.(created);

    expect(duplicate).toBe(false);
    expect(await first).toBe(true);
    expect(gateway.createCalls).toBe(1);
  });
});
