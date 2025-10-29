
// ============================================================================
// FILE 2: Api/Controllers/ProjectsController.cs (Refactored)
// ============================================================================
using DevBoard.Application.Dtos;
using DevBoard.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _projectService.GetAllAsync();
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _projectService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        [HttpGet("{projectId:guid}/boards")]
        public async Task<IActionResult> GetProjectWithBoardsByProject(Guid projectId)
        {
            var result = await _projectService.GetProjectWithBoardsAsync(projectId);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectDto dto)
        {
            var result = await _projectService.CreateAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new { ProjectId = result.Value.Id, Result = result.Value });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _projectService.DeleteAsync(id);
            return result.IsSuccess ? NoContent() : NotFound(result.Error);
        }
    }
}
