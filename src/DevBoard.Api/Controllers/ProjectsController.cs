using DevBoard.Application.Dtos;
using DevBoard.Application.Services;
using DevBoard.Domain.Entities;
using DevBoard.Domain.Events;
using DevBoard.Infrastructure.Contexts.Application;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Xml.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public ProjectsController(ApplicationDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
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

            var projectId = Guid.NewGuid();

            var project = new Project
            {
                Id = projectId,
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

            await _publishEndpoint.Publish<ProjectCreated>(new
            {
                ProjectId = projectId,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow
            });

            return Ok(new { ProjectId = projectId, Result = result });
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
                        b.TenantId,
                        b.Tasks.Select(t => new TaskItemResponseDto(
                            t.Id,
                            t.Name,
                            t.Description,
                            (int)t.Status,
                            t.DueDate,
                            t.BoardId,
                            t.TenantId
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
