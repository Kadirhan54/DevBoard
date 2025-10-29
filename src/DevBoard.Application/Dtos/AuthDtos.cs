using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Application.Dtos
{
    public record LoginDto(string Email, string Password);
    public record RegisterDto(string Email, string Password, bool EnableNotifications, string OrganizationName, string? Domain);

    public record LoginResultDto(string Email, string Token, Guid TenantId);
    public record RegisterResultDto(string Email, string Token, Guid TenantId, bool EnableNotifications, string OrganizationName, string? Domain);


}
