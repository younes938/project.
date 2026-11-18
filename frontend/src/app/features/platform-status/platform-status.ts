import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthStore } from '../../core/auth/auth.store';
import { OperatingContextStore } from '../../core/operating-context/operating-context.store';
import { PassengerService } from '../passengers/passenger.service';
import { VEHICLE_VISIT_GATEWAY } from '../../application/ports/vehicle-visit.gateway';
import { VehicleVisitSummary } from '../../domain/models/vehicle-visit';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { APP_CONFIG } from '../../core/config/app-config';

@Component({
  selector: 'et-platform-status',
  imports: [RouterLink],
  templateUrl: './platform-status.html',
  styleUrl: './platform-status.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlatformStatus {
  readonly auth = inject(AuthStore);
  readonly operatingContext = inject(OperatingContextStore);
  private readonly router = inject(Router);
  private readonly passengers = inject(PassengerService);
  private readonly http = inject(HttpClient);
  private readonly config = inject(APP_CONFIG);
  private readonly vehicleVisits = inject(VEHICLE_VISIT_GATEWAY);
  readonly passengerCount = signal(0);
  readonly allowedPassengerCount = signal(0);
  readonly vehicleCount = signal(0);
  readonly deniedVehicleCount = signal(0);
  readonly shipmentCount = signal(0);
  readonly pendingShipmentCount = signal(0);
  readonly securityAlertCount = signal(0);
  readonly loadingStatistics = signal(false);
  readonly statisticsError = signal('');

  constructor() { void this.refreshStatistics(); }

  async refreshStatistics(): Promise<void> {
    this.loadingStatistics.set(true);
    this.statisticsError.set('');
    const [passengerResult, vehicleResult, reportResult] = await Promise.allSettled([
      this.passengers.list(),
      this.vehicleVisits.list(),
      firstValueFrom(this.http.get<{ shipments: number; shipmentsPending: number; securityAlerts: number }>(`${this.config.apiBaseUrl}/reports/summary`, { withCredentials: true })),
    ]);
    if (passengerResult.status === 'fulfilled') {
      const passengers = passengerResult.value;
      this.passengerCount.set(passengers.length);
      this.allowedPassengerCount.set(passengers.filter((item) => item.status === 'Allowed').length);
    } else {
      this.statisticsError.set('Unable to load passenger statistics. Check the API and database connection.');
    }
    if (vehicleResult.status === 'fulfilled') {
      const vehicles: readonly VehicleVisitSummary[] = vehicleResult.value;
      this.vehicleCount.set(vehicles.length);
      this.deniedVehicleCount.set(vehicles.filter((item) => item.status === 'entryDenied').length);
    } else {
      this.statisticsError.set(this.statisticsError() || 'Unable to load vehicle statistics. Make sure a work location is selected.');
    }
    if (reportResult.status === 'fulfilled') {
      this.shipmentCount.set(reportResult.value.shipments);
      this.pendingShipmentCount.set(reportResult.value.shipmentsPending);
      this.securityAlertCount.set(reportResult.value.securityAlerts);
    } else {
      this.statisticsError.set(this.statisticsError() || 'Unable to load shipment and inspection statistics.');
    }
    this.loadingStatistics.set(false);
  }

  async signOut(): Promise<void> {
    await this.auth.signOut();
    await this.router.navigateByUrl('/sign-in');
  }
}
