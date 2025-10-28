// Infrastructure/Services/OutboxService.cs
using DevBoard.Application.Events;
using DevBoard.Application.Services;
using DevBoard.Domain.Common;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DevBoard.Infrastructure.Services
{
    public class OutboxService : IOutboxService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<OutboxService> _logger;

        public OutboxService(
            ApplicationDbContext dbContext,
            ILogger<OutboxService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class, IIntegrationEvent
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = typeof(TEvent).AssemblyQualifiedName ?? typeof(TEvent).FullName!,
                Content = JsonSerializer.Serialize(@event, new JsonSerializerOptions
                {
                    WriteIndented = false
                }),
                OccurredOnUtc = DateTime.UtcNow,
                TenantId = @event.TenantId,
                RetryCount = 0
            };

            _dbContext.OutboxMessages.Add(outboxMessage);
            // TODO Note: Saving changes is typically handled outside this method in a unit of work pattern ??

            _logger.LogDebug(
                "Outbox message created: {EventType}, EventId: {EventId}, TenantId: {TenantId}",
                typeof(TEvent).Name,
                @event.EventId,
                @event.TenantId);
        }
    }
}