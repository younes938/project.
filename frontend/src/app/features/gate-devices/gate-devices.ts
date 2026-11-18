import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe, UpperCasePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { RouterLink } from '@angular/router';
import { APP_CONFIG } from '../../core/config/app-config';

type Device = {
  deviceId: string;
  deviceType: string;
  status: string;
  timestamp: string;
  value?: number | null;
  unit?: string | null;
  message?: string | null;
};
type GateStatus = { gateId: string; agentId: string; mode: string; timestamp: string; devices: Device[] };
type WeightHistory = {
  id: string;
  gateId: string;
  deviceId: string;
  weightKg: number;
  weightStatus: 'NORMAL' | 'WARNING' | 'OVERWEIGHT' | string;
  timestamp: string;
};

@Component({
  selector: 'et-gate-devices',
  imports: [RouterLink, DatePipe, DecimalPipe, UpperCasePipe],
  templateUrl: './gate-devices.html',
  styleUrl: './gate-devices.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GateDevices {
  private readonly http = inject(HttpClient);
  private readonly config = inject(APP_CONFIG);
  private readonly destroyRef = inject(DestroyRef);

  readonly lastUpdated = signal<Date>(new Date());
  readonly devices = signal<Device[]>([]);
  readonly gateId = signal('GATE-01');
  readonly mode = signal('simulation');
  readonly error = signal<string | null>(null);
  readonly historyError = signal<string | null>(null);
  readonly weightHistory = signal<WeightHistory[]>([]);
  readonly loading = signal(false);
  readonly historyLoading = signal(false);

  readonly onlineCount = computed(() => this.devices().filter(d => this.isOnline(d)).length);
  readonly offlineCount = computed(() => this.devices().length - this.onlineCount());
  readonly simulationCount = computed(() => this.devices().filter(d => d.status.toLowerCase() === 'simulated').length);
  readonly normalCount = computed(() => this.weightHistory().filter(x => x.weightStatus === 'NORMAL').length);
  readonly warningCount = computed(() => this.weightHistory().filter(x => x.weightStatus === 'WARNING').length);
  readonly overweightCount = computed(() => this.weightHistory().filter(x => x.weightStatus === 'OVERWEIGHT').length);
  readonly totalReadings = computed(() => this.weightHistory().length);

  constructor() {
    void this.refresh();
    void this.loadWeightHistory();
    const statusTimer = window.setInterval(() => void this.refresh(), 5_000);
    const historyTimer = window.setInterval(() => void this.loadWeightHistory(), 15_000);
    this.destroyRef.onDestroy(() => {
      window.clearInterval(statusTimer);
      window.clearInterval(historyTimer);
    });
  }

  private isOnline(device: Device): boolean {
    const reportedStatus = device.status.toLowerCase();
    if (['offline', 'disconnected', 'fault', 'error'].includes(reportedStatus)) return false;

    const timestamp = new Date(device.timestamp).getTime();
    return Number.isFinite(timestamp) && Date.now() - timestamp <= 30_000 && Date.now() >= timestamp - 5_000;
  }

  deviceName(type: string): string {
    const names: Record<string, string> = {
      'anpr-camera': 'ANPR Camera',
      weighbridge: 'Vehicle Weighbridge',
      'rfid-reader': 'RFID Reader',
      barrier: 'Automatic Barrier',
    };
    return names[type] ?? type;
  }

  deviceDescription(type: string): string {
    const descriptions: Record<string, string> = {
      'anpr-camera': 'License Plate Recognition',
      weighbridge: 'Vehicle Weight Measurement',
      'rfid-reader': 'Vehicle Identification',
      barrier: 'Gate Control',
    };
    return descriptions[type] ?? 'Gate equipment';
  }

  async refresh(): Promise<void> {
    this.loading.set(true);
    try {
      const base = this.config.apiBaseUrl.replace(/\/$/, '');
      const gates = await firstValueFrom(this.http.get<GateStatus[]>(`${base}/gate-companion/status`, { withCredentials: true }));
      const gate = gates[0];
      this.devices.set(gate?.devices ?? []);
      if (gate) {
        this.gateId.set(gate.gateId);
        this.mode.set(gate.mode || 'simulation');
      }
      this.error.set(gate ? null : 'No Gate Companion heartbeat received yet. Start the GateCompanion service.');
      this.lastUpdated.set(new Date());
    } catch (error) {
      console.error('Gate status API error:', error);
      this.error.set('Unable to connect to Gate Companion. Check the backend and your session.');
    } finally {
      this.loading.set(false);
    }
  }

  async loadWeightHistory(): Promise<void> {
    this.historyLoading.set(true);
    try {
      const base = this.config.apiBaseUrl.replace(/\/$/, '');
      const rows = await firstValueFrom(this.http.get<WeightHistory[]>(`${base}/gate-companion/weight-history?limit=100`, { withCredentials: true }));
      this.weightHistory.set(rows);
      this.historyError.set(null);
    } catch (error) {
      console.error('Weight history API error:', error);
      this.historyError.set('Unable to load weight history. Check your session and SQL Server table.');
    } finally {
      this.historyLoading.set(false);
    }
  }

  isOnlineForTemplate(device: Device): boolean { return this.isOnline(device); }

  statusLabel(device: Device): string {
    return this.isOnline(device) ? 'ONLINE' : 'OFFLINE';
  }
}
