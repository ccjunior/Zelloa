using Zelloa.Application.Payments;

namespace Zelloa.Infrastructure.Payments;

public sealed class DevelopmentSimulationPaymentGateway : IPaymentGateway
{
    public Task<PixChargeResult> CreatePixChargeAsync(Guid paymentId, decimal amount,
        DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var reference = paymentId.ToString("N");
        return Task.FromResult(new PixChargeResult(
            "DevelopmentSimulation",
            $"SIM-{reference}",
            $"SIMULACAO-ZELLOA-{reference}",
            $"SIMULATED://PIX/{reference}",
            expiresAt,
            IsSimulated: true));
    }

    public Task<string> GetPaymentStatusAsync(string externalTransactionId,
        CancellationToken cancellationToken) => Task.FromResult("SIMULATED_PENDING");
}
