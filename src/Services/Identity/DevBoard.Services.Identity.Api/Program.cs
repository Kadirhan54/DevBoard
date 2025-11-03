using DevBoard.Services.Identity.Core.Entities;
using DevBoard.Services.Identity.Infrastructure.Data;
using DevBoard.Services.Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// JWT Token service
builder.Services.AddScoped<TokenService>();

builder.Services.AddDbContext<IdentityDbContext>((options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DevBoardIdentityDb"));
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
