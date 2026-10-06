namespace Zelloa.Domain.Orders;

public sealed class OrderCutoff
{
    private OrderCutoff() { }

    public OrderCutoff(Guid id, Guid tenantId, string shift, TimeOnly cutoffTime)
    {
        if (id == Guid.Empty || tenantId == Guid.Empty) throw new ArgumentException("IDs cannot be empty.");
        ArgumentException.ThrowIfNullOrWhiteSpace(shift);
        Id = id;
        TenantId = tenantId;
        Shift = shift.Trim();
        ShiftKey = Shift.ToUpperInvariant();
        CutoffTime = cutoffTime;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Shift { get; private set; } = string.Empty;
    public string ShiftKey { get; private set; } = string.Empty;
    public TimeOnly CutoffTime { get; private set; }

    public void Update(TimeOnly cutoffTime) => CutoffTime = cutoffTime;
}
