using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zelloa.Api.Catalog;
using Zelloa.Application.Catalog;
using Zelloa.Domain.Catalog;
using Zelloa.Infrastructure.Persistence;

namespace Zelloa.IntegrationTests;

public sealed class CatalogTests(IdentityTenantFixture fixture) : IClassFixture<IdentityTenantFixture>
{
    private const string Password = "Zelloa-Test!Password42";
    private static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private readonly IdentityTenantFixture _fixture = fixture;

    [Fact]
    public async Task School_admin_can_manage_categories_products_prices_and_status()
    {
        using var client = _fixture.CreateClient();
        await LoginAsync(client, "admin-a@zelloa.test");

        using var createCategory = await PostAsync(client, "/api/catalog/categories", new { name = "Lanches" });
        Assert.Equal(HttpStatusCode.Created, createCategory.StatusCode);
        var category = await createCategory.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(category);
        Assert.True(category.IsActive);

        using var updateCategory = await PutAsync(
            client, $"/api/catalog/categories/{category.Id}", new { name = "Lanches rápidos" });
        Assert.Equal(HttpStatusCode.OK, updateCategory.StatusCode);
        Assert.Equal("Lanches rápidos", (await updateCategory.Content.ReadFromJsonAsync<CategoryResponse>())!.Name);

        using var createProduct = await PostAsync(client, "/api/catalog/products", new
        {
            name = "Pão de queijo",
            description = "Porção individual",
            categoryId = category.Id,
            price = 5.50m,
            imageUrl = "https://images.example.test/pao-de-queijo.jpg",
            available = true
        });
        Assert.Equal(HttpStatusCode.Created, createProduct.StatusCode);
        var product = await createProduct.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(product);
        Assert.Equal(5.50m, product.Price);
        Assert.Equal(category.Id, product.CategoryId);
        Assert.Equal("https://images.example.test/pao-de-queijo.jpg", product.ImageUrl);

        using var invalidPrice = await PostAsync(client, "/api/catalog/products", new
        {
            name = "Preço inválido",
            categoryId = category.Id,
            price = 1.234m,
            available = true
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidPrice.StatusCode);

        using var invalidImage = await PostAsync(client, "/api/catalog/products", new
        {
            name = "Imagem inválida",
            categoryId = category.Id,
            price = 1m,
            imageUrl = "http://images.example.test/product.jpg",
            available = true
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidImage.StatusCode);

        using var updateProduct = await PutAsync(client, $"/api/catalog/products/{product.Id}", new
        {
            name = "Pão de queijo grande",
            description = (string?)null,
            categoryId = category.Id,
            price = 6.25m,
            imageUrl = (string?)null
        });
        Assert.Equal(HttpStatusCode.OK, updateProduct.StatusCode);
        Assert.Equal(6.25m, (await updateProduct.Content.ReadFromJsonAsync<ProductResponse>())!.Price);

        using var unavailable = await PatchAsync(
            client, $"/api/catalog/products/{product.Id}/availability", new { available = false });
        Assert.Equal(HttpStatusCode.OK, unavailable.StatusCode);
        Assert.False((await unavailable.Content.ReadFromJsonAsync<ProductResponse>())!.IsAvailable);

        using var inactiveProduct = await PatchAsync(
            client, $"/api/catalog/products/{product.Id}/status", new { status = "Inactive" });
        Assert.Equal(HttpStatusCode.OK, inactiveProduct.StatusCode);
        Assert.False((await inactiveProduct.Content.ReadFromJsonAsync<ProductResponse>())!.IsActive);

        using var invalidStatus = await PatchAsync(
            client, $"/api/catalog/products/{product.Id}/status", new { status = (string?)null });
        Assert.Equal(HttpStatusCode.BadRequest, invalidStatus.StatusCode);

        using var inactiveCategory = await PatchAsync(
            client, $"/api/catalog/categories/{category.Id}/status", new { status = "Inactive" });
        Assert.Equal(HttpStatusCode.OK, inactiveCategory.StatusCode);
        Assert.False((await inactiveCategory.Content.ReadFromJsonAsync<CategoryResponse>())!.IsActive);

        using var categories = await client.GetAsync("/api/catalog/categories?status=Inactive&page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, categories.StatusCode);
        var categoryPage = await categories.Content.ReadFromJsonAsync<PagedCatalogResponse<CategoryResponse>>();
        Assert.NotNull(categoryPage);
        Assert.Contains(categoryPage.Items, item => item.Id == category.Id && !item.IsActive);

        using var products = await client.GetAsync($"/api/catalog/products?categoryId={category.Id}&page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, products.StatusCode);
        var productPage = await products.Content.ReadFromJsonAsync<PagedCatalogResponse<ProductResponse>>();
        Assert.NotNull(productPage);
        var updatedProduct = Assert.Single(productPage.Items);
        Assert.Equal(6.25m, updatedProduct.Price);
        Assert.False(updatedProduct.IsActive);
        Assert.False(updatedProduct.IsAvailable);
    }

    [Fact]
    public async Task Family_catalog_contains_only_active_categories_and_available_active_products()
    {
        using var schoolAdmin = _fixture.CreateClient();
        await LoginAsync(schoolAdmin, "admin-a@zelloa.test");

        var visibleCategory = await CreateCategoryAsync(schoolAdmin, "Visível " + Guid.NewGuid().ToString("N"));
        var inactiveCategory = await CreateCategoryAsync(schoolAdmin, "Inativa " + Guid.NewGuid().ToString("N"));
        var visibleProduct = await CreateProductAsync(schoolAdmin, visibleCategory.Id, "Disponível", 3.50m, true);
        _ = await CreateProductAsync(schoolAdmin, visibleCategory.Id, "Indisponível", 4m, false);
        var inactiveProduct = await CreateProductAsync(schoolAdmin, visibleCategory.Id, "Inativo", 5m, true);

        using var disableProduct = await PatchAsync(
            schoolAdmin, $"/api/catalog/products/{inactiveProduct.Id}/status", new { status = "Inactive" });
        Assert.Equal(HttpStatusCode.OK, disableProduct.StatusCode);
        using var disableCategory = await PatchAsync(
            schoolAdmin, $"/api/catalog/categories/{inactiveCategory.Id}/status", new { status = "Inactive" });
        Assert.Equal(HttpStatusCode.OK, disableCategory.StatusCode);
        _ = await CreateProductAsync(schoolAdmin, inactiveCategory.Id, "Sob categoria inativa", 6m, true);

        using var guardian = _fixture.CreateClient();
        await LoginAsync(guardian, "responsavel-a@zelloa.test");
        using var response = await guardian.GetAsync("/api/catalog/available");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var catalog = await response.Content.ReadFromJsonAsync<AvailableCatalogResponse>();
        Assert.NotNull(catalog);
        var category = Assert.Single(catalog.Categories, item => item.Id == visibleCategory.Id);
        var product = Assert.Single(category.Products);
        Assert.Equal(visibleProduct.Id, product.Id);
        Assert.DoesNotContain(catalog.Categories, item => item.Id == inactiveCategory.Id);
    }

    [Fact]
    public async Task Catalog_operations_are_tenant_scoped_and_role_protected()
    {
        var foreignCategoryId = Guid.NewGuid();
        var foreignProductId = Guid.NewGuid();
        await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ZelloaDbContext>();
            dbContext.Categories.Add(new Category(foreignCategoryId, TenantBId, "Categoria Escola B"));
            dbContext.Products.Add(new Product(
                foreignProductId,
                TenantBId,
                foreignCategoryId,
                "Produto Escola B",
                null,
                9.90m,
                null,
                true));
            await dbContext.SaveChangesAsync();
        }

        using var schoolAdmin = _fixture.CreateClient();
        await LoginAsync(schoolAdmin, "admin-a@zelloa.test");
        using var missingAntiforgeryRequest = new HttpRequestMessage(
            HttpMethod.Post, "/api/catalog/categories")
        {
            Content = JsonContent.Create(new { name = "Sem token" })
        };
        using var missingAntiforgery = await schoolAdmin.SendAsync(missingAntiforgeryRequest);
        Assert.Equal(HttpStatusCode.BadRequest, missingAntiforgery.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound,
            (await schoolAdmin.GetAsync($"/api/catalog/products/{foreignProductId}")).StatusCode);

        using var crossTenantCategoryUpdate = await PutAsync(
            schoolAdmin, $"/api/catalog/categories/{foreignCategoryId}", new { name = "Alteração não autorizada" });
        Assert.Equal(HttpStatusCode.NotFound, crossTenantCategoryUpdate.StatusCode);
        using var crossTenantProductUpdate = await PutAsync(schoolAdmin, $"/api/catalog/products/{foreignProductId}", new
        {
            name = "Alteração não autorizada",
            categoryId = foreignCategoryId,
            price = 1m
        });
        Assert.Equal(HttpStatusCode.NotFound, crossTenantProductUpdate.StatusCode);

        using var crossTenantCategoryAssignment = await PostAsync(schoolAdmin, "/api/catalog/products", new
        {
            name = "Produto inválido",
            categoryId = foreignCategoryId,
            price = 2m,
            available = true
        });
        Assert.Equal(HttpStatusCode.NotFound, crossTenantCategoryAssignment.StatusCode);

        using var guardian = _fixture.CreateClient();
        await LoginAsync(guardian, "responsavel-a@zelloa.test");
        using var deniedWrite = await PostAsync(guardian, "/api/catalog/categories", new { name = "Proibida" });
        Assert.Equal(HttpStatusCode.Forbidden, deniedWrite.StatusCode);
        using var deniedAdminList = await guardian.GetAsync("/api/catalog/products");
        Assert.Equal(HttpStatusCode.Forbidden, deniedAdminList.StatusCode);

        using var anonymous = _fixture.CreateClient();
        using var deniedCatalog = await anonymous.GetAsync("/api/catalog/available");
        Assert.Equal(HttpStatusCode.Unauthorized, deniedCatalog.StatusCode);
    }

    private static async Task<CategoryResponse> CreateCategoryAsync(HttpClient client, string name)
    {
        using var response = await PostAsync(client, "/api/catalog/categories", new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CategoryResponse>())!;
    }

    private static async Task<ProductResponse> CreateProductAsync(
        HttpClient client, Guid categoryId, string name, decimal price, bool available)
    {
        using var response = await PostAsync(client, "/api/catalog/products", new
        {
            name,
            categoryId,
            price,
            available
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var token = await GetAntiforgeryTokenAsync(client);
        using var login = await PostAsync(client, "/api/auth/login", new { email, password = Password }, token);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
    }

    private static async Task<HttpResponseMessage> PostAsync(
        HttpClient client, string path, object? body, string? token = null)
    {
        token ??= await GetAntiforgeryTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("X-XSRF-TOKEN", token);
        if (body is not null) request.Content = JsonContent.Create(body);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PutAsync(HttpClient client, string path, object body)
    {
        var token = await GetAntiforgeryTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-XSRF-TOKEN", token);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PatchAsync(HttpClient client, string path, object body)
    {
        var token = await GetAntiforgeryTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Patch, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-XSRF-TOKEN", token);
        return await client.SendAsync(request);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client)
    {
        var token = await client.GetFromJsonAsync<CsrfTokenResponse>("/api/auth/csrf");
        Assert.NotNull(token);
        return token.RequestToken;
    }

    private sealed record CsrfTokenResponse(string RequestToken);
}
