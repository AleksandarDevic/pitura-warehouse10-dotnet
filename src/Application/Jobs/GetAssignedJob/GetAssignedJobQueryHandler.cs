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
        var operatorTerminalId = currentUserService.OperatorTerminalId;

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

        if (lastAssignedJobInProgress is null || lastAssignedJobInProgress.Job is null || lastAssignedJobInProgress.JobInProgress is null)
            return Result.Failure<JobInProgressResponse>(JobErrors.JobInProgressNotFound);

        var jobInProgressResponse = new JobInProgressResponse
        {
            Id = lastAssignedJobInProgress.JobInProgress.Id,
            JobId = lastAssignedJobInProgress.Job.Id,
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
            OperatorTerminalId = lastAssignedJobInProgress.JobInProgress.OperatorTerminalId,
            StartDateTime = lastAssignedJobInProgress.JobInProgress.StartDateTime,
            EndDateTime = lastAssignedJobInProgress.JobInProgress.EndDateTime,
            CompletionType = (JobCompletitionType)lastAssignedJobInProgress.JobInProgress.CompletionType,
            Note = lastAssignedJobInProgress.JobInProgress.Note
        };

        return jobInProgressResponse;
    }
}