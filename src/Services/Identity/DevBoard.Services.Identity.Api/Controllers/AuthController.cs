
// ============================================================================
// FILE: Services/Identity/DevBoard.Services.Identity.Api/Controllers/AuthController.cs
// ============================================================================
using DevBoard.Services.Identity.Infrastructure.Services;
using DevBoard.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Services.Identity.Api.Controllers;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Errors.Any() ? result.Errors : new[] { result.Error });

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
