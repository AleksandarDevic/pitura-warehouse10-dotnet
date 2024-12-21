using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Jobs.ChooseJob;

internal sealed class ChooseJobCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider) : ICommandHandler<ChooseJobCommand, JobInProgressResponse>
{
    public async Task<Result<JobInProgressResponse>> Handle(ChooseJobCommand request, CancellationToken cancellationToken)
    {
        var job = await dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == request.JobId, cancellationToken);
        if (job is null)
            return Result.Failure<JobInProgressResponse>(JobErrors.NotFound);

        if (job.AssignedOperatorId is not null)
            return Result.Failure<JobInProgressResponse>(JobErrors.AlreadyAssigned);

        var operatorTerminalId = currentUserService.OperatorTerminalId;
        var operatorId = currentUserService.OperatorId;

        if (job.TakenOverByOperatorName is null)
            job.TakenOverByOperatorName = operatorId.ToString();

        job.AssignedOperatorId = operatorId;

        int currentMaxId = await dbContext.JobsInProgress.MaxAsync(x => x.Id, cancellationToken);

        var newJobInProgress = new JobInProgress
        {
            Id = currentMaxId + 1,
            JobId = job.Id,
            OperatorTerminalId = operatorTerminalId,
            StartDateTime = dateTimeProvider.UtcNow,
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
