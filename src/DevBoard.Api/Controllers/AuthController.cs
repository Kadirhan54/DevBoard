using DevBoard.Application.Dtos;
using DevBoard.Application.Interfaces;
using DevBoard.Domain.Entities;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Select(u => new SimpleUserDto(
                    Guid.Parse(u.Id),
                    u.Email,
                    u.TenantId
                )
           ).ToListAsync();
            return Ok(users);
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

            return Ok(new
            {
                Token = token,
                TenantId = user.TenantId
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null) return BadRequest("User already exists");

            // Find or create tenant
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Name == registerDto.OrganizationName);
            if (tenant == null)
            {
                tenant = new Tenant { Name = registerDto.OrganizationName, Domain = registerDto.Domain };
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                TenantId = tenant.Id,
                EnableNotifications = registerDto.EnableNotifications
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, Roles.Member);

            // Attach to tenant navigation property
            tenant.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.CreateToken(user);

            return Ok(new
            {
                Message = "Registration successful",
                Token = token,
                TenantId = tenant.Id,
                Organization = tenant.Name
            });
        }


    }
}
