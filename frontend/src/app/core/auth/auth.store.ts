import { computed, inject, Injectable, signal } from '@angular/core';
import { AUTH_ADAPTER, AuthenticatedUserSummary } from '../../application/ports/external-adapters';
import { Permission } from '../../domain/permissions/permission';

export type AuthenticationStatus = 'initializing' | 'anonymous' | 'authenticated' | 'busy';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly adapter = inject(AUTH_ADAPTER);
  private initialization: Promise<void> | null = null;
  private readonly sessionState = signal<AuthenticatedUserSummary | null>(null);
  private readonly statusState = signal<AuthenticationStatus>('initializing');
  private readonly errorState = signal<string | null>(null);

  readonly session = this.sessionState.asReadonly();
  readonly status = this.statusState.asReadonly();
  readonly errorMessage = this.errorState.asReadonly();
  readonly isAuthenticated = computed(() => this.sessionState() !== null);
  readonly isBusy = computed(
    () => this.statusState() === 'initializing' || this.statusState() === 'busy',
  );

  initialize(): Promise<void> {
    this.initialization ??= this.loadSession();
    return this.initialization;
  }

  async signIn(): Promise<boolean> {
    this.statusState.set('busy');
    this.errorState.set(null);

    try {
      const session = await this.adapter.signIn();
      this.sessionState.set(session);
      this.statusState.set('authenticated');
      return true;
    } catch {
      this.sessionState.set(null);
      this.statusState.set('anonymous');
      this.errorState.set('Unable to create a session. Check service availability.');
      return false;
    }
  }

  async signOut(): Promise<void> {
    this.statusState.set('busy');

    try {
      await this.adapter.signOut();
    } catch {
      // An expired or unreachable server session must not keep local access alive.
    } finally {
      this.sessionState.set(null);
      this.statusState.set('anonymous');
      this.errorState.set(null);
    }
  }

  async recordActivity(): Promise<boolean> {
    if (!this.sessionState()) {
      return false;
    }

    try {
      await this.adapter.recordActivity();
      return true;
    } catch {
      this.sessionState.set(null);
      this.statusState.set('anonymous');
      return false;
    }
  }

  hasPermission(permission: Permission): boolean {
    return this.sessionState()?.permissions.includes(permission) ?? false;
  }

  canAccessCheckpoint(checkpointId: string): boolean {
    return this.sessionState()?.checkpointIds.includes(checkpointId) ?? false;
  }

  private async loadSession(): Promise<void> {
    try {
      const session = await this.adapter.getCurrentSession();
      this.sessionState.set(session);
      this.statusState.set(session ? 'authenticated' : 'anonymous');
    } catch {
      this.sessionState.set(null);
      this.statusState.set('anonymous');
      this.errorState.set('The authentication service is temporarily unavailable.');
    }
  }
}
