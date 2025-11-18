// ============================================================================
// FILE: Shared/DevBoard.Shared.Contracts/Tasks/AttachmentContracts.cs
// ============================================================================

namespace DevBoard.Shared.Contracts.Tasks;

public record AttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSize,
    string? Description,
    Guid? TaskItemId,
    Guid? CommentId,
    Guid UploadedByUserId,
    DateTime UploadedAt,
    string DownloadUrl,
    string? ThumbnailUrl,
    bool IsImage
);

public record UploadAttachmentResponse(
    Guid AttachmentId,
    string FileName,
    long FileSize,
    string DownloadUrl
);