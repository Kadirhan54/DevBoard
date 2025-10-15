using DevBoard.Domain.Entities;

namespace DevBoard.Application.Dtos
{
    public record SimpleBoardDto(Guid Id, string Name, string Description);
    public record CreateBoardDto(string Name, string Description, Guid ProjectId, ICollection<TaskItem>? TaskItems);
    public record DeleteBoardDto(Guid Id);


    public record BoardWithTasksDto(
        Guid Id,
        string Name,
        string Description,
        IEnumerable<TaskItemResponseDto> Tasks
    );

    public record PaginatedTasksDto(
        int Page,
        int PageSize,
        int TotalCount,
        IEnumerable<TaskItemResponseDto> Items
    );

}
