
// ============================================================================
// FILE: Services/Notifications/DevBoard.Services.Notifications.Infrastructure/HttpClients/IdentityServiceClient.cs
// ============================================================================
using DevBoard.Shared.Common;
using DevBoard.Shared.Common.HttpClients;
using DevBoard.Shared.Contracts.Users;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace DevBoard.Services.Notifications.Infrastructure.HttpClients;

public class IdentityServiceClient : IIdentityServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityServiceClient> _logger;

    public IdentityServiceClient(HttpClient httpClient, ILogger<IdentityServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return Result<UserDto>.Failure($"User {userId} not found");

                return Result<UserDto>.Failure($"Identity service returned {response.StatusCode}");
            }

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            if (user == null)
                return Result<UserDto>.Failure("Failed to deserialize user");

            return Result<UserDto>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return Result<UserDto>.Failure("Failed to get user from Identity Service");
        }
    }

    public async Task<Result<TenantDto>> GetTenantByIdAsync(Guid tenantId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/tenants/{tenantId}");

            if (!response.IsSuccessStatusCode)
                return Result<TenantDto>.Failure($"Tenant {tenantId} not found");

            var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            if (tenant == null)
                return Result<TenantDto>.Failure("Failed to deserialize tenant");

            return Result<TenantDto>.Success(tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant {TenantId}", tenantId);
            return Result<TenantDto>.Failure("Failed to get tenant");
        }
    }

    public async Task<Result<bool>> ValidateTenantExistsAsync(Guid tenantId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Head, $"/api/v1/tenants/{tenantId}");
            var response = await _httpClient.SendAsync(request);

            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tenant {TenantId}", tenantId);
            return Result<bool>.Failure("Failed to validate tenant");
        }
    }
}