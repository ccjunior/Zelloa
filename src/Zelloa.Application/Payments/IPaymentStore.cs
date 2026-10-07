using Zelloa.Domain.Orders;
using Zelloa.Domain.Payments;

namespace Zelloa.Application.Payments;

public interface IPaymentStore
{
    Task<Order?> GetOrderForGuardianAsync(Guid orderId, Guid guardianId, CancellationToken cancellationToken);
    Task<Payment?> GetPendingForOrderAsync(Guid orderId, CancellationToken cancellationToken);
    Task<Payment?> GetForGuardianAsync(Guid paymentId, Guid guardianId, CancellationToken cancellationToken);
    void Add(Payment payment);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
