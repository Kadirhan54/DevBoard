// ============================================================================
// FILE: Services/Tasks/DevBoard.Services.Tasks.Infrastructure/HttpClients/ProjectServiceClient.cs
// ============================================================================
using DevBoard.Shared.Common;
using DevBoard.Shared.Common.HttpClients;
using DevBoard.Shared.Contracts.Projects;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace DevBoard.Services.Tasks.Infrastructure.HttpClients;

public class ProjectServiceClient : IProjectServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProjectServiceClient> _logger;

    public ProjectServiceClient(HttpClient httpClient, ILogger<ProjectServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<BoardDto>> GetBoardByIdAsync(Guid boardId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/boards/{boardId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return Result<BoardDto>.Failure($"Board {boardId} not found");

                return Result<BoardDto>.Failure($"Project service returned {response.StatusCode}");
            }

            var board = await response.Content.ReadFromJsonAsync<BoardDto>();
            if (board == null)
                return Result<BoardDto>.Failure("Failed to deserialize board");

            return Result<BoardDto>.Success(board);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Project Service for board {BoardId}", boardId);
            return Result<BoardDto>.Failure("Failed to communicate with Project Service");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating board {BoardId}", boardId);
            return Result<BoardDto>.Failure("Unexpected error validating board");
        }
    }

    public async Task<Result<bool>> ValidateBoardExistsAsync(Guid boardId)
    {
        try
        {
            // HEAD request is more efficient for existence check
            var request = new HttpRequestMessage(HttpMethod.Head, $"/api/v1/boards/{boardId}");
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return Result<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Result<bool>.Success(false);

            return Result<bool>.Failure($"Project service returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating board {BoardId} exists", boardId);
            return Result<bool>.Failure("Failed to validate board existence");
        }
    }
}