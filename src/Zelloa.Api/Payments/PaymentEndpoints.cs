using Microsoft.AspNetCore.Antiforgery;
using Zelloa.Api.SchoolAcademic;
using Zelloa.Application.Identity;
using Zelloa.Application.Payments;

namespace Zelloa.Api.Payments;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(
        this IEndpointRouteBuilder endpoints, bool includeSimulationOperations)
    {
        var orders = endpoints.MapGroup("/api/orders").WithTags("Payments");
        orders.MapPost("/{orderId:guid}/payments/pix", async (Guid orderId,
                PaymentOperations operations, CancellationToken ct) =>
                await RunAsync(() => operations.CreatePixChargeAsync(orderId, ct), created: true))
            .RequireAuthorization("CanCreateOrder")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("CreatePixCharge");

        endpoints.MapGet("/api/payments/{paymentId:guid}", async (Guid paymentId,
                PaymentOperations operations, CancellationToken ct) =>
            {
                var result = await operations.GetForCurrentGuardianAsync(paymentId, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .RequireAuthorization(policy => policy.RequireRole(ZelloaRoles.Guardian))
            .WithTags("Payments")
            .WithName("GetPayment");

        if (includeSimulationOperations)
        {
            var simulation = endpoints.MapGroup("/api/development/payments")
                .WithTags("Development Payment Simulation")
                .RequireAuthorization("CanCreateOrder")
                .AddEndpointFilter<AntiforgeryEndpointFilter>();
            simulation.MapPost("/{paymentId:guid}/confirm", async (Guid paymentId,
                    PaymentOperations operations, CancellationToken ct) =>
                    await RunAsync(async () => await operations.SimulateConfirmationAsync(paymentId, ct)
                        ?? throw new PaymentRuleException("PaymentNotFound", "Pagamento não encontrado.")))
                .WithName("SimulatePaymentConfirmation");
            simulation.MapPost("/{paymentId:guid}/expire", async (Guid paymentId,
                    PaymentOperations operations, CancellationToken ct) =>
                    await RunAsync(async () => await operations.SimulateExpirationAsync(paymentId, ct)
                        ?? throw new PaymentRuleException("PaymentNotFound", "Pagamento não encontrado.")))
                .WithName("SimulatePaymentExpiration");
        }

        return endpoints;
    }

    private static async Task<IResult> RunAsync(Func<Task<PaymentResponse>> operation, bool created = false)
    {
        try
        {
            var result = await operation();
            return created
                ? Results.Json(result, statusCode: StatusCodes.Status201Created)
                : Results.Ok(result);
        }
        catch (PaymentRuleException ex)
        {
            var status = ex.Code is "OrderNotFound" or "PaymentNotFound"
                ? StatusCodes.Status404NotFound
                : ex.Code is "OrderNotPayable" or "PaymentCannotBeSimulated" or "PaymentExpired"
                    or "PaymentStillPending"
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status400BadRequest;
            return Results.Json(new { code = ex.Code, message = ex.Message }, statusCode: status);
        }
        catch (HttpRequestException)
        {
            return Results.Json(new
            {
                code = "PaymentProviderUnavailable",
                message = "Não foi possível concluir a operação no provedor de pagamento."
            }, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
