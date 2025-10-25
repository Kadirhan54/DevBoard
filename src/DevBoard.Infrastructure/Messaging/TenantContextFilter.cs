// Infrastructure/Messaging/TenantContextFilter.cs
using DevBoard.Application.Events;
using MassTransit;

namespace DevBoard.Infrastructure.Messaging
{
    public class TenantContextFilter<T> : IFilter<PublishContext<T>> where T : class, IIntegrationEvent
    {
        public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            // Add tenant ID to message headers for observability
            context.Headers.Set("TenantId", context.Message.TenantId.ToString());

            await next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("tenant-context");
        }
    }
}