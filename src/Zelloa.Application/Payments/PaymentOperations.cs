using Zelloa.Application.Identity;
using Zelloa.Domain.Orders;
using Zelloa.Domain.Payments;

namespace Zelloa.Application.Payments;

public sealed class PaymentOperations(
    IPaymentStore store,
    IPaymentGateway gateway,
    ICurrentUser currentUser,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan ChargeLifetime = TimeSpan.FromMinutes(30);

    public async Task<PaymentResponse> CreateSimulatedPixChargeAsync(Guid orderId,
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
            pending.SimulateExpiration(now);
            await store.SaveChangesAsync(cancellationToken);
        }

        var expiresAt = now.Add(ChargeLifetime);
        var paymentId = Guid.NewGuid();
        var charge = await gateway.CreatePixChargeAsync(paymentId, order.Total, expiresAt, cancellationToken);
        if (!charge.IsSimulated)
            throw new InvalidOperationException("Development simulation gateway returned a real charge.");

        var payment = new Payment(paymentId, tenantContext.TenantId, order.Id, charge.Provider,
            charge.ExternalTransactionId, order.Total, now, expiresAt, charge.PixCopyPaste,
            charge.QrCodeReference, isSimulated: true);
        store.Add(payment);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(payment);
    }

    public async Task<PaymentResponse?> GetForCurrentGuardianAsync(Guid paymentId,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } guardianId) return null;
        var payment = await store.GetForGuardianAsync(paymentId, guardianId, cancellationToken);
        return payment is null ? null : ToResponse(payment);
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

    private static PaymentResponse ToResponse(Payment payment) => new(
        payment.Id, payment.OrderId, payment.Amount, payment.Currency, payment.Status.ToString(),
        payment.PixCopyPaste, payment.QrCodeReference, payment.ExpiresAt, payment.ConfirmedAt,
        payment.IsSimulated, payment.SimulatedAt);
}

public sealed record PaymentResponse(Guid PaymentId, Guid OrderId, decimal Amount, string Currency,
    string Status, string PixCopyPaste, string QrCode, DateTimeOffset ExpiresAt,
    DateTimeOffset? ConfirmedAt, bool IsSimulated, DateTimeOffset? SimulatedAt);

public sealed class PaymentRuleException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
