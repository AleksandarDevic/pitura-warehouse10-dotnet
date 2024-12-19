using Application.Abstractions.Messaging;
using Domain.Models;

namespace Application.TokenRefresh;

public record TokenRefreshCommand(int OperatorTerminalId) : ICommand<JwtResponse>;

