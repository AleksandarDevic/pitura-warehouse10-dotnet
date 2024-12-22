using Application.Abstractions.Messaging;
using Domain.Models;
using SharedKernel;

namespace Application.JobItems.GetJobItemNotCompleted;

public record GetJobItemNotCompletedQuery(long JobItemId) : IQuery<JobItemResponse>;
