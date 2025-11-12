// ============================================================================
// FILE: Shared/DevBoard.Shared.Common/Storage/FileUploadSettings.cs
// ============================================================================

namespace DevBoard.Shared.Common.Storage;

public class FileUploadSettings
{
    public long MaxFileSizeMB { get; set; } = 10;
    public int MaxAttachmentsPerTask { get; set; } = 10;
    public int MaxAttachmentsPerComment { get; set; } = 5;

    public string[] AllowedImageExtensions { get; set; } =
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"
    };

    public string[] AllowedDocumentExtensions { get; set; } =
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".md", ".csv"
    };

    public string[] AllowedArchiveExtensions { get; set; } =
    {
        ".zip", ".rar", ".7z", ".tar", ".gz"
    };

    public string[] BlockedExtensions { get; set; } =
    {
        ".exe", ".dll", ".bat", ".sh", ".cmd", ".vbs", ".js", ".jar"
    };

    public long MaxFileSizeBytes => MaxFileSizeMB * 1024 * 1024;

    public string[] AllAllowedExtensions =>
        AllowedImageExtensions
            .Concat(AllowedDocumentExtensions)
            .Concat(AllowedArchiveExtensions)
            .ToArray();
}
