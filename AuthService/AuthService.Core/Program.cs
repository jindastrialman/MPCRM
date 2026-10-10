using System.Threading.RateLimiting;
using AuthService.Data;
using AuthService.Domain;
using AuthService.Endpoints;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// LiteDB: один singleton на процесс. ASP.NET Core Identity не используется
// (у него есть только EF-хранилища), из него берём лишь хэширование паролей.
builder.Services.AddSingleton<AuthDb>();
builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddHostedService<RefreshTokenCleanup>();

// Сам сервис проверяет токены локальным ключом (нужно для /auth/me)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.MapInboundClaims = false);
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<TokenService>((o, ts) => o.TokenValidationParameters = ts.ValidationParameters);
builder.Services.AddAuthorization();

// Защита от перебора паролей: 10 запросов в минуту на IP
// (за шлюзом настройте ForwardedHeaders, иначе IP будет адресом шлюза)
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
        }));
});

var app = builder.Build();

// Открываем БД и создаём индексы при старте, а не при первом запросе
app.Services.GetRequiredService<AuthDb>();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuth();
app.MapGet("/health", () => Results.Ok("healthy"));

app.Run();
