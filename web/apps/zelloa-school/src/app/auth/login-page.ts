import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthenticationService } from '../core/authentication.service';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule, RouterLink],
  styleUrl: './login-page.scss',
  templateUrl: './login-page.html',
})
export class LoginPage {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(AuthenticationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.maxLength(256)]],
  });

  readonly loading = signal(false);
  readonly showPassword = signal(false);
  readonly errorMessage = signal('');
  activated = this.route.snapshot.queryParamMap.get('activated') === 'true';
  readonly year = new Date().getFullYear();

  async submit(): Promise<void> {
    this.errorMessage.set('');
    this.form.markAllAsTouched();
    if (this.form.invalid || this.loading()) return;

    this.loading.set(true);
    const { email, password } = this.form.getRawValue();
    try {
      await this.auth.login(email, password);
      const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
      const safeReturnUrl = returnUrl?.startsWith('/') && !returnUrl.startsWith('//')
        ? returnUrl
        : '/';
      await this.router.navigateByUrl(safeReturnUrl);
    } catch (error) {
      this.errorMessage.set(error instanceof Error && !(error instanceof HttpErrorResponse)
        ? error.message
        : error instanceof HttpErrorResponse && error.status === 401
          ? 'E-mail ou senha inválidos.'
          : 'Não foi possível entrar agora. Confira sua conexão e tente novamente.');
    } finally {
      this.loading.set(false);
    }
  }
}
