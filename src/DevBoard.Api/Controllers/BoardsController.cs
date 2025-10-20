using DevBoard.Application.Dtos;
using DevBoard.Domain.Entities;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BoardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/boards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SimpleBoardDto>>> GetAll()
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

            return Ok(boards);
        }

        // POST: api/boards
        [HttpPost]
        public async Task<ActionResult<SimpleBoardDto>> Create([FromBody] CreateBoardDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var projectExists = await _context.Projects
                .AsNoTracking()
                .AnyAsync(p => p.Id == dto.ProjectId);

            if (!projectExists)
                return NotFound($"Project with ID {dto.ProjectId} not found.");

            var board = new Board
            {
                Name = dto.Name,
                Description = dto.Description,
                ProjectId = dto.ProjectId
            };

            await _context.Boards.AddAsync(board);
            await _context.SaveChangesAsync();

            var result = new SimpleBoardDto(
                board.Id,
                board.Name,
                board.Description,
                board.TenantId
            );

            return CreatedAtAction(nameof(GetById), new { id = board.Id }, result);
        }

        // GET: api/boards/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SimpleBoardDto>> GetById(Guid id)
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
                return NotFound($"Board with ID {id} not found.");

            return Ok(board);
        }

        // DELETE: api/boards/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var board = await _context.Boards.FindAsync(id);

            if (board is null)
                return NotFound($"Board with ID {id} not found.");

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
