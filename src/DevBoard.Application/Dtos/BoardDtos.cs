using DevBoard.Domain.Entities;

namespace DevBoard.Application.Dtos
{
    public record SimpleBoardDto(
        Guid Id, 
        string Name, 
        string Description,
        Guid TenantId
    );

    public record CreateBoardDto(
        string Name, 
        string Description, 
        Guid ProjectId, 
        ICollection<TaskItem>? TaskItems
    );

    public record BoardWithTasksDto(
        Guid Id,
        string Name,
        string Description,
        Guid TenantId,
        IEnumerable<TaskItemResponseDto> Tasks
    );
    public record DeleteBoardDto(Guid Id);

}
