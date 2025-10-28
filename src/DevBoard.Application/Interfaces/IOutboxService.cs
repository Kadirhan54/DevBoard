// Application/Services/IOutboxService.cs
using DevBoard.Application.Events;

namespace DevBoard.Application.Services
{
    public interface IOutboxService
    {
        Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class, IIntegrationEvent;
    }
}