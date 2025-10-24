namespace DevBoard.Application.Dtos
{
    public record TenantResultDto(
        Guid Id,
        string Name,
        string? Domain
    );
}
