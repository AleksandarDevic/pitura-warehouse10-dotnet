namespace Application.Abstractions.Authentication;

public interface ICurrentUserService
{
    int OperatorTerminalId { get; }
    short OperatorId { get; }
    int TerminalId { get; }
}
