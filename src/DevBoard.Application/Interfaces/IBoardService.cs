// ============================================
// Application/Interfaces/IBoardService.cs
// ============================================
using DevBoard.Application.Common;
using DevBoard.Application.Dtos;

namespace DevBoard.Application.Interfaces
{
    public interface IBoardService
    {
        Task<Result<IEnumerable<SimpleBoardDto>>> GetAllAsync();
        Task<Result<SimpleBoardDto>> GetByIdAsync(Guid id);
        Task<Result<SimpleBoardDto>> CreateAsync(CreateBoardDto dto);
        Task<Result> DeleteAsync(Guid id);
    }
}