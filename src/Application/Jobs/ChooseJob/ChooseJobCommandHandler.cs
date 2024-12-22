using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Jobs.ChooseJob;

internal sealed class ChooseJobCommandHandler(
    IApplicationDbContext dbContext, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider, ILogger<ChooseJobCommandHandler> logger)
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

        // 1.Nema poslova u toku, tada mozes da izaberes posao! 
        // 2.Neko je uzeo da radi posao ima 1 posao u toku i tada ne moze da izabere posao!
        // 3.Neko se vise puta logova i izlogovao ali nista nije radio a izabrao je posao -> to znaci da ima vise poslova u toku sa 0, tu ne moze da izabere posao!
        // 4.Ako je poslovaUPocetku status !=0 tada moze da radi save. i tada radim novi posao u toku sa statusom =0 i tada je taj posao ponovo zakljucan i menjam glavni status PosloviUPocetku na 0.
        // 0.Assign vrati poslednji duplikat iz poslova u toku

        var jobsInProgressNotCompletedForOperator = await dbContext.JobsInProgress
             .Where(x =>
                x.OperatorTerminal.OperatorId == operatorId &&
                x.CompletionType == (byte)JobCompletitionType.Initial &&
                x.Job.AssignedOperatorId == operatorId &&
                x.Job.CompletionType != (byte)JobCompletitionType.SuccessfullyCompleted)
             .OrderBy(x => x.Id)
         .ToListAsync(cancellationToken);

        if (jobsInProgressNotCompletedForOperator.Count > 0)
        {
            logger.LogInformation(
                "For OperatorId: {OperatorId} there is: {Count} JobInProgress with status: {Status}",
                 operatorId, jobsInProgressNotCompletedForOperator.Count, (byte)JobCompletitionType.Initial);

            return Result.Failure<JobInProgressResponse>(Error.Failure("ContactAdministrator", "ContactAdministrator"));
        }

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
