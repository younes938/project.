
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { VEHICLE_VISIT_GATEWAY } from '../../application/ports/vehicle-visit.gateway';
import { VehicleVisitSummary, VehicleVisitStatus } from '../../domain/models/vehicle-visit';

@Component({
  selector: 'et-vehicle-visits',
  imports: [DatePipe, RouterLink],
  templateUrl: './vehicle-visits.html',
  styleUrl: './vehicle-visits.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VehicleVisits {
  private readonly gateway = inject(VEHICLE_VISIT_GATEWAY);
  readonly visits = signal<readonly VehicleVisitSummary[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly search = signal('');
  readonly statusFilter = signal('all');
  readonly passedCount = computed(() => this.visits().filter((visit) => visit.status === 'passed').length);
  readonly filteredVisits = computed(() => {
    const search = this.search().trim().toLocaleLowerCase();
    const status = this.statusFilter();
    return this.visits().filter((visit) => {
      const matchesSearch = !search || visit.plateNumber.toLocaleLowerCase().includes(search) || visit.id.toLocaleLowerCase().includes(search);
      return matchesSearch && (status === 'all' || visit.status === status);
    });
  });

  constructor() { void this.refresh(); }

  async refresh(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      this.visits.set(await this.gateway.list());
    } catch {
      this.error.set('Unable to load vehicle records. Check that the ASP.NET Core services and database are running, and that an operating location is selected.');
    } finally {
      this.loading.set(false);
    }
  }

  statusLabel(status: VehicleVisitStatus): string {
    const labels: Record<VehicleVisitStatus, string> = {
      entryDenied: 'Denied',
      awaitingControls: 'Awaiting Inspection',
      controlFailed: 'Inspection Failed',
      awaitingExit: 'Awaiting Exit',
      passed: 'Passed',
      return: 'Vehicle Return',
      cancelled: 'Cancelled',
    };
    return labels[status];
  }

  statusClass(status: VehicleVisitStatus): string {
    if (status === 'passed') return 'passed';
    if (status === 'entryDenied' || status === 'controlFailed' || status === 'cancelled') return 'denied';
    return 'pending';
  }

  controlLabel(kind: string): string {
    const labels: Record<string, string> = {
      vehicleInspection: 'Vehicle Inspection',
      border: 'Border Control',
      customs: 'Customs',
    };
    return labels[kind] ?? kind;
  }

  controlStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      pending: 'Pending',
      passed: 'Passed',
      failed: 'Failed',
      notRequired: 'Not Required',
      cancelled: 'Cancelled',
    };
    return labels[status] ?? status;
  }
}
