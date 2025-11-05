
// ============================================================================
// FILE: Services/Identity/DevBoard.Services.Identity.Api/Services/AuthService.cs
// ============================================================================
using DevBoard.Services.Identity.Core.Entities;
using DevBoard.Services.Identity.Infrastructure.Data;
using DevBoard.Shared.Common;
using DevBoard.Shared.Contracts.Auth;
using DevBoard.Shared.Contracts.Events;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevBoard.Services.Identity.Infrastructure.Services;

public class AuthService
{
    private readonly IdentityDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IdentityDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TokenService tokenService,
        IPublishEndpoint publishEndpoint,
        ILogger<AuthService> logger)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Result<AuthResponse>.Failure($"User with email {request.Email} already exists");

            // Find or create tenant
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Name == request.OrganizationName);
            if (tenant == null)
            {
                tenant = new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = request.OrganizationName,
                    Domain = request.Domain
                };
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                // Publish tenant created event
                await _publishEndpoint.Publish(new TenantCreatedEvent
                {
                    TenantId = tenant.Id,
                    Name = tenant.Name
                });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                TenantId = tenant.Id,
                EnableNotifications = request.EnableNotifications
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result<AuthResponse>.Failure(errors);
            }

            await _userManager.AddToRoleAsync(user, "Member");

            var token = await _tokenService.CreateTokenAsync(user);

            // Publish user registered event
            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                TenantId = tenant.Id,
                UserId = Guid.Parse(user.Id),
                Email = user.Email!
            });

            _logger.LogInformation("User {Email} registered successfully", request.Email);

            return Result<AuthResponse>.Success(new AuthResponse(
                request.Email,
                token,
                tenant.Id
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {Email}", request.Email);
            return Result<AuthResponse>.Failure("Failed to register user");
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result<AuthResponse>.Failure($"User with email {request.Email} not found");

            var loginResult = await _signInManager.PasswordSignInAsync(user, request.Password, true, false);
            if (!loginResult.Succeeded)
                return Result<AuthResponse>.Failure("Invalid password");

            var token = await _tokenService.CreateTokenAsync(user);

            _logger.LogInformation("User {Email} logged in", request.Email);

            return Result<AuthResponse>.Success(new AuthResponse(
                request.Email,
                token,
                user.TenantId
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in user {Email}", request.Email);
            return Result<AuthResponse>.Failure("Failed to log in");
        }
    }
}
