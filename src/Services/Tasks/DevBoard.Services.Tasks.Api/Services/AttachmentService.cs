// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Api/Services/AttachmentService.cs
// ============================================================================

using DevBoard.Services.Tasks.Core.Entities;
using DevBoard.Services.Tasks.Infrastructure.Data;
using DevBoard.Services.Tasks.Infrastructure.Storage;
using DevBoard.Shared.Common;
using DevBoard.Shared.Common.Storage;
using DevBoard.Shared.Contracts.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Services.Tasks.Api.Services;

public class AttachmentService
{
    private readonly TaskDbContext _context;
    private readonly IFileStorageService _storageService;
    private readonly ITenantProvider _tenantProvider;
    private readonly FileUploadSettings _uploadSettings;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(
        TaskDbContext context,
        IFileStorageService storageService,
        ITenantProvider tenantProvider,
        FileUploadSettings uploadSettings,
        ILogger<AttachmentService> logger)
    {
        _context = context;
        _storageService = storageService;
        _tenantProvider = tenantProvider;
        _uploadSettings = uploadSettings;
        _logger = logger;
    }

    public async Task<Result<UploadAttachmentResponse>> UploadTaskAttachmentAsync(
        Guid taskId,
        IFormFile file,
        string? description,
        Guid userId)
    {
        try
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Validate file
            var (isValid, error) = FileValidator.ValidateFile(file, _uploadSettings);
            if (!isValid)
                return Result<UploadAttachmentResponse>.Failure(error!);

            // Check task exists
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return Result<UploadAttachmentResponse>.Failure("Task not found");

            // Check attachment limit
            var attachmentCount = await _context.Set<Attachment>()
                .CountAsync(a => a.TaskItemId == taskId);

            if (attachmentCount >= _uploadSettings.MaxAttachmentsPerTask)
                return Result<UploadAttachmentResponse>.Failure(
                    $"Maximum {_uploadSettings.MaxAttachmentsPerTask} attachments per task");

            // Upload file to MinIO
            var storagePath = await _storageService.UploadFileAsync(
                file, tenantId, taskId, "tasks");

            // Generate thumbnail for images
            string? thumbnailPath = null;
            if (FileValidator.IsImageFile(file.FileName))
            {
                thumbnailPath = await _storageService.UploadThumbnailAsync(
                    file, tenantId, taskId);
            }

            // Save attachment record
            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                StoragePath = storagePath,
                ThumbnailPath = thumbnailPath,
                Description = description,
                TaskItemId = taskId,
                TenantId = tenantId,
                UploadedByUserId = userId,
                IsImage = FileValidator.IsImageFile(file.FileName)
            };

            _context.Set<Attachment>().Add(attachment);
            await _context.SaveChangesAsync();

            // Generate download URL (valid for 7 days)
            var downloadUrl = await _storageService.GetPresignedDownloadUrlAsync(
                storagePath, TimeSpan.FromDays(7));

            _logger.LogInformation(
                "Attachment {AttachmentId} uploaded for task {TaskId}",
                attachment.Id, taskId);

            return Result<UploadAttachmentResponse>.Success(new UploadAttachmentResponse(
                attachment.Id,
                attachment.FileName,
                attachment.FileSize,
                downloadUrl
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment for task {TaskId}", taskId);
            return Result<UploadAttachmentResponse>.Failure("Failed to upload attachment");
        }
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> GetTaskAttachmentsAsync(Guid taskId)
    {
        try
        {
            var attachments = await _context.Set<Attachment>()
                .Where(a => a.TaskItemId == taskId)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();

            var dtos = new List<AttachmentDto>();

            foreach (var attachment in attachments)
            {
                var downloadUrl = await _storageService.GetPresignedDownloadUrlAsync(
                    attachment.StoragePath, TimeSpan.FromHours(1));

                string? thumbnailUrl = null;
                if (attachment.ThumbnailPath != null)
                {
                    thumbnailUrl = await _storageService.GetPresignedDownloadUrlAsync(
                        attachment.ThumbnailPath, TimeSpan.FromHours(1));
                }

                dtos.Add(new AttachmentDto(
                    attachment.Id,
                    attachment.FileName,
                    attachment.ContentType,
                    attachment.FileSize,
                    attachment.Description,
                    attachment.TaskItemId,
                    attachment.CommentId,
                    attachment.UploadedByUserId,
                    attachment.UploadedAt,
                    downloadUrl,
                    thumbnailUrl,
                    attachment.IsImage
                ));
            }

            return Result<IEnumerable<AttachmentDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for task {TaskId}", taskId);
            return Result<IEnumerable<AttachmentDto>>.Failure("Failed to retrieve attachments");
        }
    }

    public async Task<Result> DeleteAttachmentAsync(Guid attachmentId, Guid userId)
    {
        try
        {
            var attachment = await _context.Set<Attachment>().FindAsync(attachmentId);
            if (attachment == null)
                return Result.Failure("Attachment not found");

            // Optional: Check if user has permission to delete
            // if (attachment.UploadedByUserId != userId)
            //     return Result.Failure("Unauthorized");

            // Delete from storage
            await _storageService.DeleteFileAsync(attachment.StoragePath);

            if (attachment.ThumbnailPath != null)
            {
                await _storageService.DeleteFileAsync(attachment.ThumbnailPath);
            }

            // Delete from database
            _context.Set<Attachment>().Remove(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Attachment {AttachmentId} deleted", attachmentId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
            return Result.Failure("Failed to delete attachment");
        }
    }
}