using Application.Abstractions.Messaging;
using Domain.Models;

namespace Application.OperatorTerminals.GetOperatorTerminalDetails;

public record GetOperatorTerminalDetailsQuery : IQuery<OperatorTerminalResponse>;
