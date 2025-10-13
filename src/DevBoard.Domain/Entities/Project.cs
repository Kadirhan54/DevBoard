using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Domain.Entities
{
    public class Project : EntityBase<Guid>
    {
        public string Name { get;  set; }
        public string Description { get;  set; }
        public Guid TenantId { get;  set; }


        // Navigation property
        public ICollection<Board> Boards { get; set; } = new List<Board>();

    }
}
