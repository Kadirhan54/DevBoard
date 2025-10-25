// Application/Events/IIntegrationEvent.cs
namespace DevBoard.Application.Events
{
    public interface IIntegrationEvent
    {
        Guid EventId { get; }
        DateTime OccurredAt { get; }
        Guid TenantId { get; } // Critical for multi-tenancy
    }
}