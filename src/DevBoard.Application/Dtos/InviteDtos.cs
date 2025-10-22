using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Application.Dtos
{
    public record AcceptInviteDto(
        string InviteToken,
        string Email,
        string Password
    );
}
