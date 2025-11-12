// ============================================================================
// FILE: Services/Notifications/DevBoard.Services.Notifications.Api/Consumers/TaskCreatedConsumer.cs
// ============================================================================
using DevBoard.Services.Notifications.Api.Services;
using DevBoard.Shared.Common.HttpClients;
using DevBoard.Shared.Contracts.Events;
using MassTransit;

namespace DevBoard.Services.Notifications.Api.Consumers;

public class TaskCreatedConsumer : IConsumer<TaskCreatedEvent>
{
    private readonly IIdentityServiceClient _identityClient;
    private readonly IEmailService _emailService;
    private readonly ILogger<TaskCreatedConsumer> _logger;

    public TaskCreatedConsumer(
        IIdentityServiceClient identityClient,
        IEmailService emailService,
        ILogger<TaskCreatedConsumer> logger)
    {
        _identityClient = identityClient;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TaskCreatedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Processing TaskCreatedEvent {EventId} for Tenant {TenantId}, Task {TaskId}",
            @event.EventId, @event.TenantId, @event.TaskItemId);

        try
        {
            // If task is assigned, notify the user
            if (@event.AssignedToUserId.HasValue)
            {
                var userResult = await _identityClient.GetUserByIdAsync(@event.AssignedToUserId.Value);

                if (userResult.IsSuccess && userResult.Value.EnableNotifications)
                {
                    await _emailService.SendTaskAssignedEmailAsync(
                        userResult.Value.Email,
                        @event.Title,
                        @event.TaskItemId);

                    _logger.LogInformation(
                        "Sent notification to user {UserId} for new task: {Title}",
                        @event.AssignedToUserId, @event.Title);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TaskCreatedEvent {EventId}", @event.EventId);
            throw; // Will trigger retry
        }
    }
}