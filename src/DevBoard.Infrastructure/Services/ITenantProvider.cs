using DevBoard.Domain.Common;
using DevBoard.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace DevBoard.Infrastructure.Services
{
    public interface ITenantProvider
    {
        Guid GetTenantId();
        Result<string> CheckResultPattern(); // ← Added for testing
    }

    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetTenantId()
        {
            var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId");
            if (tenantClaim == null || string.IsNullOrWhiteSpace(tenantClaim?.Value))
            {
                return Guid.Empty;
            }

            return Guid.Parse(tenantClaim.Value);
        }

        public Result<string> CheckResultPattern()
        {
            return Result<string>.Failure("Tenant not found", ErrorType.NotFound);
        }
    }
}
