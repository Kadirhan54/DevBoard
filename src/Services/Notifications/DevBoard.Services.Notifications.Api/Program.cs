using DevBoard.Services.Notifications.Api.Consumers;
using DevBoard.Services.Notifications.Api.Services;
using DevBoard.Services.Notifications.Infrastructure.HttpClients;
using DevBoard.Shared.Common.HttpClients;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services
builder.Services.AddSingleton<IEmailService, EmailService>();

// HTTP Client for Identity Service
builder.Services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:IdentityService:Url"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// MassTransit with Consumers
builder.Services.AddMassTransit(x =>
{
    // Register all consumers
    x.AddConsumer<TaskCreatedConsumer>();
    x.AddConsumer<TaskStatusChangedConsumer>();
    x.AddConsumer<CommentAddedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        // Configure receive endpoints
        cfg.ReceiveEndpoint("notification-task-created", e =>
        {
            e.ConfigureConsumer<TaskCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("notification-task-status-changed", e =>
        {
            e.ConfigureConsumer<TaskStatusChangedConsumer>(context);
        });

        cfg.ReceiveEndpoint("notification-comment-added", e =>
        {
            e.ConfigureConsumer<CommentAddedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevBoard API v1");
        c.RoutePrefix = string.Empty; // serve UI at root
    });
}

app.MapControllers();

app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
