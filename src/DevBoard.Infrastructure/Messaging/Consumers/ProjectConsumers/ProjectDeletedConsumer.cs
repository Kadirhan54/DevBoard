// Infrastructure/Messaging/Consumers/ProjectDeletedConsumer.cs
using DevBoard.Application.Events;
using DevBoard.Infrastructure.Contexts.Application;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Messaging.Consumers.ProjectConsumers
{
    public class ProjectDeletedConsumer : IConsumer<ProjectDeletedEvent>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ProjectDeletedConsumer> _logger;

        public ProjectDeletedConsumer(
            ApplicationDbContext dbContext,
            ILogger<ProjectDeletedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProjectDeletedEvent> context)
        {
            var @event = context.Message;

            _logger.LogInformation(
                "Processing ProjectDeletedEvent {EventId} for Tenant {TenantId}, Project {ProjectId}",
                @event.EventId, @event.TenantId, @event.ProjectId);

            try
            {
                // Major cleanup operation
                _logger.LogInformation("Cleaning up all resources for deleted project {ProjectId}", @event.ProjectId);

                // TODO: Archive project data to data warehouse
                // TODO: Remove all related boards, tasks from search index
                // TODO: Cancel all pending notifications
                // TODO: Cleanup file storage

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ProjectDeletedEvent {EventId}", @event.EventId);
                throw;
            }
        }
    }
}