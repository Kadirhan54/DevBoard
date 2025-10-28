// Infrastructure/DependencyInjection.cs
using DevBoard.Application.Services;
using DevBoard.Infrastructure.BackgroundServices;
using DevBoard.Infrastructure.Messaging;
using DevBoard.Infrastructure.Messaging.Configuration;
using DevBoard.Infrastructure.Messaging.Consumers.BoardConsumers;
using DevBoard.Infrastructure.Messaging.Consumers.ProjectConsumers;
using DevBoard.Infrastructure.Messaging.Consumers.TaskItemConsumers;
using DevBoard.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

//using MassTransit.Configuration;

namespace DevBoard.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddScoped<ITenantProvider, TenantProvider>();
            services.AddScoped<IEventPublisher, EventPublisher>();

            // Outbox Processor Settings
            var outboxProcessorSettings = configuration.GetSection("OutboxProcessor").Get<OutboxProcessorSettings>() ?? new OutboxProcessorSettings();
            services.AddSingleton(outboxProcessorSettings);

            // Outbox Service
            services.AddScoped<IOutboxService, OutboxService>();

            // Background Service for Outbox Processing
            services.AddHostedService<OutboxProcessorService>();

            // Background Service for Outbox Cleanup
            services.AddHostedService<OutboxCleanupService>();

            // RabbitMQ Settings
            var rabbitMqSettings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() ?? new RabbitMqSettings();
            services.AddSingleton(rabbitMqSettings);

            // MassTransit Configuration
            services.AddMassTransit(x =>
            {
                // Register all TaskItem consumers
                x.AddConsumer<TaskItemCreatedConsumer>();
                x.AddConsumer<TaskItemUpdatedConsumer>();
                x.AddConsumer<TaskItemDeletedConsumer>();
                x.AddConsumer<TaskItemStatusChangedConsumer>();
                // TODO
                //x.AddConsumer<TaskItemAssignedConsumer>();

                // Register all Board consumers
                x.AddConsumer<BoardCreatedConsumer>();
                x.AddConsumer<BoardUpdatedConsumer>();
                x.AddConsumer<BoardDeletedConsumer>();

                // Register all Project consumers
                x.AddConsumer<ProjectCreatedConsumer>();
                x.AddConsumer<ProjectUpdatedConsumer>();
                x.AddConsumer<ProjectDeletedConsumer>();
                // TODO
                //x.AddConsumer<ProjectMemberAddedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.Port, rabbitMqSettings.VirtualHost, h =>
                    {
                        h.Username(rabbitMqSettings.Username);
                        h.Password(rabbitMqSettings.Password);
                    });

                    // Configure retry policy with exponential backoff
                    cfg.UseMessageRetry(r =>
                    {
                        r.Incremental(
                            retryLimit: rabbitMqSettings.RetryCount,
                            initialInterval: TimeSpan.FromSeconds(rabbitMqSettings.RetryIntervalSeconds),
                            intervalIncrement: TimeSpan.FromSeconds(rabbitMqSettings.RetryIntervalSeconds)
                        );

                        //
                        // TODO : Monitor this behavior 
                        //

                        // Ignore (don’t retry) explicitly non-retryable exceptions
                        foreach (var exceptionType in ExceptionFilters.NonRetryableExceptions)
                        {
                            var ignoreMethod = typeof(IRetryConfigurator)
                                .GetMethods()
                                .FirstOrDefault(m => m.Name == "Ignore" && m.IsGenericMethod && m.GetParameters().Length == 0);
                            ignoreMethod?.MakeGenericMethod(exceptionType).Invoke(r, null);
                        }

                        //
                        // TODO : Monitor this behavior 
                        //
                        
                        // Handle (force retry) explicitly retryable exceptions
                        foreach (var exceptionType in ExceptionFilters.RetryableExceptions)
                        {
                            var handleMethod = typeof(IRetryConfigurator)
                                .GetMethods()
                                .FirstOrDefault(m => m.Name == "Handle" && m.IsGenericMethod && m.GetParameters().Length == 0);
                            handleMethod?.MakeGenericMethod(exceptionType).Invoke(r, null);
                        }
                    });

                    // Configure dead-letter queue
                    cfg.ReceiveEndpoint("devboard-error-queue", e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        // This is the dead-letter queue - no consumers
                    });

                    // TaskItem endpoints
                    cfg.ReceiveEndpoint("task-item-created", e =>
                    {
                        e.ConfigureConsumer<TaskItemCreatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("task-item-updated", e =>
                    {
                        e.ConfigureConsumer<TaskItemUpdatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("task-item-deleted", e =>
                    {
                        e.ConfigureConsumer<TaskItemDeletedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("task-item-status-changed", e =>
                    {
                        e.ConfigureConsumer<TaskItemStatusChangedConsumer>(context);
                    });

                    // TODO
                    //cfg.ReceiveEndpoint("task-item-assigned", e =>
                    //{
                    //    e.ConfigureConsumer<TaskItemAssignedConsumer>(context);
                    //});

                    // Board endpoints
                    cfg.ReceiveEndpoint("board-created", e =>
                    {
                        e.ConfigureConsumer<BoardCreatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("board-updated", e =>
                    {
                        e.ConfigureConsumer<BoardUpdatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("board-deleted", e =>
                    {
                        e.ConfigureConsumer<BoardDeletedConsumer>(context);
                    });

                    // Project endpoints
                    cfg.ReceiveEndpoint("project-created", e =>
                    {
                        e.ConfigureConsumer<ProjectCreatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("project-updated", e =>
                    {
                        e.ConfigureConsumer<ProjectUpdatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("project-deleted", e =>
                    {
                        e.ConfigureConsumer<ProjectDeletedConsumer>(context);
                    });

                    // TODO
                    //cfg.ReceiveEndpoint("project-member-added", e =>
                    //{
                    //    e.ConfigureConsumer<ProjectMemberAddedConsumer>(context);
                    //});

                    cfg.ConfigureEndpoints(context);
                });

                // Add tenant context filter
                //x.AddPublishMessageFilter(typeof(TenantContextFilter<>));
            });

        }
    }
}