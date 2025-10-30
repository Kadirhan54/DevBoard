
// ============================================
// Application/Interfaces/ITenantService.cs
// ============================================
using DevBoard.Application.Common;
using DevBoard.Domain.Entities;

namespace DevBoard.Application.Interfaces
{
    public interface ITenantService
    {
        Task<Result<IEnumerable<Tenant>>> GetAllTenantsAsync();
    }
}