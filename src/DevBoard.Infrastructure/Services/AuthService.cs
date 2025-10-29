using DevBoard.Application.Common;
using DevBoard.Application.Dtos;
using DevBoard.Application.Interfaces;
using DevBoard.Domain.Entities;
using DevBoard.Domain.Identity;
using DevBoard.Infrastructure.Contexts.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DevBoard.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            IConfiguration configuration, 
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<SimpleUserDto>>> GetUsersAsync()
        {
            try
            {
                var users = await _context.Users.
                    AsNoTracking().
                    Select(u => new SimpleUserDto(
                    Guid.Parse(u.Id),
                    u.Email,
                    u.TenantId
                )).ToListAsync();

                return Result<IEnumerable<SimpleUserDto>>.Success(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return Result<IEnumerable<SimpleUserDto>>.Failure("Failed to retrieve users");
            }
        }

        public async Task<Result<LoginResultDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);

                if (user == null)
                {
                    return Result<LoginResultDto>.Failure($"user with email {loginDto.Email} not found");

                }

                var loginResult = await _signInManager.PasswordSignInAsync(user, loginDto.Password, true, false);

                if (!loginResult.Succeeded)
                {
                    return Result<LoginResultDto>.Failure($"user password mismatch error with email {loginDto.Email}");

                }

                var token = await _tokenService.CreateTokenAsync(user);

                var result = new LoginResultDto(
                    loginDto.Email,
                    token,
                    user.TenantId
                );

                _logger.LogInformation("User {UserEmail} logged in", loginDto.Email);

                return Result<LoginResultDto>.Success(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user {UserEmail}", loginDto.Email);
                return Result<LoginResultDto>.Failure("Failed to log in");
            }
        }

        public async Task<Result<RegisterResultDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                
                if (existingUser != null)
                {
                    return Result<RegisterResultDto>.Failure($"user with email {registerDto.Email} already taken");
                }

                // Find or create tenant
                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Name == registerDto.OrganizationName);
                if (tenant == null)
                {
                    // TODO : TenantService to handle tenant creation logic
                    tenant = new Tenant { Name = registerDto.OrganizationName, Domain = registerDto.Domain };
                   
                    _context.Tenants.Add(tenant);
                    await _context.SaveChangesAsync();

                    // TODO : Logger for tenant creation
                    // _logger.LogInformation("Creating new tenant {TenantName}", registerDto.OrganizationName);
                }

                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    TenantId = tenant.Id,
                    EnableNotifications = registerDto.EnableNotifications
                };

                var userCreationResult = await _userManager.CreateAsync(user, registerDto.Password);

                if (!userCreationResult.Succeeded)
                {
                    _logger.LogError("error registering user {UserEmail}", registerDto.Email);
                    return Result<RegisterResultDto>.Failure("Failed to register");
                }

                await _userManager.AddToRoleAsync(user, Roles.Member);
                
                _logger.LogInformation("Assigned role {UserRole} to user {UserEmail}", Roles.Member, registerDto.Email);

                // Attach to tenant navigation property
                tenant.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = await _tokenService.CreateTokenAsync(user);

                var result = new RegisterResultDto(
                    registerDto.Email,
                    token,
                    tenant.Id,
                    registerDto.EnableNotifications,
                    registerDto.OrganizationName,
                    registerDto.Domain
                );

                _logger.LogInformation("User {UserEmail} registered successfully", registerDto.Email);

                return Result<RegisterResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {UserEmail}", registerDto.Email);
                return Result<RegisterResultDto>.Failure("Failed to register");
            }
        }
    }
}
