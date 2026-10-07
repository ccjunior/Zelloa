import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { PlatformAuthenticationService } from '../core/platform-authentication.service';

@Component({
  selector: 'app-admin-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './admin-shell.html',
  styleUrl: './admin-shell.scss',
})
export class AdminShell {
  readonly auth = inject(PlatformAuthenticationService);
  menuOpen = false;

  async logout(): Promise<void> {
    await this.auth.logout();
  }

  closeMenu(): void {
    this.menuOpen = false;
  }
}
