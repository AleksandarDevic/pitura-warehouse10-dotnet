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

        var dateTimeNow = dateTimeProvider.Now;

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
            LoginDateTime = dateTimeNow
        };

        await dbContext.OperatorTerminalSessions.AddAsync(newOperatorTerminal, cancellationToken);

        var lastAssignedJobInProgress = await dbContext.Jobs
            .AsNoTracking()
            .Where(x =>
                x.AssignedOperatorId == operatorId &&
                x.CompletionType != (byte)JobCompletitionType.SuccessfullyCompleted &&
                x.JobsInProgress.Any(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.EndDateTime == null))
            .Include(x => x.JobsInProgress)
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                Job = x,
                JobInProgress = x.JobsInProgress
                    .Where(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.EndDateTime == null)
                    .OrderByDescending(jip => jip.Id)
                .FirstOrDefault()
            })
        .FirstOrDefaultAsync(cancellationToken);

        var assignedJob = lastAssignedJobInProgress?.Job;
        var assignedJobInProgress = lastAssignedJobInProgress?.JobInProgress;

        if (assignedJob is not null && assignedJobInProgress is not null)
        {
            logger.LogWarning("JobInProgressId: {JobInProgressId} realated to JobId: {JobId} and OperatorId: {OperatorId} didn't completed in past by OperatorTerminalId: {OperatorTerminalId}, but now ended with new login by NewOperatorTerminalId: {NewOperatorTerminalId}.", assignedJobInProgress.Id, assignedJob.Id, operatorId, assignedJobInProgress.OperatorTerminalId, newOperatorTerminal.Id);

            var assignedJobInProgressDb = await dbContext.JobsInProgress.FirstAsync(x => x.Id == assignedJobInProgress.Id, cancellationToken);
            assignedJobInProgressDb.EndDateTime = dateTimeNow;
            assignedJobInProgressDb.Note = $"Ended with new login by OperatorTerminalId: {newOperatorTerminal.Id}.";

            // Set logout time in OperatorTerminal and JobInProgress if not set.
            var operatorTerminalDb = await dbContext.OperatorTerminalSessions
                .Where(x =>
                    x.Id == assignedJobInProgressDb.OperatorTerminalId &&
                    x.LogoutDateTime == null)
            .FirstOrDefaultAsync(cancellationToken);
            if (operatorTerminalDb is not null)
                operatorTerminalDb.LogoutDateTime = dateTimeNow;

            int currentJobInProgressMaxId = await dbContext.JobsInProgress.MaxAsync(x => x.Id, cancellationToken);

            var newJobInProgress = new JobInProgress
            {
                Id = currentJobInProgressMaxId + 1,
                JobId = assignedJobInProgressDb.JobId,
                OperatorTerminalId = newOperatorTerminal.Id,
                StartDateTime = dateTimeNow,
                EndDateTime = null,
                CompletionType = (byte)JobCompletitionType.Initial,
                Note = null
            };

            await dbContext.JobsInProgress.AddAsync(newJobInProgress, cancellationToken);
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
