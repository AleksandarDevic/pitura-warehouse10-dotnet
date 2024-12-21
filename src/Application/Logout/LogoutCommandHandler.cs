using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Logout;

internal sealed class LogoutCommandHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var operatorTerminal = await dbContext.OperatorTerminalSessions.FirstOrDefaultAsync(x => x.Id == request.OperatorTerminalId, cancellationToken);
        if (operatorTerminal is null)
            return Result.Failure<Result>(OperatorTerminalErrors.NotFound);

        if (operatorTerminal.LogoutDateTime is not null)
            return Result.Success();

        var jobsInProgressForOperatorTerminal = await dbContext.JobsInProgress
            .Where(x =>
                x.OperatorTerminalId == operatorTerminal.Id &&
                x.CompletionType == (byte)JobCompletitionType.Initial)
            .Include(x => x.Job)
            .Include(x => x.OperatorTerminal)
        .ToListAsync(cancellationToken);

        foreach (var jobInProgress in jobsInProgressForOperatorTerminal)
        {
            jobInProgress.CompletionType = (byte)JobCompletitionType.Aborted;
            jobInProgress.Job.AssignedOperatorId = null;
            jobInProgress.Job.CompletionType = (byte)JobCompletitionType.Aborted;
        }

        operatorTerminal.LogoutDateTime = dateTimeProvider.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
