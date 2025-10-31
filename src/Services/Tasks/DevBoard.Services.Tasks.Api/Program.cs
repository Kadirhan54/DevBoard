using DevBoard.Services.Tasks.Api.Services;
using DevBoard.Services.Tasks.Infrastructure.Data;
using DevBoard.Services.Tasks.Infrastructure.HttpClients;
using DevBoard.Shared.Common.HttpClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TaskDatabase")));

// Services
builder.Services.AddScoped<TaskService>();

// ✅ HTTP Client for Project Service
builder.Services.AddHttpClient<IProjectServiceClient, ProjectServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProjectService:Url"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
