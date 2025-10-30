
// ============================================================================
// FILE 4: Infrastructure/Services/InviteService.cs
// ============================================================================
using DevBoard.Application.Common;
using DevBoard.Application.Dtos;
using DevBoard.Application.Interfaces;
using DevBoard.Domain.Entities.DevBoard.Domain.Entities;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Services
{
    public class InviteService : IInviteService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ITenantProvider _tenantProvider;
        private readonly ILogger<InviteService> _logger;

        public InviteService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            ITenantProvider tenantProvider,
            ILogger<InviteService> logger)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
            _tenantProvider = tenantProvider;
            _logger = logger;
        }

        public async Task<Result<SendInviteResponseDto>> SendInviteAsync(string invitedEmail)
        {
            try
            {
                var tenantId = _tenantProvider.GetTenantId();

                if (tenantId == Guid.Empty)
                    return Result<SendInviteResponseDto>.Failure("TenantId missing from token");

                // Generate secure random token
                var inviteToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("=", "").Replace("+", "").Replace("/", "");

                var invitation = new TenantInvitation
                {
                    TenantId = tenantId,
                    InvitedEmail = invitedEmail,
                    InviteToken = inviteToken
                };

                _context.TenantInvitations.Add(invitation);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Invitation created for {Email} by tenant {TenantId}",
                    invitedEmail,
                    tenantId
                );

                return Result<SendInviteResponseDto>.Success(
                    new SendInviteResponseDto("Invitation created successfully", inviteToken)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invitation to {Email}", invitedEmail);
                return Result<SendInviteResponseDto>.Failure("Failed to send invitation");
            }
        }

        public async Task<Result<AcceptInviteResponseDto>> AcceptInviteAsync(AcceptInviteDto dto)
        {
            try
            {
                var invitation = await _context.TenantInvitations
                    .FirstOrDefaultAsync(i => i.InviteToken == dto.InviteToken && !i.IsUsed);

                if (invitation == null || invitation.ExpiresAt < DateTime.UtcNow)
                    return Result<AcceptInviteResponseDto>.Failure("Invalid or expired invite token");

                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return Result<AcceptInviteResponseDto>.Failure("User with this email already exists");

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    TenantId = invitation.TenantId,
                    EnableNotifications = true
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Result<AcceptInviteResponseDto>.Failure(errors);
                }

                await _userManager.AddToRoleAsync(user, Roles.Member);

                // Mark invite as used
                invitation.IsUsed = true;
                await _context.SaveChangesAsync();

                var token = await _tokenService.CreateTokenAsync(user);

                _logger.LogInformation(
                    "User {Email} registered via invitation for tenant {TenantId}",
                    dto.Email,
                    invitation.TenantId
                );

                return Result<AcceptInviteResponseDto>.Success(
                    new AcceptInviteResponseDto(
                        "User successfully registered via invitation",
                        token,
                        invitation.TenantId
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting invitation");
                return Result<AcceptInviteResponseDto>.Failure("Failed to accept invitation");
            }
        }
    }
}
