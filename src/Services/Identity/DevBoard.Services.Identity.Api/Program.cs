using DevBoard.Services.Identity.Core.Entities;
using DevBoard.Services.Identity.Infrastructure.Data;
using DevBoard.Services.Identity.Infrastructure.Seed;
using DevBoard.Services.Identity.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    //x.AddConsumer<BoardDeletedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        //cfg.ReceiveEndpoint("task-board-deleted", e =>
        //{
        //    e.ConfigureConsumer<BoardDeletedConsumer>(context);
        //});

        cfg.ConfigureEndpoints(context);
    });
});

// JWT Token service
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddDbContext<IdentityDbContext>((options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityDatabase"));
    options.EnableSensitiveDataLogging(); // optional
});

// Identity Core
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.MigrateAsync();
}

// ✅ Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<IdentityDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();

        await IdentityDbSeeder.SeedAsync(context, userManager, roleManager, loggerFactory);
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

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
