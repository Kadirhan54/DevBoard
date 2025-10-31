// ============================================================================
// FILE: Shared/DevBoard.Shared.Common/TenantContext.cs
// ============================================================================
namespace DevBoard.Shared.Common;

public interface ITenantProvider
{
    Guid GetTenantId();
}