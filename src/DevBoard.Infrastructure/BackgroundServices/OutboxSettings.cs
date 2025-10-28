namespace DevBoard.Infrastructure.BackgroundServices
{
    public class OutboxSettings
    {
        // Shared between services
        public int MaxRetries { get; set; } = 5;

        // Processor-specific
        public int ProcessorIntervalSeconds { get; set; } = 10;
        public int ProcessorBatchSize { get; set; } = 20;

        // Cleanup-specific
        public int CleanupIntervalHours { get; set; } = 24;
        public int CleanupRetentionDays { get; set; } = 7;
        public int CleanupStartupDelayMinutes { get; set; } = 5;
    }
}
