using DevBoard.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Domain.Entities
{
    public class Board : EntityBase<Guid>, ITenantEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Guid TenantId { get; set; }  // redundant but required for filtering
        public Tenant Tenant { get; set; }


        // Foreign key to Project
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;


        // Navigation property
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
