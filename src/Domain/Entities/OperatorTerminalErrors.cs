using SharedKernel;

namespace Domain.Entities;

public static class OperatorTerminalErrors
{
    public static readonly Error Unauthorized = Error.Unauthorized(
        "OperatorTerminal.Unauthorized",
        "Nije izvršena prijava na sistem.");

    public static readonly Error Forbidden = Error.Unauthorized(
        "OperatorTerminal.Forbidden",
        "Nemate dozvolu za ovaj resurs.");

    public static readonly Error NotFound = Error.NotFound(
        "OperatorTerminal.NotFound",
        "Operator i Terminal nisu pronađeni.");

    public static readonly Error TerminalNotFound = Error.NotFound(
        "OperatorTerminal.TerminalNotFound",
        "Terminal nije pronađen.");

    public static readonly Error TerminalAlreadyInUse = Error.Conflict(
        "OperatorTerminal.TerminalAlreadyInUse",
        "Terminal je već u upotrebi.");

    public static readonly Error OperatorNotFound = Error.NotFound(
        "OperatorTerminal.OperatorNotFound",
        "Operator nije pronađen.");

    public static readonly Error BadCredentials = Error.Unauthorized(
        "OperatorTerminal.BadCredentials",
        "Uneta lozinka nije validna za nijednog operatora.");
}
