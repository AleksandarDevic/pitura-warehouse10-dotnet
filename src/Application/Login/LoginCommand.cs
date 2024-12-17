using Application.Abstractions.Messaging;

namespace Application.Login;

public record LoginCommand(int TerminalId, int OperatorId, string OperatorPassword) : ICommand<JwtResponse>;
