import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OperatingContextStore } from '../../core/operating-context/operating-context.store';
import { VehicleEntryStore } from '../../core/vehicle-entry/vehicle-entry.store';
import { VehicleEntryDecision } from '../../domain/models/vehicle-visit';

@Component({
  selector: 'et-vehicle-entry',
  imports: [DatePipe, ReactiveFormsModule, RouterLink],
  templateUrl: './vehicle-entry.html',
  styleUrl: './vehicle-entry.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VehicleEntry {
  readonly operatingContext = inject(OperatingContextStore);
  readonly store = inject(VehicleEntryStore);
  private readonly formBuilder = inject(FormBuilder);
  private idempotencyKey = globalThis.crypto.randomUUID();

  readonly form = this.formBuilder.group({
    plateNumber: ['', [plateNumberValidator(true)]],
    brand: ['', [Validators.required, Validators.maxLength(80)]],
    type: ['', [Validators.required, Validators.maxLength(80)]],
    isServiceVehicle: this.formBuilder.nonNullable.control(false),
    personsCount: [
      1 as number | null,
      [Validators.required, Validators.min(1), Validators.max(100)],
    ],
    trailerOne: ['', [plateNumberValidator(false)]],
    trailerTwo: ['', [plateNumberValidator(false)]],
    photoReference: ['', [Validators.maxLength(2048)]],
  });

  readonly supportsEntry = computed(() => {
    const mode = this.operatingContext.current()?.mode;
    return mode === 'rearExit' || mode === 'borderEntry';
  });

  constructor() {
    this.form.controls.isServiceVehicle.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((isServiceVehicle) => this.applyServiceVehicleRules(isServiceVehicle));
    this.form.controls.plateNumber.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.store.clearQueueLookup());
  }

  preventSubmit(event: Event): void {
    event.preventDefault();
  }

  async lookupQueue(): Promise<void> {
    const control = this.form.controls.plateNumber;
    control.markAsTouched();
    if (control.invalid || !control.value) {
      return;
    }

    await this.store.lookupQueue(control.value);
  }

  async submit(decision: VehicleEntryDecision): Promise<void> {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.store.isBusy() || this.store.created() || !this.supportsEntry()) {
      return;
    }

    const value = this.form.getRawValue();
    await this.store.create({
      plateNumber: value.plateNumber ?? '',
      brand: value.brand ?? '',
      type: value.type ?? '',
      isServiceVehicle: value.isServiceVehicle,
      personsCount: value.isServiceVehicle ? null : value.personsCount,
      trailerPlateNumbers: value.isServiceVehicle
        ? []
        : [value.trailerOne, value.trailerTwo].filter((plate): plate is string => Boolean(plate)),
      photoReference: value.photoReference?.trim() || null,
      decision,
      idempotencyKey: this.idempotencyKey,
    });
  }

  startNewEntry(): void {
    this.idempotencyKey = globalThis.crypto.randomUUID();
    this.store.reset();
    this.form.reset({
      plateNumber: '',
      brand: '',
      type: '',
      isServiceVehicle: false,
      personsCount: 1,
      trailerOne: '',
      trailerTwo: '',
      photoReference: '',
    });
    this.applyServiceVehicleRules(false);
  }

  private applyServiceVehicleRules(isServiceVehicle: boolean): void {
    const conditionalControls = [
      this.form.controls.personsCount,
      this.form.controls.trailerOne,
      this.form.controls.trailerTwo,
    ];

    if (isServiceVehicle) {
      this.form.controls.personsCount.setValue(null, { emitEvent: false });
      this.form.controls.trailerOne.setValue('', { emitEvent: false });
      this.form.controls.trailerTwo.setValue('', { emitEvent: false });
      conditionalControls.forEach((control) => control.disable({ emitEvent: false }));
      return;
    }

    conditionalControls.forEach((control) => control.enable({ emitEvent: false }));
    if (this.form.controls.personsCount.value === null) {
      this.form.controls.personsCount.setValue(1, { emitEvent: false });
    }
  }
}

function plateNumberValidator(required: boolean): ValidatorFn {
  return (control: AbstractControl<unknown>): ValidationErrors | null => {
    const raw = typeof control.value === 'string' ? control.value.trim() : '';
    if (!raw) {
      return required ? { required: true } : null;
    }

    const normalized = raw
      .normalize('NFKC')
      .toLocaleUpperCase('en-US')
      .replace(/[\s\-‐‑]/gu, '');
    return /^[\p{L}\p{N}]{2,16}$/u.test(normalized) ? null : { plateNumber: true };
  };
}
