import { Component, input, output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

export interface SchoolFormValue {
  name: string;
  tradeName: string;
  identifier: string;
  contactEmail: string;
  contactPhone: string;
  timeZone: string;
}

@Component({
  selector: 'app-institution-details-form',
  imports: [ReactiveFormsModule],
  templateUrl: './institution-details-form.html',
  styleUrl: './institution-details-form.scss',
})
export class InstitutionDetailsForm {
  private readonly formBuilder = inject(FormBuilder);

  readonly submitting = input(false);
  readonly submitted = output<SchoolFormValue>();
  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    tradeName: ['', [Validators.required, Validators.maxLength(200)]],
    identifier: ['', Validators.maxLength(64)],
    contactEmail: ['', [Validators.email, Validators.maxLength(256)]],
    contactPhone: ['', Validators.maxLength(32)],
    timeZone: ['America/Bahia', [Validators.required, Validators.maxLength(100)]],
  });

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting()) return;
    this.submitted.emit(this.form.getRawValue());
  }
}
