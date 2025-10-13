

using DevBoard.Domain.Entities;
using DevBoard.Domain.Enums;

namespace DevBoard.Application.Dtos
{
    public record CreateTaskItemDto(string Name, string Description, TaskItemStatus Status, Guid BoardId, DateTime? DueDate,  string? AssignedUserId);
    public class TaskItemResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int Status { get; set; }
        public DateTime? DueDate { get; set; }

        public Guid BoardId { get; set; }
        public SimpleBoardDto? Board { get; set; }
        public Guid? AssignedUserId { get; set; }
        public SimpleUserDto? AssignedUser { get; set; }
    }

    public class SimpleBoardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public class SimpleUserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
    }

}
