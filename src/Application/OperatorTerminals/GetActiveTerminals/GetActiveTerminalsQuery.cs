using Application.Abstractions.Messaging;
using Domain.Models;

namespace Application.OperatorTerminals.GetActiveTerminals;

public record GetActiveTerminalsQuery : IQuery<List<TerminalResponse>>;
