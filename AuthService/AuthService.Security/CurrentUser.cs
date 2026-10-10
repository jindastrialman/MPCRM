using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AuthService.Security;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    Guid TenantId { get; }
    bool HasPermission(string permission);
}

internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid UserId => ReadGuid(AuthServiceClaims.Sub);

    // tenant_id берётся ТОЛЬКО из проверенного токена, никогда из тела запроса/URL
    public Guid TenantId => ReadGuid(AuthServiceClaims.TenantId);

    public bool HasPermission(string permission) =>
        Principal?.HasClaim(AuthServiceClaims.Permission, permission) == true;

    private Guid ReadGuid(string claim) =>
        Guid.TryParse(Principal?.FindFirst(claim)?.Value, out var id)
            ? id
            : throw new UnauthorizedAccessException($"Claim '{claim}' is missing");
}
