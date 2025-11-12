
// ============================================================================
// FILE: Services/Notifications/DevBoard.Services.Notifications.Api/Services/IEmailService.cs
// ============================================================================
namespace DevBoard.Services.Notifications.Api.Services;

public interface IEmailService
{
    Task SendTaskAssignedEmailAsync(string toEmail, string taskTitle, Guid taskId);
    Task SendCommentNotificationEmailAsync(string toEmail, string taskTitle, string commentText);
    Task SendWelcomeEmailAsync(string toEmail, string userName);
}