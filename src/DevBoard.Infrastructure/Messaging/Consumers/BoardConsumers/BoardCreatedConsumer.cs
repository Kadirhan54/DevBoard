// Infrastructure/Messaging/Consumers/BoardCreatedConsumer.cs
using DevBoard.Application.Events;
using DevBoard.Infrastructure.Contexts.Application;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Messaging.Consumers.BoardConsumers
{
    public class BoardCreatedConsumer : IConsumer<BoardCreatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<BoardCreatedConsumer> _logger;

        public BoardCreatedConsumer(
            ApplicationDbContext dbContext,
            ILogger<BoardCreatedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BoardCreatedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing BoardCreatedEvent {EventId} for Tenant {TenantId}, Board {BoardId}",
                @event.EventId, @event.TenantId, @event.BoardId);

            try
            {
                // Notify project members
                var projectMembers = await _dbContext.Users
                    .Include(u => u.Tenant)
                    .Where(u => u.TenantId == @event.TenantId)
                    .ToListAsync();

                foreach (var member in projectMembers.Where(m => m.EnableNotifications))
                {
                    _logger.LogInformation(
                        "Notifying user {UserId} about new board: {BoardName}",
                        member.Id, @event.Name);

                    // TODO: Send notification
                }

                // Initialize default columns or settings
                _logger.LogInformation("Board {BoardId} created in project {ProjectId}",
                    @event.BoardId, @event.ProjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing BoardCreatedEvent {EventId}", @event.EventId);
                throw;
            }
        }
    }
}