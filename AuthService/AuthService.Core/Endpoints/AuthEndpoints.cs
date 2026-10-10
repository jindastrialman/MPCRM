using AuthService.Services;
using Microsoft.Extensions.Options;

namespace AuthService.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this WebApplication app)
    {
        var g = app.MapGroup("/auth").RequireRateLimiting("auth");

        g.MapPost("/register", (RegisterCompanyRequest r, AccountService s, HttpContext c) =>
        {
            var res = s.RegisterCompany(r, Client(c));
            return res.Tokens is null ? Results.BadRequest(new { error = res.Error }) : Results.Ok(res.Tokens);
        });

        g.MapPost("/login", (LoginRequest r, AccountService s, HttpContext c) =>
        {
            var res = s.Login(r, Client(c));
            return res.Tokens is null ? Results.Json(new { error = res.Error }, statusCode: 401) : Results.Ok(res.Tokens);
        });

        g.MapPost("/refresh", (RefreshRequest r, AccountService s, HttpContext c) =>
        {
            var res = s.Refresh(r.RefreshToken, Client(c));
            return res.Tokens is null ? Results.Json(new { error = res.Error }, statusCode: 401) : Results.Ok(res.Tokens);
        });

        g.MapPost("/logout", (RefreshRequest r, AccountService s) =>
        {
            s.Logout(r.RefreshToken);
            return Results.NoContent();
        });

        g.MapGet("/me", (System.Security.Claims.ClaimsPrincipal u) => Results.Ok(new
        {
            id = u.FindFirst("sub")?.Value,
            email = u.FindFirst("email")?.Value,
            name = u.FindFirst("name")?.Value,
            tenantId = u.FindFirst("tenant_id")?.Value,
            roles = u.FindAll("role").Select(c => c.Value),
            permissions = u.FindAll("permission").Select(c => c.Value),
        })).RequireAuthorization();

        // ── Discovery + JWKS: по ним остальные сервисы находят публичный ключ ──
        app.MapGet("/.well-known/openid-configuration", (HttpContext c, IOptions<JwtOptions> o) => Results.Json(new
        {
            issuer = o.Value.Issuer,
            jwks_uri = $"{c.Request.Scheme}://{c.Request.Host}/.well-known/jwks.json",
        }));

        app.MapGet("/.well-known/jwks.json", (TokenService t) => Results.Json(t.GetJwks()));
    }

    private static ClientInfo Client(HttpContext c) =>
        new(c.Connection.RemoteIpAddress?.ToString(), c.Request.Headers.UserAgent.ToString());
}
