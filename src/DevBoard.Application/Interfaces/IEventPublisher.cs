// Application/Services/IEventPublisher.cs
namespace DevBoard.Application.Services
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class;
    }
}