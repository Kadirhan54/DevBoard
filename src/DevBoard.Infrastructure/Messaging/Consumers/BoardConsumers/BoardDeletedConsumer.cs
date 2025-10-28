// Infrastructure/Messaging/Consumers/BoardDeletedConsumer.cs
using DevBoard.Application.Events;
using DevBoard.Infrastructure.Contexts.Application;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Messaging.Consumers.BoardConsumers
{
    public class BoardDeletedConsumer : IConsumer<BoardDeletedEvent>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<BoardDeletedConsumer> _logger;

        public BoardDeletedConsumer(
            ApplicationDbContext dbContext,
            ILogger<BoardDeletedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BoardDeletedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing BoardDeletedEvent {EventId} for Tenant {TenantId}, Board {BoardId}",
                @event.EventId, @event.TenantId, @event.BoardId);

            try
            {
                // Cascade cleanup of related resources
                _logger.LogInformation("Cleaning up resources for deleted board {BoardId}", @event.BoardId);

                // TODO: Archive board data
                // TODO: Remove from search index
                // TODO: Cancel pending notifications for tasks in this board

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing BoardDeletedEvent {EventId}", @event.EventId);
                throw;
            }
        }
    }
}