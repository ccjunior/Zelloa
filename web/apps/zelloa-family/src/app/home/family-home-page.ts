import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FamilyApiService, LinkedStudent } from '../core/family-api.service';
import { FamilyAuthenticationService } from '../core/family-authentication.service';

@Component({
  selector: 'app-family-home-page',
  imports: [RouterLink],
  templateUrl: './family-home-page.html',
  styleUrl: './family-home-page.scss',
})
export class FamilyHomePage implements OnInit {
  private readonly api = inject(FamilyApiService);
  private readonly auth = inject(FamilyAuthenticationService);
  private readonly router = inject(Router);

  readonly user = this.auth.user;
  readonly students = signal<LinkedStudent[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    void this.loadStudents();
  }

  async loadStudents(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');
    try {
      this.students.set(await this.api.get<LinkedStudent[]>('/api/me/students'));
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        this.auth.user.set(null);
        await this.router.navigateByUrl('/login');
        return;
      }
      this.errorMessage.set('Não foi possível carregar os vínculos agora. Tente novamente.');
    } finally {
      this.loading.set(false);
    }
  }

  async logout(): Promise<void> {
    await this.auth.logout();
  }
}
