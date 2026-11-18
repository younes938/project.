import { Injectable } from '@angular/core';
import {
  AuthAdapter,
  AuthenticatedUserSummary,
  BorderControlAdapter,
  CameraRecognitionSummary,
  CustomsAdapter,
  EQueueAdapter,
  QueueEntrySummary,
  VmsAdapter,
} from '../../application/ports/external-adapters';
import { ControlResultSummary, GateMode } from '../../domain/models/vehicle-visit';

@Injectable()
export class MockAuthAdapter implements AuthAdapter {
  private currentSession: AuthenticatedUserSummary | null = null;

  async getCurrentSession(): Promise<AuthenticatedUserSummary | null> {
    return this.currentSession;
  }

  async signIn(): Promise<AuthenticatedUserSummary> {
    const session: AuthenticatedUserSummary = {
      id: '8fd9706f-a25e-4dbd-85cc-14d61a93106d',
      displayName: 'Demo Border Control Operator',
      roles: ['checkpoint-operator'],
      permissions: [
        'vehicle.process',
        'vehicle.inspect',
        'vehicle.search.recent',
        'vehicle.view',
        'queue.view',
      ],
      checkpointIds: ['19fb5569-2fcf-4d4e-9535-2d2798702af8'],
      authenticationMethod: 'mock',
      idleTimeoutMinutes: 15,
    };

    this.currentSession = Object.freeze(session);
    return session;
  }

  async signOut(): Promise<void> {
    this.currentSession = null;
  }

  async recordActivity(): Promise<void> {
    return Promise.resolve();
  }
}

@Injectable()
export class MockVmsAdapter implements VmsAdapter {
  async listRecognitions(mode: GateMode): Promise<readonly CameraRecognitionSummary[]> {
    void mode;
    return [];
  }

  async markProcessed(recognitionId: string): Promise<void> {
    void recognitionId;
  }
}

@Injectable()
export class MockEQueueAdapter implements EQueueAdapter {
  async findByPlate(checkpointId: string, plateNumber: string): Promise<QueueEntrySummary | null> {
    void checkpointId;
    void plateNumber;
    return null;
  }

  async setPaused(paused: boolean): Promise<void> {
    void paused;
  }
}

@Injectable()
export class MockCustomsAdapter implements CustomsAdapter {
  async getControlResult(visitId: string): Promise<ControlResultSummary | null> {
    void visitId;
    return null;
  }
}

@Injectable()
export class MockBorderControlAdapter implements BorderControlAdapter {
  async getControlResult(visitId: string): Promise<ControlResultSummary | null> {
    void visitId;
    return null;
  }
}
