// Infrastructure/BackgroundServices/OutboxProcessorSettings.cs
namespace DevBoard.Infrastructure.BackgroundServices
{
    public class OutboxProcessorSettings
    {
        public int IntervalSeconds { get; set; } = 10;
        public int BatchSize { get; set; } = 20;
        public int MaxRetries { get; set; } = 5;
    }
}