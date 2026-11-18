import { MockOperatingContextGateway } from './mock-operating-context.gateway';

describe('MockOperatingContextGateway', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('exposes the four centrally defined gate modes for the visible checkpoint', async () => {
    const gateway = new MockOperatingContextGateway(localStorage);

    const options = await gateway.getOptions();

    expect(options).toHaveLength(1);
    expect(options[0].gates.map((gate) => gate.mode)).toEqual([
      'borderExit',
      'borderEntry',
      'rearExit',
      'rearEntry',
    ]);
  });

  it('restores the last selection in a new adapter instance', async () => {
    const firstGateway = new MockOperatingContextGateway(localStorage);
    const options = await firstGateway.getOptions();
    const checkpoint = options[0];
    const gate = checkpoint.gates[0];
    await firstGateway.select({ checkpointId: checkpoint.id, gateId: gate.id });

    const restored = await new MockOperatingContextGateway(localStorage).getCurrent();

    expect(restored?.checkpointId).toBe(checkpoint.id);
    expect(restored?.gateId).toBe(gate.id);
  });

  it('rejects an unknown gate instead of trusting stored identifiers', async () => {
    const gateway = new MockOperatingContextGateway(localStorage);
    const checkpoint = (await gateway.getOptions())[0];

    await expect(
      gateway.select({ checkpointId: checkpoint.id, gateId: 'unknown-gate' }),
    ).rejects.toThrow('not available');
  });
});
