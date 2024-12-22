using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.JobItems.GetJobItemNotCompleted;

internal sealed class GetJobItemNotCompletedQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    : IQueryHandler<GetJobItemNotCompletedQuery, JobItemResponse>
{
    public async Task<Result<JobItemResponse>> Handle(GetJobItemNotCompletedQuery request, CancellationToken cancellationToken)
    {

        var operatorId = currentUserService.OperatorId;
        var operatorTerminalId = currentUserService.OperatorTerminalId;

        var lastAssignedJob = await dbContext.Jobs
            .AsNoTracking()
            .Where(x =>
                x.AssignedOperatorId == operatorId &&
                x.CompletionType != (byte)JobCompletitionType.SuccessfullyCompleted &&
                x.JobsInProgress.Any(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.OperatorTerminalId == operatorTerminalId))
            .Include(x => x.JobsInProgress)
            .OrderByDescending(x => x.Id)
        .FirstOrDefaultAsync(cancellationToken);

        if (lastAssignedJob is null)
            return Result.Failure<JobItemResponse>(JobErrors.JobInProgressNotFound);

        var lastAssignedJobInProgress = lastAssignedJob.JobsInProgress
            .Where(x => x.CompletionType == (byte)JobCompletitionType.Initial && x.OperatorTerminalId == operatorTerminalId)
            .OrderByDescending(x => x.Id)
        .FirstOrDefault();

        if (lastAssignedJobInProgress is null)
            return Result.Failure<JobItemResponse>(JobErrors.JobInProgressNotFound);

        var result = await dbContext.JobItems
            .AsNoTracking()
            .Where(x => x.Id == request.JobItemId)
            .Select(x => new JobItemResponse
            {
                Id = x.Id,
                JobId = x.JobId,
                ItemDescription = x.ItemDescription,
                RequiredField1 = x.RequiredField1,
                RequiredField2 = x.RequiredField2,
                RequiredField3 = x.RequiredField3,
                JobInProgressId = x.JobInProgressId,
                ItemStatus = x.ItemStatus.HasValue ? (JobItemStatus)x.ItemStatus.Value : JobItemStatus.Unread
            })
        .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
            return Result.Failure<JobItemResponse>(JobErrors.JobItemNotFound);

        if (result.ItemStatus == JobItemStatus.ReadWithRequestedQuantity)
            return Result.Failure<JobItemResponse>(JobErrors.JobItemAlreadyReaded);

        if (result.JobId != lastAssignedJob.Id)
            return Result.Failure<JobItemResponse>(Error.Conflict("Conflict.Bug", "Conflict.Bug"));

        return result;
    }
}
