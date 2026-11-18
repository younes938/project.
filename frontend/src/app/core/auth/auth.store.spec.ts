import { TestBed } from '@angular/core/testing';
import {
  AUTH_ADAPTER,
  AuthAdapter,
  AuthenticatedUserSummary,
} from '../../application/ports/external-adapters';
import { AuthStore } from './auth.store';

const demoSession: AuthenticatedUserSummary = {
  id: 'user-1',
  displayName: 'Demo Operator',
  roles: ['operator'],
  permissions: ['vehicle.view'],
  checkpointIds: ['checkpoint-1'],
  authenticationMethod: 'mock',
  idleTimeoutMinutes: 15,
};

class FakeAuthAdapter implements AuthAdapter {
  currentSession: AuthenticatedUserSummary | null = null;
  failSignOut = false;

  async getCurrentSession(): Promise<AuthenticatedUserSummary | null> {
    return this.currentSession;
  }

  async signIn(): Promise<AuthenticatedUserSummary> {
    this.currentSession = demoSession;
    return demoSession;
  }

  async signOut(): Promise<void> {
    if (this.failSignOut) {
      throw new Error('Session already expired');
    }
    this.currentSession = null;
  }

  async recordActivity(): Promise<void> {
    return Promise.resolve();
  }
}

describe('AuthStore', () => {
  let adapter: FakeAuthAdapter;
  let store: AuthStore;

  beforeEach(() => {
    adapter = new FakeAuthAdapter();
    TestBed.configureTestingModule({
      providers: [AuthStore, { provide: AUTH_ADAPTER, useValue: adapter }],
    });
    store = TestBed.inject(AuthStore);
  });

  it('initializes as anonymous when the adapter has no session', async () => {
    await store.initialize();

    expect(store.status()).toBe('anonymous');
    expect(store.isAuthenticated()).toBe(false);
  });

  it('derives permissions and visibility only from the authenticated session', async () => {
    await store.signIn();

    expect(store.hasPermission('vehicle.view')).toBe(true);
    expect(store.hasPermission('vehicle.export')).toBe(false);
    expect(store.canAccessCheckpoint('checkpoint-1')).toBe(true);
    expect(store.canAccessCheckpoint('checkpoint-2')).toBe(false);
  });

  it('clears local access even when the remote session is already unavailable', async () => {
    await store.signIn();
    adapter.failSignOut = true;

    await expect(store.signOut()).resolves.toBeUndefined();
    expect(store.isAuthenticated()).toBe(false);
    expect(store.status()).toBe('anonymous');
  });
});
