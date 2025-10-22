
namespace DevBoard.Application.Dtos
{
    public record TenantDto(
        Guid Id,
        string Name,
        string? Domain,
        IEnumerable<SimpleUserDto> Users,
        IEnumerable<ProjectDto> Projects
    );


    public record SimpleUserDto (
        Guid Id,
        string Email,
        Guid TenantId
    );
}
