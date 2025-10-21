using DevBoard.Domain.Entities;
using DevBoard.Domain.Enums;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext)
        {
            await dbContext.Database.EnsureCreatedAsync();

            // ✅ 1. Seed Tenants first
            if (!await dbContext.Set<Tenant>().AnyAsync())
            {
                var tenants = new List<Tenant>
                {
                    new Tenant { Id = Guid.NewGuid(), Name = "Alpha Corp" },
                    new Tenant { Id = Guid.NewGuid(), Name = "Beta Industries" }
                };

                await dbContext.AddRangeAsync(tenants);
                await dbContext.SaveChangesAsync();
            }

            var existingTenants = await dbContext.Set<Tenant>().ToListAsync();

            // ✅ 2. Seed Projects with existing tenant IDs
            if (!await dbContext.Projects.IgnoreQueryFilters().AnyAsync())
            {
                var projects = GetMockProjects(existingTenants);
                await dbContext.Projects.AddRangeAsync(projects);
                await dbContext.SaveChangesAsync();
            }
        }

        private static List<Project> GetMockProjects(List<Tenant> tenants)
        {
            var tenantA = tenants.First();
            var tenantB = tenants.Last();

            return new List<Project>
            {
                new Project
                {
                    Name = "DevBoard Platform",
                    Description = "Internal development management platform.",
                    TenantId = tenantA.Id,
                    Boards = new List<Board>
                    {
                        new Board
                        {
                            Name = "Development Board",
                            Description = "Tracks backend and API development tasks.",
                            TenantId = tenantA.Id,
                            Tasks = new List<TaskItem>
                            {
                                new TaskItem
                                {
                                    Name = "Implement Authentication",
                                    Description = "Add JWT authentication with Identity.",
                                    Status = TaskItemStatus.InProgress,
                                    DueDate = DateTime.UtcNow.AddDays(7),
                                    TenantId = tenantA.Id
                                },
                                new TaskItem
                                {
                                    Name = "Refactor Database Layer",
                                    Description = "Improve EF Core context and relationships.",
                                    Status = TaskItemStatus.Todo,
                                    DueDate = DateTime.UtcNow.AddDays(10),
                                    TenantId = tenantA.Id
                                }
                            }
                        }
                    }
                },
                new Project
                {
                    Name = "Mobile App",
                    Description = "Mobile version of the DevBoard app.",
                    TenantId = tenantB.Id,
                    Boards = new List<Board>
                    {
                        new Board
                        {
                            Name = "Feature Board",
                            Description = "Tracks new mobile app feature tasks.",
                            TenantId = tenantB.Id,
                            Tasks = new List<TaskItem>
                            {
                                new TaskItem
                                {
                                    Name = "Implement Notifications",
                                    Description = "Add push notifications for task updates.",
                                    Status = TaskItemStatus.Todo,
                                    DueDate = DateTime.UtcNow.AddDays(14),
                                    TenantId = tenantB.Id
                                },
                                new TaskItem
                                {
                                    Name = "Integrate API",
                                    Description = "Connect the app to the backend API.",
                                    Status = TaskItemStatus.InProgress,
                                    DueDate = DateTime.UtcNow.AddDays(8),
                                    TenantId = tenantB.Id
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
