// Infrastructure/Messaging/Consumers/ProjectCreatedConsumer.cs
using DevBoard.Application.Events;
using DevBoard.Infrastructure.Contexts.Application;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Messaging.Consumers.ProjectConsumers
{
    public class ProjectCreatedConsumer : IConsumer<ProjectCreatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ProjectCreatedConsumer> _logger;

        public ProjectCreatedConsumer(
            ApplicationDbContext dbContext,
            ILogger<ProjectCreatedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProjectCreatedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing ProjectCreatedEvent {EventId} for Tenant {TenantId}, Project {ProjectId}",
                @event.EventId, @event.TenantId, @event.ProjectId);

            try
            {
                // Notify tenant admins
                var tenantAdmins = await _dbContext.Users
                    .Where(u => u.TenantId == @event.TenantId)
                    .ToListAsync();

                foreach (var admin in tenantAdmins.Where(a => a.EnableNotifications))
                {
                    _logger.LogInformation(
                        "Notifying admin {UserId} about new project: {ProjectName}",
                        admin.Id, @event.Name);

                    // TODO: Send notification
                }

                // Initialize project resources
                _logger.LogInformation("Project {ProjectId} created: {Name}",
                    @event.ProjectId, @event.Name);

                // TODO: Create default board structure
                // TODO: Setup project analytics
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ProjectCreatedEvent {EventId}", @event.EventId);
                throw;
            }
        }
    }
}