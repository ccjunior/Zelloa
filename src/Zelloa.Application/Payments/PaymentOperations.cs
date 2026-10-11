using Zelloa.Application.Identity;
using Zelloa.Domain.Orders;
using Zelloa.Domain.Payments;

namespace Zelloa.Application.Payments;

public sealed class PaymentOperations(
    IPaymentStore store,
    IPaymentGatewayResolver gatewayResolver,
    ICurrentUser currentUser,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan ChargeLifetime = TimeSpan.FromMinutes(30);

    public async Task<PaymentResponse> CreatePixChargeAsync(Guid orderId,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } guardianId)
            throw new PaymentRuleException("OrderNotFound", "Pedido não encontrado.");

        var order = await store.GetOrderForGuardianAsync(orderId, guardianId, cancellationToken)
            ?? throw new PaymentRuleException("OrderNotFound", "Pedido não encontrado.");
        if (order.Status != OrderStatus.AwaitingPayment)
            throw new PaymentRuleException("OrderNotPayable", "O pedido não está aguardando pagamento.");

        var now = timeProvider.GetUtcNow();
        var pending = await store.GetPendingForOrderAsync(orderId, cancellationToken);
        if (pending is not null && now < pending.ExpiresAt) return ToResponse(pending);
        if (pending is not null)
        {
            if (pending.IsSimulated)
            {
                pending.SimulateExpiration(now);
            }
            else
            {
                var existingGateway = gatewayResolver.Resolve(pending.Provider);
                var providerStatus = await existingGateway.GetPaymentStatusAsync(
                    pending.ExternalTransactionId, cancellationToken);
                if (providerStatus is not ("OVERDUE" or "DELETED" or "EXPIRED"))
                    throw new PaymentRuleException("PaymentStillPending",
                        "A cobrança anterior ainda consta ativa no provedor.");
                pending.ExpireFromProvider(now);
            }
            await store.SaveChangesAsync(cancellationToken);
        }

        var expiresAt = now.Add(ChargeLifetime);
        var paymentId = Guid.NewGuid();
        var gateway = gatewayResolver.ResolveConfigured();
        var charge = await gateway.CreatePixChargeAsync(paymentId, order.Total, expiresAt, cancellationToken);

        var payment = new Payment(paymentId, tenantContext.TenantId, order.Id, charge.Provider,
            charge.ExternalTransactionId, order.Total, now, charge.ExpiresAt, charge.PixCopyPaste,
            charge.QrCodeReference, isSimulated: charge.IsSimulated);
        store.Add(payment);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(payment);
    }

    public async Task<PaymentResponse?> GetForCurrentGuardianAsync(Guid paymentId,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } guardianId) return null;
        var payment = await store.GetForGuardianAsync(paymentId, guardianId, cancellationToken);
        if (payment is null) return null;
        var gateway = gatewayResolver.Resolve(payment.Provider);
        var providerStatus = payment.IsSimulated
            ? payment.Status.ToString()
            : await gateway.GetPaymentStatusAsync(payment.ExternalTransactionId, cancellationToken);
        return ToResponse(payment, providerStatus);
    }

    public async Task<PaymentResponse?> SimulateConfirmationAsync(Guid paymentId,
        CancellationToken cancellationToken)
    {
        var payment = await GetOwnedPaymentAsync(paymentId, cancellationToken);
        if (payment is null) return null;
        var now = timeProvider.GetUtcNow();
        if (now >= payment.ExpiresAt && payment.Status == PaymentStatus.Pending)
        {
            payment.SimulateExpiration(now);
            await store.SaveChangesAsync(cancellationToken);
            throw new PaymentRuleException("PaymentExpired", "A cobrança simulada já expirou.");
        }
        try { payment.SimulateConfirmation(now); }
        catch (InvalidOperationException ex)
        {
            throw new PaymentRuleException("PaymentCannotBeSimulated", ex.Message);
        }
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(payment);
    }

    public async Task<PaymentResponse?> SimulateExpirationAsync(Guid paymentId,
        CancellationToken cancellationToken)
    {
        var payment = await GetOwnedPaymentAsync(paymentId, cancellationToken);
        if (payment is null) return null;
        try { payment.SimulateExpiration(timeProvider.GetUtcNow()); }
        catch (InvalidOperationException ex)
        {
            throw new PaymentRuleException("PaymentCannotBeSimulated", ex.Message);
        }
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(payment);
    }

    private Task<Payment?> GetOwnedPaymentAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } guardianId) return Task.FromResult<Payment?>(null);
        return store.GetForGuardianAsync(paymentId, guardianId, cancellationToken);
    }

    private static PaymentResponse ToResponse(Payment payment, string? providerStatus = null) => new(
        payment.Id, payment.OrderId, payment.Amount, payment.Currency, payment.Status.ToString(),
        payment.PixCopyPaste, payment.QrCodeReference, payment.ExpiresAt, payment.ConfirmedAt,
        payment.IsSimulated, payment.SimulatedAt, providerStatus);
}

public sealed record PaymentResponse(Guid PaymentId, Guid OrderId, decimal Amount, string Currency,
    string Status, string PixCopyPaste, string QrCode, DateTimeOffset ExpiresAt,
    DateTimeOffset? ConfirmedAt, bool IsSimulated, DateTimeOffset? SimulatedAt,
    string? ProviderStatus);

public sealed class PaymentRuleException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
