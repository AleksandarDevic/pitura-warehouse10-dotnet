using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Login;

internal sealed class LoginCommandHandler(IApplicationDbContext dbContext, IJwtProvider jwtProvider, IDateTimeProvider dateTimeProvider) : ICommandHandler<LoginCommand, JwtResponse>
{
    public async Task<Result<JwtResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var terminal = await dbContext.Terminals.FirstOrDefaultAsync(x => x.Id == request.TerminalId && x.IsActive, cancellationToken);
        if (terminal is null)
            return Result.Failure<JwtResponse>(OperatorTerminalErrors.TerminalNotFound);

        // var operater = await dbContext.Operators.FirstOrDefaultAsync(x => x.Id == request.OperatorId && x.IsActive, cancellationToken);
        // if (operater is null)
        //     return Result.Failure<JwtResponse>(OperatorTerminalErrors.OperatorNotFound);

        // if (operater.Password != request.OperatorPassword)
        //     return Result.Failure<JwtResponse>(OperatorTerminalErrors.BadCredentials);

        var operater = await dbContext.Operators.FirstOrDefaultAsync(x => x.Password == request.OperatorPassword && x.IsActive, cancellationToken);
        if (operater is null)
            return Result.Failure<JwtResponse>(OperatorTerminalErrors.BadCredentials);

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

        var result = jwtProvider.Create(newOperatorTerminal);

        return result;
    }
}
