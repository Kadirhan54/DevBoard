// ============================================================================
// FILE 1: Api/Controllers/AuthController.cs (Refactored)
// ============================================================================
using DevBoard.Application.Dtos;
using DevBoard.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsersAsync()
        {
            var result = await _authService.GetUsersAsync();
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromForm] LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new
            {
                Token = result.Value.Token,
                TenantId = result.Value.TenantId
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromForm] RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any())
                    return BadRequest(result.Errors);
                return BadRequest(result.Error);
            }

            return Ok(new
            {
                Token = result.Value.Token,
                TenantId = result.Value.TenantId,
                Organization = result.Value.OrganizationName
            });
        }
    }
}