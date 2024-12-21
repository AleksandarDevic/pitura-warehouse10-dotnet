using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Jobs.CompleteJobInProgress;

internal sealed class CompleteJobInProgressCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider) : ICommandHandler<CompleteJobInProgressCommand>
{
    public async Task<Result> Handle(CompleteJobInProgressCommand request, CancellationToken cancellationToken)
    {
        var operatorId = currentUserService.OperatorId;
        var operatorTerminalId = currentUserService.OperatorTerminalId;

        var jobInProgress = await dbContext.JobsInProgress
            .Where(x =>
                x.OperatorTerminalId == operatorTerminalId &&
                x.Job.AssignedOperatorId == operatorId &&
                x.CompletionType != (byte)JobCompletitionType.SuccessfullyCompleted)
            .Include(x => x.Job)
            .OrderByDescending(x => x.Id)
        .FirstOrDefaultAsync(cancellationToken);

        if (jobInProgress is null)
            return Result.Failure<Result>(JobErrors.JobInProgressNotFound);

        jobInProgress.EndDateTime = dateTimeProvider.UtcNow;
        jobInProgress.Note = request.Note;

        if (request.CompletitionType == JobCompletitionType.SuccessfullyCompleted)
        {
            // TODO: Add constraint for JobItems -> all must have status completed
            jobInProgress.Job.CompletedByOperatorName = operatorId.ToString();
            jobInProgress.Job.LastNote = request.Note;
        }
        else
        {
            jobInProgress.Job.AssignedOperatorId = null;
        }

        jobInProgress.CompletionType = (byte)request.CompletitionType;
        jobInProgress.Job.CompletionType = (byte)request.CompletitionType;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
