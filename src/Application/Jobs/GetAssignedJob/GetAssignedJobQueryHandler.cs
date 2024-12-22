using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Jobs.GetAssignedJob;

internal sealed class GetAssignedJobQueryHandler(ICurrentUserService currentUserService, IApplicationDbContext dbContext)
    : IQueryHandler<GetAssignedJobQuery, JobInProgressResponse>
{
    public async Task<Result<JobInProgressResponse>> Handle(GetAssignedJobQuery request, CancellationToken cancellationToken)
    {
        var operatorId = currentUserService.OperatorId;

        var lastAssignedJob = await dbContext.Jobs
            .AsNoTracking()
            .Where(x =>
                x.AssignedOperatorId == operatorId &&
                x.ClosingType == (byte)JobCompletitionType.Initial)
            .Include(x => x.JobsInProgress)
            .Include(x => x.JobItems)
            .OrderByDescending(x => x.Id)
        .FirstOrDefaultAsync(cancellationToken);

        if (lastAssignedJob is null)
            return Result.Failure<JobInProgressResponse>(JobErrors.JobInProgressNotFound);

        var lastAssignedJobInProgress = lastAssignedJob.JobsInProgress
            .Where(x => x.CompletionType == (byte)JobCompletitionType.Initial)
            .OrderByDescending(x => x.Id)
        .FirstOrDefault();

        if (lastAssignedJobInProgress is null)
            return Result.Failure<JobInProgressResponse>(JobErrors.JobInProgressNotFound);

        var jobInProgressResponse = new JobInProgressResponse
        {
            Id = lastAssignedJobInProgress.Id,
            JobId = lastAssignedJobInProgress.JobId,
            Job = new JobResponse
            {
                Id = lastAssignedJobInProgress.Job.Id,
                Description = lastAssignedJobInProgress.Job.Description,
                AssignedOperatorId = lastAssignedJobInProgress.Job.AssignedOperatorId,
                Type = (JobType)lastAssignedJobInProgress.Job.Type,
                CreationDateTime = lastAssignedJobInProgress.Job.CreationDateTime,
                DueDateTime = lastAssignedJobInProgress.Job.DueDateTime,
                CompletionType = (JobCompletitionType)lastAssignedJobInProgress.Job.CompletionType,
                LastNote = lastAssignedJobInProgress.Job.LastNote,
                InventoryId = lastAssignedJobInProgress.Job.InventoryId,
                IsVerified = lastAssignedJobInProgress.Job.IsVerified,
                Client = lastAssignedJobInProgress.Job.Client
            },
            OperatorTerminalId = lastAssignedJobInProgress.OperatorTerminalId,
            StartDateTime = lastAssignedJobInProgress.StartDateTime,
            EndDateTime = lastAssignedJobInProgress.EndDateTime,
            CompletionType = (JobCompletitionType)lastAssignedJobInProgress.CompletionType,
            Note = lastAssignedJobInProgress.Note
        };

        return jobInProgressResponse;
    }
}