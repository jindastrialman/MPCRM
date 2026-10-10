using AuthService.Data;

namespace AuthService.Services;

/// <summary>Периодически удаляет давно просроченные refresh-токены, чтобы файл БД не рос.</summary>
public sealed class RefreshTokenCleanup(AuthDb store, ILogger<RefreshTokenCleanup> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));
        do
        {
            try
            {
                var cutoff = DateTime.UtcNow.AddDays(-1);
                var removed = store.RefreshTokens.DeleteMany(t => t.ExpiresAt < cutoff);
                if (removed > 0)
                    log.LogInformation("Removed {Count} expired refresh tokens", removed);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Refresh token cleanup failed");
            }
        }
        while (await timer.WaitForNextTickAsync(ct));
    }
}
