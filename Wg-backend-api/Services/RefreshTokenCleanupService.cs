using Wg_backend_api.Data;

// Class to clean up expired or revoked refresh tokens periodically
public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;

    public RefreshTokenCleanupService(IServiceProvider services)
    {
        this._services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = this._services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GlobalDbContext>();

            var expiredTokens = db.RefreshTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || t.RevokedAt != null);

            db.RefreshTokens.RemoveRange(expiredTokens);
            await db.SaveChangesAsync();

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // TODO make interval configurable
        }
    }
}
