// ============================================================================
// FILE: Shared/DevBoard.Shared.Common/TenantProvider.cs
// ============================================================================
using Microsoft.AspNetCore.Http;

namespace DevBoard.Shared.Common;

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
        var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId");
        if (tenantClaim == null || string.IsNullOrWhiteSpace(tenantClaim.Value))
            return Guid.Empty;

        return Guid.TryParse(tenantClaim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}