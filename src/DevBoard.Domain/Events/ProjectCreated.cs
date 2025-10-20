using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Domain.Events
{
    public interface ProjectCreated
    {
        Guid ProjectId { get; }
        string Name { get; }
        DateTime CreatedAt { get; }
    }
}
