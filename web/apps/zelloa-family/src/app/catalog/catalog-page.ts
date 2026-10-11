import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AvailableCatalogCategory, AvailableCatalogResponse, FamilyApiService } from '../core/family-api.service';
import { FamilyAuthenticationService } from '../core/family-authentication.service';
import { FamilyCartService } from '../core/family-cart.service';

@Component({
  selector: 'app-family-catalog-page',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './catalog-page.html',
  styleUrl: './catalog-page.scss',
})
export class FamilyCatalogPage implements OnInit {
  private readonly api = inject(FamilyApiService);
  private readonly auth = inject(FamilyAuthenticationService);
  private readonly router = inject(Router);
  readonly cart = inject(FamilyCartService);

  readonly categories = signal<AvailableCatalogCategory[]>([]);
  readonly selectedCategoryId = signal<string | null>(null);
  readonly visibleCategories = computed(() => {
    const selected = this.selectedCategoryId();
    return selected === null
      ? this.categories()
      : this.categories().filter(category => category.id === selected);
  });
  readonly loading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    void this.loadCatalog();
  }

  async loadCatalog(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');
    try {
      const response = await this.api.get<AvailableCatalogResponse>('/api/catalog/available');
      this.categories.set(response.categories);
      if (!response.categories.some(category => category.id === this.selectedCategoryId())) {
        this.selectedCategoryId.set(null);
      }
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        this.auth.user.set(null);
        await this.router.navigateByUrl('/login');
        return;
      }
      this.errorMessage.set('Não foi possível carregar o cardápio. Tente novamente.');
    } finally {
      this.loading.set(false);
    }
  }

  selectCategory(categoryId: string | null): void {
    this.selectedCategoryId.set(categoryId);
  }

  hideImage(event: Event): void {
    (event.target as HTMLImageElement).hidden = true;
  }

  addToCart(product: AvailableCatalogCategory['products'][number]): void {
    this.cart.add(product);
  }

  quantityInCart(productId: string): number {
    return this.cart.items().find(item => item.id === productId)?.quantity ?? 0;
  }
}
