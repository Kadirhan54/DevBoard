// ============================================================================
// FILE: Services/Projects/DevBoard.Services.Projects.Api/Services/BoardService.cs
// ============================================================================
using DevBoard.Services.Projects.Core.Entities;
using DevBoard.Services.Projects.Infrastructure.Data;
using DevBoard.Shared.Common;
using DevBoard.Shared.Contracts.Events;
using DevBoard.Shared.Contracts.Projects;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Services.Projects.Api.Services;

public class BoardService
{
    private readonly ProjectDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<BoardService> _logger;

    public BoardService(
        ProjectDbContext context,
        ITenantProvider tenantProvider,
        IPublishEndpoint publishEndpoint,
        ILogger<BoardService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<BoardDto>>> GetAllAsync()
    {
        try
        {
            var boards = await _context.Boards
                .AsNoTracking()
                .Select(b => new BoardDto(
                    b.Id,
                    b.Name,
                    b.Description,
                    b.TenantId,
                    b.ProjectId
                ))
                .ToListAsync();

            return Result<IEnumerable<BoardDto>>.Success(boards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving boards");
            return Result<IEnumerable<BoardDto>>.Failure("Failed to retrieve boards");
        }
    }

    public async Task<Result<BoardDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var board = await _context.Boards
                .AsNoTracking()
                .Where(b => b.Id == id)
                .Select(b => new BoardDto(
                    b.Id,
                    b.Name,
                    b.Description,
                    b.TenantId,
                    b.ProjectId
                ))
                .FirstOrDefaultAsync();

            if (board == null)
                return Result<BoardDto>.Failure($"Board {id} not found");

            return Result<BoardDto>.Success(board);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving board {BoardId}", id);
            return Result<BoardDto>.Failure("Failed to retrieve board");
        }
    }

    public async Task<Result<BoardDto>> CreateAsync(CreateBoardRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId == Guid.Empty)
                return Result<BoardDto>.Failure("Tenant ID is required");

            // Validate project exists
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == request.ProjectId);

            if (!projectExists)
                return Result<BoardDto>.Failure($"Project {request.ProjectId} not found");

            var board = new Board
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                ProjectId = request.ProjectId,
                TenantId = tenantId
            };

            await _context.Boards.AddAsync(board);
            await _context.SaveChangesAsync();

            // Publish event
            await _publishEndpoint.Publish(new BoardCreatedEvent
            {
                TenantId = tenantId,
                BoardId = board.Id,
                Name = board.Name,
                ProjectId = board.ProjectId
            });

            _logger.LogInformation("Board {BoardId} created for project {ProjectId}",
                board.Id, request.ProjectId);

            return Result<BoardDto>.Success(new BoardDto(
                board.Id,
                board.Name,
                board.Description,
                board.TenantId,
                board.ProjectId
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating board");
            return Result<BoardDto>.Failure("Failed to create board");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null)
                return Result.Failure($"Board {id} not found");

            var tenantId = _tenantProvider.GetTenantId();

            // Publish event for Task Service to handle cascade delete
            await _publishEndpoint.Publish(new BoardDeletedEvent
            {
                TenantId = tenantId,
                BoardId = board.Id
            });

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Board {BoardId} deleted", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting board {BoardId}", id);
            return Result.Failure("Failed to delete board");
        }
    }

    // For Task Service validation (called via HTTP)
    public async Task<Result<bool>> ValidateExistsAsync(Guid boardId)
    {
        try
        {
            var exists = await _context.Boards
                .AsNoTracking()
                .AnyAsync(b => b.Id == boardId);

            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating board {BoardId}", boardId);
            return Result<bool>.Failure("Failed to validate board");
        }
    }
}