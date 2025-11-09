// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Infrastructure/Storage/MinIOFileStorageService.cs
// ============================================================================

using DevBoard.Services.Tasks.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DevBoard.Services.Tasks.Infrastructure.Storage;

public class MinIOFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinIOSettings _settings;
    private readonly ILogger<AttachmentService> _logger;

    public MinIOFileStorageService(
        IMinioClient minioClient,
        MinIOSettings settings,
        ILogger<AttachmentService> logger)
    {
        _minioClient = minioClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(
        IFormFile file,
        Guid tenantId,
        Guid resourceId,
        string resourceType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure bucket exists
            await EnsureBucketExistsAsync(cancellationToken);

            // TODO : change Guid.NewGuid to actual attachment ID.
            // Generate storage path: {tenantId}/{resourceType}/{resourceId}/{fileName}
            var sanitizedFileName = SanitizeFileName(file.FileName);
            var storagePath = $"{tenantId}/{resourceType}/{resourceId}/{Guid.NewGuid()}_{sanitizedFileName}";

            // Upload to MinIO
            using var stream = file.OpenReadStream();

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(storagePath)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType);

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            _logger.LogInformation("Uploaded file {FileName} to {StoragePath}", file.FileName, storagePath);

            return storagePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
            throw;
        }
    }

    public async Task<string?> UploadThumbnailAsync(
        IFormFile file,
        Guid tenantId,
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsImageFile(file.FileName))
                return null;

            using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream(), cancellationToken);

            // Resize to thumbnail (200x200 max, maintain aspect ratio)
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(200, 200),
                Mode = ResizeMode.Max
            }));

            using var thumbnailStream = new MemoryStream();
            await image.SaveAsJpegAsync(thumbnailStream, cancellationToken);
            thumbnailStream.Position = 0;

            var thumbnailPath = $"{tenantId}/thumbnails/{resourceId}/{Guid.NewGuid()}_thumb.jpg";

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(thumbnailPath)
                .WithStreamData(thumbnailStream)
                .WithObjectSize(thumbnailStream.Length)
                .WithContentType("image/jpeg");

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            return thumbnailPath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate thumbnail for {FileName}", file.FileName);
            return null; // Non-critical, continue without thumbnail
        }
    }

    public async Task<Stream> DownloadFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryStream = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(storagePath)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                });

            await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from {StoragePath}", storagePath);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(storagePath);

            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);

            _logger.LogInformation("Deleted file from {StoragePath}", storagePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from {StoragePath}", storagePath);
            return false;
        }
    }

    public async Task<string> GetPresignedDownloadUrlAsync(
        string storagePath,
        TimeSpan expiry)
    {
        try
        {
            var presignedArgs = new PresignedGetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(storagePath)
                .WithExpiry((int)expiry.TotalSeconds);

            var url = await _minioClient.PresignedGetObjectAsync(presignedArgs);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for {StoragePath}", storagePath);
            throw;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(_settings.BucketName);

        bool exists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

        if (!exists)
        {
            var makeBucketArgs = new MakeBucketArgs()
                .WithBucket(_settings.BucketName);

            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);

            _logger.LogInformation("Created bucket {BucketName}", _settings.BucketName);
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars));
        return sanitized;
    }

    private static bool IsImageFile(string fileName)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return imageExtensions.Contains(extension);
    }
}