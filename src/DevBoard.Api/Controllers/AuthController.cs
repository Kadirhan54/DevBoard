using DevBoard.Application.Dtos;
using DevBoard.Application.Interfaces;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AuthController(ApplicationDbContext context,UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration,ITokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return BadRequest("User not Found!");
            }

            var loginResult = await _signInManager.PasswordSignInAsync(user, loginDto.Password, true, false);

            if (!loginResult.Succeeded)
            {
                return BadRequest("Invalid Credentials");
            }

            var token = _tokenService.CreateToken(user);

            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
        {
            var isUserNull = await _userManager.FindByEmailAsync(registerDto.Email);

            if (isUserNull != null)
            {
                return BadRequest("User already exists");
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                EnableNotifications = registerDto.EnableNotifications
            };

            IdentityResult identityResult = await _userManager.CreateAsync(user, registerDto.Password);
            if (!identityResult.Succeeded)
            {
                return BadRequest(identityResult.Errors);
            }

            IdentityResult addToRoleResult = await _userManager.AddToRoleAsync(user, Roles.Member);
            if (!addToRoleResult.Succeeded)
            {
                return BadRequest(addToRoleResult.Errors);
            }

            return Ok(user);
        }

    }
}
