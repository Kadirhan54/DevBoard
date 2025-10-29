// ============================================
// Application/Interfaces/ITaskItemService.cs
// ============================================
using DevBoard.Application.Common;
using DevBoard.Application.Dtos;

namespace DevBoard.Application.Interfaces
{
    public interface ITaskItemService
    {
        Task<Result<IEnumerable<TaskItemResponseDto>>> GetAllAsync();
        Task<Result<TaskItemResponseDto>> GetByIdAsync(Guid id);
        Task<Result<TaskItemResponseDto>> CreateAsync(CreateTaskItemDto dto);
        Task<Result> DeleteAsync(Guid id);
    }
}