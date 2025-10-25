using DevBoard.Application.Dtos;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TenantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _context.Tenants
                .Include(t => t.Users)
                .Include(t => t.Projects)
                    .ThenInclude(p => p.Boards)
                        .ThenInclude(b => b.Tasks)
                .AsNoTracking()
                .ToListAsync();

            var result = tenants.Select(t => new TenantResultDto(
                   t.Id,
                   t.Name,
                   t.Domain
                   )).ToList();

            return Ok(new ResultDto<List<TenantResultDto>>(true, "Tenants retrieved successfully", result));
        }
    }
}
