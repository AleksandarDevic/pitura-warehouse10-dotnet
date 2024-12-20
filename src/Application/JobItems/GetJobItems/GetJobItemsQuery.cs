using Application.Abstractions.Messaging;
using Domain.Models;
using SharedKernel;

namespace Application.JobItems.GetJobItems;

public record GetJobItemsQuery : BasePagedRequest, IQuery<PagedList<JobItemResponse>>
{
    public long JobId { get; init; }
};