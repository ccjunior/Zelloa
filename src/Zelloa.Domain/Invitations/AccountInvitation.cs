namespace Zelloa.Domain.Invitations;

public sealed class AccountInvitation
{
    private AccountInvitation() { }

    public AccountInvitation(
        Guid id,
        Guid userId,
        Guid tenantId,
        string role,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        if (id == Guid.Empty || userId == Guid.Empty || tenantId == Guid.Empty)
            throw new ArgumentException("Invitation IDs cannot be empty.");
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (expiresAt <= createdAt) throw new ArgumentException("Invitation must expire after it is created.");

        Id = id;
        UserId = userId;
        TenantId = tenantId;
        Role = role.Trim();
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RedeemedAt { get; private set; }

    public bool IsRedeemable(DateTimeOffset now) => RedeemedAt is null && now < ExpiresAt;

    public void Redeem(DateTimeOffset now)
    {
        if (!IsRedeemable(now)) throw new InvalidOperationException("Invitation is expired or already redeemed.");
        RedeemedAt = now;
    }
}
