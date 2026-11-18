import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../../core/auth/auth.store';
import { PassengerDraft, PassengerRecord, PassengerService, PassengerStatus, TravelDirection } from './passenger.service';

@Component({
  selector: 'et-passengers',
  imports: [FormsModule, RouterLink],
  templateUrl: './passengers.html',
  styleUrl: './passengers.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Passengers {
  readonly auth = inject(AuthStore);
  private readonly service = inject(PassengerService);
  readonly passengers = signal<PassengerRecord[]>([]);
  readonly search = signal('');
  readonly statusFilter = signal('All');
  readonly showForm = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly busy = signal(false);
  readonly message = signal('');
  readonly error = signal('');
  readonly statuses: PassengerStatus[] = ['Pending Verification', 'Allowed', 'Denied'];
  readonly directions: TravelDirection[] = ['Entry', 'Exit'];
  readonly form = signal<PassengerDraft>(this.emptyDraft());
  readonly filtered = computed(() => {
    const term = this.search().trim().toLocaleLowerCase();
    return this.passengers().filter((p) => {
      const matchesTerm = !term || [p.fullName, p.passportNumber, p.nationality, p.destinationCountry].some((v) => v.toLocaleLowerCase().includes(term));
      return matchesTerm && (this.statusFilter() === 'All' || p.status === this.statusFilter());
    });
  });
  readonly allowedCount = computed(() => this.passengers().filter((p) => p.status === 'Allowed').length);
  readonly reviewCount = computed(() => this.passengers().filter((p) => p.status === 'Pending Verification').length);

  constructor() { void this.load(); }

  async load(): Promise<void> {
    this.error.set('');
    try { this.passengers.set(await this.service.list()); }
    catch { this.error.set('Unable to load passenger records. Check the server connection.'); }
  }

  openCreate(): void { this.editingId.set(null); this.form.set(this.emptyDraft()); this.message.set(''); this.error.set(''); this.showForm.set(true); }
  openEdit(passenger: PassengerRecord): void {
    const { fullName, passportNumber, nationality, originCountry, destinationCountry, status, direction, notes } = passenger;
    this.editingId.set(passenger.id); this.form.set({ fullName, passportNumber, nationality, originCountry, destinationCountry, status, direction, notes }); this.message.set(''); this.error.set(''); this.showForm.set(true);
  }
  updateField<K extends keyof PassengerDraft>(key: K, value: PassengerDraft[K]): void { this.form.update((form) => ({ ...form, [key]: value })); }

  async save(): Promise<void> {
    const draft = this.form();
    if (!draft.fullName.trim() || !draft.passportNumber.trim() || !draft.nationality.trim() || !draft.originCountry.trim() || !draft.destinationCountry.trim()) {
      this.error.set('Please enter the name, passport number, nationality, origin country, and destination.'); return;
    }
    this.busy.set(true); this.error.set(''); this.message.set('');
    try {
      const id = this.editingId();
      if (id) await this.service.update(id, draft); else await this.service.create(draft);
      await this.load(); this.showForm.set(false); this.message.set(id ? 'Passenger details updated.' : 'Passenger registered successfully.');
    } catch { this.error.set('Unable to save data. Check your connection and try again.'); }
    finally { this.busy.set(false); }
  }

  async remove(passenger: PassengerRecord): Promise<void> {
    if (!globalThis.confirm(`Are you sure you want to delete passenger record ${passenger.fullName}?`)) return;
    this.error.set(''); this.message.set('');
    try { await this.service.remove(passenger.id); await this.load(); this.message.set('Passenger record deleted.'); }
    catch { this.error.set('Unable to delete the record.'); }
  }

  private emptyDraft(): PassengerDraft {
    return { fullName: '', passportNumber: '', nationality: '', originCountry: '', destinationCountry: '', status: 'Pending Verification', direction: 'Entry', notes: '' };
  }
}
