using DevBoard.Domain.Common;
using DevBoard.Domain.Enums;
using DevBoard.Domain.Identity;
using System.Xml.Linq;

namespace DevBoard.Domain.Entities
{
    public class TaskItem : EntityBase<Guid>, ITenantEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TaskItemStatus Status { get; set; }
        public DateTime? DueDate { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }

        public Guid BoardId { get; set; }
        public Board Board { get; set; } = null!;

        //  Assigned user (optional)
        public string? AssignedUserId { get; set; }
        public ApplicationUser? AssignedUser { get; set; }

        //  Image support for future Azure integration (Nullable for now)
        public string? ImageUrl { get; set; }

        //  Comments linked to this task
        public List<Comment> Comments { get; set; } = new();
    }
}
