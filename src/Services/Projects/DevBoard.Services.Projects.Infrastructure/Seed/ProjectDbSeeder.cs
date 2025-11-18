// ============================================================================
// FILE: Services/Projects/DevBoard.Services.Projects.Infrastructure/Seed/ProjectDbSeeder.cs
// ============================================================================
using DevBoard.Services.Projects.Core.Entities;
using DevBoard.Services.Projects.Infrastructure.Data;
using DevBoard.Shared.Common.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Services.Projects.Infrastructure.Seed;

public static class ProjectDbSeeder
{
    public static async Task SeedAsync(
        ProjectDbContext context,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("ProjectDbSeeder");

        try
        {
            await context.Database.MigrateAsync();

            // Seed projects with boards
            await SeedProjectsWithBoardsAsync(context, logger);

            logger.LogInformation("✅ Project database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding Project database");
            throw;
        }
    }

    private static async Task SeedProjectsWithBoardsAsync(
        ProjectDbContext context,
        ILogger logger)
    {
        // Check if projects already exist
        if (await context.Projects.AnyAsync())
        {
            logger.LogInformation("ℹ️  Projects already exist, skipping seed");
            return;
        }

        var projects = new List<Project>
        {
            // ========================================
            // Acme Corporation Projects
            // ========================================
            new Project
            {
                Id = SeedIds.Projects.ECommerce,
                Name = "E-Commerce Platform",
                Description = "Next-generation e-commerce solution with AI-powered recommendations",
                TenantId = SeedIds.Tenants.AcmeCorp,
                CreatedAt = DateTime.UtcNow.AddDays(-90),
                Boards = new List<Board>
                {
                    new Board
                    {
                        Id = SeedIds.Boards.ECommerceBackend,
                        Name = "Backend Development",
                        Description = "API development and microservices",
                        TenantId = SeedIds.Tenants.AcmeCorp,
                        CreatedAt = DateTime.UtcNow.AddDays(-90)
                    }
                }
            },

            // ========================================
            // Tech Startup Projects
            // ========================================
            new Project
            {
                Id = SeedIds.Projects.MVP,
                Name = "MVP Development",
                Description = "Minimum viable product for Series A funding",
                TenantId = SeedIds.Tenants.TechStartup,
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                Boards = new List<Board>
                {
                    new Board
                    {
                        Id = SeedIds.Boards.MVPSprint1,
                        Name = "Sprint 1",
                        Description = "Core features implementation",
                        TenantId = SeedIds.Tenants.TechStartup,
                        CreatedAt = DateTime.UtcNow.AddDays(-45)
                    }
                }
            },

            // ========================================
            // Enterprise Solutions Projects
            // ========================================
            new Project
            {
                Id = SeedIds.Projects.LegacyMigration,
                Name = "Legacy System Migration",
                Description = "Migration from monolith to microservices",
                TenantId = SeedIds.Tenants.Enterprise,
                CreatedAt = DateTime.UtcNow.AddDays(-120),
                Boards = new List<Board>
                {
                    new Board
                    {
                        Id = SeedIds.Boards.MigrationPhase1,
                        Name = "Phase 1 - Assessment",
                        Description = "Current system analysis",
                        TenantId = SeedIds.Tenants.Enterprise,
                        CreatedAt = DateTime.UtcNow.AddDays(-120)
                    }
                }
            }
        };

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        var totalBoards = projects.Sum(p => p.Boards.Count);
        logger.LogInformation("✅ Seeded {ProjectCount} projects with {BoardCount} boards",
            projects.Count, totalBoards);
    }
}