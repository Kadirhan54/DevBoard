using DevBoard.Domain.Entities;
using DevBoard.Domain.Enums;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            await dbContext.Database.EnsureCreatedAsync();

            // 1️⃣ Seed Tenants
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

            var tenantsList = await dbContext.Set<Tenant>().ToListAsync();

            // 2️⃣ Seed Projects/Boards/Tasks
            if (!await dbContext.Projects.IgnoreQueryFilters().AnyAsync())
            {
                var projects = GetMockProjects(tenantsList);
                await dbContext.Projects.AddRangeAsync(projects);
                await dbContext.SaveChangesAsync();
            }

            // 3️⃣ Seed Admin user
            if (!await userManager.Users.AnyAsync())
            {
                var adminTenant = tenantsList.First();
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@alphacorp.com",
                    Email = "admin@alphacorp.com",
                    TenantId = adminTenant.Id,
                    EnableNotifications = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);

                // Attach to tenant navigation
                adminTenant.Users.Add(adminUser);
                await dbContext.SaveChangesAsync();
            }
        }

        private static List<Project> GetMockProjects(List<Tenant> tenants)
        {
            var tenantA = tenants[0];
            var tenantB = tenants[1];

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
                            new TaskItem { Name = "Auth", Description = "JWT setup", Status = TaskItemStatus.InProgress, DueDate = DateTime.UtcNow.AddDays(7), TenantId = tenantA.Id }
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
                        Description = "Mobile features",
                        TenantId = tenantB.Id,
                        Tasks = new List<TaskItem>
                        {
                            new TaskItem { Name = "Push Notifications", Description = "Add notifications", Status = TaskItemStatus.Todo, DueDate = DateTime.UtcNow.AddDays(10), TenantId = tenantB.Id }
                        }
                    }
                }
            }
        };
        }
    }

}
