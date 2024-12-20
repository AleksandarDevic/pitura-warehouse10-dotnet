using Application.Abstractions.Messaging;

namespace Application.Login;

public record LoginCommand(int TerminalId, string OperatorPassword) : ICommand<LoginCommandResult>;
