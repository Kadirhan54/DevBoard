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
        private readonly OutboxSettings _settings;

        public OutboxCleanupService(
            IServiceProvider serviceProvider,
            ILogger<OutboxCleanupService> logger,
            OutboxSettings settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Cleanup Service started");

            // Wait before first run
            await Task.Delay(_settings.CleanupStartupDelayMinutes, stoppingToken);

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

                await Task.Delay(_settings.CleanupIntervalHours, stoppingToken);
            }

            _logger.LogInformation("Outbox Cleanup Service stopped");
        }

        private async Task CleanupOldMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(-_settings.CleanupRetentionDays);

            var deletedCount = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc != null && m.ProcessedOnUtc < cutoffDate)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Cleaned up {Count} old outbox messages older than {Days} days",
                    deletedCount,
                    _settings.CleanupRetentionDays);
            }
        }
    }
}