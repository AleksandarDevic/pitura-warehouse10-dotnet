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
    : IQueryHandler<GetAssignedJobQuery, JobResponse>
{
    public async Task<Result<JobResponse>> Handle(GetAssignedJobQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Job> query = dbContext.Jobs
            .AsNoTracking()
            .Where(x =>
                x.AssignedOperatorId == currentUserService.OperatorId &&
                x.ClosingType != 1);

        var jobResponsesQuery = query.Select(x => new JobResponse
        {
            Id = x.Id,
            Description = x.Description,
            AssignedOperatorId = x.AssignedOperatorId,
            Type = (JobType)x.Type,
            CreationDateTime = x.CreationDateTime,
            DueDateTime = x.DueDateTime,
            CompletionType = (JobCompletitionType)x.CompletionType,
            LastNote = x.LastNote,
            InventoryId = x.InventoryId,
            IsVerified = x.IsVerified,
            Client = x.Client
        });

        var result = await jobResponsesQuery.FirstOrDefaultAsync(cancellationToken);

        return result;
    }
}