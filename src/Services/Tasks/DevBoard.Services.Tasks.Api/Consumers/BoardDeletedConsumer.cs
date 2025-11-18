// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Api/Consumers/BoardDeletedConsumer.cs
// ============================================================================
using DevBoard.Services.Tasks.Infrastructure.Data;
using DevBoard.Shared.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Services.Tasks.Api.Consumers;

/// <summary>
/// Handles cascade deletion of tasks when a board is deleted in Project Service
/// This demonstrates eventual consistency and service autonomy
/// </summary>
public class BoardDeletedConsumer : IConsumer<BoardDeletedEvent>
{
    private readonly TaskDbContext _context;
    private readonly ILogger<BoardDeletedConsumer> _logger;

    public BoardDeletedConsumer(
        TaskDbContext context,
        ILogger<BoardDeletedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BoardDeletedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Processing BoardDeletedEvent {EventId} for Board {BoardId}, Tenant {TenantId}",
            @event.EventId, @event.BoardId, @event.TenantId);

        try
        {
            // Find all tasks belonging to the deleted board
            var tasksToDelete = await _context.Tasks
                .Where(t => t.BoardId == @event.BoardId && t.TenantId == @event.TenantId)
                .Include(t => t.Comments) // Include for cascade delete
                .ToListAsync();

            if (tasksToDelete.Count == 0)
            {
                _logger.LogInformation(
                    "No tasks found for deleted board {BoardId}",
                    @event.BoardId);
                return;
            }

            _logger.LogInformation(
                "Deleting {Count} tasks for board {BoardId}",
                tasksToDelete.Count, @event.BoardId);

            // Option 1: Hard Delete (remove from database)
            _context.Tasks.RemoveRange(tasksToDelete);

            // Option 2: Soft Delete (mark as deleted) - Uncomment if you prefer this
            // foreach (var task in tasksToDelete)
            // {
            //     task.IsDeleted = true;
            //     task.DeletedAt = DateTime.UtcNow;
            // }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully deleted {Count} tasks for board {BoardId}",
                tasksToDelete.Count, @event.BoardId);

            // Optional: Publish TaskDeletedEvent for each task
            // This allows Notification Service to inform users
            // foreach (var task in tasksToDelete)
            // {
            //     await context.Publish(new TaskDeletedEvent
            //     {
            //         TenantId = @event.TenantId,
            //         TaskItemId = task.Id
            //     });
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing BoardDeletedEvent {EventId} for board {BoardId}",
                @event.EventId, @event.BoardId);

            // Throw to trigger retry mechanism
            throw;
        }
    }
}
