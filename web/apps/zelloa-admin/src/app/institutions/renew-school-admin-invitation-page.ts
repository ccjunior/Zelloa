import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivationInvitationCard } from './activation-invitation-card';
import { InvitationResponse, PlatformApiService } from '../core/platform-api.service';

@Component({
  selector: 'app-renew-school-admin-invitation-page',
  imports: [ReactiveFormsModule, ActivationInvitationCard],
  templateUrl: './renew-school-admin-invitation-page.html',
  styleUrl: './renew-school-admin-invitation-page.scss',
})
export class RenewSchoolAdminInvitationPage {
  private readonly formBuilder = inject(FormBuilder);
  private readonly api = inject(PlatformApiService);

  readonly submitting = signal(false);
  readonly errorMessage = signal('');
  readonly invitation = signal<InvitationResponse | null>(null);
  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
  });

  async submit(): Promise<void> {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting()) return;

    this.errorMessage.set('');
    this.submitting.set(true);
    try {
      const response = await this.api.post<InvitationResponse>(
        '/api/school-administrator-invitations/renew',
        { email: this.form.controls.email.value.trim() },
      );
      this.invitation.set(response);
    } catch (error) {
      this.errorMessage.set(error instanceof HttpErrorResponse && error.status === 404
        ? 'Não encontrei um convite pendente de administrador escolar com esse e-mail.'
        : error instanceof HttpErrorResponse && error.status === 409
          ? 'Essa conta já foi ativada e não precisa de um novo convite.'
          : error instanceof HttpErrorResponse && error.status === 401
            ? 'Sua sessão expirou. Entre novamente para continuar.'
            : error instanceof HttpErrorResponse && error.status === 403
              ? 'Sua conta não tem permissão para renovar convites escolares.'
              : 'Não foi possível renovar o convite. Confira o e-mail e tente novamente.');
    } finally {
      this.submitting.set(false);
    }
  }

  startAnotherRenewal(): void {
    this.invitation.set(null);
    this.errorMessage.set('');
  }
}
