
// ============================================================================
// FILE 5: Infrastructure/Services/TenantService.cs
// ============================================================================
using DevBoard.Application.Common;
using DevBoard.Application.Interfaces;
using DevBoard.Domain.Entities;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TenantService> _logger;

        public TenantService(ApplicationDbContext context, ILogger<TenantService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<Tenant>>> GetAllTenantsAsync()
        {
            try
            {
                var tenants = await _context.Tenants
                    .Include(t => t.Users)
                    .Include(t => t.Projects)
                        .ThenInclude(p => p.Boards)
                            .ThenInclude(b => b.Tasks)
                    .AsNoTracking()
                    .ToListAsync();

                return Result<IEnumerable<Tenant>>.Success(tenants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenants");
                return Result<IEnumerable<Tenant>>.Failure("Failed to retrieve tenants");
            }
        }
    }
}