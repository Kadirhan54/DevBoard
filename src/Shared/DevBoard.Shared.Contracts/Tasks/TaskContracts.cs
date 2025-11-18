// ============================================================================
// FILE: Shared/DevBoard.Shared.Contracts/Tasks/TaskContracts.cs
// ============================================================================
namespace DevBoard.Shared.Contracts.Tasks;

public record CreateTaskRequest(
    string Name,
    string Description,
    int Status,
    Guid BoardId,
    DateTime? DueDate,
    string? AssignedUserId
);

public record TaskDto(
    Guid Id,
    string Name,
    string Description,
    int Status,
    DateTime? DueDate,
    Guid BoardId,
    Guid TenantId,
    string? AssignedUserId
);

// TODO : move comment dtos to responsive contract
public record CreateCommentRequest(string Text);

public record CommentDto(
    Guid Id,
    string Text,
    Guid TaskItemId,
    Guid CommentedByUserId,
    DateTime CreatedAt
);
