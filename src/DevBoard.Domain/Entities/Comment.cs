using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Domain.Entities
{
    public class Comment : EntityBase<Guid>
    {
        public string Text { get; set; }

        public Guid TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = null!;
    }
}
