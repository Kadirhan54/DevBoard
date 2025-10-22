using DevBoard.Application.Dtos;
using DevBoard.Application.Interfaces;
using DevBoard.Domain.Entities.DevBoard.Domain.Entities;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvitesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; 
        private readonly ITokenService _tokenService;

        public InvitesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendInvite([FromBody] string invitedEmail)
        {
            // Get the current user's tenant from JWT
            var tenantIdClaim = User.FindFirst("tenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim))
                return BadRequest("TenantId missing from token.");

            var tenantId = Guid.Parse(tenantIdClaim);

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

            // TODO: Send via email — for now, return it in response
            return Ok(new
            {
                Message = "Invitation created successfully.",
                InviteToken = inviteToken
            });
        }


        [HttpPost("accept-invite")]
        public async Task<IActionResult> AcceptInvite([FromForm] AcceptInviteDto dto)
        {
            var invitation = await _context.TenantInvitations
                .FirstOrDefaultAsync(i => i.InviteToken == dto.InviteToken && !i.IsUsed);

            if (invitation == null || invitation.ExpiresAt < DateTime.UtcNow)
                return BadRequest("Invalid or expired invite token.");


            // TODO : Implement if user is already signed In ????
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("User with this email already exists.");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                TenantId = invitation.TenantId,
                EnableNotifications = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, Roles.Member);

            // Mark invite as used
            invitation.IsUsed = true;
            await _context.SaveChangesAsync();

            var token = _tokenService.CreateToken(user);

            return Ok(new
            {
                Message = "User successfully registered via invitation.",
                Token = token,
                TenantId = invitation.TenantId
            });
        }




    }
}
