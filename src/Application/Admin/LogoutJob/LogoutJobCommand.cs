using Application.Abstractions.Messaging;

namespace Application.Admin.LogoutJob;
public record LogoutJobCommand(string Password) : ICommand;
