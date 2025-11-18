// ============================================================================
// FILE: Shared/DevBoard.Shared.Common/Seed/SeedIds.cs
// ============================================================================
namespace DevBoard.Shared.Common.Seed;

/// <summary>
/// Centralized seed data IDs to ensure consistency across all microservices.
/// These IDs are ONLY used for development/demo seeding, never in production.
/// </summary>
public static class SeedIds
{
    /// <summary>
    /// Tenant identifiers that must match across Identity, Project, and Task services
    /// </summary>
    public static class Tenants
    {
        public static readonly Guid System = new("00000000-0000-0000-0000-000000000001");
        public static readonly Guid AcmeCorp = new("11111111-1111-1111-1111-111111111111");
        public static readonly Guid TechStartup = new("22222222-2222-2222-2222-222222222222");
        public static readonly Guid Enterprise = new("33333333-3333-3333-3333-333333333333");
    }

    /// <summary>
    /// User identifiers from Identity Service
    /// </summary>
    public static class Users
    {
        // Acme Corp users
        public static readonly Guid AdminAcme = new("10000000-0000-0000-0000-000000000001");
        public static readonly Guid JohnDoe = new("10000000-0000-0000-0000-000000000002");
        public static readonly Guid JaneSmith = new("10000000-0000-0000-0000-000000000003");

        // Tech Startup users
        public static readonly Guid AdminTech = new("20000000-0000-0000-0000-000000000001");
        public static readonly Guid Developer = new("20000000-0000-0000-0000-000000000002");

        // Enterprise users
        public static readonly Guid AdminEnterprise = new("30000000-0000-0000-0000-000000000001");
    }

    /// <summary>
    /// Project identifiers from Project Service
    /// </summary>
    public static class Projects
    {
        // Acme Corp projects
        public static readonly Guid ECommerce = new("11100000-0000-0000-0000-000000000001");
        public static readonly Guid MobileApp = new("11100000-0000-0000-0000-000000000002");
        public static readonly Guid Analytics = new("11100000-0000-0000-0000-000000000003");

        // Tech Startup projects
        public static readonly Guid MVP = new("22200000-0000-0000-0000-000000000001");
        public static readonly Guid CustomerPortal = new("22200000-0000-0000-0000-000000000002");

        // Enterprise projects
        public static readonly Guid LegacyMigration = new("33300000-0000-0000-0000-000000000001");
    }

    /// <summary>
    /// Board identifiers from Project Service (child of Projects)
    /// </summary>
    public static class Boards
    {
        // E-Commerce Platform boards
        public static readonly Guid ECommerceBackend = new("11110000-0000-0000-0000-000000000001");
        public static readonly Guid ECommerceFrontend = new("11110000-0000-0000-0000-000000000002");
        public static readonly Guid ECommerceDevOps = new("11110000-0000-0000-0000-000000000003");

        // Mobile App boards
        public static readonly Guid MobileDesign = new("11120000-0000-0000-0000-000000000001");
        public static readonly Guid MobileIOS = new("11120000-0000-0000-0000-000000000002");
        public static readonly Guid MobileAndroid = new("11120000-0000-0000-0000-000000000003");

        // Analytics boards
        public static readonly Guid AnalyticsDataEng = new("11130000-0000-0000-0000-000000000001");
        public static readonly Guid AnalyticsDashboard = new("11130000-0000-0000-0000-000000000002");

        // MVP boards
        public static readonly Guid MVPSprint1 = new("22210000-0000-0000-0000-000000000001");
        public static readonly Guid MVPSprint2 = new("22210000-0000-0000-0000-000000000002");
        public static readonly Guid MVPLaunch = new("22210000-0000-0000-0000-000000000003");

        // Customer Portal boards
        public static readonly Guid PortalBackend = new("22220000-0000-0000-0000-000000000001");
        public static readonly Guid PortalFrontend = new("22220000-0000-0000-0000-000000000002");

        // Legacy Migration boards
        public static readonly Guid MigrationPhase1 = new("33310000-0000-0000-0000-000000000001");
        public static readonly Guid MigrationPhase2 = new("33310000-0000-0000-0000-000000000002");
        public static readonly Guid MigrationPhase3 = new("33310000-0000-0000-0000-000000000003");
        public static readonly Guid MigrationPhase4 = new("33310000-0000-0000-0000-000000000004");
    }

    /// <summary>
    /// Helper to check if running in seed mode (development/demo)
    /// </summary>
    public static bool IsSeedMode(string environment)
    {
        return environment.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
               environment.Equals("Demo", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Seed user credentials for testing
    /// </summary>
    public static class Credentials
    {
        public const string SuperAdminEmail = "superadmin@devboard.com";
        public const string SuperAdminPassword = "SuperAdmin123!";

        public const string AdminPassword = "Admin123!";
        public const string UserPassword = "User123!";

        public static readonly Dictionary<string, string> TestUsers = new()
        {
            { "admin@acme.com", AdminPassword },
            { "john.doe@acme.com", UserPassword },
            { "jane.smith@acme.com", UserPassword },
            { "admin@techstartup.com", AdminPassword },
            { "developer@techstartup.com", UserPassword },
            { "admin@enterprise.com", AdminPassword }
        };
    }
}
