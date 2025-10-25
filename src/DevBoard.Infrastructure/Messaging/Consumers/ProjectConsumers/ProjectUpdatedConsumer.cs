// Infrastructure/Messaging/Consumers/ProjectUpdatedConsumer.cs
using DevBoard.Application.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Messaging.Consumers.ProjectConsumers
{
    public class ProjectUpdatedConsumer : IConsumer<ProjectUpdatedEvent>
    {
        private readonly ILogger<ProjectUpdatedConsumer> _logger;

        public ProjectUpdatedConsumer(ILogger<ProjectUpdatedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProjectUpdatedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing ProjectUpdatedEvent {EventId} for Tenant {TenantId}, Project {ProjectId}",
                @event.EventId, @event.TenantId, @event.ProjectId);

            try
            {
                _logger.LogInformation("Project {ProjectId} updated with name: {Name}",
                    @event.ProjectId, @event.Name);

                // TODO: Update search index
                // TODO: Sync to analytics dashboard

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ProjectUpdatedEvent {EventId}", @event.EventId);
                throw;
            }
        }
    }
}