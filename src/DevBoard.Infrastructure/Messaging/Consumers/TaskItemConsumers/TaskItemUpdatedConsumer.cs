// Infrastructure/Messaging/Consumers/TaskItemUpdatedConsumer.cs
using DevBoard.Application.Events;
using DevBoard.Infrastructure.Contexts.Application;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Messaging.Consumers.TaskItemConsumers
{
    public class TaskItemUpdatedConsumer : IConsumer<TaskItemUpdatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TaskItemUpdatedConsumer> _logger;

        public TaskItemUpdatedConsumer(
            ApplicationDbContext dbContext,
            ILogger<TaskItemUpdatedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TaskItemUpdatedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing TaskItemUpdatedEvent {EventId} for Tenant {TenantId}, TaskItem {TaskItemId}",
                @event.EventId, @event.TenantId, @event.TaskItemId);

            try
            {
                // Sync to search index, analytics, or audit log
                _logger.LogInformation(
                    "Task {TaskItemId} updated with new title: {Title}",
                    @event.TaskItemId, @event.Title);

                // TODO: Update search index (Elasticsearch, etc.)
                // TODO: Log audit trail

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing TaskItemUpdatedEvent {EventId}", @event.EventId);
                throw;
            }
        }
    }
}