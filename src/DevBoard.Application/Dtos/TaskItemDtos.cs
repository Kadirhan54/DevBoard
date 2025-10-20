

using DevBoard.Domain.Entities;
using DevBoard.Domain.Enums;

namespace DevBoard.Application.Dtos
{
    public record CreateTaskItemDto(
        string Name, 
        string Description, 
        TaskItemStatus Status, 
        Guid BoardId, 
        DateTime? DueDate,  
        string? AssignedUserId
    );

    public record TaskItemResponseDto(
        Guid Id,
        string Name,
        string? Description,
        int Status,
        DateTime? DueDate,
        Guid BoardId,
        Guid TenantId
    );

}
