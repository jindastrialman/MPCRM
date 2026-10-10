using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Domain;
using AuthService.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "auth-service";
    public string Audience { get; set; } = "crm-api";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
    /// <summary>PEM приватного RSA-ключа. Пусто = сгенерировать на старте (только для dev!).</summary>
    public string? PrivateKeyPem { get; set; }
}

/// <summary>
/// Подписывает access-токены приватным RSA-ключом (RS256).
/// Публичная часть отдаётся через JWKS, поэтому остальные сервисы проверяют токен сами,
/// не ходя в сервис авторизации на каждый запрос и не зная секрета.
/// </summary>
public sealed class TokenService
{
    private readonly JwtOptions _opt;
    private readonly RSA _rsa;
    private readonly RsaSecurityKey _key;
    private readonly SigningCredentials _creds;
    private readonly JsonWebTokenHandler _handler = new();

    public TokenService(IOptions<JwtOptions> options, ILogger<TokenService> log)
    {
        _opt = options.Value;
        _rsa = RSA.Create(2048);

        if (!string.IsNullOrWhiteSpace(_opt.PrivateKeyPem))
            _rsa.ImportFromPem(_opt.PrivateKeyPem);
        else
            log.LogWarning("Jwt:PrivateKeyPem is empty: generated an ephemeral key. " +
                           "All tokens become invalid after restart. Use a real key in production.");

        var kid = Convert.ToHexString(SHA256.HashData(_rsa.ExportParameters(false).Modulus!))[..16].ToLowerInvariant();
        _key = new RsaSecurityKey(_rsa) { KeyId = kid };
        _creds = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256);
    }

    /// <summary>Для проверки токенов самим сервисом авторизации (эндпоинт /auth/me).</summary>
    public TokenValidationParameters ValidationParameters => new()
    {
        ValidateIssuer = true,
        ValidIssuer = _opt.Issuer,
        ValidateAudience = true,
        ValidAudience = _opt.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = _key,
        NameClaimType = AuthServiceClaims.Sub,
        RoleClaimType = AuthServiceClaims.Role,
        ClockSkew = TimeSpan.FromSeconds(30),
    };

    public (string Token, int ExpiresInSeconds) CreateAccessToken(AppUser user, IEnumerable<string> roles)
    {
        var roleList = roles.ToList();
        var permissions = roleList
            .SelectMany(r => Permissions.ByRole.TryGetValue(r, out var p) ? p : Array.Empty<string>())
            .Distinct();

        var claims = new List<Claim>
        {
            new(AuthServiceClaims.Sub, user.Id.ToString()),
            new("email", user.Email!),
            new("name", user.FullName),
            new(AuthServiceClaims.TenantId, user.TenantId.ToString()),
            new("jti", Guid.NewGuid().ToString()),
        };
        claims.AddRange(roleList.Select(r => new Claim(AuthServiceClaims.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim(AuthServiceClaims.Permission, p)));

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _opt.Issuer,
            Audience = _opt.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes),
            SigningCredentials = _creds,
        });

        return (token, _opt.AccessTokenMinutes * 60);
    }

    public static string GenerateRefreshToken() =>
        Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    public object GetJwks()
    {
        var p = _rsa.ExportParameters(false);
        return new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    alg = "RS256",
                    kid = _key.KeyId,
                    n = Base64UrlEncoder.Encode(p.Modulus),
                    e = Base64UrlEncoder.Encode(p.Exponent),
                }
            }
        };
    }
}
