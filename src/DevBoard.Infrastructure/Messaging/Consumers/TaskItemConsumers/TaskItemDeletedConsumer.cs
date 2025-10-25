// Infrastructure/Messaging/Consumers/TaskItemDeletedConsumer.cs
using DevBoard.Application.Events;
using DevBoard.Infrastructure.Contexts.Application;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Messaging.Consumers.TaskItemConsumers
{
    public class TaskItemDeletedConsumer : IConsumer<TaskItemDeletedEvent>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TaskItemDeletedConsumer> _logger;

        public TaskItemDeletedConsumer(
            ApplicationDbContext dbContext,
            ILogger<TaskItemDeletedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TaskItemDeletedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing TaskItemDeletedEvent {EventId} for Tenant {TenantId}, TaskItem {TaskItemId}",
                @event.EventId, @event.TenantId, @event.TaskItemId);

            try
            {
                // Cleanup related resources
                _logger.LogInformation("Cleaning up resources for deleted task {TaskItemId}", @event.TaskItemId);

                // TODO: Remove from search index
                // TODO: Archive in data warehouse
                // TODO: Cancel pending notifications

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing TaskItemDeletedEvent {EventId}", @event.EventId);
                throw;
            }
        }
    }
}