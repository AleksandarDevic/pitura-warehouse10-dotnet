using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Extensions;

namespace Application.JobItems.CompleteJobItem;

internal sealed class CompleteJobItemCommandHandler(
    IApplicationDbContext dbContext, ICurrentUserService currentUserService) : ICommandHandler<CompleteJobItemCommand>
{
    public async Task<Result> Handle(CompleteJobItemCommand request, CancellationToken cancellationToken)
    {
        var operatorId = currentUserService.OperatorId;
        var operatorTerminalId = currentUserService.OperatorTerminalId;

        var jobItem = await dbContext.JobItems
            .Include(x => x.Job)
            .Include(x => x.JobInProgress)
        .FirstOrDefaultAsync(x => x.Id == request.JobItemId, cancellationToken);

        if (jobItem is null)
            return Result.Failure<Result>(JobErrors.JobItemNotFound);

        if (jobItem.ItemStatus == (byte)JobItemStatus.ReadWithRequestedQuantity)
            return Result.Failure<Result>(JobErrors.JobItemAlreadyReaded);

        if (!IsJobItemEligibleForCompletion(jobItem, request))
            return Result.Failure<Result>(JobErrors.JobItemRequestedAndReadedValueNotMatch);

        if ((request.Status == JobItemStatus.ReadWithRequestedQuantity && jobItem.RequiredField3 is not null && !request.RequiredFieldRead3.IsApproximatelyEqual((double)jobItem.RequiredField3)) || request.RequiredFieldRead3 < 0)
            return Result.Failure<Result>(JobErrors.JobItemBadQuanity);

        var lastAssignedJobInProgress = await dbContext.Jobs
           .AsNoTracking()
           .Where(x =>
               x.AssignedOperatorId == operatorId &&
               x.CompletionType != (byte)JobCompletitionType.SuccessfullyCompleted &&
               x.JobsInProgress.Any(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.EndDateTime == null && jip.OperatorTerminalId == operatorTerminalId))
           .Include(x => x.JobsInProgress)
           .OrderByDescending(x => x.Id)
           .Select(x => new
           {
               Job = x,
               JobInProgress = x.JobsInProgress
                   .Where(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.EndDateTime == null && jip.OperatorTerminalId == operatorTerminalId)
                   .OrderByDescending(jip => jip.Id)
               .FirstOrDefault()
           })
       .FirstOrDefaultAsync(cancellationToken);

        if (lastAssignedJobInProgress is null || lastAssignedJobInProgress.JobInProgress is null)
            return Result.Failure<Result>(JobErrors.JobInProgressNotFound);

        if (lastAssignedJobInProgress.JobInProgress.Id != request.JobInProgressId)
            return Result.Failure<Result>(OperatorTerminalErrors.Forbidden);

        if (lastAssignedJobInProgress.JobInProgress.CompletionType != (byte)JobCompletitionType.Initial)
            return Result.Failure<Result>(JobErrors.JobInProgressAlreadyCompleted);

        if (lastAssignedJobInProgress.Job.CompletionType == (byte)JobCompletitionType.SuccessfullyCompleted)
            return Result.Failure<Result>(JobErrors.AlreadyCompleted);

        jobItem.JobInProgressId = lastAssignedJobInProgress.JobInProgress.Id;
        jobItem.ReadedField3 = request.RequiredFieldRead3;
        jobItem.ItemStatus = (byte)request.Status;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool IsJobItemEligibleForCompletion(JobItem jobItem, CompleteJobItemCommand request)
    {
        return
            request.RequiredFieldRead1 == jobItem.RequiredField1 &&
            request.RequiredFieldRead2 == jobItem.RequiredField2 &&
            jobItem.RequiredField3 is not null && request.RequiredFieldRead3.IsApproximatelyEqual((double)jobItem.RequiredField3);
    }
}