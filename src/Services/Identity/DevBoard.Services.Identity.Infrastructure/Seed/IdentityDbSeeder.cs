// ============================================================================
// FILE: Services/Identity/DevBoard.Services.Identity.Infrastructure/Seed/IdentityDbSeeder.cs
// ============================================================================
using DevBoard.Services.Identity.Core.Entities;
using DevBoard.Services.Identity.Infrastructure.Data;
using DevBoard.Shared.Common.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Services.Identity.Infrastructure.Seed;

public static class IdentityDbSeeder
{
    // System-level constants
    private static class SystemConfig
    {
        public static readonly Guid SystemTenantId = new Guid("00000000-0000-0000-0000-000000000001");
        public const string SystemTenantName = "System Tenant";
        public const string SuperAdminEmail = "superadmin@devboard.com";
        public const string SuperAdminPassword = "SuperAdmin123!";
    }

    public static async Task SeedAsync(
        IdentityDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("IdentityDbSeeder");

        try
        {
            // Ensure database is created and migrated
            await context.Database.MigrateAsync();

            // Seed in order: Roles → Tenants → Users
            await SeedRolesAsync(roleManager, logger);
            await SeedTenantsAsync(context, logger);
            await SeedUsersAsync(context, userManager, logger);

            logger.LogInformation("✅ Identity database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding Identity database");
            throw;
        }
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        string[] roles = new[]
        {
            "SuperAdmin",  // Platform-wide access
            "Admin",       // Tenant admin
            "Manager",     // Team/project manager
            "Member",      // Standard user
            "Viewer",      // Read-only
            "Guest"        // Limited temporary access
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
                }
            }
        }
    }

    private static async Task SeedTenantsAsync(
        IdentityDbContext context,
        ILogger logger)
    {
        // Check if tenants already exist
        if (await context.Tenants.AnyAsync())
        {
            logger.LogInformation("ℹ️  Tenants already exist, skipping seed");
            return;
        }

        var tenants = new List<Tenant>
        {
            // System tenant (for super admin)
            new Tenant
            {
                Id = SystemConfig.SystemTenantId,
                Name = SystemConfig.SystemTenantName,
                Domain = null
            },
            
            // Demo tenants
            new Tenant
            {
                Id = SeedIds.Tenants.AcmeCorp,
                Name = "Acme Corporation",
                Domain = "acme"
            },
            new Tenant
            {
                Id = SeedIds.Tenants.TechStartup,
                Name = "Tech Startup Inc",
                Domain = "techstartup"
            },
            new Tenant
            {
                Id = SeedIds.Tenants.Enterprise,
                Name = "Enterprise Solutions Ltd",
                Domain = "enterprise"
            }
        };

        await context.Tenants.AddRangeAsync(tenants);
        await context.SaveChangesAsync();

        logger.LogInformation("✅ Seeded {Count} tenants", tenants.Count);
    }

    private static async Task SeedUsersAsync(
        IdentityDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        // Check if users already exist
        if (await userManager.Users.AnyAsync())
        {
            logger.LogInformation("ℹ️  Users already exist, skipping seed");
            return;
        }

        // Get tenants for user assignment
        var systemTenant = await context.Tenants
            .FirstAsync(t => t.Id == SystemConfig.SystemTenantId);

        var demoTenants = await context.Tenants
            .Where(t => t.Id != SystemConfig.SystemTenantId)
            .ToListAsync();

        var usersToCreate = new List<(ApplicationUser User, string Password, string Role)>();

        // 1. Super Admin (System Tenant)
        usersToCreate.Add((
            new ApplicationUser
            {
                UserName = SystemConfig.SuperAdminEmail,
                Email = SystemConfig.SuperAdminEmail,
                EmailConfirmed = true,
                TenantId = systemTenant.Id,
                EnableNotifications = true
            },
            SystemConfig.SuperAdminPassword,
            "SuperAdmin"
        ));

        // 2. Demo users for each tenant
        if (demoTenants.Count > 0)
        {
            // Acme Corporation users
            var acmeTenant = demoTenants[0];
            usersToCreate.AddRange(new[]
            {
                (
                    new ApplicationUser
                    {
                        Id = SeedIds.Users.AdminAcme.ToString(),
                        UserName = "admin@acme.com",
                        Email = "admin@acme.com",
                        EmailConfirmed = true,
                        TenantId = acmeTenant.Id,
                        EnableNotifications = true
                    },
                    "Admin123!",
                    "Admin"
                ),
                (
                    new ApplicationUser
                    {
                        Id = SeedIds.Users.JohnDoe.ToString(),
                        UserName = "john.doe@acme.com",
                        Email = "john.doe@acme.com",
                        EmailConfirmed = true,
                        TenantId = acmeTenant.Id,
                        EnableNotifications = true
                    },
                    "User123!",
                    "Member"
                ),
                (
                    new ApplicationUser
                    {
                        Id = SeedIds.Users.JaneSmith.ToString(),
                        UserName = "jane.smith@acme.com",
                        Email = "jane.smith@acme.com",
                        EmailConfirmed = true,
                        TenantId = acmeTenant.Id,
                        EnableNotifications = false
                    },
                    "User123!",
                    "Member"
                )
            });

            // Tech Startup users
            if (demoTenants.Count > 1)
            {
                var techTenant = demoTenants[1];
                usersToCreate.AddRange(new[]
                {
                    (
                        new ApplicationUser
                        {
                            Id = SeedIds.Users.AdminTech.ToString(),
                            UserName = "admin@techstartup.com",
                            Email = "admin@techstartup.com",
                            EmailConfirmed = true,
                            TenantId = techTenant.Id,
                            EnableNotifications = true
                        },
                        "Admin123!",
                        "Admin"
                    ),
                    (
                        new ApplicationUser
                        {
                            Id = SeedIds.Users.Developer.ToString(),
                            UserName = "developer@techstartup.com",
                            Email = "developer@techstartup.com",
                            EmailConfirmed = true,
                            TenantId = techTenant.Id,
                            EnableNotifications = true
                        },
                        "User123!",
                        "Member"
                    )
                });
            }
            
            // Enterprise Startup users
            if (demoTenants.Count > 2)
            {
                var enterpriseTenant = demoTenants[2];
                usersToCreate.AddRange(new[]
                {
                    (
                        new ApplicationUser
                        {
                            Id = SeedIds.Users.AdminEnterprise.ToString(),
                            UserName = "admin@enterprise.com",
                            Email = "admin@enterprise.com",
                            EmailConfirmed = true,
                            TenantId = enterpriseTenant.Id,
                            EnableNotifications = true
                        },
                        "Admin123!",
                        "Admin"
                    )
                });
            }
        }

        // Create all users
        foreach (var (user, password, role) in usersToCreate)
        {
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                logger.LogInformation("✅ Created user: {Email} with role: {Role}", 
                    user.Email, role);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("❌ Failed to create user {Email}: {Errors}", 
                    user.Email, errors);
            }
        }

        logger.LogInformation("✅ Seeded {Count} users", usersToCreate.Count);
    }
}