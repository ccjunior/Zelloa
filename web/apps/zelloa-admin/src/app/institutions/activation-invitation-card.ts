import { Component, input, output, signal } from '@angular/core';
import { InvitationResponse } from '../core/platform-api.service';

@Component({
  selector: 'app-activation-invitation-card',
  templateUrl: './activation-invitation-card.html',
  styleUrl: './activation-invitation-card.scss',
})
export class ActivationInvitationCard {
  readonly invitation = input.required<InvitationResponse>();
  readonly startAnotherLabel = input('Cadastrar outra instituição');
  readonly startAnother = output<void>();
  readonly copied = signal(false);
  readonly copyMessage = signal('');

  get formattedExpiry(): string {
    return new Intl.DateTimeFormat('pt-BR', { dateStyle: 'long', timeStyle: 'short' }).format(new Date(this.invitation().expiresAt));
  }

  async copyActivationLink(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.invitation().activationUrl);
      this.copied.set(true);
      this.copyMessage.set('Link copiado. Envie-o diretamente ao administrador escolar.');
    } catch {
      this.copyMessage.set('Não foi possível copiar automaticamente. Selecione e copie o link acima.');
    }
  }
}
