using DevBoard.Domain.Common;
using DevBoard.Domain.Enums;
using DevBoard.Domain.Identity;

namespace DevBoard.Domain.Entities
{
    public class TaskItem : EntityBase<Guid>, ITenantEntity, ICreatedByEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TaskItemStatus Status { get; set; }
        public DateTime? DueDate { get; set; }

        public Guid TenantId { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }


        public Guid BoardId { get; set; }
        public Board Board { get; set; } = null!;


        // 🔹 Adjusted for IdentityUser default
        public string? AssignedUserId { get; set; }
        public ApplicationUser? AssignedUser { get; set; }

    }
}
