using Microsoft.EntityFrameworkCore;
using Zelloa.Application.Payments;
using Zelloa.Domain.Orders;
using Zelloa.Domain.Payments;

namespace Zelloa.Infrastructure.Persistence;

public sealed class PaymentStore(ZelloaDbContext dbContext) : IPaymentStore
{
    public Task<Order?> GetOrderForGuardianAsync(Guid orderId, Guid guardianId,
        CancellationToken cancellationToken) => dbContext.Orders
        .Include(order => order.Items)
        .FirstOrDefaultAsync(order => order.Id == orderId && order.GuardianId == guardianId,
            cancellationToken);

    public Task<Payment?> GetPendingForOrderAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.Payments.FirstOrDefaultAsync(payment => payment.OrderId == orderId
            && payment.Status == PaymentStatus.Pending, cancellationToken);

    public Task<Payment?> GetForGuardianAsync(Guid paymentId, Guid guardianId,
        CancellationToken cancellationToken) => dbContext.Payments.FirstOrDefaultAsync(payment =>
            payment.Id == paymentId && dbContext.Orders.Any(order => order.Id == payment.OrderId
                && order.GuardianId == guardianId), cancellationToken);

    public void Add(Payment payment) => dbContext.Payments.Add(payment);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
