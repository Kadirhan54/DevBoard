using DevBoard.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevBoard.Application.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(ApplicationUser user);
    }
}
