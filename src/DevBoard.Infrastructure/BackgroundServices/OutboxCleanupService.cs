// Infrastructure/BackgroundServices/OutboxCleanupService.cs
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.BackgroundServices
{
    public class OutboxCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Run daily
        private readonly int _retentionDays = 7; // Keep for 7 days

        public OutboxCleanupService(
            IServiceProvider serviceProvider,
            ILogger<OutboxCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Cleanup Service started");

            // Wait before first run
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldMessagesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up outbox messages");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Outbox Cleanup Service stopped");
        }

        private async Task CleanupOldMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(-_retentionDays);

            var deletedCount = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc != null && m.ProcessedOnUtc < cutoffDate)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Cleaned up {Count} old outbox messages older than {Days} days",
                    deletedCount,
                    _retentionDays);
            }
        }
    }
}