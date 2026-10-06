using Zelloa.Domain.Catalog;
using Zelloa.Domain.Orders;
using Zelloa.Domain.Schools;

namespace Zelloa.Application.Orders;

public interface IOrderStore
{
    Task<OrderStudentContext?> GetStudentContextAsync(Guid studentId, CancellationToken cancellationToken);
    Task<bool> GuardianHasStudentAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken);
    Task<OrderCutoff?> GetCutoffAsync(string shift, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderCutoff>> GetCutoffsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetProductsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    Task<Order?> GetOrderForGuardianAsync(Guid orderId, Guid guardianId, CancellationToken cancellationToken);
    Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> GetOrdersForGuardianAsync(Guid guardianId, CancellationToken cancellationToken);
    void Add(Order order);
    void Add(OrderCutoff cutoff);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record OrderStudentContext(Student Student, Classroom Classroom, School School);
public sealed record OrderLineRequest(Guid ProductId, int Quantity);
public sealed record CreateOrderRequest(Guid StudentId, IReadOnlyList<OrderLineRequest> Items);
public sealed record OrderCutoffResponse(string Shift, TimeOnly CutoffTime);
public sealed record OrderItemResponse(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal Subtotal);
public sealed record OrderResponse(Guid OrderId, Guid StudentId, string ClassroomName, string Shift,
    DateOnly OperationalDate, string Status, DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItemResponse> Items, decimal Total);

public sealed class OrderRuleException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
