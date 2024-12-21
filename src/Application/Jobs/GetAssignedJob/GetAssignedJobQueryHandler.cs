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

        var result = await dbContext.JobsInProgress
            .AsNoTracking()
            .Where(x =>
                x.OperatorTerminalId == operatorTerminalId &&
                x.Job.AssignedOperatorId == operatorId &&
                x.EndDateTime == null &&
                x.CompletionType == (byte)JobCompletitionType.Initial)
            .Select(jip => new JobInProgressResponse
            {
                Id = jip.Id,
                JobId = jip.JobId,
                Job = new JobResponse
                {
                    Id = jip.Job.Id,
                    Description = jip.Job.Description,
                    AssignedOperatorId = jip.Job.AssignedOperatorId,
                    Type = (JobType)jip.Job.Type,
                    CreationDateTime = jip.Job.CreationDateTime,
                    DueDateTime = jip.Job.DueDateTime,
                    CompletionType = (JobCompletitionType)jip.Job.CompletionType,
                    LastNote = jip.Job.LastNote,
                    InventoryId = jip.Job.InventoryId,
                    IsVerified = jip.Job.IsVerified,
                    Client = jip.Job.Client
                },
                OperatorTerminalId = jip.OperatorTerminalId,
                StartDateTime = jip.StartDateTime,
                EndDateTime = jip.EndDateTime,
                CompletionType = (JobCompletitionType)jip.CompletionType,
                Note = jip.Note
            })
            .OrderByDescending(x => x.Id)
        .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
            return Result.Failure<JobInProgressResponse>(JobErrors.JobInProgressNotFound);

        return result;
    }
}