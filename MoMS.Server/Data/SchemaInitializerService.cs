using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Data;

// Runs once at startup to bring the schema up to date. By design this only
// applies additive migrations (new tables / columns). Column or table drops
// are performed manually and must never be introduced here.
public class SchemaInitializerService(
    IServiceScopeFactory scopeFactory,
    ILogger<SchemaInitializerService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MoMsDbContext>();

        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
        {
            logger.LogInformation("Schema is up to date; no pending migrations.");
            return;
        }

        logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
            pending.Count, string.Join(", ", pending));

        await context.Database.MigrateAsync(cancellationToken);

        logger.LogInformation("Schema initialization complete.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
