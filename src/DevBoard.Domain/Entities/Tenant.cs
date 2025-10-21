using DevBoard.Domain.Common;
using DevBoard.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Domain.Entities
{
    public class Tenant : EntityBase<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public string? Domain; // optional (for custom subdomains)

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();

    }

}
