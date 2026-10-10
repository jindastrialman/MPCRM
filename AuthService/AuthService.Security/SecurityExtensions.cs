using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Security;

public static class SecurityExtensions
{
    /// <summary>
    /// Подключает проверку JWT от сервиса авторизации.
    /// Ключи подтягиваются автоматически из JWKS (секрет между сервисами не нужен).
    /// Конфиг: секция "Auth" { MetadataAddress, Issuer, Audience, RequireHttpsMetadata }.
    /// </summary>
    public static IServiceCollection AddAuthServiceSecurity(this IServiceCollection services, IConfiguration config)
    {
        var auth = config.GetSection("Auth");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MetadataAddress = auth["MetadataAddress"]
                    ?? throw new InvalidOperationException("Auth:MetadataAddress is not configured");
                o.RequireHttpsMetadata = auth.GetValue("RequireHttpsMetadata", true);
                o.MapInboundClaims = false; // оставляем "sub", "role" как есть
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = auth["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = auth["Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = AuthServiceClaims.Sub,
                    RoleClaimType = AuthServiceClaims.Role,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
                // при неизвестном kid (ротация ключей) конфигурация перечитывается автоматически
            });

        services.AddAuthorization(o =>
        {
            foreach (var permission in Permissions.All)
                o.AddPolicy(permission, p => p
                    .RequireAuthenticatedUser()
                    .RequireClaim(AuthServiceClaims.Permission, permission));
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }
}
