// ============================================
// Application/Interfaces/IInviteService.cs
// ============================================
using DevBoard.Application.Common;
using DevBoard.Application.Dtos;

namespace DevBoard.Application.Interfaces
{
    public interface IInviteService
    {
        Task<Result<SendInviteResponseDto>> SendInviteAsync(string invitedEmail);
        Task<Result<AcceptInviteResponseDto>> AcceptInviteAsync(AcceptInviteDto dto);
    }

    public record SendInviteResponseDto(string Message, string InviteToken);

    public record AcceptInviteResponseDto(
        string Message,
        string Token,
        Guid TenantId
    );
}