using DevBoard.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DevBoard.Domain.Identity
{
    public  class ApplicationUser : IdentityUser
    {
        // Additional properties can be added here
        public bool EnableNotifications { get; set; } = true;

        // Navigation: tasks assigned to this user
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    }
}
