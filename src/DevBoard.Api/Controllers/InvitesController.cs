
// ============================================================================
// FILE 5: Api/Controllers/InvitesController.cs (Refactored)
// ============================================================================
using DevBoard.Application.Dtos;
using DevBoard.Application.Interfaces;
using DevBoard.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvitesController : ControllerBase
    {
        private readonly IInviteService _inviteService;

        public InvitesController(IInviteService inviteService)
        {
            _inviteService = inviteService;
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("send")]
        public async Task<IActionResult> SendInvite([FromBody] string invitedEmail)
        {
            var result = await _inviteService.SendInviteAsync(invitedEmail);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new
            {
                Message = result.Value.Message,
                InviteToken = result.Value.InviteToken
            });
        }

        [HttpPost("accept-invite")]
        public async Task<IActionResult> AcceptInvite([FromForm] AcceptInviteDto dto)
        {
            var result = await _inviteService.AcceptInviteAsync(dto);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any())
                    return BadRequest(result.Errors);
                return BadRequest(result.Error);
            }

            return Ok(new
            {
                Message = result.Value.Message,
                Token = result.Value.Token,
                TenantId = result.Value.TenantId
            });
        }
    }
}
