using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zelloa.Api.Identity;
using Zelloa.Api.SchoolAcademic;
using Zelloa.Application.Orders;
using Zelloa.Application.Catalog;
using Zelloa.Domain.Catalog;
using Zelloa.Infrastructure.Persistence;

namespace Zelloa.IntegrationTests;

public sealed class OrderTests(IdentityTenantFixture fixture) : IClassFixture<IdentityTenantFixture>
{
    private const string Password = "Zelloa-Test!Password42";
    private readonly IdentityTenantFixture _fixture = fixture;

    [Fact]
    public async Task Guardian_orders_use_server_prices_cutoff_and_revalidate_on_repeat()
    {
        using var schoolAdmin = _fixture.CreateClient();
        await LoginAsync(schoolAdmin, "admin-a@zelloa.test");
        using var link = await PostAsync(schoolAdmin,
            $"/api/students/{_fixture.TenantAStudentId}/guardians/{_fixture.TenantAUserId}", new { });
        Assert.Equal(HttpStatusCode.NoContent, link.StatusCode);

        using var categoryResponse = await PostAsync(schoolAdmin, "/api/catalog/categories", new { name = "Pedidos" });
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        var category = (await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>())!;
        using var productResponse = await PostAsync(schoolAdmin, "/api/catalog/products", new
        {
            name = "Suco", categoryId = category.Id, price = 4.50m, available = true
        });
        Assert.Equal(HttpStatusCode.Created, productResponse.StatusCode);
        var product = (await productResponse.Content.ReadFromJsonAsync<ProductResponse>())!;

        using var unconfiguredClient = _fixture.CreateClient();
        await LoginAsync(unconfiguredClient, "responsavel-a@zelloa.test");
        var orderBody = new { studentId = _fixture.TenantAStudentId,
            items = new[] { new { productId = product.Id, quantity = 2, unitPrice = 0.01m } } };
        using var unconfigured = await PostAsync(unconfiguredClient, "/api/orders", orderBody);
        Assert.Equal(HttpStatusCode.Conflict, unconfigured.StatusCode);
        Assert.Contains("OrderCutoffNotConfigured", await unconfigured.Content.ReadAsStringAsync());

        using var setCutoff = await PutAsync(schoolAdmin, "/api/school/order-cutoffs/Matutino", new { cutoffTime = "23:59" });
        Assert.Equal(HttpStatusCode.OK, setCutoff.StatusCode);
        using var noAccess = await PutAsync(unconfiguredClient, "/api/school/order-cutoffs/Matutino", new { cutoffTime = "23:59" });
        Assert.Equal(HttpStatusCode.Forbidden, noAccess.StatusCode);

        using var create = await PostAsync(unconfiguredClient, "/api/orders", orderBody);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var order = (await create.Content.ReadFromJsonAsync<OrderResponse>())!;
        Assert.Equal(9.00m, order.Total);
        Assert.Equal(4.50m, Assert.Single(order.Items).UnitPrice);
        Assert.Equal(DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("America/Bahia")).DateTime), order.OperationalDate);
        Assert.Equal("AwaitingPayment", order.Status);
        using var ownOrder = await unconfiguredClient.GetAsync($"/api/orders/{order.OrderId}");
        Assert.Equal(HttpStatusCode.OK, ownOrder.StatusCode);
        using var schoolOrder = await schoolAdmin.GetAsync($"/api/orders/{order.OrderId}");
        Assert.Equal(HttpStatusCode.OK, schoolOrder.StatusCode);
        using var otherGuardian = _fixture.CreateClient();
        await LoginAsync(otherGuardian, "responsavel-b@zelloa.test");
        using var hiddenOrder = await otherGuardian.GetAsync($"/api/orders/{order.OrderId}");
        Assert.Equal(HttpStatusCode.NotFound, hiddenOrder.StatusCode);

        using var priceUpdate = await PutAsync(schoolAdmin, $"/api/catalog/products/{product.Id}", new
        {
            name = "Suco", categoryId = category.Id, price = 6m,
            description = (string?)null, imageUrl = (string?)null
        });
        Assert.Equal(HttpStatusCode.OK, priceUpdate.StatusCode);
        using var repeat = await PostAsync(unconfiguredClient, $"/api/orders/{order.OrderId}/repeat", new { });
        Assert.Equal(HttpStatusCode.Created, repeat.StatusCode);
        var repeated = (await repeat.Content.ReadFromJsonAsync<OrderResponse>())!;
        Assert.Equal(12m, repeated.Total);

        using var cancel = await PostAsync(unconfiguredClient, $"/api/orders/{order.OrderId}/cancel", new { });
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);
        Assert.Equal("Cancelled", (await cancel.Content.ReadFromJsonAsync<OrderResponse>())!.Status);
        var mine = await unconfiguredClient.GetFromJsonAsync<OrderResponse[]>("/api/me/orders");
        Assert.NotNull(mine);
        Assert.Equal(2, mine.Length);

        using var passedCutoff = await PutAsync(schoolAdmin,
            "/api/school/order-cutoffs/Matutino", new { cutoffTime = "00:00" });
        Assert.Equal(HttpStatusCode.OK, passedCutoff.StatusCode);
        using var rejected = await PostAsync(unconfiguredClient, "/api/orders", orderBody);
        Assert.Equal(HttpStatusCode.Conflict, rejected.StatusCode);
        Assert.Contains("OrderCutoffPassed", await rejected.Content.ReadAsStringAsync());

        using var unlinked = await PostAsync(unconfiguredClient, "/api/orders", new
        {
            studentId = _fixture.TenantBStudentId,
            items = new[] { new { productId = product.Id, quantity = 1 } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, unlinked.StatusCode);
        Assert.Contains("StudentNotLinked", await unlinked.Content.ReadAsStringAsync());

        using var reopenCutoff = await PutAsync(schoolAdmin,
            "/api/school/order-cutoffs/Matutino", new { cutoffTime = "23:59" });
        Assert.Equal(HttpStatusCode.OK, reopenCutoff.StatusCode);

        using var invalidQuantity = await PostAsync(unconfiguredClient, "/api/orders", new
        {
            studentId = _fixture.TenantAStudentId,
            items = new[] { new { productId = product.Id, quantity = 0 } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidQuantity.StatusCode);
        Assert.Contains("InvalidOrder", await invalidQuantity.Content.ReadAsStringAsync());

        using var missingProduct = await PostAsync(unconfiguredClient, "/api/orders", new
        {
            studentId = _fixture.TenantAStudentId,
            items = new[] { new { productId = Guid.NewGuid(), quantity = 1 } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, missingProduct.StatusCode);
        Assert.Contains("ProductNotFound", await missingProduct.Content.ReadAsStringAsync());

        var unavailableProduct = await CreateProductAsync(schoolAdmin, category.Id, "Indisponível", 2m);
        using var markUnavailable = await PatchAsync(schoolAdmin,
            $"/api/catalog/products/{unavailableProduct.Id}/availability", new { available = false });
        Assert.Equal(HttpStatusCode.OK, markUnavailable.StatusCode);
        using var unavailableOrder = await PostAsync(unconfiguredClient, "/api/orders", new
        {
            studentId = _fixture.TenantAStudentId,
            items = new[] { new { productId = unavailableProduct.Id, quantity = 1 } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, unavailableOrder.StatusCode);
        Assert.Contains("ProductUnavailable", await unavailableOrder.Content.ReadAsStringAsync());

        var inactiveProduct = await CreateProductAsync(schoolAdmin, category.Id, "Inativo", 3m);
        using var markInactive = await PatchAsync(schoolAdmin,
            $"/api/catalog/products/{inactiveProduct.Id}/status", new { status = "Inactive" });
        Assert.Equal(HttpStatusCode.OK, markInactive.StatusCode);
        using var inactiveOrder = await PostAsync(unconfiguredClient, "/api/orders", new
        {
            studentId = _fixture.TenantAStudentId,
            items = new[] { new { productId = inactiveProduct.Id, quantity = 1 } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, inactiveOrder.StatusCode);
        Assert.Contains("ProductUnavailable", await inactiveOrder.Content.ReadAsStringAsync());

        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ZelloaDbContext>();
        var stored = await db.Orders.IgnoreQueryFilters().Include(item => item.Items)
            .SingleAsync(item => item.Id == order.OrderId);
        Assert.Equal(4.50m, Assert.Single(stored.Items).UnitPrice);
        var foreignTenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var foreignCategoryId = Guid.NewGuid();
        var foreignProductId = Guid.NewGuid();
        db.Categories.Add(new Category(foreignCategoryId, foreignTenantId, "Categoria B"));
        db.Products.Add(new Product(foreignProductId, foreignTenantId, foreignCategoryId,
            "Produto B", null, 8m, null, true));
        await db.SaveChangesAsync();
        using var foreignProductOrder = await PostAsync(unconfiguredClient, "/api/orders", new
        {
            studentId = _fixture.TenantAStudentId,
            items = new[] { new { productId = foreignProductId, quantity = 1 } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, foreignProductOrder.StatusCode);
        Assert.Contains("ProductNotFound", await foreignProductOrder.Content.ReadAsStringAsync());
    }

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var token = await GetTokenAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email, password = Password })
        };
        request.Headers.Add("X-XSRF-TOKEN", token);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-XSRF-TOKEN", await GetTokenAsync(client));
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PutAsync(HttpClient client, string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-XSRF-TOKEN", await GetTokenAsync(client));
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PatchAsync(HttpClient client, string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-XSRF-TOKEN", await GetTokenAsync(client));
        return await client.SendAsync(request);
    }

    private static async Task<ProductResponse> CreateProductAsync(
        HttpClient client, Guid categoryId, string name, decimal price)
    {
        using var response = await PostAsync(client, "/api/catalog/products", new
        {
            name, categoryId, price, available = true
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private static async Task<string> GetTokenAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<CsrfTokenResponse>("/api/auth/csrf"))!.RequestToken;
}
