namespace Zelloa.Domain.Orders;

public enum OrderStatus { AwaitingPayment, Paid, Preparing, Ready, Delivered, Cancelled, PaymentExpired }

public sealed class Order
{
    private readonly List<OrderItem> _items = [];
    private Order() { }

    public Order(Guid id, Guid tenantId, Guid guardianId, Guid studentId, Guid classroomId,
        string classroomName, string shift, DateOnly operationalDate, DateTimeOffset createdAt,
        IEnumerable<OrderItem> items)
    {
        if (id == Guid.Empty || tenantId == Guid.Empty || guardianId == Guid.Empty
            || studentId == Guid.Empty || classroomId == Guid.Empty)
            throw new ArgumentException("Order identifiers cannot be empty.");
        ArgumentException.ThrowIfNullOrWhiteSpace(classroomName);
        ArgumentException.ThrowIfNullOrWhiteSpace(shift);
        var materialized = items.ToArray();
        if (materialized.Length == 0) throw new ArgumentException("An order requires items.", nameof(items));
        Id = id;
        TenantId = tenantId;
        GuardianId = guardianId;
        StudentId = studentId;
        ClassroomId = classroomId;
        ClassroomName = classroomName.Trim();
        Shift = shift.Trim();
        OperationalDate = operationalDate;
        CreatedAt = createdAt;
        Status = OrderStatus.AwaitingPayment;
        _items.AddRange(materialized);
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid GuardianId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid ClassroomId { get; private set; }
    public string ClassroomName { get; private set; } = string.Empty;
    public string Shift { get; private set; } = string.Empty;
    public DateOnly OperationalDate { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;
    public decimal Total => _items.Sum(item => item.Subtotal);

    public void Cancel()
    {
        if (Status != OrderStatus.AwaitingPayment)
            throw new InvalidOperationException("Only unpaid orders can be cancelled.");
        Status = OrderStatus.Cancelled;
    }
}

public sealed class OrderItem
{
    private OrderItem() { }
    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        if (unitPrice <= 0 || decimal.Round(unitPrice, 2) != unitPrice)
            throw new ArgumentOutOfRangeException(nameof(unitPrice));
        if (quantity is < 1 or > 99) throw new ArgumentOutOfRangeException(nameof(quantity));
        ProductId = productId;
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        Subtotal = unitPrice * quantity;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Subtotal { get; private set; }
}
