using AuthService.Domain;
using LiteDB;

namespace AuthService.Data;

/// <summary>
/// Обёртка над LiteDB. Регистрируется как singleton: один экземпляр LiteDatabase на процесс
/// (он потокобезопасный). Файл блокируется процессом, поэтому реплик сервиса может быть только одна.
/// </summary>
public sealed class AuthDb : IDisposable
{
    public LiteDatabase Db { get; }

    public ILiteCollection<AppUser> Users => Db.GetCollection<AppUser>("users");
    public ILiteCollection<Tenant> Tenants => Db.GetCollection<Tenant>("tenants");
    public ILiteCollection<RefreshToken> RefreshTokens => Db.GetCollection<RefreshToken>("refresh_tokens");

    public AuthDb(IConfiguration config)
    {
        var cs = new ConnectionString(config.GetConnectionString("Auth") ?? "Filename=bin/data/auth.db");

        var dir = Path.GetDirectoryName(Path.GetFullPath(cs.Filename));
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        Db = new LiteDatabase(cs);

        Users.EnsureIndex(u => u.NormalizedEmail, unique: true);
        Users.EnsureIndex(u => u.TenantId);
        RefreshTokens.EnsureIndex(t => t.TokenHash, unique: true);
        RefreshTokens.EnsureIndex(t => t.FamilyId);
    }

    public void Dispose() => Db.Dispose();
}
