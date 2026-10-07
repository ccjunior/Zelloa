import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FamilyAuthenticationService } from '../core/family-authentication.service';

function uniqueCharacterCount(control: AbstractControl): ValidationErrors | null {
  return new Set(String(control.value ?? '')).size >= 4 ? null : { uniqueCharacters: true };
}

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmation = control.get('confirmation')?.value;
  return password && confirmation && password !== confirmation ? { passwordsMismatch: true } : null;
}

@Component({
  selector: 'app-family-activate-invitation-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './activate-invitation-page.html',
  styleUrl: './activate-invitation-page.scss',
})
export class FamilyActivateInvitationPage {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(FamilyAuthenticationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly email = this.route.snapshot.queryParamMap.get('email') ?? '';
  readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';
  readonly invalidLink = !this.email || !this.token;
  readonly loading = signal(false);
  readonly activated = signal(false);
  readonly errorMessage = signal('');
  readonly form = this.formBuilder.nonNullable.group({
    password: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(256),
      Validators.pattern(/[A-Z]/), Validators.pattern(/[a-z]/), Validators.pattern(/[0-9]/),
      Validators.pattern(/[^A-Za-z0-9]/), uniqueCharacterCount]],
    confirmation: ['', Validators.required],
  }, { validators: passwordsMatch });

  async submit(): Promise<void> {
    this.form.markAllAsTouched();
    if (this.invalidLink || this.form.invalid || this.loading()) return;

    this.errorMessage.set('');
    this.loading.set(true);
    try {
      await this.auth.activate(this.email, this.token, this.form.controls.password.value);
      this.activated.set(true);
    } catch (error) {
      this.errorMessage.set(error instanceof HttpErrorResponse && error.status === 401
        ? 'Este convite é inválido ou expirou. Peça à escola que envie um novo link.'
        : error instanceof HttpErrorResponse && error.status === 400
          ? 'A senha não atende aos requisitos. Revise as orientações e tente novamente.'
          : 'Não foi possível ativar a conta agora. Tente novamente em instantes.');
    } finally {
      this.loading.set(false);
    }
  }

  async goToLogin(): Promise<void> {
    await this.router.navigate(['/login'], { queryParams: { activated: true } });
  }
}
