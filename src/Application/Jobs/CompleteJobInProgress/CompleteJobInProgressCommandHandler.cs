using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Jobs.CompleteJobInProgress;

internal sealed class CompleteJobInProgressCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CompleteJobInProgressCommand>
{
    public async Task<Result> Handle(CompleteJobInProgressCommand request, CancellationToken cancellationToken)
    {
        var operatorId = currentUserService.OperatorId;
        var operatorTerminalId = currentUserService.OperatorTerminalId;

        var jobInProgress = await dbContext.JobsInProgress
            .Where(x =>
                x.Id == request.JobInProgressId)
            .Include(x => x.Job)
                .ThenInclude(x => x.JobItems)
            .OrderByDescending(x => x.Id)
        .FirstOrDefaultAsync(cancellationToken);

        if (jobInProgress is null)
            return Result.Failure<Result>(JobErrors.JobInProgressNotFound);

        if (jobInProgress.CompletionType == (byte)JobCompletitionType.SuccessfullyCompleted)
            return Result.Failure<Result>(JobErrors.JobInProgressAlreadyCompleted);

        if (jobInProgress.OperatorTerminalId != operatorTerminalId || jobInProgress.Job.AssignedOperatorId != operatorId)
            return Result.Failure<Result>(OperatorTerminalErrors.Forbidden);

        jobInProgress.EndDateTime = dateTimeProvider.Now;

        if (request.CompletitionType == JobCompletitionType.SuccessfullyCompleted)
        {
            if (!AllJobItemsReadWithRequestedQuantity(jobInProgress.Job))
                return Result.Failure<Result>(JobErrors.JobItemsNotReadedWithRequestedQuantity);

            jobInProgress.Job.CompletedByOperatorName = operatorId.ToString();
        }
        else
        {
            jobInProgress.Job.AssignedOperatorId = null;
        }

        jobInProgress.CompletionType = (byte)request.CompletitionType;
        jobInProgress.Job.CompletionType = (byte)request.CompletitionType;

        jobInProgress.Note = request.Note;
        jobInProgress.Job.LastNote = request.Note;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool AllJobItemsReadWithRequestedQuantity(Job job) =>
         job.JobItems.All(x => x.ItemStatus == (byte)JobItemStatus.ReadWithRequestedQuantity);
}
