// Infrastructure/Messaging/RabbitMqSettings.cs
namespace DevBoard.Infrastructure.Messaging
{
    public class RabbitMqSettings
    {
        public string Host { get; set; } = "rabbitmq";
        public ushort Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public int RetryCount { get; set; } = 3;
        public int RetryIntervalSeconds { get; set; } = 5;
    }
}