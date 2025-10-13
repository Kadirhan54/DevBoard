using DevBoard.Domain.Entities;

namespace DevBoard.Application.Dtos
{
    public record BoardDto(Guid Id, string Name, string Description, Guid ProjectId, ICollection<TaskItem>? TaskItems);
    public record CreateBoardDto(string Name, string Description, Guid ProjectId, ICollection<TaskItem>? TaskItems);
    public record DeleteBoardDto(Guid Id);

}
