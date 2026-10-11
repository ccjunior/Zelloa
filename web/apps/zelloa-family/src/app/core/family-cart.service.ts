import { Injectable, signal } from '@angular/core';
import { AvailableCatalogProduct } from './family-api.service';

export interface FamilyCartItem extends AvailableCatalogProduct {
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class FamilyCartService {
  readonly items = signal<FamilyCartItem[]>([]);

  readonly itemCount = () => this.items().reduce((total, item) => total + item.quantity, 0);
  readonly estimatedTotal = () => this.items().reduce((total, item) => total + item.price * item.quantity, 0);

  add(product: AvailableCatalogProduct): void {
    this.items.update(items => {
      const existing = items.find(item => item.id === product.id);
      return existing
        ? items.map(item => item.id === product.id ? { ...item, quantity: Math.min(item.quantity + 1, 99) } : item)
        : [...items, { ...product, quantity: 1 }];
    });
  }

  setQuantity(productId: string, quantity: number): void {
    if (quantity < 1) {
      this.remove(productId);
      return;
    }
    this.items.update(items => items.map(item => item.id === productId
      ? { ...item, quantity: Math.min(quantity, 99) }
      : item));
  }

  remove(productId: string): void {
    this.items.update(items => items.filter(item => item.id !== productId));
  }

  clear(): void {
    this.items.set([]);
  }
}
