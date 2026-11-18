import { MockAuthAdapter } from './mock-external-adapters';

describe('MockAuthAdapter', () => {
  it('starts without an authenticated session', async () => {
    const adapter = new MockAuthAdapter();

    await expect(adapter.getCurrentSession()).resolves.toBeNull();
  });

  it('creates the fixed least-privilege demo session', async () => {
    const adapter = new MockAuthAdapter();

    const session = await adapter.signIn();

    expect(session.authenticationMethod).toBe('mock');
    expect(session.permissions).toContain('vehicle.view');
    expect(session.permissions).not.toContain('vehicle.export');
    expect(session.checkpointIds).toHaveLength(1);
  });

  it('clears the session on sign out', async () => {
    const adapter = new MockAuthAdapter();
    await adapter.signIn();

    await adapter.signOut();

    await expect(adapter.getCurrentSession()).resolves.toBeNull();
  });
});
