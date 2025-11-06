// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Core/Entities/Attachment.cs
// ============================================================================

namespace DevBoard.Services.Tasks.Core.Entities;

public class Attachment
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty; // Path in MinIO
    public string? Description { get; set; }
    public Guid TenantId { get; set; }

    
    // Relationships
    public Guid? TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    
    public Guid? CommentId { get; set; }
    public Comment? Comment { get; set; }

    
    // Metadata
    public Guid UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    
    // Optional: Thumbnail for images
    public string? ThumbnailPath { get; set; }
    public bool IsImage { get; set; }
}