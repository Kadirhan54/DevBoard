
// ============================================================================
// FILE 3: Infrastructure/Services/TaskItemService.cs
// ============================================================================
using DevBoard.Application.Common;
using DevBoard.Application.Dtos;
using DevBoard.Application.Events;
using DevBoard.Application.Interfaces;
using DevBoard.Application.Services;
using DevBoard.Domain.Entities;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IOutboxService _outboxService;
        private readonly ILogger<TaskItemService> _logger;

        public TaskItemService(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IOutboxService outboxService,
            ILogger<TaskItemService> logger)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _outboxService = outboxService;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<TaskItemResponseDto>>> GetAllAsync()
        {
            try
            {
                var taskItems = await _context.Tasks
                    .Include(t => t.Board)
                    .Include(t => t.AssignedUser)
                    .AsNoTracking()
                    .ToListAsync();

                var response = taskItems.Select(t => new TaskItemResponseDto(
                    t.Id,
                    t.Name,
                    t.Description,
                    (int)t.Status,
                    t.DueDate,
                    t.BoardId,
                    t.TenantId
                )).ToList();

                return Result<IEnumerable<TaskItemResponseDto>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task items");
                return Result<IEnumerable<TaskItemResponseDto>>.Failure("Failed to retrieve task items");
            }
        }

        public async Task<Result<TaskItemResponseDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var taskItem = await _context.Tasks
                    .Include(t => t.Board)
                    .Include(t => t.AssignedUser)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (taskItem == null)
                    return Result<TaskItemResponseDto>.Failure("Task item not found");

                var response = new TaskItemResponseDto(
                    taskItem.Id,
                    taskItem.Name,
                    taskItem.Description,
                    (int)taskItem.Status,
                    taskItem.DueDate,
                    taskItem.BoardId,
                    taskItem.TenantId
                );

                return Result<TaskItemResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task item {TaskItemId}", id);
                return Result<TaskItemResponseDto>.Failure("Failed to retrieve task item");
            }
        }

        public async Task<Result<TaskItemResponseDto>> CreateAsync(CreateTaskItemDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var tenantId = _tenantProvider.GetTenantId();

                var board = await _context.Boards.FirstOrDefaultAsync(p => p.Id == dto.BoardId);
                if (board == null)
                    return Result<TaskItemResponseDto>.Failure("Board not found");

                ApplicationUser? user = null;
                if (dto.AssignedUserId != null)
                {
                    user = await _context.Users.FindAsync(dto.AssignedUserId);
                    if (user == null)
                        return Result<TaskItemResponseDto>.Failure("Assigned user not found");
                }

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

                await _context.Tasks.AddAsync(taskItem);

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

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("TaskItem {TaskItemId} created for tenant {TenantId}", taskItem.Id, tenantId);

                var response = new TaskItemResponseDto(
                    taskItem.Id,
                    taskItem.Name,
                    taskItem.Description,
                    (int)taskItem.Status,
                    taskItem.DueDate,
                    taskItem.BoardId,
                    taskItem.TenantId
                );

                return Result<TaskItemResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create TaskItem");
                return Result<TaskItemResponseDto>.Failure("An error occurred while creating the task item");
            }
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            try
            {
                var taskItem = await _context.Tasks.FirstOrDefaultAsync(p => p.Id == id);

                if (taskItem == null)
                    return Result.Failure("Task item not found");

                _context.Tasks.Remove(taskItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation("TaskItem {TaskItemId} deleted", id);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task item {TaskItemId}", id);
                return Result.Failure("Failed to delete task item");
            }
        }
    }
}
