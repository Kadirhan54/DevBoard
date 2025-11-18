// ============================================================================
// FILE: Services/Projects/DevBoard.Services.Projects.Core/Entities/Project.cs
// ============================================================================
namespace DevBoard.Services.Projects.Core.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Board> Boards { get; set; } = new List<Board>();
}

public class Board
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ⚠️ NO navigation to Tasks (different service!)
}
