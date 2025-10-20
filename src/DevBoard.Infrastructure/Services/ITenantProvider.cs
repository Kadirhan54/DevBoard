using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Infrastructure.Services
{
    public interface ITenantProvider
    {
        Guid GetTenantId();
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
            // If no HTTP context, return a dummy tenant for seeding
            var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId");
            if (tenantClaim == null || string.IsNullOrWhiteSpace(tenantClaim?.Value))
            {
                // return Guid.Empty (or a fixed "seed tenant" ID) during seeding/migrations
                return Guid.Empty;
            }

            return Guid.Parse(tenantClaim.Value);
        }
    }

}
