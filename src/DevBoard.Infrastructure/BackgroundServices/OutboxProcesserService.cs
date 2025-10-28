// Infrastructure/BackgroundServices/OutboxProcessorService.cs
using DevBoard.Application.Events;
using DevBoard.Domain.Common;
using DevBoard.Infrastructure.Contexts.Application;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DevBoard.Infrastructure.BackgroundServices
{
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessorService> _logger;
        private readonly OutboxProcessorSettings _settings;

        public OutboxProcessorService(
            IServiceProvider serviceProvider,
            ILogger<OutboxProcessorService> logger,
            OutboxProcessorSettings settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Outbox Processor Service started with interval: {Interval}s, batch size: {BatchSize}",
                _settings.IntervalSeconds,
                _settings.BatchSize);

            // Wait for application to fully start
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox messages");
                }

                await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Outbox Processor Service stopped");
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            // Fetch unprocessed messages
            var messages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null && m.RetryCount < _settings.MaxRetries)
                .OrderBy(m => m.OccurredOnUtc)
                .Take(_settings.BatchSize)
                .ToListAsync(cancellationToken);

            if (!messages.Any())
            {
                return;
            }

            _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

            var successCount = 0;
            var failureCount = 0;

            foreach (var message in messages)
            {
                try
                {
                    // Deserialize and publish the event
                    var eventType = Type.GetType(message.Type);
                    if (eventType == null)
                    {
                        _logger.LogError(
                            "Could not resolve event type: {Type} for message {MessageId}",
                            message.Type,
                            message.Id);

                        message.Error = $"Could not resolve type: {message.Type}";
                        message.ProcessedOnUtc = DateTime.UtcNow;
                        failureCount++;
                        continue;
                    }

                    var @event = JsonSerializer.Deserialize(message.Content, eventType);
                    if (@event == null)
                    {
                        _logger.LogError(
                            "Could not deserialize event: {Type} for message {MessageId}",
                            message.Type,
                            message.Id);

                        message.Error = "Deserialization failed";
                        message.ProcessedOnUtc = DateTime.UtcNow;
                        failureCount++;
                        continue;
                    }

                    // Publish to RabbitMQ
                    await publishEndpoint.Publish(@event, eventType, cancellationToken);

                    // Mark as processed
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    successCount++;

                    _logger.LogDebug(
                        "Outbox message published: {MessageId}, Type: {Type}, TenantId: {TenantId}",
                        message.Id,
                        eventType.Name,
                        message.TenantId);
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.Error = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {ex.Message}";

                    _logger.LogError(ex,
                        "Failed to process outbox message {MessageId}, Retry: {RetryCount}/{MaxRetries}",
                        message.Id,
                        message.RetryCount,
                        _settings.MaxRetries);

                    // Mark as processed after max retries
                    if (message.RetryCount >= _settings.MaxRetries)
                    {
                        message.ProcessedOnUtc = DateTime.UtcNow;
                        _logger.LogError(
                            "Outbox message {MessageId} failed after {RetryCount} retries. Marking as failed.",
                            message.Id,
                            message.RetryCount);
                    }

                    failureCount++;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            if (successCount > 0 || failureCount > 0)
            {
                _logger.LogInformation(
                    "Outbox batch processed: {Success} succeeded, {Failure} failed",
                    successCount,
                    failureCount);
            }
        }


    }
}