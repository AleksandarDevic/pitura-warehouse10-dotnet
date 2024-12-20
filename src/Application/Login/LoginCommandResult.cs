using Domain.Models;

namespace Application.Login;
public record LoginCommandResult
{
    public AccessToken AccessToken { get; init; } = null!;
    public RefreshToken RefreshToken { get; init; } = null!;
    public int? OperatorAlreadyLoggedInToTerminalId { get; set; }

}
