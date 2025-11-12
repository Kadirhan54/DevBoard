// ============================================================================
// FILE: Services/Projects/DevBoard.Services.Projects.Api/Controllers/BoardsController.cs
// ============================================================================
using DevBoard.Services.Projects.Api.Services;
using DevBoard.Shared.Contracts.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Services.Projects.Api.Controllers;

[Route("api/v1/boards")]
[ApiController]
[Authorize]
[AllowAnonymous] // ⚠️ TEMPORARY - Remove after implementing proper auth! // TODO
public class BoardsController : ControllerBase
{
    private readonly BoardService _boardService;

    public BoardsController(BoardService boardService)
    {
        _boardService = boardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _boardService.GetAllAsync();
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _boardService.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    // HEAD endpoint for existence check (used by Task Service)
    [HttpHead("{id:guid}")]
    public async Task<IActionResult> CheckExists(Guid id)
    {
        var result = await _boardService.ValidateExistsAsync(id);
        return result.IsSuccess && result.Value ? Ok() : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _boardService.CreateAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _boardService.DeleteAsync(id);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}