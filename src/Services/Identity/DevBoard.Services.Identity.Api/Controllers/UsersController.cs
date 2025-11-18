
// ============================================================================
// FILE: Services/Identity/DevBoard.Services.Identity.Api/Controllers/UsersController.cs
// ============================================================================
using DevBoard.Services.Identity.Infrastructure.Data;
using DevBoard.Shared.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Services.Identity.Api.Controllers;

[Route("api/v1/users")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IdentityDbContext _context;

    public UsersController(IdentityDbContext context)
    {
        _context = context;
    }


    // TODO : Filter by tenant using ITenantProvider
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id.ToString())
            .Select(u => new UserDto(
                Guid.Parse(u.Id),
                u.Email!,
                u.TenantId,
                u.EnableNotifications
            ))
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // TODO : Filter by tenant using ITenantProvider
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Filtered by tenant via JWT in production
        var users = await _context.Users
            .AsNoTracking()
            .Select(u => new UserDto(
                Guid.Parse(u.Id),
                u.Email!,
                u.TenantId,
                u.EnableNotifications
            ))
            .ToListAsync();

        return Ok(users);
    }
}