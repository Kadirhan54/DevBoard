// ============================================================================
// FILE: Shared/DevBoard.Shared.Contracts/Users/UserContracts.cs
// ============================================================================
namespace DevBoard.Shared.Contracts.Users;

public record UserDto(
    Guid Id,
    string Email,
    Guid TenantId,
    bool EnableNotifications
);

public record TenantDto(
    Guid Id,
    string Name,
    string? Domain
);