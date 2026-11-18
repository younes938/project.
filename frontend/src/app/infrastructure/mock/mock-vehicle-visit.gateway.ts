import { VehicleVisitGateway } from '../../application/ports/vehicle-visit.gateway';
import { OperatingContextGateway } from '../../application/ports/operating-context.gateway';
import {
  CreatedVehicleEntry,
  CreateVehicleEntry,
  QueueEntrySummary,
  QueueLookupResult,
  VehicleVisitSummary,
} from '../../domain/models/vehicle-visit';

const demoCheckpointId = '19fb5569-2fcf-4d4e-9535-2d2798702af8';
const demoGateId = '6adb7fb0-4a48-49ae-ab75-d91775205120';
const queueEntry: QueueEntrySummary = {
  id: 'eq-demo-001',
  plateNumber: 'AA1234BB',
  externalStatus: 'confirmed',
  lastSynchronizedAt: '2026-09-07T08:00:00.000Z',
};

export class MockVehicleVisitGateway implements VehicleVisitGateway {
  private readonly storageKey = 'eticket.vehicle-visits.v1';
  private readonly commandsKey = 'eticket.vehicle-commands.v1';
  private readonly items: VehicleVisitSummary[] = this.readStorage<VehicleVisitSummary[]>(this.storageKey, []);
  private readonly commands = new Map<
    string,
    { readonly fingerprint: string; readonly result: CreatedVehicleEntry }
  >(this.readStorage<Array<[string, { readonly fingerprint: string; readonly result: CreatedVehicleEntry }]>>(this.commandsKey, []));

  constructor(private readonly contextGateway?: OperatingContextGateway) {}

  async list(): Promise<readonly VehicleVisitSummary[]> {
    return this.items;
  }

  async getById(id: string): Promise<CreatedVehicleEntry | null> {
    return [...this.commands.values()].find((item) => item.result.id === id)?.result ?? null;
  }

  async lookupQueue(plateNumber: string): Promise<QueueLookupResult> {
    const normalized = normalizePlate(plateNumber);
    assertPlate(normalized);
    const entry = normalized === queueEntry.plateNumber ? queueEntry : null;
    return { plateNumber: normalized, found: entry !== null, entry };
  }

  async createEntry(command: CreateVehicleEntry): Promise<CreatedVehicleEntry> {
    const normalizedCommand = normalizeCommand(command);
    validateCommand(normalizedCommand);
    const context = await this.contextGateway?.getCurrent();
    const gateMode = context?.mode ?? 'rearExit';
    if (gateMode !== 'rearExit' && gateMode !== 'borderEntry') {
      throw new Error('The selected gate mode does not receive vehicles.');
    }
    const fingerprint = JSON.stringify(normalizedCommand);
    const previous = this.commands.get(command.idempotencyKey);

    if (previous) {
      if (previous.fingerprint !== fingerprint) {
        throw new Error('The idempotency key has already been used for another request.');
      }

      return { ...previous.result, wasCreated: false };
    }

    const entry = normalizedCommand.plateNumber === queueEntry.plateNumber ? queueEntry : null;
    const result: CreatedVehicleEntry = {
      id: globalThis.crypto.randomUUID(),
      vehicleId: globalThis.crypto.randomUUID(),
      checkpointId: context?.checkpointId ?? demoCheckpointId,
      gateId: context?.gateId ?? demoGateId,
      gateMode,
      direction: gateMode === 'borderEntry' ? 'entry' : 'exit',
      plateNumber: normalizedCommand.plateNumber,
      brand: normalizedCommand.brand,
      type: normalizedCommand.type,
      isServiceVehicle: normalizedCommand.isServiceVehicle,
      personsCount: normalizedCommand.personsCount,
      trailerPlateNumbers: normalizedCommand.trailerPlateNumbers,
      photoReference: normalizedCommand.photoReference,
      queueEntry: entry,
      status: normalizedCommand.decision === 'allow' ? 'awaitingControls' : 'entryDenied',
      enteredAt: new Date().toISOString(),
      version: 1,
      wasCreated: true,
    };

    this.commands.set(command.idempotencyKey, { fingerprint, result });
    this.items.unshift({
      id: result.id,
      checkpointId: result.checkpointId,
      plateNumber: result.plateNumber,
      status: result.status,
      controls:
        result.status === 'awaitingControls'
          ? [{ kind: 'vehicleInspection', status: 'pending', decidedAt: null }]
          : [],
      enteredAt: result.enteredAt,
      version: result.version,
    });
    this.persist();

    return result;
  }
  private persist(): void {
    try {
      globalThis.localStorage?.setItem(this.storageKey, JSON.stringify(this.items));
      globalThis.localStorage?.setItem(this.commandsKey, JSON.stringify([...this.commands.entries()]));
    } catch {
      // Browser storage may be unavailable; the current session can still operate in memory.
    }
  }

  private readStorage<T>(key: string, fallback: T): T {
    try {
      const raw = globalThis.localStorage?.getItem(key);
      return raw ? (JSON.parse(raw) as T) : fallback;
    } catch {
      return fallback;
    }
  }
}

function normalizeCommand(command: CreateVehicleEntry): CreateVehicleEntry {
  return {
    ...command,
    plateNumber: normalizePlate(command.plateNumber),
    brand: command.brand.trim(),
    type: command.type.trim(),
    personsCount: command.personsCount,
    trailerPlateNumbers: command.trailerPlateNumbers.filter(Boolean).map(normalizePlate),
    photoReference: command.photoReference?.trim() || null,
  };
}

function validateCommand(command: CreateVehicleEntry): void {
  if (command.idempotencyKey.trim().length < 8 || command.idempotencyKey.length > 100) {
    throw new Error('Idempotency key must contain 8 to 100 characters.');
  }
  assertPlate(command.plateNumber);
  if (!command.brand || command.brand.length > 80 || !command.type || command.type.length > 80) {
    throw new Error('Vehicle brand and type are required.');
  }
  if (
    !command.isServiceVehicle &&
    (command.personsCount === null || command.personsCount < 1 || command.personsCount > 100)
  ) {
    throw new Error('Persons count must be between 1 and 100.');
  }
  if (
    command.isServiceVehicle &&
    (command.personsCount !== null || command.trailerPlateNumbers.length > 0)
  ) {
    throw new Error('Persons count and trailers are not used for a service vehicle.');
  }
  if (command.trailerPlateNumbers.length > 2) {
    throw new Error('A vehicle entry can contain at most two trailers.');
  }
  if (command.photoReference && command.photoReference.length > 2048) {
    throw new Error('Photo reference cannot exceed 2048 characters.');
  }
  command.trailerPlateNumbers.forEach(assertPlate);
  if (
    new Set([command.plateNumber, ...command.trailerPlateNumbers]).size !==
    command.trailerPlateNumbers.length + 1
  ) {
    throw new Error('Vehicle and trailer plate numbers must be unique.');
  }
}

function normalizePlate(value: string): string {
  return value
    .normalize('NFKC')
    .toLocaleUpperCase('uk-UA')
    .replace(/[\s\-‐‑]/gu, '');
}

function assertPlate(value: string): void {
  if (!/^[\p{L}\p{N}]{2,16}$/u.test(value)) {
    throw new Error('Plate number must contain 2 to 16 letters or digits.');
  }
}
