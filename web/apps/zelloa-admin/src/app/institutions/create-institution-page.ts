import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { AdministratorInvitationForm } from './administrator-invitation-form';
import { ActivationInvitationCard } from './activation-invitation-card';
import { InstitutionDetailsForm, SchoolFormValue } from './institution-details-form';
import { InvitationResponse, PlatformApiService, SchoolResponse } from '../core/platform-api.service';

@Component({
  selector: 'app-create-institution-page',
  imports: [InstitutionDetailsForm, AdministratorInvitationForm, ActivationInvitationCard],
  templateUrl: './create-institution-page.html',
  styleUrl: './create-institution-page.scss',
})
export class CreateInstitutionPage {
  private readonly api = inject(PlatformApiService);

  readonly school = signal<SchoolResponse | null>(null);
  readonly invitation = signal<InvitationResponse | null>(null);
  readonly creatingSchool = signal(false);
  readonly invitingAdministrator = signal(false);
  readonly errorMessage = signal('');

  async createSchool(value: SchoolFormValue): Promise<void> {
    this.errorMessage.set('');
    if (this.creatingSchool()) return;

    this.creatingSchool.set(true);
    const body = {
      ...value,
      name: value.name.trim(),
      tradeName: value.tradeName.trim(),
      identifier: value.identifier.trim() || null,
      contactEmail: value.contactEmail.trim() || null,
      contactPhone: value.contactPhone.trim() || null,
      timeZone: value.timeZone.trim(),
    };

    try {
      this.school.set(await this.api.post<SchoolResponse>('/api/schools', body));
    } catch (error) {
      this.errorMessage.set(error instanceof HttpErrorResponse && error.status === 0
        ? 'A conexão caiu antes da confirmação. Verifique com a equipe se a instituição foi criada antes de tentar novamente.'
        : error instanceof HttpErrorResponse && error.status === 401
          ? 'Sua sessão expirou. Entre novamente para continuar.'
          : error instanceof HttpErrorResponse && error.status === 403
            ? 'Sua conta não tem permissão para cadastrar instituições.'
            : 'Não foi possível cadastrar a instituição. Confira os dados e tente novamente.');
    } finally {
      this.creatingSchool.set(false);
    }
  }

  async inviteAdministrator(value: { displayName: string; email: string }): Promise<void> {
    const school = this.school();
    if (!school) return;
    this.errorMessage.set('');
    if (this.invitingAdministrator()) return;

    this.invitingAdministrator.set(true);
    try {
      const invitation = await this.api.post<InvitationResponse>(
        `/api/schools/${school.id}/administrator-invitation`,
        { displayName: value.displayName.trim(), email: value.email.trim() },
      );
      this.invitation.set(invitation);
    } catch (error) {
      this.errorMessage.set(error instanceof HttpErrorResponse && error.status === 409
        ? 'Já existe uma conta ou convite ativo para esse e-mail. Informe outro e-mail.'
        : error instanceof HttpErrorResponse && error.status === 404
          ? 'A instituição não foi localizada. Atualize a página e verifique o cadastro antes de convidar.'
          : error instanceof HttpErrorResponse && error.status === 401
            ? 'Sua sessão expirou. Entre novamente para continuar.'
            : error instanceof HttpErrorResponse && error.status === 403
              ? 'Sua conta não tem permissão para convidar administradores escolares.'
              : 'Não foi possível gerar o convite. Confira o e-mail e tente novamente.');
    } finally {
      this.invitingAdministrator.set(false);
    }
  }

  startAnotherSchool(): void {
    this.school.set(null);
    this.invitation.set(null);
    this.errorMessage.set('');
  }
}
