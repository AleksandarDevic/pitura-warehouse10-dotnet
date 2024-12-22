using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Login;

internal sealed class LoginCommandHandler(
    IApplicationDbContext dbContext, IJwtProvider jwtProvider, IDateTimeProvider dateTimeProvider, ILogger<LoginCommandHandler> logger)
    : ICommandHandler<LoginCommand, LoginCommandResult>
{
    public async Task<Result<LoginCommandResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var terminal = await dbContext.Terminals.FirstOrDefaultAsync(x => x.Id == request.TerminalId && x.IsActive, cancellationToken);
        if (terminal is null)
            return Result.Failure<LoginCommandResult>(OperatorTerminalErrors.TerminalNotFound);

        var operatorDb = await dbContext.Operators.FirstOrDefaultAsync(x => x.Password == request.OperatorPassword && x.IsActive, cancellationToken);
        if (operatorDb is null)
            return Result.Failure<LoginCommandResult>(OperatorTerminalErrors.BadCredentials);

        var operatorId = operatorDb.Id;
        var terminalId = terminal.Id;

        var terminalAlreadyInUse = await dbContext.OperatorTerminalSessions.AnyAsync(x => x.TerminalId == terminal.Id && x.LogoutDateTime == null, cancellationToken);
        if (terminalAlreadyInUse)
            return Result.Failure<LoginCommandResult>(OperatorTerminalErrors.TerminalAlreadyInUse);

        var operatorAlreadyLoggedIn = await dbContext.OperatorTerminalSessions.FirstOrDefaultAsync(x => x.OperatorId == operatorDb.Id && x.LogoutDateTime == null, cancellationToken);

        int currentOperatorTerminalMaxId = await dbContext.OperatorTerminalSessions.MaxAsync(x => x.Id, cancellationToken);

        OperatorTerminal newOperatorTerminal = new()
        {
            Id = currentOperatorTerminalMaxId + 1,
            OperatorId = operatorId,
            TerminalId = terminalId,
            LoginDateTime = dateTimeProvider.UtcNow
        };

        await dbContext.OperatorTerminalSessions.AddAsync(newOperatorTerminal, cancellationToken);

        var lastAssignedJob = await dbContext.Jobs
            .AsNoTracking()
            .Where(x =>
                x.AssignedOperatorId == operatorId &&
                x.CompletionType != (byte)JobCompletitionType.SuccessfullyCompleted)
            .Include(x => x.JobsInProgress)
            .OrderByDescending(x => x.Id)
        .FirstOrDefaultAsync(cancellationToken);

        if (lastAssignedJob is not null)
        {
            var lastAssignedJobInProgress = lastAssignedJob.JobsInProgress
                .Where(x => x.CompletionType == (byte)JobCompletitionType.Initial)
                .OrderByDescending(x => x.Id)
            .FirstOrDefault();

            if (lastAssignedJobInProgress is not null)
            {
                int currentJobInProgressMaxId = await dbContext.JobsInProgress.MaxAsync(x => x.Id, cancellationToken);

                var newJobInProgress = new JobInProgress
                {
                    Id = currentJobInProgressMaxId + 1,
                    JobId = lastAssignedJobInProgress.JobId,
                    OperatorTerminalId = newOperatorTerminal.Id,
                    StartDateTime = dateTimeProvider.UtcNow,
                    EndDateTime = null,
                    CompletionType = (byte)JobCompletitionType.Initial,
                    Note = null
                };

                await dbContext.JobsInProgress.AddAsync(newJobInProgress, cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var jwtResponse = jwtProvider.Create(newOperatorTerminal);

        var result = new LoginCommandResult
        {
            AccessToken = jwtResponse.AccessToken,
            RefreshToken = jwtResponse.RefreshToken,
            OperatorAlreadyLoggedInToTerminalId = operatorAlreadyLoggedIn?.TerminalId
        };

        return result;
    }
}
