import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FamilyAuthenticationService } from '../core/family-authentication.service';

@Component({
  selector: 'app-family-login-page',
  imports: [ReactiveFormsModule],
  templateUrl: './login-page.html',
  styleUrl: './login-page.scss',
})
export class FamilyLoginPage {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(FamilyAuthenticationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.maxLength(256)]],
  });
  readonly loading = signal(false);
  readonly showPassword = signal(false);
  readonly errorMessage = signal('');
  readonly activated = this.route.snapshot.queryParamMap.get('activated') === 'true';

  async submit(): Promise<void> {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.loading()) return;

    this.errorMessage.set('');
    this.loading.set(true);
    const { email, password } = this.form.getRawValue();
    try {
      await this.auth.login(email.trim(), password);
      const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
      const safeReturnUrl = returnUrl?.startsWith('/') && !returnUrl.startsWith('//') ? returnUrl : '/';
      await this.router.navigateByUrl(safeReturnUrl);
    } catch (error) {
      this.errorMessage.set(error instanceof Error && !(error instanceof HttpErrorResponse)
        ? error.message
        : error instanceof HttpErrorResponse && error.status === 401
          ? 'E-mail ou senha inválidos.'
          : 'Não foi possível entrar agora. Confira a conexão e tente novamente.');
    } finally {
      this.loading.set(false);
    }
  }

  togglePassword(): void {
    this.showPassword.update(value => !value);
  }
}
