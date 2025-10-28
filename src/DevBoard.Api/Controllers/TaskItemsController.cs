using DevBoard.Application.Dtos;
using DevBoard.Application.Events;
using DevBoard.Application.Services;
using DevBoard.Domain.Entities;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Contexts.Application;
using DevBoard.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ITenantProvider _tenantProvider;
        private readonly IOutboxService _outboxService;
        private readonly ILogger<TaskItemsController> _logger;

        public TaskItemsController(ApplicationDbContext context, IPublishEndpoint publishEndpoint, ITenantProvider tenantProvider, IOutboxService outboxService, ILogger<TaskItemsController> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _tenantProvider = tenantProvider;
            _outboxService = outboxService;
            _logger = logger;
        }

        // ✅ GET api/taskitems
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var taskItems = await _context.Tasks
                .Include(t => t.Board)
                .Include(t => t.AssignedUser)
                .AsNoTracking()
                .ToListAsync();

            var response = taskItems.Select(taskItem => new TaskItemResponseDto(
                taskItem.Id,
                taskItem.Name,
                taskItem.Description,
                (int)taskItem.Status,
                taskItem.DueDate,
                taskItem.BoardId,
                taskItem.TenantId
            )).ToList();

            return Ok(response);
        }

        // ✅ POST api/taskitems
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskItemDto dto)
        {
            var tenantId = _tenantProvider.GetTenantId();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var board = await _context.Boards.FirstOrDefaultAsync(p => p.Id == dto.BoardId);
            if (board == null)
                return NotFound("Board not found.");

            ApplicationUser? user = null;

            if (dto.AssignedUserId != null)
            {
                user = await _context.Users.FindAsync(dto.AssignedUserId);
                if (user == null)
                    return BadRequest("Assigned user not found.");
            }

            // Start transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var taskItem = new TaskItem
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    BoardId = dto.BoardId,
                    Board = board,
                    Status = dto.Status,
                    AssignedUserId = dto.AssignedUserId,
                    AssignedUser = user,
                    DueDate = dto.DueDate,
                    CreatedAt = DateTime.UtcNow,
                };

                // Save event to outbox (same transaction)
                await _outboxService.SaveEventAsync(new TaskItemCreatedEvent
                {
                    TenantId = tenantId,
                    TaskItemId = taskItem.Id,
                    Title = taskItem.Name,
                    AssignedToUserId = !string.IsNullOrEmpty(taskItem.AssignedUserId)
                        ? Guid.Parse(taskItem.AssignedUserId)
                        : null,
                    BoardId = taskItem.BoardId
                });

                // Commit transaction (atomically saves both TaskItem and OutboxMessage)
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "TaskItem {TaskItemId} created for tenant {TenantId}",
                    taskItem.Id, tenantId);

                return CreatedAtAction(nameof(GetById), new { id = taskItem.Id }, taskItem);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create TaskItem");
                return StatusCode(500, "An error occurred while creating the task item");
            }
        }

        // ✅ GET api/taskitems/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var taskItem = await _context.Tasks
                .Include(t => t.Board)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null)
                return NotFound("Task item not found.");

            return Ok(new TaskItemResponseDto(
                taskItem.Id,
                taskItem.Name,
                taskItem.Description,
                (int)taskItem.Status,
                taskItem.DueDate,
                taskItem.BoardId,
                taskItem.TenantId
            ));
        }

        // ✅ DELETE api/taskitems/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var taskItem = await _context.Tasks.FirstOrDefaultAsync(p => p.Id == id);

            if (taskItem == null)
                return NotFound("Task item not found.");

            _context.Tasks.Remove(taskItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
