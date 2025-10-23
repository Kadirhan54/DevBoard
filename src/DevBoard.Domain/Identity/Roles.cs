using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Domain.Identity
{
    public static class Roles
    {
        // Core Roles
        public const string Admin = "Admin";          // Full tenant control
        public const string Member = "Member";        // Standard user access

        // Optional Intermediate Roles
        public const string Manager = "Manager";      // Manage specific modules or teams
        public const string Viewer = "Viewer";        // Read-only access
        public const string Guest = "Guest";          // Temporary or limited access (e.g., external invite)

        // System-Level Roles (for platform-wide operations)
        public const string SuperAdmin = "SuperAdmin"; // Full platform access across tenants
    }
}
