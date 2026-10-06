using Zelloa.Domain.Catalog;

namespace Zelloa.Application.Catalog;

public interface ICatalogStore
{
    Task<Category?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Category>> GetCategoriesAsync(
        bool? active, string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<Category>> GetCategoriesByIdsAsync(
        IReadOnlyCollection<Guid> categoryIds, CancellationToken cancellationToken);
    Task<int> CountCategoriesAsync(bool? active, string? search, CancellationToken cancellationToken);
    Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetProductsAsync(
        Guid? categoryId,
        bool? active,
        bool? available,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
    Task<int> CountProductsAsync(
        Guid? categoryId, bool? active, bool? available, string? search, CancellationToken cancellationToken);
    Task<IReadOnlyList<AvailableCatalogEntry>> GetAvailableCatalogAsync(CancellationToken cancellationToken);
    void Add(Category category);
    void Add(Product product);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record CategoryResponse(Guid Id, string Name, bool IsActive);
public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    decimal Price,
    string? ImageUrl,
    bool IsActive,
    bool IsAvailable);
public sealed record AvailableCatalogEntry(
    Guid CategoryId,
    string CategoryName,
    Guid ProductId,
    string ProductName,
    string? Description,
    decimal Price,
    string? ImageUrl);
public sealed record AvailableCatalogCategory(
    Guid Id,
    string Name,
    IReadOnlyList<AvailableCatalogProduct> Products);
public sealed record AvailableCatalogProduct(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl);
public sealed record AvailableCatalogResponse(IReadOnlyList<AvailableCatalogCategory> Categories);
public sealed record PagedCatalogResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
