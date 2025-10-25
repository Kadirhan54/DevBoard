namespace DevBoard.Application.Dtos
{
    public record LoginResultDto(
        string Token,
        string Username,
        string Role
    );
}
