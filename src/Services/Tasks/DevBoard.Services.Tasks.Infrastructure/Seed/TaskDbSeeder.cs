// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Infrastructure/Seed/TaskDbSeeder.cs
// ============================================================================
using DevBoard.Services.Tasks.Core.Entities;
using DevBoard.Services.Tasks.Infrastructure.Data;
using DevBoard.Shared.Common.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Services.Tasks.Infrastructure.Seed;

public static class TaskDbSeeder
{
    // Must match Identity Service tenant IDs!
    private static class TenantIds
    {
        public static readonly Guid AcmeCorp = SeedIds.Tenants.AcmeCorp;
        public static readonly Guid TechStartup = SeedIds.Tenants.TechStartup;
        public static readonly Guid Enterprise = SeedIds.Tenants.Enterprise;
    }

    // Sample user IDs (must match Identity Service)
    private static class UserIds
    {
        public static readonly Guid JohnDoe = SeedIds.Users.JohnDoe;
        public static readonly Guid JaneSmith = SeedIds.Users.JaneSmith;
        public static readonly Guid Developer = SeedIds.Users.Developer;
    }

    // Task statuses
    public enum TaskStatus
    {
        Todo = 0,
        InProgress = 1,
        Review = 2,
        Done = 3,
        Blocked = 4
    }

    public static async Task SeedAsync(
        TaskDbContext context,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("TaskDbSeeder");

        try
        {
            await context.Database.MigrateAsync();

            // Get board IDs from Project Service (or use hardcoded for seeding)
            await SeedTasksAndCommentsAsync(context, logger);

            logger.LogInformation("✅ Task database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding Task database");
            throw;
        }
    }

    private static async Task SeedTasksAndCommentsAsync(
        TaskDbContext context,
        ILogger logger)
    {
        // Check if tasks already exist
        if (await context.Tasks.AnyAsync())
        {
            logger.LogInformation("ℹ️  Tasks already exist, skipping seed");
            return;
        }

        // ⚠️ IMPORTANT: Replace these GUIDs with actual board IDs from Project Service
        // You can either:
        // 1. Hardcode matching GUIDs in Project seeder
        // 2. Query Project Service via HTTP
        // 3. Use a shared seed configuration file

        var sampleBoardIds = new
        {
            BackendDev = SeedIds.Boards.PortalBackend,
            FrontendDev = SeedIds.Boards.PortalFrontend,
            Sprint1 = SeedIds.Boards.MVPSprint1,
        };

        var tasks = new List<TaskItem>
        {
            // ========================================
            // Backend Development Board Tasks
            // ========================================
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Implement JWT Authentication",
                Description = "Setup JWT-based authentication with refresh tokens. Include role-based authorization middleware.",
                Status = (int)TaskStatus.Done,
                BoardId = sampleBoardIds.BackendDev,
                TenantId = TenantIds.AcmeCorp,
                AssignedUserId = UserIds.JohnDoe.ToString(),
                DueDate = DateTime.UtcNow.AddDays(-5),
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Text = "Completed JWT implementation with RS256 signing. Refresh token rotation implemented.",
                        CommentedByUserId = UserIds.JohnDoe,
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    },
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Text = "Great work! Can you add unit tests for the token validation?",
                        CommentedByUserId = UserIds.JaneSmith,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    }
                }
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Database Migration Strategy",
                Description = "Design and implement zero-downtime database migration approach using EF Core migrations.",
                Status = (int)TaskStatus.InProgress,
                BoardId = sampleBoardIds.BackendDev,
                TenantId = TenantIds.AcmeCorp,
                AssignedUserId = UserIds.JohnDoe.ToString(),
                DueDate = DateTime.UtcNow.AddDays(7),
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Text = "Started looking into online schema changes. Considering Postgres partitioning for large tables.",
                        CommentedByUserId = UserIds.JohnDoe,
                        CreatedAt = DateTime.UtcNow.AddHours(-6)
                    }
                }
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "API Rate Limiting",
                Description = "Implement rate limiting middleware with Redis backing. Support per-user and per-IP limits.",
                Status = (int)TaskStatus.Todo,
                BoardId = sampleBoardIds.BackendDev,
                TenantId = TenantIds.AcmeCorp,
                AssignedUserId = null,
                DueDate = DateTime.UtcNow.AddDays(14)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Implement Caching Layer",
                Description = "Add distributed caching with Redis. Implement cache-aside pattern for frequently accessed data.",
                Status = (int)TaskStatus.Todo,
                BoardId = sampleBoardIds.BackendDev,
                TenantId = TenantIds.AcmeCorp,
                AssignedUserId = UserIds.JaneSmith.ToString(),
                DueDate = DateTime.UtcNow.AddDays(10)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Setup OpenTelemetry",
                Description = "Integrate OpenTelemetry for distributed tracing. Configure Jaeger backend.",
                Status = (int)TaskStatus.Blocked,
                BoardId = sampleBoardIds.BackendDev,
                TenantId = TenantIds.AcmeCorp,
                AssignedUserId = UserIds.JohnDoe.ToString(),
                DueDate = DateTime.UtcNow.AddDays(21),
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Text = "Blocked: Waiting for infrastructure team to setup Jaeger in staging environment.",
                        CommentedByUserId = UserIds.JohnDoe,
                        CreatedAt = DateTime.UtcNow.AddHours(-12)
                    }
                }
            },

            // ========================================
            // Frontend Development Board Tasks
            // ========================================
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Setup React Router v6",
                Description = "Migrate from React Router v5 to v6. Update all route definitions and navigation logic.",
                Status = (int)TaskStatus.Done,
                BoardId = sampleBoardIds.FrontendDev,
                TenantId = TenantIds.AcmeCorp,
                AssignedUserId = UserIds.JaneSmith.ToString(),
                DueDate = DateTime.UtcNow.AddDays(-10)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Implement Dark Mode",
                Description = "Add dark mode theme support with system preference detection. Use Tailwind CSS dark mode utilities.",
                Status = (int)TaskStatus.InProgress,
                BoardId = sampleBoardIds.FrontendDev,
                TenantId = TenantIds.AcmeCorp,
                AssignedUserId = UserIds.JaneSmith.ToString(),
                DueDate = DateTime.UtcNow.AddDays(5),
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Text = "Dark mode UI implemented. Working on persisting user preference to localStorage.",
                        CommentedByUserId = UserIds.JaneSmith,
                        CreatedAt = DateTime.UtcNow.AddHours(-3)
                    }
                }
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Optimize Bundle Size",
                Description = "Reduce initial bundle size below 200KB. Implement code splitting and lazy loading for routes.",
                Status = (int)TaskStatus.Review,
                BoardId = sampleBoardIds.FrontendDev,
                TenantId = TenantIds.AcmeCorp,
                AssignedUserId = UserIds.JaneSmith.ToString(),
                DueDate = DateTime.UtcNow.AddDays(2),
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Text = "Bundle size reduced from 450KB to 180KB. Ready for review.",
                        CommentedByUserId = UserIds.JaneSmith,
                        CreatedAt = DateTime.UtcNow.AddHours(-1)
                    }
                }
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Add Loading Skeletons",
                Description = "Replace spinners with content-aware loading skeletons for better perceived performance.",
                Status = (int)TaskStatus.Todo,
                BoardId = sampleBoardIds.FrontendDev,
                TenantId = TenantIds.AcmeCorp,
                AssignedUserId = null,
                DueDate = DateTime.UtcNow.AddDays(8)
            },

            // ========================================
            // Tech Startup Sprint 1 Tasks
            // ========================================
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "User Registration Flow",
                Description = "Implement complete user registration with email verification and password strength validation.",
                Status = (int)TaskStatus.Done,
                BoardId = sampleBoardIds.Sprint1,
                TenantId = TenantIds.TechStartup,
                AssignedUserId = UserIds.Developer.ToString(),
                DueDate = DateTime.UtcNow.AddDays(-20)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Dashboard MVP",
                Description = "Create minimal dashboard with key metrics and activity feed.",
                Status = (int)TaskStatus.InProgress,
                BoardId = sampleBoardIds.Sprint1,
                TenantId = TenantIds.TechStartup,
                AssignedUserId = UserIds.Developer.ToString(),
                DueDate = DateTime.UtcNow.AddDays(3),
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Text = "Metrics API integrated. Working on responsive charts using Recharts.",
                        CommentedByUserId = UserIds.Developer,
                        CreatedAt = DateTime.UtcNow.AddHours(-8)
                    }
                }
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = "Payment Integration",
                Description = "Integrate Stripe for subscription payments. Implement webhook handling for payment events.",
                Status = (int)TaskStatus.Todo,
                BoardId = sampleBoardIds.Sprint1,
                TenantId = TenantIds.TechStartup,
                AssignedUserId = UserIds.Developer.ToString(),
                DueDate = DateTime.UtcNow.AddDays(15)
            }
        };

        await context.Tasks.AddRangeAsync(tasks);
        await context.SaveChangesAsync();

        var totalComments = tasks.Sum(t => t.Comments.Count);
        logger.LogInformation("✅ Seeded {TaskCount} tasks with {CommentCount} comments",
            tasks.Count, totalComments);
    }
}
