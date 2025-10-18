using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
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

        var query = dbContext.JobItems
            .AsNoTracking()
            .Where(x => x.JobId == request.JobId);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var lowerSearchTerm = request.SearchTerm.ToLower();
            query = query.Where(x =>
                !string.IsNullOrEmpty(x.ItemDescription) &&
                EF.Functions.Like(x.ItemDescription.ToLower(), $"%{lowerSearchTerm}%"));
        }

        query = ApplySorting(query);

        var jobItemResponsesQuery = query.Select(x => new JobItemResponse
        {
            Id = x.Id,
            JobId = x.JobId,
            ItemDescription = x.ItemDescription,
            RequiredField1 = x.RequiredField1,
            RequiredField2 = x.RequiredField2,
            RequiredField3 = x.RequiredField3,
            JobInProgressId = x.JobInProgressId,
            ReadedField3 = x.ReadedField3,
            ItemStatus = x.ItemStatus.HasValue ? (JobItemStatus)x.ItemStatus.Value : JobItemStatus.Unread
        });

        // var result = await jobItemResponsesQuery.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        // result.Items = result.Items.OrderBy(x => x.ItemStatus).ToList();
        var jobItemsAll = await jobItemResponsesQuery.ToListAsync(cancellationToken);

        var unreadJobItems = jobItemsAll.Where(x => x.ItemStatus == JobItemStatus.Unread).ToList();
        var readJobItems = jobItemsAll.Where(x => x.ItemStatus != JobItemStatus.Unread).ToList();

        if (request.AnchorItemId.HasValue)
        {
            var anchorItemId = request.AnchorItemId.Value;
            var idx = jobItemsAll.FindIndex(x => x.Id == anchorItemId);
            if (idx >= 0)
            {
                var l1 = jobItemsAll.Skip(0).Take(idx + 1).ToList();
                var l2 = jobItemsAll.Skip(idx + 1).Take(jobItemsAll.Count - (idx + 1)).ToList();

                var l1Unread = l1.Where(x => x.ItemStatus == JobItemStatus.Unread).ToList();
                var l1Read = l1.Where(x => x.ItemStatus != JobItemStatus.Unread).ToList();

                var l2Unread = l2.Where(x => x.ItemStatus == JobItemStatus.Unread).ToList();
                var l2Read = l2.Where(x => x.ItemStatus != JobItemStatus.Unread).ToList();

                var rotatedUnread = l2Unread.Concat(l1Unread).ToList();
                var rotatedRead = l1Read.Concat(l2Read).ToList();

                unreadJobItems = rotatedUnread;
                readJobItems = rotatedRead;
            }
        }

        jobItemsAll = [.. unreadJobItems, .. readJobItems];

        var jobItemsForResponse = jobItemsAll
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
        .ToList();

        var result = new PagedList<JobItemResponse>(jobItemsForResponse, request.PageNumber, request.PageSize, jobItemsAll.Count);

        return result;
    }

    private IQueryable<JobItem> ApplySorting(IQueryable<JobItem> query)
    {
        var finalQuery = query
            .OrderBy(jobItem =>
                jobItem.RequiredField1 == null ? 8 :
                jobItem.RequiredField1.Contains("MIX") ? 7 :
                jobItem.RequiredField1.StartsWith("6D") ? 1 :
                jobItem.RequiredField1.StartsWith("6Y") ? 2 :
                jobItem.RequiredField1.StartsWith("6S") ? 3 :
                jobItem.RequiredField1.StartsWith("6X") ? 4 :
                jobItem.RequiredField1.StartsWith("6L") ? 5 : 6)
            .ThenBy(jobItem => jobItem.RequiredField1); // Sort full value alphabetically

        return finalQuery.AsQueryable();
    }
}
