using System.Linq.Expressions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Extensions;
using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Jobs.GetAvailableJobs;

internal sealed class GetAvailableJobsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetAvailableJobsQuery, PagedList<JobResponse>>
{
    public async Task<Result<PagedList<JobResponse>>> Handle(GetAvailableJobsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Job> query = dbContext.Jobs
            .AsNoTracking()
            .Where(x => x.AssignedOperatorId == null);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var lowerSearchTerm = request.SearchTerm.ToLower();
            query = query.Where(x => x.Description != null && EF.Functions.Like(x.Description.ToLower(), $"%{lowerSearchTerm}%"));
        }


        Expression<Func<Job, object?>> keySelector = request.OrderBy switch
        {
            "description" => job => job.Description,
            "dueDateTime" => job => job.DueDateTime,
            _ => job => job.CreationDateTime
        };

        query = request.IsDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);


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

        var result = await jobResponsesQuery.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);

        return result;
    }
}
