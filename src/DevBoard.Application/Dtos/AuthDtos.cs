using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Application.Dtos
{
    public record RegisterDto(string Email, string Password, bool EnableNotifications, string OrganizationName, string? Domain);
    public record LoginDto(string Email, string Password);
}
