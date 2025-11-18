// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Core/Entities/TaskItem.cs
// ============================================================================
namespace DevBoard.Services.Tasks.Core.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid TenantId { get; set; }

    // ⚠️ NO navigation property to Board (different service!)
    public Guid BoardId { get; set; }

    public string? AssignedUserId { get; set; }
    public string? ImageUrl { get; set; }

    public List<Comment> Comments { get; set; } = new();
    public List<Attachment> Attachments { get; set; } = new();
}


// TODO : Seperate comment entity to another file
public class Comment
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public Guid CommentedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    //public List<Attachment> Attachments { get; set; } = new();
}