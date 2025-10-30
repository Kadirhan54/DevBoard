
// ============================================================================
// FILE 6: Api/Controllers/TenantsController.cs (Refactored)
// ============================================================================
using DevBoard.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTenants()
        {
            var result = await _tenantService.GetAllTenantsAsync();
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
    }
}