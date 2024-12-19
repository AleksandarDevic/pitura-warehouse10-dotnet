using Application.Abstractions.Messaging;
using Domain.Models;

namespace Application.Login;

public record LoginCommand(int TerminalId, string OperatorPassword) : ICommand<JwtResponse>;
