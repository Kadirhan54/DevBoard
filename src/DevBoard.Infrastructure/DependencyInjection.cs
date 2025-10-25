// Infrastructure/DependencyInjection.cs
using DevBoard.Infrastructure.Messaging;
using DevBoard.Infrastructure.Messaging.Configuration;
using DevBoard.Infrastructure.Messaging.Consumers;
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
            // ... existing services ...

            // RabbitMQ Settings
            var rabbitMqSettings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() ?? new RabbitMqSettings();
            services.AddSingleton(rabbitMqSettings);

            // MassTransit Configuration
            services.AddMassTransit(x =>
            {
                // Register all consumers
                x.AddConsumer<TaskItemCreatedConsumer>();
                //x.AddConsumer<TaskItemAssignedConsumer>();
                //x.AddConsumer<ProjectMemberAddedConsumer>();
                // Add more consumers as needed

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

                    // Configure consumers with DLQ
                    cfg.ReceiveEndpoint("task-item-created", e =>
                    {
                        e.ConfigureConsumer<TaskItemCreatedConsumer>(context);
                        e.UseMessageRetry(r => r.Incremental(
                            retryLimit: rabbitMqSettings.RetryCount,
                            initialInterval: TimeSpan.FromSeconds(rabbitMqSettings.RetryIntervalSeconds),
                            intervalIncrement: TimeSpan.FromSeconds(rabbitMqSettings.RetryIntervalSeconds)
                        ));
                    });

                    //cfg.ReceiveEndpoint("task-item-assigned", e =>
                    //{
                    //    e.ConfigureConsumer<TaskItemAssignedConsumer>(context);
                    //    e.UseMessageRetry(r => r.Incremental(
                    //        retryLimit: rabbitMqSettings.RetryCount,
                    //        initialInterval: TimeSpan.FromSeconds(rabbitMqSettings.RetryIntervalSeconds),
                    //        intervalIncrement: TimeSpan.FromSeconds(rabbitMqSettings.RetryIntervalSeconds)
                    //    ));
                    //});

                    //cfg.ReceiveEndpoint("project-member-added", e =>
                    //{
                    //    e.ConfigureConsumer<ProjectMemberAddedConsumer>(context);
                    //    e.UseMessageRetry(r => r.Incremental(
                    //        retryLimit: rabbitMqSettings.RetryCount,
                    //        initialInterval: TimeSpan.FromSeconds(rabbitMqSettings.RetryIntervalSeconds),
                    //        intervalIncrement: TimeSpan.FromSeconds(rabbitMqSettings.RetryIntervalSeconds)
                    //    ));
                    //});

                    cfg.ConfigureEndpoints(context);
                });

                // Add tenant context filter
                //x.AddPublishMessageFilter(typeof(TenantContextFilter<>));
            });

        }
    }
}