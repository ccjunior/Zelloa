import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthenticationService } from '../core/authentication.service';

@Component({
  selector: 'app-school-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './school-shell.html',
  styleUrl: './school-shell.scss',
})
export class SchoolShell {
  readonly auth = inject(AuthenticationService);
  menuOpen = false;

  async logout(): Promise<void> {
    await this.auth.logout();
  }

  closeMenu(): void { this.menuOpen = false; }
}
