using Domain.Models;

namespace Application.Login;
public record LoginCommandResult
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public int? OperatorAlreadyLoggedInToTerminalId { get; set; }

}
