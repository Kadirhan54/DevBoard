using DevBoard.Application.Dtos;
using DevBoard.Infrastructure.Contexts.Application;
using DevBoard.Infrastructure.Services;
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
        private readonly ITenantProvider _tenantProvider;

        public TenantsController(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
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

            return Ok(tenants);
        }
    }
}
