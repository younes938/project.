import { CreateVehicleEntry } from '../../domain/models/vehicle-visit';
import { MockVehicleVisitGateway } from './mock-vehicle-visit.gateway';

const command: CreateVehicleEntry = {
  plateNumber: 'aa 1234 bb',
  brand: 'Volvo',
  type: 'Truck',
  isServiceVehicle: false,
  personsCount: 2,
  trailerPlateNumbers: ['bb-9876-aa'],
  photoReference: null,
  decision: 'allow',
  idempotencyKey: 'request-0001',
};

describe('MockVehicleVisitGateway', () => {
  it('starts without embedded visit records', async () => {
    const gateway = new MockVehicleVisitGateway();

    await expect(gateway.list()).resolves.toEqual([]);
  });

  it('returns deterministic e-Queue information for the demo plate', async () => {
    const gateway = new MockVehicleVisitGateway();

    const result = await gateway.lookupQueue('aa-1234-bb');

    expect(result.found).toBe(true);
    expect(result.entry?.id).toBe('eq-demo-001');
  });

  it('creates an awaiting-controls visit for an allow decision', async () => {
    const gateway = new MockVehicleVisitGateway();

    const result = await gateway.createEntry(command);

    expect(result.plateNumber).toBe('AA1234BB');
    expect(result.trailerPlateNumbers).toEqual(['BB9876AA']);
    expect(result.status).toBe('awaitingControls');
    await expect(gateway.list()).resolves.toHaveLength(1);
  });

  it('returns the same visit when an idempotent command is repeated', async () => {
    const gateway = new MockVehicleVisitGateway();

    const first = await gateway.createEntry(command);
    const repeated = await gateway.createEntry(command);

    expect(repeated.id).toBe(first.id);
    expect(repeated.wasCreated).toBe(false);
    await expect(gateway.list()).resolves.toHaveLength(1);
  });

  it('creates an entry-denied visit without service-only fields', async () => {
    const gateway = new MockVehicleVisitGateway();

    const result = await gateway.createEntry({
      ...command,
      isServiceVehicle: true,
      personsCount: null,
      trailerPlateNumbers: [],
      decision: 'deny',
      idempotencyKey: 'request-0002',
    });

    expect(result.personsCount).toBeNull();
    expect(result.trailerPlateNumbers).toEqual([]);
    expect(result.status).toBe('entryDenied');
  });
});
