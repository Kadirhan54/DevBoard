// ============================================================================
// FILE: Services/Identity/DevBoard.Services.Identity.Core/Entities/ApplicationUser.cs
// ============================================================================
using Microsoft.AspNetCore.Identity;

namespace DevBoard.Services.Identity.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public bool EnableNotifications { get; set; } = true;
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}

public class TenantInvitation
{
    public Guid Id { get; set; }
    public string InvitedEmail { get; set; } = string.Empty;
    public string InviteToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
    public bool IsUsed { get; set; } = false;
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}