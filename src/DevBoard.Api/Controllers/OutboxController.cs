// Presentation/Controllers/OutboxController.cs
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin")] // Restrict to admins only
    public class OutboxController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<OutboxController> _logger;

        public OutboxController(
            ApplicationDbContext dbContext,
            ILogger<OutboxController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var total = await _dbContext.OutboxMessages.CountAsync();
            var pending = await _dbContext.OutboxMessages
                .CountAsync(m => m.ProcessedOnUtc == null);
            var processed = await _dbContext.OutboxMessages
                .CountAsync(m => m.ProcessedOnUtc != null && string.IsNullOrEmpty(m.Error));
            var failed = await _dbContext.OutboxMessages
                .CountAsync(m => m.ProcessedOnUtc != null && !string.IsNullOrEmpty(m.Error));

            return Ok(new
            {
                Total = total,
                Pending = pending,
                Processed = processed,
                Failed = failed
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingMessages([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var messages = await _dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null)
                .OrderBy(m => m.OccurredOnUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.Type,
                    m.OccurredOnUtc,
                    m.RetryCount,
                    m.TenantId,
                    m.Error
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("failed")]
        public async Task<IActionResult> GetFailedMessages([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var messages = await _dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc != null && !string.IsNullOrEmpty(m.Error))
                .OrderByDescending(m => m.ProcessedOnUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.Type,
                    m.OccurredOnUtc,
                    m.ProcessedOnUtc,
                    m.RetryCount,
                    m.TenantId,
                    m.Error
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("{id}/retry")]
        public async Task<IActionResult> RetryMessage(Guid id)
        {
            var message = await _dbContext.OutboxMessages.FindAsync(id);
            if (message == null)
                return NotFound();

            if (message.ProcessedOnUtc == null)
                return BadRequest("Message is already pending");

            // Reset for retry
            message.ProcessedOnUtc = null;
            message.RetryCount = 0;
            message.Error = null;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Outbox message {MessageId} manually queued for retry", id);

            return Ok(new { message = "Message queued for retry" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            var message = await _dbContext.OutboxMessages.FindAsync(id);
            if (message == null)
                return NotFound();

            _dbContext.OutboxMessages.Remove(message);
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning("Outbox message {MessageId} manually deleted", id);

            return NoContent();
        }

        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupOldMessages([FromQuery] int days = 7)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            var deletedCount = await _dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc != null && m.ProcessedOnUtc < cutoffDate)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Manually cleaned up {Count} outbox messages", deletedCount);

            return Ok(new { deletedCount });
        }
    }
}