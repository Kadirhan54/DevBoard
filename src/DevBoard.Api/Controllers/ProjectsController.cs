using DevBoard.Application.Dtos;
using DevBoard.Domain.Entities;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _context.Projects
                .AsNoTracking()
                .Select(p => new ProjectDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.TenantId.ToString()
                ))
                .ToListAsync();

            return Ok(projects);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectDto dto)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                TenantId = Guid.Parse(dto.TenantId)
            };

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var result = new ProjectDto(
                project.Id,
                project.Name,
                project.Description,
                project.TenantId.ToString()
            );

            return Ok(result);
        }

        [HttpGet("{projectId:guid}/boards")]
        public async Task<IActionResult> GetProjectWithBoardsByProject(Guid projectId)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == projectId)
                .Select(p => new ProjectWithBoardsDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.TenantId.ToString(),
                    p.Boards.Select(b => new BoardWithTasksDto(
                        b.Id,
                        b.Name,
                        b.Description,
                        b.Tasks.Select(t => new TaskItemResponseDto(
                            t.Id,
                            t.Name,
                            t.Description,
                            (int)t.Status,
                            t.DueDate,
                            t.BoardId
                        ))
                    ))
                ))
                .FirstOrDefaultAsync();

            if (project is null)
                return NotFound($"Project with ID {projectId} not found.");

            return Ok(project);
        }

        // GET: api/projects/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProjectDto>> GetById(Guid id)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProjectDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.TenantId.ToString()
                ))
                .FirstOrDefaultAsync();

            if (project is null)
                return NotFound();

            return Ok(project);
        }

        // DELETE: api/projects/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project is null)
                return NotFound($"Project with ID {id} not found.");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent(); // ✅ 204 No Content (standard for DELETE)
        }
    }
}
