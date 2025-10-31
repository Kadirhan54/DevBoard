
// ============================================================================
// FILE: Shared/DevBoard.Shared.Common/HttpClients/IProjectServiceClient.cs
// ============================================================================
namespace DevBoard.Shared.Common.HttpClients;

using DevBoard.Shared.Contracts.Projects;

/// <summary>
/// Interface for Task Service to validate BoardIds exist
/// </summary>
public interface IProjectServiceClient
{
    Task<Result<BoardDto>> GetBoardByIdAsync(Guid boardId);
    Task<Result<bool>> ValidateBoardExistsAsync(Guid boardId);
}