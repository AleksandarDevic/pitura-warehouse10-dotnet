using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

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

        if ((request.Status == JobItemStatus.ReadWithRequestedQuantity && request.EnteredQuantity != jobItem.RequiredField3) || request.EnteredQuantity < 0)
            return Result.Failure<Result>(JobErrors.JobItemBadQuanity);

        var lastAssignedJobInProgress = await dbContext.JobsInProgress
            .AsNoTracking()
            .Where(x => x.CompletionType == (byte)JobCompletitionType.Initial)
            .Include(x => x.Job)
            .OrderByDescending(x => x.Id)
        .FirstOrDefaultAsync(cancellationToken);

        if (lastAssignedJobInProgress is null)
            return Result.Failure<Result>(JobErrors.JobInProgressNotFound);

        if (lastAssignedJobInProgress.CompletionType != (byte)JobCompletitionType.Initial)
            return Result.Failure<Result>(JobErrors.JobInProgressAlreadyCompleted);

        if (lastAssignedJobInProgress.Job.CompletionType == (byte)JobCompletitionType.SuccessfullyCompleted)
            return Result.Failure<Result>(JobErrors.AlreadyCompleted);

        if (lastAssignedJobInProgress.OperatorTerminalId != operatorTerminalId || lastAssignedJobInProgress.Job.AssignedOperatorId != operatorId)
            return Result.Failure<Result>(OperatorTerminalErrors.Unauthorized);

        jobItem.JobInProgressId = lastAssignedJobInProgress.Id;
        jobItem.ReadedField3 = request.EnteredQuantity;
        jobItem.ItemStatus = (byte)request.Status;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}