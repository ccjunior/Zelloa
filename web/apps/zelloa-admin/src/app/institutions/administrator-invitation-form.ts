import { Component, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-administrator-invitation-form',
  imports: [ReactiveFormsModule],
  templateUrl: './administrator-invitation-form.html',
  styleUrl: './administrator-invitation-form.scss',
})
export class AdministratorInvitationForm {
  private readonly formBuilder = inject(FormBuilder);

  readonly submitting = input(false);
  readonly submitted = output<{ displayName: string; email: string }>();
  readonly form = this.formBuilder.nonNullable.group({
    displayName: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
  });

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting()) return;
    this.submitted.emit(this.form.getRawValue());
  }
}
