using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Jobs.ChooseJob;

internal sealed class ChooseJobCommandHandler(
    IApplicationDbContext dbContext, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ChooseJobCommand, JobInProgressResponse>
{
    public async Task<Result<JobInProgressResponse>> Handle(ChooseJobCommand request, CancellationToken cancellationToken)
    {
        var job = await dbContext.Jobs
            .Where(x => x.Id == request.JobId)
        .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
            return Result.Failure<JobInProgressResponse>(JobErrors.NotFound);

        if (job.CompletionType == (byte)JobCompletitionType.SuccessfullyCompleted)
            return Result.Failure<JobInProgressResponse>(JobErrors.AlreadyCompleted);

        var operatorTerminalId = currentUserService.OperatorTerminalId;
        var operatorId = currentUserService.OperatorId;

        if (job.AssignedOperatorId is not null && job.AssignedOperatorId != operatorId)
            return Result.Failure<JobInProgressResponse>(JobErrors.AlreadyAssigned);

        var lastAssignedJobInProgress = await dbContext.Jobs
            .AsNoTracking()
            .Where(x =>
                x.Id == request.JobId &&
                x.AssignedOperatorId == operatorId &&
                x.CompletionType != (byte)JobCompletitionType.SuccessfullyCompleted &&
                // x.JobsInProgress.Any(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.OperatorTerminalId == operatorTerminalId))
                x.JobsInProgress.Any(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.EndDateTime == null && jip.OperatorTerminalId == operatorTerminalId))
            .Include(x => x.JobsInProgress)
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                Job = x,
                JobInProgress = x.JobsInProgress
                    // .Where(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.OperatorTerminalId == operatorTerminalId)
                    .Where(jip => jip.CompletionType == (byte)JobCompletitionType.Initial && jip.EndDateTime == null && jip.OperatorTerminalId == operatorTerminalId)
                    .OrderByDescending(jip => jip.Id)
                .FirstOrDefault()
            })
        .FirstOrDefaultAsync(cancellationToken);

        if (lastAssignedJobInProgress is not null && lastAssignedJobInProgress.JobInProgress is not null)
            return Result.Failure<JobInProgressResponse>(JobErrors.NotCompleted(lastAssignedJobInProgress.Job.Description ?? $"{lastAssignedJobInProgress.Job.Id}"));

        job.TakenOverByOperatorName ??= operatorId.ToString();

        job.AssignedOperatorId = operatorId;

        int currentMaxId = await dbContext.JobsInProgress.MaxAsync(x => x.Id, cancellationToken);

        var newJobInProgress = new JobInProgress
        {
            Id = currentMaxId + 1,
            JobId = job.Id,
            OperatorTerminalId = operatorTerminalId,
            StartDateTime = dateTimeProvider.Now,
            EndDateTime = null,
            CompletionType = (byte)JobCompletitionType.Initial,
            Note = null
        };

        await dbContext.JobsInProgress.AddAsync(newJobInProgress, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var jobInProgressResponse = new JobInProgressResponse
        {
            Id = newJobInProgress.Id,
            JobId = newJobInProgress.JobId,
            Job = new JobResponse
            {
                Id = job.Id,
                Description = job.Description,
                AssignedOperatorId = job.AssignedOperatorId,
                Type = (JobType)job.Type,
                CreationDateTime = job.CreationDateTime,
                DueDateTime = job.DueDateTime,
                CompletionType = (JobCompletitionType)job.CompletionType,
                LastNote = job.LastNote,
                InventoryId = job.InventoryId,
                IsVerified = job.IsVerified,
                Client = job.Client
            },
            OperatorTerminalId = newJobInProgress.OperatorTerminalId,
            StartDateTime = newJobInProgress.StartDateTime,
            EndDateTime = newJobInProgress.EndDateTime,
            CompletionType = (JobCompletitionType)newJobInProgress.CompletionType,
            Note = newJobInProgress.Note
        };

        return jobInProgressResponse;
    }
}
