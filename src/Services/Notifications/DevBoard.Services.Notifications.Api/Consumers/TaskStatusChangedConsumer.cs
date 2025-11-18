
// ============================================================================
// FILE: Services/Notifications/DevBoard.Services.Notifications.Api/Consumers/TaskStatusChangedConsumer.cs
// ============================================================================
using DevBoard.Services.Notifications.Api.Services;
using DevBoard.Shared.Common.HttpClients;
using DevBoard.Shared.Contracts.Events;
using MassTransit;

namespace DevBoard.Services.Notifications.Api.Consumers;

public class TaskStatusChangedConsumer : IConsumer<TaskStatusChangedEvent>
{
    private readonly IIdentityServiceClient _identityClient;
    private readonly IEmailService _emailService;
    private readonly ILogger<TaskStatusChangedConsumer> _logger;

    public TaskStatusChangedConsumer(
        IIdentityServiceClient identityClient,
        IEmailService emailService,
        ILogger<TaskStatusChangedConsumer> logger)
    {
        _identityClient = identityClient;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TaskStatusChangedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Processing TaskStatusChangedEvent {EventId} for Task {TaskId}",
            @event.EventId, @event.TaskItemId);

        try
        {
            // TODO: Get task watchers from Task Service
            // For now, just log the status change

            _logger.LogInformation(
                "Task {TaskId} status changed from {OldStatus} to {NewStatus}",
                @event.TaskItemId, @event.OldStatus, @event.NewStatus);

            // Future: Notify team members watching this task
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TaskStatusChangedEvent {EventId}", @event.EventId);
            throw;
        }
    }
}