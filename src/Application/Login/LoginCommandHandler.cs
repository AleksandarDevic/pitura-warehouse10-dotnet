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

        var operater = await dbContext.Operators.FirstOrDefaultAsync(x => x.Password == request.OperatorPassword && x.IsActive, cancellationToken);
        if (operater is null)
            return Result.Failure<LoginCommandResult>(OperatorTerminalErrors.BadCredentials);

        var terminalAlreadyInUse = await dbContext.OperatorTerminalSessions.AnyAsync(x => x.TerminalId == terminal.Id && x.LogoutDateTime == null, cancellationToken);
        if (terminalAlreadyInUse)
            return Result.Failure<LoginCommandResult>(OperatorTerminalErrors.TerminalAlreadyInUse);

        var operatorAlreadyLoggedIn = await dbContext.OperatorTerminalSessions.FirstOrDefaultAsync(x => x.OperatorId == operater.Id && x.LogoutDateTime == null, cancellationToken);

        int currentOperatorTerminalMaxId = await dbContext.OperatorTerminalSessions.MaxAsync(x => x.Id, cancellationToken);

        OperatorTerminal newOperatorTerminal = new()
        {
            Id = currentOperatorTerminalMaxId + 1,
            Operator = operater,
            Terminal = terminal,
            LoginDateTime = dateTimeProvider.UtcNow
        };

        await dbContext.OperatorTerminalSessions.AddAsync(newOperatorTerminal, cancellationToken);

        var jobsInProgressNotCompletedForOperator = await dbContext.JobsInProgress
            .Where(x =>
                x.OperatorTerminal.OperatorId == operater.Id &&
                x.OperatorTerminal.LogoutDateTime == null &&
                x.Job.AssignedOperatorId == operater.Id &&
                x.EndDateTime == null &&
                x.CompletionType == (byte)JobCompletitionType.Initial)
            .OrderBy(x => x.Id)
        .ToListAsync(cancellationToken);

        if (jobsInProgressNotCompletedForOperator.Count > 1)
        {
            logger.LogInformation(
                "For OperatorId: {OperatorId} there is: {Count} JobInProgress with status: {Status}",
                 operater.Id, jobsInProgressNotCompletedForOperator.Count, (byte)JobCompletitionType.Initial);

            return Result.Failure<LoginCommandResult>(Error.Failure("ContactAdministrator", "ContactAdministrator"));
        }

        if (jobsInProgressNotCompletedForOperator.Count == 1)
        {
            var existingJobInProgressNotCompletedForOperator = jobsInProgressNotCompletedForOperator[0];

            int currentJobInProgressMaxId = await dbContext.JobsInProgress.MaxAsync(x => x.Id, cancellationToken);

            var newJobInProgress = new JobInProgress
            {
                Id = currentJobInProgressMaxId + 1,
                JobId = existingJobInProgressNotCompletedForOperator.JobId,
                OperatorTerminalId = newOperatorTerminal.Id,
                StartDateTime = dateTimeProvider.UtcNow,
                EndDateTime = null,
                CompletionType = (byte)JobCompletitionType.Initial,
                Note = null
            };

            await dbContext.JobsInProgress.AddAsync(newJobInProgress, cancellationToken);

            // Check with Gacic
            existingJobInProgressNotCompletedForOperator.EndDateTime = dateTimeProvider.UtcNow;
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
