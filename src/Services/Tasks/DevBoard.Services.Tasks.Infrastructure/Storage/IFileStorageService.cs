// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Infrastructure/Storage/IFileStorageService.cs
// ============================================================================

using Microsoft.AspNetCore.Http;

namespace DevBoard.Services.Tasks.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(
        IFormFile file,
        Guid tenantId,
        Guid resourceId,
        string resourceType,
        CancellationToken cancellationToken = default);

    Task<string?> UploadThumbnailAsync(
        IFormFile file,
        Guid tenantId,
        Guid resourceId,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    Task<string> GetPresignedDownloadUrlAsync(
        string storagePath,
        TimeSpan expiry);
}