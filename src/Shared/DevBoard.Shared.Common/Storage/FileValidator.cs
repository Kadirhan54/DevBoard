// ============================================================================
// FILE: Shared/DevBoard.Shared.Common/Storage/FileValidator.cs
// ============================================================================

using Microsoft.AspNetCore.Http;

namespace DevBoard.Shared.Common.Storage;

public static class FileValidator
{
    public static (bool IsValid, string? Error) ValidateFile(
        IFormFile file,
        FileUploadSettings settings)
    {
        if (file == null || file.Length == 0)
            return (false, "File is empty");

        // Check file size
        if (file.Length > settings.MaxFileSizeBytes)
            return (false, $"File exceeds maximum size of {settings.MaxFileSizeMB}MB");

        // Get extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // Check if blocked
        if (settings.BlockedExtensions.Contains(extension))
            return (false, $"File type {extension} is not allowed for security reasons");

        // Check if allowed
        if (!settings.AllAllowedExtensions.Contains(extension))
            return (false, $"File type {extension} is not supported");

        // Validate content type matches extension
        if (!IsContentTypeValid(file.ContentType, extension))
            return (false, "File content type does not match extension");

        return (true, null);
    }

    private static bool IsContentTypeValid(string contentType, string extension)
    {
        var validMappings = new Dictionary<string, string[]>
        {
            { ".jpg", new[] { "image/jpeg", "image/jpg" } },
            { ".jpeg", new[] { "image/jpeg", "image/jpg" } },
            { ".png", new[] { "image/png" } },
            { ".gif", new[] { "image/gif" } },
            { ".pdf", new[] { "application/pdf" } },
            { ".zip", new[] { "application/zip", "application/x-zip-compressed" } },
            // Add more as needed
        };

        if (validMappings.TryGetValue(extension, out var validTypes))
        {
            return validTypes.Any(vt => contentType.Contains(vt, StringComparison.OrdinalIgnoreCase));
        }

        return true; // Allow if not in strict mapping
    }

    public static bool IsImageFile(string fileName)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return imageExtensions.Contains(extension);
    }
}