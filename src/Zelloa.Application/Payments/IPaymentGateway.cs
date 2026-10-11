namespace Zelloa.Application.Payments;

public interface IPaymentGateway
{
    Task<PixChargeResult> CreatePixChargeAsync(Guid paymentId, decimal amount,
        DateTimeOffset expiresAt, CancellationToken cancellationToken);

    Task<string> GetPaymentStatusAsync(string externalTransactionId,
        CancellationToken cancellationToken);
}

public interface IPaymentGatewayResolver
{
    IPaymentGateway ResolveConfigured();
    IPaymentGateway Resolve(string provider);
}

public sealed record PixChargeResult(
    string Provider,
    string ExternalTransactionId,
    string PixCopyPaste,
    string QrCodeReference,
    DateTimeOffset ExpiresAt,
    bool IsSimulated);
