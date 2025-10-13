using DevBoard.Application.Dtos;
using DevBoard.Domain.Entities;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskItemsController(ApplicationDbContext context)
        {
            _context = context;
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

            var response = taskItems.Select(taskItem => new TaskItemResponseDto
            {
                Id = taskItem.Id,
                Name = taskItem.Name,
                Description = taskItem.Description,
                Status = (int)taskItem.Status,
                DueDate = taskItem.DueDate,
                BoardId = taskItem.BoardId,
                Board = taskItem.Board == null
                    ? null
                    : new SimpleBoardDto
                    {
                        Id = taskItem.Board.Id,
                        Name = taskItem.Board.Name
                    },
                AssignedUserId = taskItem.AssignedUser == null
                    ? (Guid?)null
                    : Guid.Parse(taskItem.AssignedUser.Id),
                AssignedUser = taskItem.AssignedUser == null
                    ? null
                    : new SimpleUserDto
                    {
                        Id = Guid.Parse(taskItem.AssignedUser.Id),
                        UserName = taskItem.AssignedUser.UserName!,
                        Email = taskItem.AssignedUser.Email!
                    }
            }).ToList();

            return Ok(response);
        }

        // ✅ POST api/taskitems
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskItemDto dto)
        {
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

            var taskItem = new TaskItem
            {
                Name = dto.Name,
                Description = dto.Description,
                BoardId = dto.BoardId,
                Board = board,
                Status = dto.Status,
                AssignedUserId = dto.AssignedUserId,
                AssignedUser = user,
                DueDate = dto.DueDate
            };

            await _context.Tasks.AddAsync(taskItem);
            await _context.SaveChangesAsync();

            return Ok(new TaskItemResponseDto
            {
                Id = taskItem.Id,
                Name = taskItem.Name,
                Description = taskItem.Description,
                Status = (int)taskItem.Status,
                DueDate = taskItem.DueDate,
                BoardId = taskItem.BoardId,
                Board = taskItem.Board == null ? null : new SimpleBoardDto
                {
                    Id = taskItem.Board.Id,
                    Name = taskItem.Board.Name
                },
                AssignedUserId = taskItem.AssignedUser == null ? (Guid?)null : Guid.Parse(taskItem.AssignedUser.Id),
                AssignedUser = taskItem.AssignedUser == null ? null : new SimpleUserDto
                {
                    Id = Guid.Parse(taskItem.AssignedUser.Id),
                    UserName = taskItem.AssignedUser.UserName!,
                    Email = taskItem.AssignedUser.Email!
                }
            });
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

            return Ok(new TaskItemResponseDto
            {
                Id = taskItem.Id,
                Name = taskItem.Name,
                Description = taskItem.Description,
                Status = (int)taskItem.Status,
                DueDate = taskItem.DueDate,
                BoardId = taskItem.BoardId,
                Board = taskItem.Board == null ? null : new SimpleBoardDto
                {
                    Id = taskItem.Board.Id,
                    Name = taskItem.Board.Name
                },
                AssignedUserId = taskItem.AssignedUser == null ? (Guid?)null : Guid.Parse(taskItem.AssignedUser.Id),
                AssignedUser = taskItem.AssignedUser == null ? null : new SimpleUserDto
                {
                    Id = Guid.Parse(taskItem.AssignedUser.Id),
                    UserName = taskItem.AssignedUser.UserName!,
                    Email = taskItem.AssignedUser.Email!
                }
            });
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
