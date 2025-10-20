using DevBoard.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Infrastructure.Consumers
{
    public class ProjectCreatedConsumer : IConsumer<ProjectCreated>
    {
        private readonly ILogger<ProjectCreatedConsumer> _logger;

        //public ProjectCreatedConsumer(ILogger<ProjectCreatedConsumer> logger)
        //{
        //    _logger = logger;
        //}

        public Task Consume(ConsumeContext<ProjectCreated> context)
        {
            var message = context.Message;
            //_logger.LogInformation("Received ProjectCreated event: {Id} - {Name}", message.ProjectId, message.Name);

            Console.WriteLine($"Received Project: {context.Message.Name}");


            // TODO: handle logic (e.g., send notification, cache, etc.)

            return Task.CompletedTask;
        }
    }
}
