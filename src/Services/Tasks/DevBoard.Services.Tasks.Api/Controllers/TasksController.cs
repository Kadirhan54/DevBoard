
// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Api/Controllers/TasksController.cs
// ============================================================================
using DevBoard.Services.Tasks.Api.Services;
using DevBoard.Shared.Contracts.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Services.Tasks.Api.Controllers;

[Route("api/v1/tasks")]
[ApiController]
[Authorize]
[AllowAnonymous] // ⚠️ TEMPORARY - Remove after implementing proper auth! // TODO
public class TasksController : ControllerBase
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _taskService.GetAllAsync();
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _taskService.CreateAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Implementation
        return Ok();
    }
}