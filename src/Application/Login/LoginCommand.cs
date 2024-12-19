using Application.Abstractions.Messaging;
using Domain.Models;

namespace Application.Login;

public record LoginCommand(int TerminalId, int OperatorId, string OperatorPassword) : ICommand<JwtResponse>;
