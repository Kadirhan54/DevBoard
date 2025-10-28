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
        // Constants for system configuration
        private static class SystemConfig
        {
            public static readonly Guid SystemTenantId = Guid.NewGuid();
            public const string SystemTenantName = "System Tenant";
            public const string SuperAdminEmail = "superadmin@devboard.com";
            public const string SuperAdminPassword = "SuperSecurePassword123!";
        }

        public static async Task SeedAsync(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("DbSeeder");

            await dbContext.Database.EnsureCreatedAsync();

            try
            {
                // Identity operations (UserManager/RoleManager) handle their own transactions
                await SeedRolesAsync(roleManager, logger);

                // Domain data uses explicit transaction
                using var transaction = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    var systemTenant = await EnsureSystemTenantAsync(dbContext, logger);
                    await SeedTenantsAsync(dbContext, logger);
                    await SeedProjectsAsync(dbContext, logger);

                    await transaction.CommitAsync();
                    logger.LogInformation("✅ Domain data seeding completed successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex, "❌ Domain data seeding failed, transaction rolled back");
                    throw;
                }

                // User seeding after domain data (requires committed tenant data)
                await SeedUsersAsync(dbContext, userManager, logger);
                await SeedSuperAdminAsync(userManager, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Critical seeding failure");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
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
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        logger.LogInformation("✅ Created role: {Role}", roleName);
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger.LogError("❌ Failed to create role {Role}: {Errors}", roleName, errors);
                        throw new InvalidOperationException($"Failed to create role {roleName}: {errors}");
                    }
                }
            }
        }

        private static async Task<Tenant> EnsureSystemTenantAsync(ApplicationDbContext dbContext, ILogger logger)
        {
            var systemTenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == SystemConfig.SystemTenantId);

            if (systemTenant == null)
            {
                systemTenant = new Tenant
                {
                    Id = SystemConfig.SystemTenantId,
                    Name = SystemConfig.SystemTenantName
                };
                await dbContext.Tenants.AddAsync(systemTenant);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("✅ Created System Tenant with ID: {TenantId}", systemTenant.Id);
            }
            else
            {
                logger.LogInformation("ℹ️ System Tenant already exists");
            }

            return systemTenant;
        }

        private static async Task SeedTenantsAsync(ApplicationDbContext dbContext, ILogger logger)
        {
            // Check for non-system tenants
            var existingTenantCount = await dbContext.Tenants
                .Where(t => t.Id != SystemConfig.SystemTenantId)
                .CountAsync();

            if (existingTenantCount > 0)
            {
                logger.LogInformation("ℹ️ Tenants already seeded, skipping");
                return;
            }

            var tenants = new List<Tenant>
            {
                new Tenant { Id = Guid.NewGuid(), Name = "Alpha Corp" },
                new Tenant { Id = Guid.NewGuid(), Name = "Beta Industries" }
            };

            await dbContext.Tenants.AddRangeAsync(tenants);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("✅ Seeded {Count} demo tenants", tenants.Count);
        }

        private static async Task SeedProjectsAsync(ApplicationDbContext dbContext, ILogger logger)
        {
            var existingProjectCount = await dbContext.Projects
                .IgnoreQueryFilters()
                .CountAsync();

            if (existingProjectCount > 0)
            {
                logger.LogInformation("ℹ️ Projects already seeded, skipping");
                return;
            }

            // Fetch non-system tenants for demo data
            var tenantsList = await dbContext.Tenants
                .Where(t => t.Id != SystemConfig.SystemTenantId)
                .AsNoTracking()
                .ToListAsync();

            if (tenantsList.Count == 0)
            {
                logger.LogWarning("⚠️ No non-system tenants available for project seeding");
                return;
            }

            var projects = GetMockProjects(tenantsList);
            await dbContext.Projects.AddRangeAsync(projects);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("✅ Seeded {Count} projects with boards and tasks", projects.Count);
        }

        private static async Task SeedUsersAsync(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            if (await userManager.Users.AnyAsync())
            {
                logger.LogInformation("ℹ️ Users already seeded, skipping");
                return;
            }

            // Get first non-system tenant for demo admin user
            var adminTenant = await dbContext.Tenants
                .Where(t => t.Id != SystemConfig.SystemTenantId)
                .FirstOrDefaultAsync();

            if (adminTenant == null)
            {
                logger.LogWarning("⚠️ No non-system tenant available for admin user creation");
                return;
            }

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
                logger.LogInformation("✅ Seeded demo admin user: {Email} for tenant: {Tenant}",
                    adminUser.Email, adminTenant.Name);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("❌ Failed to create admin user {Email}: {Errors}",
                    adminUser.Email, errors);
            }
        }

        private static async Task SeedSuperAdminAsync(
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            var existingSuperAdmin = await userManager.FindByEmailAsync(SystemConfig.SuperAdminEmail);
            if (existingSuperAdmin != null)
            {
                logger.LogInformation("ℹ️ SuperAdmin already exists");
                return;
            }

            var superAdmin = new ApplicationUser
            {
                UserName = SystemConfig.SuperAdminEmail,
                Email = SystemConfig.SuperAdminEmail,
                EmailConfirmed = true,
                TenantId = SystemConfig.SystemTenantId,
                EnableNotifications = true
            };

            var result = await userManager.CreateAsync(superAdmin, SystemConfig.SuperAdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
                logger.LogInformation("✅ Seeded SuperAdmin user: {Email}", SystemConfig.SuperAdminEmail);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("❌ Failed to create SuperAdmin {Email}: {Errors}",
                    SystemConfig.SuperAdminEmail, errors);
                throw new InvalidOperationException($"Failed to create SuperAdmin: {errors}");
            }
        }

        private static List<Project> GetMockProjects(List<Tenant> tenants)
        {
            var projects = new List<Project>();

            if (tenants.Count == 0)
                return projects;

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