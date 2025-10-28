// Infrastructure/Messaging/Consumers/TaskItemCreatedConsumer.cs
using DevBoard.Application.Events;
using DevBoard.Infrastructure.Contexts.Application;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Messaging.Consumers.TaskItemConsumers
{
    public class TaskItemCreatedConsumer : IConsumer<TaskItemCreatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TaskItemCreatedConsumer> _logger;

        public TaskItemCreatedConsumer(
            ApplicationDbContext dbContext,
            ILogger<TaskItemCreatedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TaskItemCreatedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing TaskItemCreatedEvent with Id: {EventId} for Tenant Id: {TenantId}, TaskItem Id: {TaskItemId}",
                @event.EventId, @event.TenantId, @event.TaskItemId);

            try
            {
                // Example: Send notification to assigned user
                if (@event.AssignedToUserId.HasValue)
                {
                    var user = await _dbContext.Users
                        .Where(u => u.TenantId == @event.TenantId && u.Id == @event.AssignedToUserId.Value.ToString())
                        .FirstOrDefaultAsync();

                    if (user?.EnableNotifications == true)
                    {
                        // TODO: Send email/push notification
                        _logger.LogInformation(
                            "Sending notification to user {UserId} for new task: {Title}",
                            user.Id, @event.Title);
                    }
                }

                _logger.LogInformation("TaskItemCreatedEvent processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing TaskItemCreatedEvent {EventId}", @event.EventId);
                throw; // Will trigger retry logic
            }
        }
    }
}