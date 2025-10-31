// ============================================================================
// FILE: Shared/DevBoard.Shared.Contracts/Projects/ProjectContracts.cs
// ============================================================================
namespace DevBoard.Shared.Contracts.Projects;

public record CreateProjectRequest(
    string Name,
    string Description
);

public record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    Guid TenantId
);

public record CreateBoardRequest(
    string Name,
    string Description,
    Guid ProjectId
);

public record BoardDto(
    Guid Id,
    string Name,
    string Description,
    Guid TenantId,
    Guid ProjectId
);