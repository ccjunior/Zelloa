import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CatalogCategory, CatalogProduct, PagedCatalogResponse, SchoolApiService } from '../core/school-api.service';

const PRODUCT_PAGE_SIZE = 20;
const MAX_PRODUCT_PRICE = 999_999_999_999;

@Component({
  selector: 'app-catalog-page',
  imports: [CurrencyPipe, ReactiveFormsModule],
  templateUrl: './catalog-page.html',
  styleUrl: './school-pages.scss',
})
export class CatalogPage {
  private readonly api = inject(SchoolApiService);
  private readonly fb = inject(FormBuilder);

  readonly categoryForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
  });
  readonly productForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    categoryId: ['', Validators.required],
    price: [0.01, [Validators.required, Validators.min(0.01), Validators.max(MAX_PRODUCT_PRICE), Validators.pattern(/^\d+(\.\d{1,2})?$/)]],
    description: ['', Validators.maxLength(2000)],
    imageUrl: ['', [Validators.maxLength(2048), Validators.pattern(/^https:\/\/.+/i)]],
  });

  readonly categories = signal<CatalogCategory[]>([]);
  readonly products = signal<CatalogProduct[]>([]);
  readonly section = signal<'products' | 'categories'>('categories');
  readonly showProductForm = signal(false);
  readonly showCategoryForm = signal(false);
  readonly totalProducts = signal(0);
  readonly page = signal(1);
  readonly categoryFilter = signal('');
  readonly searchText = signal('');
  readonly searchQuery = signal('');
  readonly loadingCategories = signal(true);
  readonly loadingProducts = signal(true);
  readonly savingCategory = signal(false);
  readonly savingProduct = signal(false);
  readonly savingCategoryId = signal<string | null>(null);
  readonly savingProductId = signal<string | null>(null);
  readonly editingCategoryId = signal<string | null>(null);
  readonly editingProductId = signal<string | null>(null);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  constructor() {
    void this.loadCategories();
    void this.loadProducts();
  }

  get pageCount(): number {
    return Math.max(1, Math.ceil(this.totalProducts() / PRODUCT_PAGE_SIZE));
  }

  async loadCategories(): Promise<void> {
    this.loadingCategories.set(true);
    try {
      const response = await this.api.get<PagedCatalogResponse<CatalogCategory>>('/api/catalog/categories?page=1&pageSize=100');
      this.categories.set(response.items);
      this.errorMessage.set('');
    } catch {
      this.errorMessage.set('Não foi possível carregar as categorias.');
    } finally {
      this.loadingCategories.set(false);
    }
  }

  async loadProducts(page = this.page()): Promise<void> {
    this.loadingProducts.set(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: String(PRODUCT_PAGE_SIZE) });
      if (this.categoryFilter()) params.set('categoryId', this.categoryFilter());
      if (this.searchQuery()) params.set('search', this.searchQuery());
      const response = await this.api.get<PagedCatalogResponse<CatalogProduct>>(`/api/catalog/products?${params.toString()}`);
      this.products.set(response.items);
      this.totalProducts.set(response.totalCount);
      this.page.set(response.page);
      this.errorMessage.set('');
    } catch {
      this.errorMessage.set('Não foi possível carregar os produtos.');
    } finally {
      this.loadingProducts.set(false);
    }
  }

  async submitCategory(): Promise<void> {
    this.categoryForm.markAllAsTouched();
    if (this.categoryForm.invalid || this.savingCategory()) return;

    this.savingCategory.set(true);
    this.clearNotices();
    try {
      const categoryId = this.editingCategoryId();
      const request = { name: this.categoryForm.controls.name.value.trim() };
      if (categoryId) {
        await this.api.put<CatalogCategory>(`/api/catalog/categories/${categoryId}`, request);
        this.successMessage.set('Categoria atualizada.');
      } else {
        await this.api.post<CatalogCategory>('/api/catalog/categories', request);
        this.successMessage.set('Categoria criada.');
      }
      this.cancelCategoryForm();
      await this.loadCategories();
      await this.loadProducts(this.page());
    } catch {
      this.errorMessage.set('Não foi possível salvar a categoria. Confira os dados e tente novamente.');
    } finally {
      this.savingCategory.set(false);
    }
  }

  beginCategoryCreate(): void {
    this.editingCategoryId.set(null);
    this.categoryForm.reset({ name: '' });
    this.showCategoryForm.set(true);
    this.clearNotices();
  }

  beginCategoryEdit(category: CatalogCategory): void {
    this.editingCategoryId.set(category.id);
    this.categoryForm.reset({ name: category.name });
    this.showCategoryForm.set(true);
    this.clearNotices();
  }

  cancelCategoryForm(): void {
    this.editingCategoryId.set(null);
    this.categoryForm.reset({ name: '' });
    this.showCategoryForm.set(false);
  }

  async toggleCategory(category: CatalogCategory): Promise<void> {
    if (this.savingCategoryId()) return;
    this.savingCategoryId.set(category.id);
    this.clearNotices();
    try {
      await this.api.patch<CatalogCategory>(`/api/catalog/categories/${category.id}/status`, {
        status: category.isActive ? 'Inactive' : 'Active',
      });
      this.successMessage.set(category.isActive ? 'Categoria desativada.' : 'Categoria ativada.');
      await this.loadCategories();
      await this.loadProducts(this.page());
    } catch {
      this.errorMessage.set('Não foi possível alterar o status da categoria.');
    } finally {
      this.savingCategoryId.set(null);
    }
  }

  beginProductEdit(product: CatalogProduct): void {
    this.editingProductId.set(product.id);
    this.showProductForm.set(true);
    this.productForm.reset({
      name: product.name,
      categoryId: product.categoryId,
      price: product.price,
      description: product.description ?? '',
      imageUrl: product.imageUrl ?? '',
    });
    this.clearNotices();
  }

  cancelProductEdit(): void {
    this.editingProductId.set(null);
    this.showProductForm.set(false);
    this.productForm.reset({ name: '', categoryId: '', price: 0.01, description: '', imageUrl: '' });
  }

  beginProductCreate(): void {
    this.editingProductId.set(null);
    this.productForm.reset({ name: '', categoryId: '', price: 0.01, description: '', imageUrl: '' });
    this.showProductForm.set(true);
    this.clearNotices();
  }

  selectSection(section: 'products' | 'categories'): void {
    if (section === this.section()) return;
    this.section.set(section);
    this.cancelProductEdit();
    this.cancelCategoryForm();
    this.clearNotices();
  }

  async submitProduct(): Promise<void> {
    this.productForm.markAllAsTouched();
    if (this.productForm.invalid || this.savingProduct()) return;

    this.savingProduct.set(true);
    this.clearNotices();
    const value = this.productForm.getRawValue();
    const request = {
      name: value.name.trim(),
      categoryId: value.categoryId,
      price: value.price,
      description: value.description.trim() || null,
      imageUrl: value.imageUrl.trim() || null,
    };

    try {
      const productId = this.editingProductId();
      if (productId) {
        await this.api.put<CatalogProduct>(`/api/catalog/products/${productId}`, request);
        this.successMessage.set('Produto atualizado.');
        this.cancelProductEdit();
      } else {
        await this.api.post<CatalogProduct>('/api/catalog/products', { ...request, available: true });
        this.successMessage.set('Produto criado e disponível para compra.');
      }
      this.cancelProductEdit();
      await this.loadProducts(this.page());
    } catch {
      this.errorMessage.set('Não foi possível salvar o produto. Confira a categoria, o preço e os dados informados.');
    } finally {
      this.savingProduct.set(false);
    }
  }

  async toggleProductStatus(product: CatalogProduct): Promise<void> {
    if (this.savingProductId()) return;
    this.savingProductId.set(product.id);
    this.clearNotices();
    try {
      await this.api.patch<CatalogProduct>(`/api/catalog/products/${product.id}/status`, {
        status: product.isActive ? 'Inactive' : 'Active',
      });
      this.successMessage.set(product.isActive ? 'Produto desativado.' : 'Produto ativado.');
      await this.loadProducts(this.page());
    } catch {
      this.errorMessage.set('Não foi possível alterar o status do produto.');
    } finally {
      this.savingProductId.set(null);
    }
  }

  async toggleProductAvailability(product: CatalogProduct): Promise<void> {
    if (this.savingProductId()) return;
    this.savingProductId.set(product.id);
    this.clearNotices();
    try {
      await this.api.patch<CatalogProduct>(`/api/catalog/products/${product.id}/availability`, {
        available: !product.isAvailable,
      });
      this.successMessage.set(product.isAvailable ? 'Produto marcado como indisponível.' : 'Produto disponível para compra.');
      await this.loadProducts(this.page());
    } catch {
      this.errorMessage.set('Não foi possível alterar a disponibilidade do produto.');
    } finally {
      this.savingProductId.set(null);
    }
  }

  async applyFilters(): Promise<void> {
    this.searchQuery.set(this.searchText().trim());
    this.page.set(1);
    await this.loadProducts(1);
  }

  async setCategoryFilter(categoryId: string): Promise<void> {
    this.categoryFilter.set(categoryId);
    this.page.set(1);
    await this.loadProducts(1);
  }

  async changePage(page: number): Promise<void> {
    if (page < 1 || page > this.pageCount || page === this.page()) return;
    await this.loadProducts(page);
  }

  private clearNotices(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
  }
}
