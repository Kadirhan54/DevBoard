// Infrastructure/Messaging/Consumers/TaskItemStatusChangedConsumer.cs
using DevBoard.Application.Events;
using DevBoard.Infrastructure.Contexts.Application;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Messaging.Consumers.TaskItemConsumers
{
    public class TaskItemStatusChangedConsumer : IConsumer<TaskItemStatusChangedEvent>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TaskItemStatusChangedConsumer> _logger;

        public TaskItemStatusChangedConsumer(
            ApplicationDbContext dbContext,
            ILogger<TaskItemStatusChangedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TaskItemStatusChangedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing TaskItemStatusChangedEvent {EventId} for Tenant {TenantId}, TaskItem {TaskItemId}",
                @event.EventId, @event.TenantId, @event.TaskItemId);

            try
            {
                // Fetch task with assigned user
                var taskItem = await _dbContext.Tasks
                    .Include(t => t.AssignedUserId)
                    .Where(t => t.Id == @event.TaskItemId)
                    .FirstOrDefaultAsync();

                //
                // TODO : Implement notification logic
                //
                // if (taskItem?.AssignedUserId?.EnableNotifications == true)

                _logger.LogInformation(
                        "Task {TaskItemId} status changed from {OldStatus} to {NewStatus}. Notifying user {UserId}",
                        @event.TaskItemId, @event.OldStatus, @event.NewStatus, taskItem.AssignedUserId);

                // Track analytics or metrics
                _logger.LogInformation(
                    "Status transition recorded: {OldStatus} -> {NewStatus} for tenant {TenantId}",
                    @event.OldStatus, @event.NewStatus, @event.TenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing TaskItemStatusChangedEvent {EventId}", @event.EventId);
                throw;
            }
        }
    }
}