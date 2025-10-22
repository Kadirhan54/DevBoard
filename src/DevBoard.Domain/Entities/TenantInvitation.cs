using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Domain.Entities
{
    using System;

    namespace DevBoard.Domain.Entities
    {
        public class TenantInvitation : EntityBase<Guid>
        {

            public string InvitedEmail { get; set; } = default!;
            public string InviteToken { get; set; } = default!;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
            public bool IsUsed { get; set; } = false;


            // Navigation
            public Tenant Tenant { get; set; } = default!;
            public Guid TenantId { get; set; }

        }
    }

}
