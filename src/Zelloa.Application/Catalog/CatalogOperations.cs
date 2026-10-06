using Zelloa.Application.Identity;
using Zelloa.Domain.Catalog;

namespace Zelloa.Application.Catalog;

public sealed class CategoryOperations(ICatalogStore store, ITenantContext tenantContext)
{
    public async Task<CategoryResponse> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var category = new Category(Guid.NewGuid(), tenantContext.TenantId, name);
        store.Add(category);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(
        Guid categoryId, string name, CancellationToken cancellationToken)
    {
        var category = await store.GetCategoryAsync(categoryId, cancellationToken);
        if (category is null) return null;
        category.Update(name);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(category);
    }

    public async Task<CategoryResponse?> SetStatusAsync(
        Guid categoryId, bool active, CancellationToken cancellationToken)
    {
        var category = await store.GetCategoryAsync(categoryId, cancellationToken);
        if (category is null) return null;
        category.SetStatus(active);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(category);
    }

    public async Task<PagedCatalogResponse<CategoryResponse>> ListAsync(
        bool? active, string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var total = await store.CountCategoriesAsync(active, search, cancellationToken);
        var categories = await store.GetCategoriesAsync(active, search, page, pageSize, cancellationToken);
        return new(categories.Select(ToResponse).ToArray(), page, pageSize, total);
    }

    private static CategoryResponse ToResponse(Category category) =>
        new(category.Id, category.Name, category.IsActive);
}

public sealed class ProductOperations(ICatalogStore store, ITenantContext tenantContext)
{
    public async Task<ProductResponse?> CreateAsync(
        string name,
        string? description,
        Guid categoryId,
        decimal price,
        string? imageUrl,
        bool available,
        CancellationToken cancellationToken)
    {
        var category = await store.GetCategoryAsync(categoryId, cancellationToken);
        if (category is null) return null;
        var product = new Product(
            Guid.NewGuid(), tenantContext.TenantId, categoryId, name, description, price, imageUrl, available);
        store.Add(product);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(product, category.Name);
    }

    public async Task<ProductResponse?> UpdateAsync(
        Guid productId,
        string name,
        string? description,
        Guid categoryId,
        decimal price,
        string? imageUrl,
        CancellationToken cancellationToken)
    {
        var product = await store.GetProductAsync(productId, cancellationToken);
        var category = await store.GetCategoryAsync(categoryId, cancellationToken);
        if (product is null || category is null) return null;
        product.Update(name, categoryId, description, price, imageUrl);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(product, category.Name);
    }

    public async Task<ProductResponse?> SetAvailabilityAsync(
        Guid productId, bool available, CancellationToken cancellationToken)
    {
        var product = await store.GetProductAsync(productId, cancellationToken);
        if (product is null) return null;
        product.SetAvailability(available);
        await store.SaveChangesAsync(cancellationToken);
        var category = await store.GetCategoryAsync(product.CategoryId, cancellationToken);
        return category is null ? null : ToResponse(product, category.Name);
    }

    public async Task<ProductResponse?> SetStatusAsync(
        Guid productId, bool active, CancellationToken cancellationToken)
    {
        var product = await store.GetProductAsync(productId, cancellationToken);
        if (product is null) return null;
        product.SetStatus(active);
        await store.SaveChangesAsync(cancellationToken);
        var category = await store.GetCategoryAsync(product.CategoryId, cancellationToken);
        return category is null ? null : ToResponse(product, category.Name);
    }

    public async Task<ProductResponse?> GetAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await store.GetProductAsync(productId, cancellationToken);
        if (product is null) return null;
        var category = await store.GetCategoryAsync(product.CategoryId, cancellationToken);
        return category is null ? null : ToResponse(product, category.Name);
    }

    public async Task<PagedCatalogResponse<ProductResponse>> ListAsync(
        Guid? categoryId,
        bool? active,
        bool? available,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var total = await store.CountProductsAsync(categoryId, active, available, search, cancellationToken);
        var products = await store.GetProductsAsync(
            categoryId, active, available, search, page, pageSize, cancellationToken);
        var categoryIds = products.Select(product => product.CategoryId).Distinct().ToArray();
        var categories = await store.GetCategoriesByIdsAsync(categoryIds, cancellationToken);
        var categoryNames = categories.ToDictionary(category => category.Id, category => category.Name);
        var items = products.Where(product => categoryNames.ContainsKey(product.CategoryId))
            .Select(product => ToResponse(product, categoryNames[product.CategoryId])).ToArray();
        return new(items, page, pageSize, total);
    }

    public async Task<AvailableCatalogResponse> GetAvailableCatalogAsync(CancellationToken cancellationToken)
    {
        var entries = await store.GetAvailableCatalogAsync(cancellationToken);
        var categories = entries.GroupBy(entry => new { entry.CategoryId, entry.CategoryName })
            .OrderBy(group => group.Key.CategoryName)
            .Select(group => new AvailableCatalogCategory(
                group.Key.CategoryId,
                group.Key.CategoryName,
                group.OrderBy(entry => entry.ProductName)
                    .Select(entry => new AvailableCatalogProduct(
                        entry.ProductId, entry.ProductName, entry.Description, entry.Price, entry.ImageUrl))
                    .ToArray()))
            .ToArray();
        return new(categories);
    }

    private static ProductResponse ToResponse(Product product, string categoryName) =>
        new(product.Id, product.Name, product.Description, product.CategoryId, categoryName,
            product.Price, product.ImageUrl, product.IsActive, product.IsAvailable);
}
