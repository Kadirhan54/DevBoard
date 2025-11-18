using DevBoard.Services.Tasks.Api.Consumers;
using DevBoard.Services.Tasks.Api.Services;
using DevBoard.Services.Tasks.Infrastructure.Data;
using DevBoard.Services.Tasks.Infrastructure.Extensions;
using DevBoard.Services.Tasks.Infrastructure.HttpClients;
using DevBoard.Services.Tasks.Infrastructure.Seed;
using DevBoard.Shared.Common;
using DevBoard.Shared.Common.HttpClients;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("TaskDb")));

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BoardDeletedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        cfg.ReceiveEndpoint("task-board-deleted", e =>
        {
            e.ConfigureConsumer<BoardDeletedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Add MinIO and File Upload Settings
builder.Services.AddMinIOStorageAndFileUploadSettings(builder.Configuration);

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

// TODO : Implement Identity Service 
//builder.Services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>(client =>
//{
//    client.BaseAddress = new Uri(builder.Configuration["Services:IdentityService:Url"]!);
//    client.Timeout = TimeSpan.FromSeconds(5);
//});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),

            // Optional for microservices behind gateway
            ClockSkew = TimeSpan.Zero
        };

    });

builder.Services.AddAuthorization();




var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    await db.Database.MigrateAsync();
}

// ✅ Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TaskDbContext>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();

        await TaskDbSeeder.SeedAsync(context, loggerFactory);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
        throw;
    }
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
