// ============================================
// Application/Interfaces/IProjectService.cs
// ============================================
using DevBoard.Application.Common;
using DevBoard.Application.Dtos;

namespace DevBoard.Application.Interfaces
{
    public interface IProjectService
    {
        Task<Result<IEnumerable<ProjectDto>>> GetAllAsync();
        Task<Result<ProjectDto>> GetByIdAsync(Guid id);
        Task<Result<ProjectDto>> CreateAsync(CreateProjectDto dto);
        Task<Result<ProjectWithBoardsDto>> GetProjectWithBoardsAsync(Guid projectId);
        Task<Result> DeleteAsync(Guid id);
    }
}