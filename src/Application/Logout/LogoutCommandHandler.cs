using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Logout;

internal sealed class LogoutCommandHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider, ILogger<LogoutCommandHandler> logger) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var operatorTerminal = await dbContext.OperatorTerminalSessions.FirstOrDefaultAsync(x => x.Id == request.OperatorTerminalId, cancellationToken);
        if (operatorTerminal is null)
            return Result.Failure<Result>(OperatorTerminalErrors.NotFound);

        if (operatorTerminal.LogoutDateTime is not null)
            return Result.Success();

        var lastAssignedJobInProgress = await dbContext.Jobs
            .AsNoTracking()
            .Where(x =>
                x.AssignedOperatorId == operatorTerminal.OperatorId &&
                x.CompletionType != (byte)JobCompletitionType.SuccessfullyCompleted &&
                x.JobsInProgress.Any(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.EndDateTime == null && jip.OperatorTerminalId == operatorTerminal.Id))
            .Include(x => x.JobsInProgress)
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                Job = x,
                JobInProgress = x.JobsInProgress
                    .Where(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.EndDateTime == null && jip.OperatorTerminalId == operatorTerminal.Id)
                    .OrderByDescending(jip => jip.Id)
                .FirstOrDefault()
            })
        .FirstOrDefaultAsync(cancellationToken);

        if (lastAssignedJobInProgress is not null && lastAssignedJobInProgress.Job is not null && lastAssignedJobInProgress.JobInProgress is not null)
            return Result.Failure<Result>(JobErrors.NotCompleted(lastAssignedJobInProgress.Job.Description ?? $"{lastAssignedJobInProgress.Job.Id}"));

        operatorTerminal.LogoutDateTime = dateTimeProvider.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
