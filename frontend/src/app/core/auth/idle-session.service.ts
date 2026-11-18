import { DOCUMENT } from '@angular/common';
import { effect, inject, Injectable, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthStore } from './auth.store';

const activityReportIntervalMs = 60_000;
const activityEvents: readonly (keyof DocumentEventMap)[] = [
  'keydown',
  'pointerdown',
  'touchstart',
];

@Injectable({ providedIn: 'root' })
export class IdleSessionService implements OnDestroy {
  private readonly auth = inject(AuthStore);
  private readonly document = inject(DOCUMENT);
  private readonly router = inject(Router);
  private timeoutId: ReturnType<typeof setTimeout> | null = null;
  private listening = false;
  private lastActivityReport = 0;

  private readonly onActivity = (): void => {
    const session = this.auth.session();
    if (!session) {
      return;
    }

    this.armTimeout(session.idleTimeoutMinutes);

    const now = Date.now();
    if (now - this.lastActivityReport >= activityReportIntervalMs) {
      this.lastActivityReport = now;
      void this.auth.recordActivity().then((active) => {
        if (!active) {
          void this.router.navigate(['/sign-in'], { queryParams: { reason: 'expired' } });
        }
      });
    }
  };

  constructor() {
    effect(() => {
      const session = this.auth.session();
      if (session) {
        this.attachListeners();
        this.lastActivityReport = Date.now();
        this.armTimeout(session.idleTimeoutMinutes);
      } else {
        this.detachListeners();
        this.clearTimeout();
      }
    });
  }

  start(): void {
    // Instantiating the singleton starts the reactive session watcher.
  }

  ngOnDestroy(): void {
    this.detachListeners();
    this.clearTimeout();
  }

  private attachListeners(): void {
    if (this.listening) {
      return;
    }

    for (const eventName of activityEvents) {
      this.document.addEventListener(eventName, this.onActivity, { passive: true });
    }
    this.listening = true;
  }

  private detachListeners(): void {
    if (!this.listening) {
      return;
    }

    for (const eventName of activityEvents) {
      this.document.removeEventListener(eventName, this.onActivity);
    }
    this.listening = false;
  }

  private armTimeout(idleTimeoutMinutes: number): void {
    this.clearTimeout();
    this.timeoutId = setTimeout(() => {
      void this.auth.signOut().finally(() => {
        void this.router.navigate(['/sign-in'], { queryParams: { reason: 'idle' } });
      });
    }, idleTimeoutMinutes * 60_000);
  }

  private clearTimeout(): void {
    if (this.timeoutId !== null) {
      clearTimeout(this.timeoutId);
      this.timeoutId = null;
    }
  }
}
