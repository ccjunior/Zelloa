namespace Zelloa.Application.Payments;

public interface IPaymentGateway
{
    Task<PixChargeResult> CreatePixChargeAsync(Guid paymentId, decimal amount,
        DateTimeOffset expiresAt, CancellationToken cancellationToken);
}

public sealed record PixChargeResult(
    string Provider,
    string ExternalTransactionId,
    string PixCopyPaste,
    string QrCodeReference,
    bool IsSimulated);
