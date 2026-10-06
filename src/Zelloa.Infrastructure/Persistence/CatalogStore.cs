using Microsoft.EntityFrameworkCore;
using Zelloa.Application.Catalog;
using Zelloa.Domain.Catalog;

namespace Zelloa.Infrastructure.Persistence;

public sealed class CatalogStore(ZelloaDbContext dbContext) : ICatalogStore
{
    public Task<Category?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken) =>
        dbContext.Categories.FirstOrDefaultAsync(category => category.Id == categoryId, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(
        bool? active, string? search, int page, int pageSize, CancellationToken cancellationToken) =>
        await FilterCategories(active, search)
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Category>> GetCategoriesByIdsAsync(
        IReadOnlyCollection<Guid> categoryIds, CancellationToken cancellationToken)
    {
        if (categoryIds.Count == 0) return [];
        return await dbContext.Categories.AsNoTracking()
            .Where(category => categoryIds.Contains(category.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountCategoriesAsync(bool? active, string? search, CancellationToken cancellationToken) =>
        FilterCategories(active, search).CountAsync(cancellationToken);

    public Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken) =>
        dbContext.Products.FirstOrDefaultAsync(product => product.Id == productId, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetProductsAsync(
        Guid? categoryId,
        bool? active,
        bool? available,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        await FilterProducts(categoryId, active, available, search)
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountProductsAsync(
        Guid? categoryId, bool? active, bool? available, string? search, CancellationToken cancellationToken) =>
        FilterProducts(categoryId, active, available, search).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<AvailableCatalogEntry>> GetAvailableCatalogAsync(
        CancellationToken cancellationToken) =>
        await (
            from category in dbContext.Categories.AsNoTracking()
            join product in dbContext.Products.AsNoTracking()
                on new { category.TenantId, Id = category.Id }
                equals new { product.TenantId, Id = product.CategoryId }
            where category.IsActive && product.IsActive && product.IsAvailable
            orderby category.Name, product.Name
            select new AvailableCatalogEntry(
                category.Id,
                category.Name,
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.ImageUrl))
            .ToListAsync(cancellationToken);

    public void Add(Category category) => dbContext.Categories.Add(category);
    public void Add(Product product) => dbContext.Products.Add(product);

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _ = await dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<Category> FilterCategories(bool? active, string? search)
    {
        var query = dbContext.Categories.AsQueryable();
        if (active is not null) query = query.Where(category => category.IsActive == active);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(category => EF.Functions.ILike(category.Name, $"%{term}%"));
        }
        return query;
    }

    private IQueryable<Product> FilterProducts(Guid? categoryId, bool? active, bool? available, string? search)
    {
        var query = dbContext.Products.AsQueryable();
        if (categoryId is not null) query = query.Where(product => product.CategoryId == categoryId);
        if (active is not null) query = query.Where(product => product.IsActive == active);
        if (available is not null) query = query.Where(product => product.IsAvailable == available);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(product => EF.Functions.ILike(product.Name, $"%{term}%"));
        }
        return query;
    }
}
