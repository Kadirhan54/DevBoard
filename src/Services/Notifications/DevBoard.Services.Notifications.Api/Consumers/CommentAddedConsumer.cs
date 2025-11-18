
// ============================================================================
// FILE: Services/Notifications/DevBoard.Services.Notifications.Api/Consumers/CommentAddedConsumer.cs
// ============================================================================
using DevBoard.Services.Notifications.Api.Services;
using DevBoard.Shared.Common.HttpClients;
using DevBoard.Shared.Contracts.Events;
using MassTransit;

namespace DevBoard.Services.Notifications.Api.Consumers;

public class CommentAddedConsumer : IConsumer<CommentAddedEvent>
{
    private readonly IIdentityServiceClient _identityClient;
    private readonly IEmailService _emailService;
    private readonly ILogger<CommentAddedConsumer> _logger;

    public CommentAddedConsumer(
        IIdentityServiceClient identityClient,
        IEmailService emailService,
        ILogger<CommentAddedConsumer> logger)
    {
        _identityClient = identityClient;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CommentAddedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Processing CommentAddedEvent {EventId} for Task {TaskId}",
            @event.EventId, @event.TaskItemId);

        try
        {
            // TODO: Get task owner and watchers
            // Send notification about new comment

            _logger.LogInformation(
                "Comment added to task {TaskId} by user {UserId}",
                @event.TaskItemId, @event.CommentedByUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CommentAddedEvent {EventId}", @event.EventId);
            throw;
        }
    }
}