// Application/Events/BoardEvents.cs
namespace DevBoard.Application.Events
{
    public record BoardCreatedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid BoardId { get; init; }
        public string Name { get; init; } = string.Empty;
        public Guid ProjectId { get; init; }
    }

    public record BoardUpdatedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid BoardId { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    public record BoardDeletedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid BoardId { get; init; }
    }
}