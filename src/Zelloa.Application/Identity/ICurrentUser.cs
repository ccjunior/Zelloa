namespace Zelloa.Application.Identity;

public interface ICurrentUser
{
    Guid? UserId { get; }

    string? DisplayName { get; }

    Guid? TenantId { get; }

    bool IsAuthenticated { get; }

    IReadOnlyCollection<string> Roles { get; }
}
