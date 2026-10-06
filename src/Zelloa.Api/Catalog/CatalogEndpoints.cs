using Microsoft.AspNetCore.Antiforgery;
using Zelloa.Application.Identity;
using Zelloa.Api.SchoolAcademic;
using Zelloa.Application.Catalog;

namespace Zelloa.Api.Catalog;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api/catalog").WithTags("Catalog");

        api.MapPost("/categories", async (
                CategoryRequest request,
                CategoryOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidName(request.Name, 100)) return Results.BadRequest();
                var category = await operations.CreateAsync(request.Name, cancellationToken);
                return Results.Json(category, statusCode: StatusCodes.Status201Created);
            })
            .RequireAuthorization("CanManageCatalog")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("CreateCategory");

        api.MapPut("/categories/{categoryId:guid}", async (
                Guid categoryId,
                CategoryRequest request,
                CategoryOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidName(request.Name, 100)) return Results.BadRequest();
                var category = await operations.UpdateAsync(categoryId, request.Name, cancellationToken);
                return category is null ? Results.NotFound() : Results.Ok(category);
            })
            .RequireAuthorization("CanManageCatalog")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("UpdateCategory");

        api.MapPatch("/categories/{categoryId:guid}/status", async (
                Guid categoryId,
                StatusRequest request,
                CategoryOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!TryParseStatusRequest(request.Status, out var active)) return Results.BadRequest();
                var category = await operations.SetStatusAsync(categoryId, active, cancellationToken);
                return category is null ? Results.NotFound() : Results.Ok(category);
            })
            .RequireAuthorization("CanManageCatalog")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("SetCategoryStatus");

        api.MapGet("/categories", async (
                string? status,
                string? search,
                int? page,
                int? pageSize,
                CategoryOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!TryParseStatus(status, out var active)
                    || !IsValidSearch(search)
                    || !IsValidPaging(page, pageSize))
                    return Results.BadRequest();
                return Results.Ok(await operations.ListAsync(
                    active, search, page ?? 1, pageSize ?? 20, cancellationToken));
            })
            .RequireAuthorization("CanManageCatalog")
            .WithName("GetCategories");

        api.MapPost("/products", async (
                CreateProductRequest request,
                ProductOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidProduct(request.Name, request.Description, request.CategoryId, request.Price, request.ImageUrl))
                    return Results.BadRequest();
                var product = await operations.CreateAsync(
                    request.Name,
                    request.Description,
                    request.CategoryId,
                    request.Price,
                    request.ImageUrl,
                    request.Available,
                    cancellationToken);
                return product is null
                    ? Results.NotFound()
                    : Results.Json(product, statusCode: StatusCodes.Status201Created);
            })
            .RequireAuthorization("CanManageCatalog")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("CreateProduct");

        api.MapPut("/products/{productId:guid}", async (
                Guid productId,
                UpdateProductRequest request,
                ProductOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidProduct(request.Name, request.Description, request.CategoryId, request.Price, request.ImageUrl))
                    return Results.BadRequest();
                var product = await operations.UpdateAsync(
                    productId,
                    request.Name,
                    request.Description,
                    request.CategoryId,
                    request.Price,
                    request.ImageUrl,
                    cancellationToken);
                return product is null ? Results.NotFound() : Results.Ok(product);
            })
            .RequireAuthorization("CanManageCatalog")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("UpdateProduct");

        api.MapPatch("/products/{productId:guid}/availability", async (
                Guid productId,
                AvailabilityRequest request,
                ProductOperations operations,
                CancellationToken cancellationToken) =>
            {
                var product = await operations.SetAvailabilityAsync(productId, request.Available, cancellationToken);
                return product is null ? Results.NotFound() : Results.Ok(product);
            })
            .RequireAuthorization("CanManageCatalog")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("SetProductAvailability");

        api.MapPatch("/products/{productId:guid}/status", async (
                Guid productId,
                StatusRequest request,
                ProductOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!TryParseStatusRequest(request.Status, out var active)) return Results.BadRequest();
                var product = await operations.SetStatusAsync(productId, active, cancellationToken);
                return product is null ? Results.NotFound() : Results.Ok(product);
            })
            .RequireAuthorization("CanManageCatalog")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("SetProductStatus");

        api.MapGet("/products/{productId:guid}", async (
                Guid productId,
                ProductOperations operations,
                CancellationToken cancellationToken) =>
            {
                var product = await operations.GetAsync(productId, cancellationToken);
                return product is null ? Results.NotFound() : Results.Ok(product);
            })
            .RequireAuthorization("CanManageCatalog")
            .WithName("GetProduct");

        api.MapGet("/products", async (
                Guid? categoryId,
                string? status,
                string? availability,
                string? search,
                int? page,
                int? pageSize,
                ProductOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!TryParseStatus(status, out var active)
                    || !TryParseAvailability(availability, out var available)
                    || !IsValidSearch(search)
                    || !IsValidPaging(page, pageSize))
                    return Results.BadRequest();
                return Results.Ok(await operations.ListAsync(
                    categoryId, active, available, search, page ?? 1, pageSize ?? 20, cancellationToken));
            })
            .RequireAuthorization("CanManageCatalog")
            .WithName("GetProducts");

        api.MapGet("/available", async (
                ProductOperations operations,
                CancellationToken cancellationToken) =>
                Results.Ok(await operations.GetAvailableCatalogAsync(cancellationToken)))
            .RequireAuthorization(policy => policy.RequireRole(ZelloaRoles.Guardian))
            .WithName("GetAvailableCatalog");

        return endpoints;
    }

    private static bool IsValidName(string? name, int maximumLength) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= maximumLength;

    private static bool IsValidProduct(
        string? name, string? description, Guid categoryId, decimal price, string? imageUrl)
    {
        if (!IsValidName(name, 200)
            || categoryId == Guid.Empty
            || description?.Trim().Length > 2000
            || price <= 0
            || price > 9_999_999_999_999_999.99m
            || decimal.Round(price, 2) != price)
            return false;

        if (string.IsNullOrWhiteSpace(imageUrl)) return true;
        return imageUrl.Trim().Length <= 2048
            && Uri.TryCreate(imageUrl.Trim(), UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps;
    }

    private static bool IsValidSearch(string? search) => search is null || search.Trim().Length <= 100;

    private static bool IsValidPaging(int? page, int? pageSize) =>
        page is null or (>= 1 and <= 1_000_000)
        && pageSize is null or (>= 1 and <= 100);

    private static bool TryParseStatus(string? status, out bool? active)
    {
        active = null;
        if (status is null) return true;
        if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            active = true;
            return true;
        }
        if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
        {
            active = false;
            return true;
        }
        return false;
    }

    private static bool TryParseAvailability(string? availability, out bool? available)
    {
        available = null;
        if (availability is null) return true;
        if (availability.Equals("Available", StringComparison.OrdinalIgnoreCase))
        {
            available = true;
            return true;
        }
        if (availability.Equals("Unavailable", StringComparison.OrdinalIgnoreCase))
        {
            available = false;
            return true;
        }
        return false;
    }

    private static bool TryParseStatusRequest(string? status, out bool active)
    {
        active = false;
        if (status is null) return false;
        if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            active = true;
            return true;
        }
        return status.Equals("Inactive", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record CategoryRequest(string Name);
public sealed record CreateProductRequest(
    string Name,
    Guid CategoryId,
    decimal Price,
    bool Available,
    string? Description,
    string? ImageUrl);
public sealed record UpdateProductRequest(
    string Name,
    Guid CategoryId,
    decimal Price,
    string? Description,
    string? ImageUrl);
public sealed record AvailabilityRequest(bool Available);
public sealed record StatusRequest(string Status);
