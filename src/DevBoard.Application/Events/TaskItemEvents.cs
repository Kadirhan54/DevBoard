// Application/Events/TaskItemEvents.cs
namespace DevBoard.Application.Events
{
    public record TaskItemCreatedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid TaskItemId { get; init; }
        public string Title { get; init; } = string.Empty;
        public Guid? AssignedToUserId { get; init; }
        public Guid BoardId { get; init; }
    }

    public record TaskItemUpdatedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid TaskItemId { get; init; }
        public string Title { get; init; } = string.Empty;
    }

    public record TaskItemDeletedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid TaskItemId { get; init; }
    }

    public record TaskItemStatusChangedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid TaskItemId { get; init; }
        public string OldStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
    }

    public record TaskItemAssignedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid TaskItemId { get; init; }
        public Guid AssignedToUserId { get; init; }
        public Guid AssignedByUserId { get; init; }
    }
}