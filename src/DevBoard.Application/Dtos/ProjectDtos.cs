using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Application.Dtos
{
    public record ProjectDto(
        Guid Id,
        string Name,
        string Description,
        string TenantId
    );

    public record ProjectWithBoardsDto(
        Guid Id,
        string Name,
        string Description,
        string TenantId,
        IEnumerable<BoardWithTasksDto> Boards
    );

    public record CreateProjectDto(
        string Name, 
        string Description, 
        string TenantId
    );
}
