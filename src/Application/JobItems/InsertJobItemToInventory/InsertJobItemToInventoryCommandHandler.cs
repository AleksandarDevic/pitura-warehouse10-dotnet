using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.JobItems.InsertJobItemToInventory;

internal sealed class InsertJobItemToInventoryCommandHandler(
    IApplicationDbContext dbContext, ICurrentUserService currentUserService) : ICommandHandler<InsertJobItemToInventoryCommand, JobItemResponse>
{
    public async Task<Result<JobItemResponse>> Handle(InsertJobItemToInventoryCommand request, CancellationToken cancellationToken)
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

        if (jobInProgress is null || lastAssignedJobInProgress is null || lastAssignedJobInProgress.JobInProgress is null)
            return Result.Failure<JobItemResponse>(JobErrors.JobInProgressNotFound);

        if (lastAssignedJobInProgress.JobInProgress.Id != request.JobInProgressId)
            return Result.Failure<JobItemResponse>(OperatorTerminalErrors.Forbidden);

        if (jobInProgress.CompletionType == (byte)JobCompletitionType.SuccessfullyCompleted)
            return Result.Failure<JobItemResponse>(JobErrors.JobInProgressAlreadyCompleted);

        if (jobInProgress.Job.CompletionType == (byte)JobCompletitionType.SuccessfullyCompleted)
            return Result.Failure<JobItemResponse>(JobErrors.AlreadyCompleted);

        var jobItemDb = jobInProgress.Job.JobItems.FirstOrDefault(x => x.RequiredField1 == request.ReadedField1 && x.RequiredField2 == request.ReadedField2);
        if (jobItemDb is not null)
        {
            jobItemDb.RequiredField3 = request.ReadedField3;
            jobItemDb.ReadedField3 = request.ReadedField3;

            jobItemDb.ItemStatus = (byte)JobItemStatus.ReadWithRequestedQuantity;

            await dbContext.SaveChangesAsync(cancellationToken);

            var result = new JobItemResponse
            {
                Id = jobItemDb.Id,
                JobId = jobItemDb.JobId,
                ItemDescription = jobItemDb.ItemDescription,
                RequiredField1 = jobItemDb.RequiredField1,
                RequiredField2 = jobItemDb.RequiredField2,
                RequiredField3 = jobItemDb.RequiredField3,
                JobInProgressId = jobItemDb.JobInProgressId,
                ReadedField3 = jobItemDb.ReadedField3,
                ItemStatus = jobItemDb.ItemStatus.HasValue ? (JobItemStatus)jobItemDb.ItemStatus.Value : JobItemStatus.Unread
            };

            return result;
        }
        else
        {
            var newJobItem = new JobItem
            {
                JobId = jobInProgress.JobId,
                ItemDescription = string.Empty,
                RequiredField1 = request.ReadedField1,
                RequiredField2 = request.ReadedField2,
                RequiredField3 = request.ReadedField3,
                JobInProgressId = jobInProgress.Id,
                ReadedField3 = request.ReadedField3,
                ItemStatus = (byte)JobItemStatus.ReadWithRequestedQuantity
            };

            await dbContext.JobItems.AddAsync(newJobItem, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            var result = new JobItemResponse
            {
                Id = newJobItem.Id,
                JobId = newJobItem.JobId,
                ItemDescription = newJobItem.ItemDescription,
                RequiredField1 = newJobItem.RequiredField1,
                RequiredField2 = newJobItem.RequiredField2,
                RequiredField3 = newJobItem.RequiredField3,
                JobInProgressId = newJobItem.JobInProgressId,
                ReadedField3 = newJobItem.ReadedField3,
                ItemStatus = newJobItem.ItemStatus.HasValue ? (JobItemStatus)newJobItem.ItemStatus.Value : JobItemStatus.Unread
            };

            return result;
        }
    }
}
