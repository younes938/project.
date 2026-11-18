import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { APP_CONFIG } from '../../core/config/app-config';
import { AuthStore } from '../../core/auth/auth.store';

interface Shipment {
  id: string; shipmentNumber: string; goodsType: string; weightKg: number;
  originCountry: string; destinationCountry: string; customsStatus: string; notes: string;
  createdAt: string; updatedAt: string;
}
interface Inspection {
  id: string; targetType: string; targetReference: string; notes: string; status: string;
  securityAlert: boolean; alertDetails: string; createdAt: string; createdBy: string;
}
interface ReportSummary {
  passengers: number; vehicles: number; shipments: number; inspections: number;
  securityAlerts: number; suspiciousInspections: number; shipmentsPending: number; generatedAt: string;
}
interface AuditRecord {
  id: string; eventType: string; userId?: string; deviceContext: string; timestamp: string;
  entityType: string; entityId?: string; action: string; result: string;
}
type Mode = 'shipments' | 'inspections' | 'reports' | 'audit';

@Component({
  selector: 'et-operations-page',
  imports: [FormsModule, RouterLink, DatePipe],
  templateUrl: './operations-page.html',
  styleUrl: './operations-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OperationsPage {
  private readonly http = inject(HttpClient);
  private readonly config = inject(APP_CONFIG);
  private readonly route = inject(ActivatedRoute);
  readonly auth = inject(AuthStore);
  readonly mode = this.route.snapshot.data['mode'] as Mode;
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly notice = signal('');
  readonly shipments = signal<Shipment[]>([]);
  readonly inspections = signal<Inspection[]>([]);
  readonly audit = signal<AuditRecord[]>([]);
  readonly report = signal<ReportSummary | null>(null);
  readonly editingId = signal<string | null>(null);
  search = '';
  draft = this.emptyDraft();

  constructor() { void this.load(); }

  get title(): string {
    return ({ shipments: 'Shipment Management', inspections: 'Security Inspections', reports: 'Reports & Analytics', audit: 'Audit Log' })[this.mode];
  }

  async load(): Promise<void> {
    this.loading.set(true); this.error.set('');
    try {
      if (this.mode === 'shipments') this.shipments.set(await this.get<Shipment[]>('/shipments'));
      else if (this.mode === 'inspections') this.inspections.set(await this.get<Inspection[]>('/inspections'));
      else if (this.mode === 'reports') this.report.set(await this.get<ReportSummary>('/reports/summary'));
      else this.audit.set(await this.get<AuditRecord[]>('/audit?limit=250'));
    } catch { this.error.set('Unable to load data. Check that ASP.NET Core and the database are running, then try again.'); }
    finally { this.loading.set(false); }
  }

  filteredShipments(): Shipment[] {
    const q = this.search.trim().toLocaleLowerCase();
    return this.shipments().filter(x => !q || [x.shipmentNumber, x.goodsType, x.originCountry, x.destinationCountry, x.customsStatus].some(v => v.toLocaleLowerCase().includes(q)));
  }
  filteredInspections(): Inspection[] {
    const q = this.search.trim().toLocaleLowerCase();
    return this.inspections().filter(x => !q || [x.targetType, x.targetReference, x.notes, x.status, x.alertDetails].some(v => v.toLocaleLowerCase().includes(q)));
  }

  editShipment(item: Shipment): void {
    this.editingId.set(item.id); this.draft = { ...this.emptyDraft(), shipmentNumber: item.shipmentNumber, goodsType: item.goodsType, weightKg: item.weightKg, originCountry: item.originCountry, destinationCountry: item.destinationCountry, customsStatus: item.customsStatus, shipmentNotes: item.notes };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
  editInspection(item: Inspection): void {
    this.editingId.set(item.id); this.draft = { ...this.emptyDraft(), targetType: item.targetType, targetReference: item.targetReference, inspectionNotes: item.notes, inspectionStatus: item.status, securityAlert: item.securityAlert, alertDetails: item.alertDetails };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
  resetForm(): void { this.editingId.set(null); this.draft = this.emptyDraft(); this.notice.set(''); }

  async save(): Promise<void> {
    this.saving.set(true); this.error.set(''); this.notice.set('');
    try {
      const id = this.editingId();
      if (this.mode === 'shipments') {
        const payload = { shipmentNumber: this.draft.shipmentNumber.trim(), goodsType: this.draft.goodsType.trim(), weightKg: Number(this.draft.weightKg), originCountry: this.draft.originCountry.trim(), destinationCountry: this.draft.destinationCountry.trim(), customsStatus: this.draft.customsStatus, notes: this.draft.shipmentNotes.trim() };
        await this.mutate(id ? 'PUT' : 'POST', id ? `/shipments/${id}` : '/shipments', payload);
      } else {
        const payload = { targetType: this.draft.targetType, targetReference: this.draft.targetReference.trim(), notes: this.draft.inspectionNotes.trim(), status: this.draft.inspectionStatus, securityAlert: this.draft.securityAlert, alertDetails: this.draft.alertDetails.trim() };
        await this.mutate(id ? 'PUT' : 'POST', id ? `/inspections/${id}` : '/inspections', payload);
      }
      this.notice.set(id ? 'Records updated successfully.' : 'Record saved successfully.'); this.resetForm(); await this.load();
    } catch (e) {
      const message = e instanceof Error ? e.message : '';
      this.error.set(message || 'Unable to save the record. Check the data and server connection.');
    } finally { this.saving.set(false); }
  }

  async remove(id: string): Promise<void> {
    if (!window.confirm('Are you sure you want to delete this record? This action cannot be undone.')) return;
    this.error.set('');
    try { await this.mutate('DELETE', `/${this.mode}/${id}`); this.notice.set('Record deleted.'); await this.load(); }
    catch { this.error.set('Unable to delete the record. Check your connection and try again.'); }
  }

  printReport(): void { window.print(); }

  exportCsv(): void {
    let rows: Array<Record<string, string | number | boolean>> = [];
    if (this.mode === 'shipments') rows = this.filteredShipments().map(x => ({ 'Shipment Number': x.shipmentNumber, 'Goods Type': x.goodsType, 'Weight (kg)': x.weightKg, 'Origin': x.originCountry, 'Destination': x.destinationCountry, 'Customs Status': x.customsStatus, 'Created At': x.createdAt }));
    else if (this.mode === 'inspections') rows = this.filteredInspections().map(x => ({ 'Type': x.targetType, 'Reference': x.targetReference, 'Status': x.status, 'Security Alert': x.securityAlert ? 'Yes' : 'No', 'Notes': x.notes, 'Date': x.createdAt }));
    else if (this.mode === 'audit') rows = this.audit().map(x => ({ 'Event': x.eventType, 'Entity': x.entityType, 'ID': x.entityId ?? '', 'Action': x.action, 'Result': x.result, 'Date': x.timestamp }));
    else if (this.report()) rows = [this.report() as unknown as Record<string, string | number | boolean>];
    if (!rows.length) { this.error.set('There is no data to export.'); return; }
    const headers = Object.keys(rows[0]);
    const csv = '\uFEFF' + [headers, ...rows.map(row => headers.map(key => String(row[key] ?? '').replaceAll('"', '""')))].map(row => row.map(v => `"${v}"`).join(',')).join('\r\n');
    const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8;' }));
    const a = document.createElement('a'); a.href = url; a.download = `eticket-${this.mode}-${new Date().toISOString().slice(0, 10)}.csv`; a.click(); URL.revokeObjectURL(url);
  }

  private async get<T>(path: string): Promise<T> { return firstValueFrom(this.http.get<T>(`${this.config.apiBaseUrl}${path}`, { withCredentials: true })); }
  private async mutate(method: 'POST' | 'PUT' | 'DELETE', path: string, body?: unknown): Promise<void> {
    await firstValueFrom(this.http.get<void>(`${this.config.apiBaseUrl}/auth/antiforgery`, { withCredentials: true }));
    await firstValueFrom(this.http.request(method, `${this.config.apiBaseUrl}${path}`, { body, withCredentials: true, responseType: 'text' }));
  }
  private emptyDraft() {
    return { shipmentNumber: '', goodsType: '', weightKg: 1, originCountry: '', destinationCountry: '', customsStatus: 'Under Inspection', shipmentNotes: '', targetType: 'Vehicle', targetReference: '', inspectionNotes: '', inspectionStatus: 'Requires Review', securityAlert: false, alertDetails: '' };
  }
}
