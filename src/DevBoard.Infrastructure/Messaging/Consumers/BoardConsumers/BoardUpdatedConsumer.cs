// Infrastructure/Messaging/Consumers/BoardUpdatedConsumer.cs
using DevBoard.Application.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Messaging.Consumers.BoardConsumers
{
    public class BoardUpdatedConsumer : IConsumer<BoardUpdatedEvent>
    {
        private readonly ILogger<BoardUpdatedConsumer> _logger;

        public BoardUpdatedConsumer(ILogger<BoardUpdatedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BoardUpdatedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing BoardUpdatedEvent {EventId} for Tenant {TenantId}, Board {BoardId}",
                @event.EventId, @event.TenantId, @event.BoardId);

            try
            {
                _logger.LogInformation("Board {BoardId} updated with name: {Name}",
                    @event.BoardId, @event.Name);

                // TODO: Update search index
                // TODO: Sync to analytics

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing BoardUpdatedEvent {EventId}", @event.EventId);
                throw;
            }
        }
    }
}