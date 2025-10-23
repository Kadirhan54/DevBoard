using DevBoard.Domain.Entities;
using DevBoard.Domain.Enums;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("DbSeeder");

            await dbContext.Database.EnsureCreatedAsync();

            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Seed Roles (Identity first)
                string[] roles = new[]
                {
                    Roles.SuperAdmin,
                    Roles.Admin,
                    Roles.Manager,
                    Roles.Member,
                    Roles.Viewer,
                    Roles.Guest
                };

                foreach (var roleName in roles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        logger.LogInformation("✅ Created role: {Role}", roleName);
                    }
                }

                // 2️⃣ Ensure System Tenant always exists
                var systemTenant = await dbContext.Tenants
                    .FirstOrDefaultAsync(t => t.Name == "System Tenant");

                if (systemTenant == null)
                {
                    systemTenant = new Tenant
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Name = "System Tenant"
                    };
                    await dbContext.Tenants.AddAsync(systemTenant);
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("✅ Created System Tenant");
                }

                // 2️⃣ Seed Tenants
                if (!await dbContext.Set<Tenant>().AnyAsync())
                {
                    var tenants = new List<Tenant>
                    {
                        new Tenant { Id = Guid.NewGuid(), Name = "Alpha Corp" },
                        new Tenant { Id = Guid.NewGuid(), Name = "Beta Industries" }
                    };

                    await dbContext.Tenants.AddRangeAsync(tenants);
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("✅ Seeded tenants");
                }

                var tenantsList = await dbContext.Tenants.AsNoTracking().ToListAsync();

                // 3️⃣ Seed Projects, Boards, Tasks
                if (!await dbContext.Projects.IgnoreQueryFilters().AnyAsync())
                {
                    // TODO : tenantList only returns SuperAdmin tenant ???
                    var projects = GetMockProjects(tenantsList);
                    await dbContext.Projects.AddRangeAsync(projects);
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("✅ Seeded projects and boards");
                }

                // 4️⃣ Seed Admin Users
                if (!await userManager.Users.AnyAsync())
                {
                    var adminTenant = tenantsList.First();
                    var adminUser = new ApplicationUser
                    {
                        UserName = "admin@alphacorp.com",
                        Email = "admin@alphacorp.com",
                        TenantId = adminTenant.Id,
                        EmailConfirmed = true,
                        EnableNotifications = true
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                        logger.LogInformation("✅ Seeded admin user: {Email}", adminUser.Email);
                    }
                    else
                    {
                        logger.LogError("❌ Failed to seed admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // 5️⃣ Global SuperAdmin
                var superAdminEmail = "superadmin@devboard.com";
                if (await userManager.FindByEmailAsync(superAdminEmail) is null)
                {
                    var superAdmin = new ApplicationUser
                    {
                        UserName = superAdminEmail,
                        Email = superAdminEmail,
                        EmailConfirmed = true,
                        TenantId = systemTenant.Id, // ✅ valid tenant
                        EnableNotifications = true
                    };

                    var result = await userManager.CreateAsync(superAdmin, "SuperSecurePassword123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
                        logger.LogInformation("✅ Seeded SuperAdmin user: {Email}", superAdminEmail);
                    }
                }

                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "❌ Seeding failed");
                throw;
            }
        }

        private static List<Project> GetMockProjects(List<Tenant> tenants)
        {
            var projects = new List<Project>();

            if (tenants.Count == 0)
                return projects;

            // Always seed at least one project for the first tenant
            var tenantA = tenants[0];
            projects.Add(new Project
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
                                Name = "Auth",
                                Description = "JWT setup",
                                Status = TaskItemStatus.InProgress,
                                DueDate = DateTime.UtcNow.AddDays(7),
                                TenantId = tenantA.Id
                            }
                        }
                    }
                }
            });

            // If there’s a second tenant, seed another example
            if (tenants.Count > 1)
            {
                var tenantB = tenants[1];
                projects.Add(new Project
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
                                new TaskItem
                                {
                                    Name = "Push Notifications",
                                    Description = "Add notifications",
                                    Status = TaskItemStatus.Todo,
                                    DueDate = DateTime.UtcNow.AddDays(10),
                                    TenantId = tenantB.Id
                                }
                            }
                        }
                    }
                });
            }

            return projects;
        }

    }
}
