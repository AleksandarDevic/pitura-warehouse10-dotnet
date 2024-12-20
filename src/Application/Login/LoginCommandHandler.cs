using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Login;

internal sealed class LoginCommandHandler(IApplicationDbContext dbContext, IJwtProvider jwtProvider, IDateTimeProvider dateTimeProvider) : ICommandHandler<LoginCommand, LoginCommandResult>
{
    public async Task<Result<LoginCommandResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var terminal = await dbContext.Terminals.FirstOrDefaultAsync(x => x.Id == request.TerminalId && x.IsActive, cancellationToken);
        if (terminal is null)
            return Result.Failure<LoginCommandResult>(OperatorTerminalErrors.TerminalNotFound);

        var operater = await dbContext.Operators.FirstOrDefaultAsync(x => x.Password == request.OperatorPassword && x.IsActive, cancellationToken);
        if (operater is null)
            return Result.Failure<LoginCommandResult>(OperatorTerminalErrors.BadCredentials);

        var terminalAlreadyInUse = await dbContext.OperatorTerminalSessions.AnyAsync(x => x.TerminalId == terminal.Id && x.LogoutDateTime == null, cancellationToken);
        if (terminalAlreadyInUse)
            return Result.Failure<LoginCommandResult>(OperatorTerminalErrors.TerminalAlreadyInUse);

        var operatorTerminalAlreadyLoggedIn = await dbContext.OperatorTerminalSessions.FirstOrDefaultAsync(x => x.OperatorId == operater.Id && x.LogoutDateTime == null, cancellationToken);

        int currentMaxId = await dbContext.OperatorTerminalSessions.MaxAsync(x => x.Id, cancellationToken);

        OperatorTerminal newOperatorTerminal = new()
        {
            Id = currentMaxId + 1,
            Operator = operater,
            Terminal = terminal,
            LoginDateTime = dateTimeProvider.UtcNow
        };

        await dbContext.OperatorTerminalSessions.AddAsync(newOperatorTerminal, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var jwtResponse = jwtProvider.Create(newOperatorTerminal);

        var result = new LoginCommandResult
        {
            AccessToken = jwtResponse.AccessToken,
            RefreshToken = jwtResponse.RefreshToken,
            OperatorAlreadyLoggedInToTerminalId = operatorTerminalAlreadyLoggedIn?.TerminalId
        };

        return result;
    }
}
