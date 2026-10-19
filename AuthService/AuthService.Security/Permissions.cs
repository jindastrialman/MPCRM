namespace AuthService.Security;

public static class Roles
{
    public const string Owner = "Owner";
    public const string Manager = "Manager";
    public const string Viewer = "Viewer";
}

public static class AuthServiceClaims
{
    public const string Sub = "sub";
    public const string TenantId = "tenant_id";
    public const string Role = "role";
    public const string Permission = "permission";
}

public static class Permissions //this stuff is ugly, should remake with permissions database
{
    public const string HardDelete = "harddelete";
    public const string ContactsRead = "contacts.read";
    public const string ContactsWrite = "contacts.write";
    public const string DealsRead = "deals.read";
    public const string DealsWrite = "deals.write";
    public const string TasksRead = "tasks.read";
    public const string TasksWrite = "tasks.write";
    public const string UsersManage = "users.manage";

    public static readonly string[] All =
    [
        ContactsRead, ContactsWrite, DealsRead, DealsWrite,
        TasksRead, TasksWrite, UsersManage, HardDelete
    ];

    // Роль = набор прав. В токен попадают именно права, в коде проверяем только их.
    public static readonly IReadOnlyDictionary<string, string[]> ByRole =
        new Dictionary<string, string[]>
        {
            [Roles.Owner] = All,
            [Roles.Manager] = [ContactsRead, ContactsWrite, DealsRead, DealsWrite, TasksRead, TasksWrite],
            [Roles.Viewer] = [ContactsRead, DealsRead, TasksRead],
        };
}
