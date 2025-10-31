// ============================================================================
// FILE: Shared/DevBoard.Shared.Common/HttpClients/IIdentityServiceClient.cs
// ============================================================================
namespace DevBoard.Shared.Common.HttpClients;

using DevBoard.Shared.Contracts.Users;

/// <summary>
/// Interface for communicating with Identity Service from other services
/// </summary>
public interface IIdentityServiceClient
{
    Task<Result<UserDto>> GetUserByIdAsync(Guid userId);
    Task<Result<TenantDto>> GetTenantByIdAsync(Guid tenantId);
    Task<Result<bool>> ValidateTenantExistsAsync(Guid tenantId);
}