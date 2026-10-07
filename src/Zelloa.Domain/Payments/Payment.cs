namespace Zelloa.Domain.Payments;

public enum PaymentStatus
{
    Pending,
    Confirmed,
    SimulatedConfirmed,
    Expired,
    Cancelled,
    Refunded,
    Failed
}

public sealed class Payment
{
    private Payment() { }

    public Payment(Guid id, Guid tenantId, Guid orderId, string provider,
        string externalTransactionId, decimal amount, DateTimeOffset createdAt,
        DateTimeOffset expiresAt, string pixCopyPaste, string qrCodeReference, bool isSimulated)
    {
        if (id == Guid.Empty || tenantId == Guid.Empty || orderId == Guid.Empty)
            throw new ArgumentException("Payment identifiers cannot be empty.");
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalTransactionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(pixCopyPaste);
        ArgumentException.ThrowIfNullOrWhiteSpace(qrCodeReference);
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (expiresAt <= createdAt) throw new ArgumentOutOfRangeException(nameof(expiresAt));

        Id = id;
        TenantId = tenantId;
        OrderId = orderId;
        Provider = provider.Trim();
        ExternalTransactionId = externalTransactionId.Trim();
        Amount = amount;
        Currency = "BRL";
        Status = PaymentStatus.Pending;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        PixCopyPaste = pixCopyPaste.Trim();
        QrCodeReference = qrCodeReference.Trim();
        IsSimulated = isSimulated;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid OrderId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string ExternalTransactionId { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public PaymentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? SimulatedAt { get; private set; }
    public string PixCopyPaste { get; private set; } = string.Empty;
    public string QrCodeReference { get; private set; } = string.Empty;
    public bool IsSimulated { get; private set; }

    public void SimulateConfirmation(DateTimeOffset simulatedAt)
    {
        EnsurePending();
        if (!IsSimulated) throw new InvalidOperationException("Only simulated payments can be simulated.");
        if (simulatedAt >= ExpiresAt)
            throw new InvalidOperationException("The payment has expired.");
        Status = PaymentStatus.SimulatedConfirmed;
        SimulatedAt = simulatedAt;
    }

    public void SimulateExpiration(DateTimeOffset simulatedAt)
    {
        EnsurePending();
        if (!IsSimulated) throw new InvalidOperationException("Only simulated payments can be simulated.");
        Status = PaymentStatus.Expired;
        SimulatedAt = simulatedAt;
    }

    private void EnsurePending()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can change status.");
    }
}
