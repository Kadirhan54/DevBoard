
// ============================================================================
// FILE: Services/Notifications/DevBoard.Services.Notifications.Api/Services/EmailService.cs
// ============================================================================
using System.Net;
using System.Net.Mail;

namespace DevBoard.Services.Notifications.Api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendTaskAssignedEmailAsync(string toEmail, string taskTitle, Guid taskId)
    {
        var subject = $"New Task Assigned: {taskTitle}";
        var body = $@"
            <h2>You have been assigned a new task</h2>
            <p><strong>Task:</strong> {taskTitle}</p>
            <p><strong>Task ID:</strong> {taskId}</p>
            <p>Please log in to your DevBoard account to view details.</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendCommentNotificationEmailAsync(string toEmail, string taskTitle, string commentText)
    {
        var subject = $"New Comment on Task: {taskTitle}";
        var body = $@"
            <h2>New comment on your task</h2>
            <p><strong>Task:</strong> {taskTitle}</p>
            <p><strong>Comment:</strong> {commentText}</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var subject = "Welcome to DevBoard!";
        var body = $@"
            <h2>Welcome {userName}!</h2>
            <p>Thank you for joining DevBoard.</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]!);
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];

            // In development, just log instead of sending
            if (string.IsNullOrEmpty(smtpHost) || smtpHost == "smtp.gmail.com")
            {
                _logger.LogInformation(
                    "EMAIL (Dev Mode) - To: {Email}, Subject: {Subject}",
                    toEmail, subject);
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't throw - notification failure shouldn't break the system
        }
    }
}
