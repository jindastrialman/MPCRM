using AuthService.Data;
using AuthService.Domain;
using AuthService.Security;
using LiteDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AuthService.Services;

/// <summary>
/// Регистрация, логин, refresh-ротация, logout.
/// Методы синхронные намеренно: транзакции LiteDB привязаны к потоку,
/// а между BeginTrans и Commit нельзя использовать await.
/// </summary>
public sealed class AccountService(
    AuthDb store,
    IPasswordHasher<AppUser> hasher,
    TokenService tokens,
    IOptions<JwtOptions> options)
{
    private const string InvalidCredentials = "Invalid email or password";
    private const string InvalidRefresh = "Invalid refresh token";
    private const string EmailTaken = "Email is already registered";
    private const int MinPasswordLength = 10;
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly JwtOptions _opt = options.Value;
    private LiteDatabase Db => store.Db;

    /// <summary>Регистрация новой компании: создаётся тенант и пользователь-владелец.</summary>
    public AuthResult RegisterCompany(RegisterCompanyRequest r, ClientInfo client)
    {
        var email = r.Email?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(r.CompanyName) || !email.Contains('@'))
            return AuthResult.Fail("Company name and a valid email are required");
        if (r.Password is null || r.Password.Length < MinPasswordLength)
            return AuthResult.Fail($"Password must be at least {MinPasswordLength} characters");

        var normalized = Normalize(email);
        if (store.Users.Exists(u => u.NormalizedEmail == normalized))
            return AuthResult.Fail(EmailTaken);

        var tenant = new Tenant { Name = r.CompanyName.Trim() };
        var user = new AppUser
        {
            Email = email,
            NormalizedEmail = normalized,
            FullName = r.FullName?.Trim() ?? "",
            TenantId = tenant.Id,
            Roles = [Roles.Owner],
        };
        user.PasswordHash = hasher.HashPassword(user, r.Password);

        Db.BeginTrans();
        try
        {
            store.Tenants.Insert(tenant);
            store.Users.Insert(user);                    // гонка за email -> нарушение уникального индекса
            var pair = Issue(user, Guid.NewGuid(), client);
            Db.Commit();
            return AuthResult.Ok(pair);
        }
        catch (LiteException ex) when (ex.ErrorCode == LiteException.INDEX_DUPLICATE_KEY)
        {
            Db.Rollback();
            return AuthResult.Fail(EmailTaken);
        }
        catch
        {
            Db.Rollback();
            throw;
        }
    }

    public AuthResult Login(LoginRequest r, ClientInfo client)
    {
        var normalized = Normalize(r.Email ?? "");
        var user = store.Users.FindOne(u => u.NormalizedEmail == normalized);
        if (user is null || !user.IsActive)
            return AuthResult.Fail(InvalidCredentials);

        // LiteDB может вернуть время как Local, поэтому приводим к UTC перед сравнением
        if (user.LockoutEnd?.ToUniversalTime() > DateTime.UtcNow)
            return AuthResult.Fail("Account is temporarily locked. Try again later");

        var password = r.Password ?? "";
        var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (verify == PasswordVerificationResult.Failed)
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= MaxFailedAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                user.AccessFailedCount = 0;
            }
            store.Users.Update(user);
            return AuthResult.Fail(InvalidCredentials);
        }

        if (verify == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = hasher.HashPassword(user, password);

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        store.Users.Update(user);

        Db.BeginTrans();
        try
        {
            var pair = Issue(user, Guid.NewGuid(), client);
            Db.Commit();
            return AuthResult.Ok(pair);
        }
        catch
        {
            Db.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Refresh с ротацией: каждый refresh-токен одноразовый.
    /// Повторное предъявление уже использованного токена = признак кражи,
    /// вся цепочка (family) отзывается. Вся логика идёт в одной транзакции,
    /// поэтому параллельные запросы с одним токеном не пройдут оба.
    /// </summary>
    public AuthResult Refresh(string? rawToken, ClientInfo client)
    {
        var hash = TokenService.Hash(rawToken ?? "");

        Db.BeginTrans();
        try
        {
            var result = RefreshCore(hash, client);
            Db.Commit(); // фиксируем и успешную ротацию, и отзыв цепочки при reuse
            return result;
        }
        catch
        {
            Db.Rollback();
            throw;
        }
    }

    private AuthResult RefreshCore(string hash, ClientInfo client)
    {
        var token = store.RefreshTokens.FindOne(t => t.TokenHash == hash);
        if (token is null)
            return AuthResult.Fail(InvalidRefresh);

        if (token.RevokedAt is not null)
        {
            // TODO (по желанию): "окно грации" 5-10 секунд для параллельных запросов
            // с двух вкладок, чтобы не разлогинивать легитимного пользователя.
            RevokeFamily(token.FamilyId);
            return AuthResult.Fail(InvalidRefresh);
        }

        if (token.ExpiresAt.ToUniversalTime() <= DateTime.UtcNow)
            return AuthResult.Fail(InvalidRefresh);

        var user = store.Users.FindById(token.UserId);
        if (user is null || !user.IsActive)
        {
            RevokeFamily(token.FamilyId);
            return AuthResult.Fail(InvalidRefresh);
        }

        token.RevokedAt = DateTime.UtcNow;
        store.RefreshTokens.Update(token);
        return AuthResult.Ok(Issue(user, token.FamilyId, client));
    }

    public void Logout(string? rawToken)
    {
        var hash = TokenService.Hash(rawToken ?? "");
        var token = store.RefreshTokens.FindOne(t => t.TokenHash == hash);
        if (token is not null)
            RevokeFamily(token.FamilyId);
    }

    private TokenResponse Issue(AppUser user, Guid familyId, ClientInfo client)
    {
        var (access, expiresIn) = tokens.CreateAccessToken(user, user.Roles);

        var raw = TokenService.GenerateRefreshToken();
        store.RefreshTokens.Insert(new RefreshToken
        {
            UserId = user.Id,
            FamilyId = familyId,
            TokenHash = TokenService.Hash(raw),
            ExpiresAt = DateTime.UtcNow.AddDays(_opt.RefreshTokenDays),
            CreatedByIp = client.Ip,
            UserAgent = client.UserAgent?[..Math.Min(client.UserAgent.Length, 256)],
        });

        return new TokenResponse(access, raw, expiresIn);
    }

    private void RevokeFamily(Guid familyId)
    {
        var now = DateTime.UtcNow;
        foreach (var t in store.RefreshTokens.Find(t => t.FamilyId == familyId && t.RevokedAt == null).ToList())
        {
            t.RevokedAt = now;
            store.RefreshTokens.Update(t);
        }
    }

    private static string Normalize(string email) => email.Trim().ToUpperInvariant();
}
