
// ============================================================================
// FILE 3: Api/Controllers/BoardsController.cs (Refactored)
// ============================================================================
using DevBoard.Application.Dtos;
using DevBoard.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardsController : ControllerBase
    {
        private readonly IBoardService _boardService;

        public BoardsController(IBoardService boardService)
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBoardDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _boardService.CreateAsync(dto);

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
}