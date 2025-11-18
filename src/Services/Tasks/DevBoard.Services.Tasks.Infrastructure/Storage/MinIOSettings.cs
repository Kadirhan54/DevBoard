// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Infrastructure/Storage/MinIOSettings.cs
// ============================================================================

namespace DevBoard.Services.Tasks.Infrastructure.Storage;

public class MinIOSettings
{
    public string Endpoint { get; set; } = "minio:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public string BucketName { get; set; } = "task-attachments";
    public bool UseSSL { get; set; } = false;
}