using DevBoard.Application.Dtos;
using DevBoard.Domain.Enums;
using DevBoard.Infrastructure.Contexts.Application;
using DevBoard.Infrastructure.Services;
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

            var result = tenants.Select(t => new TenantResultDto(
                   t.Id,
                   t.Name,
                   t.Domain
                   )).ToList();

            return Ok(new ResultDto<List<TenantResultDto>>(true, "Tenants retrieved successfully", result));
        }

        [HttpGet("test-result")]
        public IActionResult TestResult()
        {
            var result = _tenantProvider.CheckResultPattern();

            if (result.IsSuccess)
                return Ok(result.Value);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Validation => BadRequest(result.Error),
                ErrorType.Conflict => Conflict(result.Error),
                ErrorType.Unauthorized => Unauthorized(result.Error),
                _ => StatusCode(500, result.Error)
            };
        }
    }
}
