using DevBoard.Domain.Common;
using DevBoard.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DevBoard.Domain.Identity
{
    public  class ApplicationUser : IdentityUser , ITenantEntity
    {
        public bool EnableNotifications { get; set; } = true;

        public Guid TenantId { get; set; }
        // optional navigation to the tenant

        public Tenant Tenant { get; set; }

        // Navigation: tasks assigned to this user
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    }
}
