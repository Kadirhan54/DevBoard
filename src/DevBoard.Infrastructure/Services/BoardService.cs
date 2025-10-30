
// ============================================================================
// FILE 2: Infrastructure/Services/BoardService.cs
// ============================================================================
using DevBoard.Application.Common;
using DevBoard.Application.Dtos;
using DevBoard.Application.Interfaces;
using DevBoard.Domain.Entities;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Services
{
    public class BoardService : IBoardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BoardService> _logger;

        public BoardService(ApplicationDbContext context, ILogger<BoardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<SimpleBoardDto>>> GetAllAsync()
        {
            try
            {
                var boards = await _context.Boards
                    .AsNoTracking()
                    .Select(b => new SimpleBoardDto(
                        b.Id,
                        b.Name,
                        b.Description,
                        b.TenantId
                    ))
                    .ToListAsync();

                return Result<IEnumerable<SimpleBoardDto>>.Success(boards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving boards");
                return Result<IEnumerable<SimpleBoardDto>>.Failure("Failed to retrieve boards");
            }
        }

        public async Task<Result<SimpleBoardDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var board = await _context.Boards
                    .AsNoTracking()
                    .Where(b => b.Id == id)
                    .Select(b => new SimpleBoardDto(
                        b.Id,
                        b.Name,
                        b.Description,
                        b.TenantId
                    ))
                    .FirstOrDefaultAsync();

                if (board is null)
                    return Result<SimpleBoardDto>.Failure($"Board with ID {id} not found");

                return Result<SimpleBoardDto>.Success(board);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving board {BoardId}", id);
                return Result<SimpleBoardDto>.Failure("Failed to retrieve board");
            }
        }

        public async Task<Result<SimpleBoardDto>> CreateAsync(CreateBoardDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var projectExists = await _context.Projects
                    .AsNoTracking()
                    .AnyAsync(p => p.Id == dto.ProjectId);

                if (!projectExists)
                    return Result<SimpleBoardDto>.Failure($"Project with ID {dto.ProjectId} not found");

                var board = new Board
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    ProjectId = dto.ProjectId
                };

                await _context.Boards.AddAsync(board);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var result = new SimpleBoardDto(
                    board.Id,
                    board.Name,
                    board.Description,
                    board.TenantId
                );

                _logger.LogInformation("Board {BoardId} created for project {ProjectId}", board.Id, dto.ProjectId);

                return Result<SimpleBoardDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating board");
                return Result<SimpleBoardDto>.Failure("Failed to create board");
            }
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            try
            {
                var board = await _context.Boards.FindAsync(id);

                if (board is null)
                    return Result.Failure($"Board with ID {id} not found");

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
    }
}
