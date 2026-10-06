namespace Zelloa.Application.Identity;

public static class ZelloaRoles
{
    public const string PlatformAdmin = "PlatformAdmin";

    public const string SchoolAdmin = "SchoolAdmin";

    public const string CafeteriaOperator = "CafeteriaOperator";

    public const string Guardian = "Guardian";

    public static bool IsInstitutional(string role) =>
        role is SchoolAdmin or CafeteriaOperator or Guardian;
}
