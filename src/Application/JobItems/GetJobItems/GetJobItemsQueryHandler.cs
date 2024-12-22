using System.Linq.Expressions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Extensions;
using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.JobItems.GetJobItems;

internal sealed class GetJobItemsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    : IQueryHandler<GetJobItemsQuery, PagedList<JobItemResponse>>
{
    public async Task<Result<PagedList<JobItemResponse>>> Handle(GetJobItemsQuery request, CancellationToken cancellationToken)
    {
        var job = await dbContext.Jobs.Where(x => x.Id == request.JobId).FirstOrDefaultAsync(cancellationToken);
        if (job is null)
            return Result.Failure<PagedList<JobItemResponse>>(JobErrors.NotFound);

        var operatorId = currentUserService.OperatorId;
        if (job.AssignedOperatorId != operatorId)
            return Result.Failure<PagedList<JobItemResponse>>(OperatorTerminalErrors.Forbidden);

        IQueryable<JobItem> query = dbContext.JobItems
            .AsNoTracking()
            .Where(x => x.JobId == request.JobId);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var lowerSearchTerm = request.SearchTerm.ToLower();
            query = query.Where(x => x.ItemDescription != null && EF.Functions.Like(x.ItemDescription.ToLower(), $"%{lowerSearchTerm}%"));
        }

        Expression<Func<JobItem, object?>> keySelector = request.OrderBy switch
        {
            "requiredField1" => jobItem => jobItem.RequiredField1,
            "requiredField2" => jobItem => jobItem.RequiredField2,
            _ => jobItem => jobItem.ItemDescription
        };

        query = request.IsDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        var jobItemResponsesQuery = query.Select(x => new JobItemResponse
        {
            Id = x.Id,
            JobId = x.JobId,
            ItemDescription = x.ItemDescription,
            RequiredField1 = x.RequiredField1,
            RequiredField2 = x.RequiredField2,
            RequiredField3 = x.RequiredField3,
            JobInProgressId = x.JobInProgressId,
            ItemStatus = x.ItemStatus.HasValue ? (JobItemStatus)x.ItemStatus.Value : JobItemStatus.Unread
        });

        var result = await jobItemResponsesQuery.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);

        return result;
    }
}
