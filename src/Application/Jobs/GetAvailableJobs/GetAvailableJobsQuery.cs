using Application.Abstractions.Messaging;
using Domain.Models;
using SharedKernel;

namespace Application.Jobs.GetAvailableJobs;

public record GetAvailableJobsQuery : BasePagedRequest, IQuery<PagedList<JobResponse>>;
