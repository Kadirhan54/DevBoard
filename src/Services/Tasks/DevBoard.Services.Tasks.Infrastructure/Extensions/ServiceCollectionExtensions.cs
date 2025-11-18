// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Api/Extensions/ServiceCollectionExtensions.cs
// ============================================================================

using DevBoard.Services.Tasks.Infrastructure.Services;
using DevBoard.Services.Tasks.Infrastructure.Storage;
using DevBoard.Shared.Common.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace DevBoard.Services.Tasks.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMinIOStorageAndFileUploadSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind MinIO settings
        var minioSettings = configuration.GetSection("MinIO").Get<MinIOSettings>() ?? new MinIOSettings();
        services.AddSingleton(minioSettings);

        // Bind file upload settings
        var uploadSettings = configuration.GetSection("FileUpload").Get<FileUploadSettings>() ?? new FileUploadSettings();
        services.AddSingleton(uploadSettings);

        // Register MinIO client
        services.AddSingleton<IMinioClient>(sp =>
        {
            var settings = sp.GetRequiredService<MinIOSettings>();

            return new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey)
                .WithSSL(settings.UseSSL)
                .Build();
        });

        // Register storage service
        services.AddScoped<IFileStorageService, MinIOFileStorageService>();

        services.AddScoped<AttachmentService>();

        return services;
    }

}