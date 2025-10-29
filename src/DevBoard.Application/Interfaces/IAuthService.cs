using DevBoard.Application.Common;
using DevBoard.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<IEnumerable<SimpleUserDto>>> GetUsersAsync();
        Task<Result<LoginResultDto>> LoginAsync(LoginDto loginDto);
        Task<Result<RegisterResultDto>> RegisterAsync(RegisterDto registerDto);
    }
}
