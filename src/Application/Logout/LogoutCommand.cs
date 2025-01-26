using Application.Abstractions.Messaging;

namespace Application.Logout;

public record LogoutCommand(int OperatorTerminalId, bool CheckLastAssignedJobInProgress) : ICommand;
