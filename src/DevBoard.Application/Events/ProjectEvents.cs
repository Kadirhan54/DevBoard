// Application/Events/ProjectEvents.cs
namespace DevBoard.Application.Events
{
    public record ProjectCreatedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid ProjectId { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    public record ProjectUpdatedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid ProjectId { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    public record ProjectDeletedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid ProjectId { get; init; }
    }

    public record ProjectMemberAddedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public Guid TenantId { get; init; }

        public Guid ProjectId { get; init; }
        public string UserId { get; init; } = string.Empty; // IdentityUser uses string
        public string Role { get; init; } = string.Empty;
    }
}