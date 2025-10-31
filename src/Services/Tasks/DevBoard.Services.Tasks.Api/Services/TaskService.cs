// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Api/Services/TaskService.cs
// ============================================================================
using DevBoard.Services.Tasks.Core.Entities;
using DevBoard.Services.Tasks.Infrastructure.Data;
using DevBoard.Shared.Common;
using DevBoard.Shared.Common.HttpClients;
using DevBoard.Shared.Contracts.Events;
using DevBoard.Shared.Contracts.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Services.Tasks.Api.Services;

public class TaskService
{
    private readonly TaskDbContext _context;
    private readonly IProjectServiceClient _projectServiceClient;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        TaskDbContext context,
        IProjectServiceClient projectServiceClient,
        ITenantProvider tenantProvider,
        IPublishEndpoint publishEndpoint,
        ILogger<TaskService> logger)
    {
        _context = context;
        _projectServiceClient = projectServiceClient;
        _tenantProvider = tenantProvider;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<TaskDto>> CreateAsync(CreateTaskRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty)
                return Result<TaskDto>.Failure("Tenant ID is required");

            // ✅ CRITICAL: Validate BoardId exists in Project Service
            var boardValidation = await _projectServiceClient.ValidateBoardExistsAsync(request.BoardId);
            if (!boardValidation.IsSuccess)
                return Result<TaskDto>.Failure(boardValidation.Error!);

            if (!boardValidation.Value)
                return Result<TaskDto>.Failure($"Board {request.BoardId} not found");

            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Status = request.Status,
                BoardId = request.BoardId,
                DueDate = request.DueDate,
                AssignedUserId = request.AssignedUserId,
                TenantId = tenantId
            };

            await _context.Tasks.AddAsync(taskItem);
            await _context.SaveChangesAsync();

            // Publish event
            await _publishEndpoint.Publish(new TaskCreatedEvent
            {
                TenantId = tenantId,
                TaskItemId = taskItem.Id,
                Title = taskItem.Name,
                AssignedToUserId = string.IsNullOrEmpty(taskItem.AssignedUserId)
                    ? null
                    : Guid.Parse(taskItem.AssignedUserId),
                BoardId = taskItem.BoardId
            });

            _logger.LogInformation("Task {TaskId} created for board {BoardId}", taskItem.Id, request.BoardId);

            return Result<TaskDto>.Success(new TaskDto(
                taskItem.Id,
                taskItem.Name,
                taskItem.Description,
                taskItem.Status,
                taskItem.DueDate,
                taskItem.BoardId,
                taskItem.TenantId,
                taskItem.AssignedUserId
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return Result<TaskDto>.Failure("Failed to create task");
        }
    }

    public async Task<Result<IEnumerable<TaskDto>>> GetAllAsync()
    {
        try
        {
            var tasks = await _context.Tasks
                .AsNoTracking()
                .Select(t => new TaskDto(
                    t.Id,
                    t.Name,
                    t.Description,
                    t.Status,
                    t.DueDate,
                    t.BoardId,
                    t.TenantId,
                    t.AssignedUserId
                ))
                .ToListAsync();

            return Result<IEnumerable<TaskDto>>.Success(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return Result<IEnumerable<TaskDto>>.Failure("Failed to retrieve tasks");
        }
    }
}