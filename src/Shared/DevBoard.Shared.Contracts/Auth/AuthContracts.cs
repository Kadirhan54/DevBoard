// ============================================================================
// FILE: Shared/DevBoard.Shared.Contracts/Auth/AuthContracts.cs
// ============================================================================
namespace DevBoard.Shared.Contracts.Auth;

// Request/Response for Identity Service API
public record RegisterRequest(
    string Email,
    string Password,
    bool EnableNotifications,
    string OrganizationName,
    string? Domain
);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Email,
    string Token,
    Guid TenantId
);

public record SendInviteRequest(string InvitedEmail);

public record AcceptInviteRequest(
    string InviteToken,
    string Email,
    string Password
);
