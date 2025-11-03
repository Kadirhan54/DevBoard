using DevBoard.Services.Tasks.Api.Services;
using DevBoard.Services.Tasks.Infrastructure.Data;
using DevBoard.Services.Tasks.Infrastructure.HttpClients;
using DevBoard.Shared.Common;
using DevBoard.Shared.Common.HttpClients;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DevBoard Tasks API",
        Version = "v1",
        Description = "API for DevBoard Task Service",
        Contact = new OpenApiContact
        {
            Name = "DevBoard Team",
            Email = "support@devboard.com"
        }
    });


    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement{
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                new string[]{}
            }
        });
});

// Database
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DevBoardTaskDb")));

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider,TenantProvider>();
builder.Services.AddScoped<TaskService>();

// ✅ HTTP Client for Project Service
builder.Services.AddHttpClient<IProjectServiceClient, ProjectServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProjectService:Url"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevBoard API v1");
        c.RoutePrefix = string.Empty; // serve UI at root
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
