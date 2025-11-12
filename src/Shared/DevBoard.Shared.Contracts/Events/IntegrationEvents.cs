// ============================================================================
// FILE: Shared/DevBoard.Shared.Contracts/Events/IntegrationEvents.cs
// ============================================================================
namespace DevBoard.Shared.Contracts.Events;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    Guid TenantId { get; }
}

// Identity Events
public record UserRegisteredEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
}

public record TenantCreatedEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
}

// Project Events
public record ProjectCreatedEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid TenantId { get; init; }
    public Guid ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record BoardCreatedEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid TenantId { get; init; }
    public Guid BoardId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid ProjectId { get; init; }
}

public record BoardDeletedEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid TenantId { get; init; }
    public Guid BoardId { get; init; }
}

// Task Events
public record TaskCreatedEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid TenantId { get; init; }
    public Guid TaskItemId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid? AssignedToUserId { get; init; }
    public Guid BoardId { get; init; }
}

public record TaskStatusChangedEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid TenantId { get; init; }
    public Guid TaskItemId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
}

public record CommentAddedEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid TenantId { get; init; }
    public Guid TaskItemId { get; init; }
    public Guid CommentId { get; init; }
    public string Text { get; init; } = string.Empty;
    public Guid CommentedByUserId { get; init; }
}