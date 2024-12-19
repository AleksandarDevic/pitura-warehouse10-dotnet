using SharedKernel;

namespace Domain.Entities;

public static class OperatorTerminalErrors
{
    public static readonly Error Unauthorized = Error.Unauthorized(
        "OperatorTerminal.Unauthorized",
        "OperatorTerminal not authorized.");

    public static readonly Error NotFound = Error.NotFound(
        "OperatorTerminal.NotFound",
        "OperatorTerminal not found.");

    public static readonly Error TerminalNotFound = Error.NotFound(
        "OperatorTerminal.TerminalNotFound",
        "Terminal not found.");

    public static readonly Error OperatorNotFound = Error.NotFound(
        "OperatorTerminal.OperatorNotFound",
        "Operator not found.");

    public static readonly Error BadCredentials = Error.Unauthorized(
        "OperatorTerminal.BadCredentials",
        "Bad credentials.");
}
