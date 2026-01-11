using DivingAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace DivingAPI.Services;

public sealed class RefreshTokenHousekeepingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenHousekeepingService> _logger;
    private readonly TimeSpan _interval;

    public RefreshTokenHousekeepingService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenHousekeepingService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var hours = configuration.GetValue<int?>("Auth:RefreshTokens:CleanupIntervalHours") ?? 6;
        if (hours <= 0) hours = 6;
        _interval = TimeSpan.FromHours(hours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // small delay so startup/migrations complete
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PurgeExpiredAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while purging expired refresh tokens");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task PurgeExpiredAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DivingContext>();

        var now = DateTime.UtcNow;

        // Delete tokens that have naturally expired (regardless of revoked state)
        var removed = await db.RefreshTokens
            .Where(rt => rt.Expires <= now)
            .ExecuteDeleteAsync(ct);

        if (removed > 0)
        {
            _logger.LogInformation("Purged {Count} expired refresh token(s)", removed);
        }
    }
}
