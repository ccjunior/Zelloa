using Microsoft.AspNetCore.Antiforgery;
using Zelloa.Api.SchoolAcademic;
using Zelloa.Application.Orders;
using Zelloa.Application.Identity;

namespace Zelloa.Api.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var orders = endpoints.MapGroup("/api/orders").WithTags("Orders");
        orders.MapGet("/{orderId:guid}", async (Guid orderId, OrderOperations operations, CancellationToken ct) =>
                await RunReadAsync(() => operations.GetAsync(orderId, ct)))
            .RequireAuthorization(policy => policy.RequireRole(ZelloaRoles.Guardian, ZelloaRoles.SchoolAdmin))
            .WithName("GetOrder");
        orders.MapPost("", async (CreateOrderRequest request, OrderOperations operations, CancellationToken ct) =>
                await RunAsync(() => operations.CreateAsync(request, ct), created: true))
            .RequireAuthorization("CanCreateOrder").AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("CreateOrder");
        orders.MapPost("/{orderId:guid}/repeat", async (Guid orderId, OrderOperations operations, CancellationToken ct) =>
                await RunAsync(async () => await operations.RepeatAsync(orderId, ct)
                    ?? throw new OrderRuleException("OrderNotFound", "Pedido não encontrado."), created: true))
            .RequireAuthorization("CanCreateOrder").AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("RepeatOrder");
        orders.MapPost("/{orderId:guid}/cancel", async (Guid orderId, OrderOperations operations, CancellationToken ct) =>
                await RunAsync(async () => await operations.CancelAsync(orderId, ct)
                    ?? throw new OrderRuleException("OrderNotFound", "Pedido não encontrado.")))
            .RequireAuthorization("CanCreateOrder").AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("CancelOrder");

        endpoints.MapGet("/api/me/orders", async (OrderOperations operations, CancellationToken ct) =>
                Results.Ok(await operations.ListMineAsync(ct)))
            .RequireAuthorization("CanCreateOrder").WithTags("Orders").WithName("ListMyOrders");

        var settings = endpoints.MapGroup("/api/school/order-cutoffs").WithTags("School Orders");
        settings.MapGet("", async (OrderOperations operations, CancellationToken ct) =>
                Results.Ok(await operations.GetCutoffsAsync(ct)))
            .RequireAuthorization("CanManageSchool").WithName("ListOrderCutoffs");
        settings.MapPut("/{shift}", async (string shift, OrderCutoffRequest request,
                OrderOperations operations, CancellationToken ct) =>
            {
                try { return Results.Ok(await operations.SetCutoffAsync(shift, request.CutoffTime, ct)); }
                catch (OrderRuleException ex) { return Results.BadRequest(new { code = ex.Code, message = ex.Message }); }
            })
            .RequireAuthorization("CanManageSchool").AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("SetOrderCutoff");
        return endpoints;
    }

    private static async Task<IResult> RunReadAsync(Func<Task<OrderResponse?>> operation)
    {
        var result = await operation();
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> RunAsync(Func<Task<OrderResponse>> operation, bool created = false)
    {
        try
        {
            var result = await operation();
            return created ? Results.Json(result, statusCode: StatusCodes.Status201Created) : Results.Ok(result);
        }
        catch (OrderRuleException ex)
        {
            var status = ex.Code is "OrderCutoffNotConfigured" or "OrderCutoffPassed" or "OrderCannotBeCancelled"
                ? StatusCodes.Status409Conflict
                : ex.Code == "OrderNotFound" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            return Results.Json(new { code = ex.Code, message = ex.Message }, statusCode: status);
        }
    }
}

public sealed record OrderCutoffRequest(string CutoffTime);
