// Infrastructure/Services/EventPublisher.cs
using DevBoard.Application.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Services
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<EventPublisher> _logger;

        public EventPublisher(
            IPublishEndpoint publishEndpoint,
            ILogger<EventPublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            try
            {
                await _publishEndpoint.Publish(@event, cancellationToken);

                _logger.LogInformation(
                    "Event {EventType} published successfully",
                    typeof(TEvent).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish event {EventType}",
                    typeof(TEvent).Name);

                // Don't throw - event publishing should not break the request
                // Failed events will be logged and can be retried via other mechanisms
            }
        }
    }
}