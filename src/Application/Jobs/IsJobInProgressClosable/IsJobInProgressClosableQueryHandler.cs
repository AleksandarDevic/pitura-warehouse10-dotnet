using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
namespace Application.Jobs.IsJobInProgressClosable;

internal sealed class IsJobInProgressClosableQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    : IQueryHandler<IsJobInProgressClosableQuery, IsJobInProgressClosableQueryResult>
{
    public async Task<Result<IsJobInProgressClosableQueryResult>> Handle(IsJobInProgressClosableQuery request, CancellationToken cancellationToken)
    {
        var operatorId = currentUserService.OperatorId;
        var operatorTerminalId = currentUserService.OperatorTerminalId;

        var jobInProgress = await dbContext.JobsInProgress
            .Where(x =>
                x.Id == request.JobInProgressId)
            .Include(x => x.Job)
                .ThenInclude(x => x.JobItems)
            .OrderByDescending(x => x.Id)
        .FirstOrDefaultAsync(cancellationToken);

        if (jobInProgress is null)
            return Result.Failure<IsJobInProgressClosableQueryResult>(JobErrors.JobInProgressNotFound);

        if (jobInProgress.CompletionType == (byte)JobCompletitionType.SuccessfullyCompleted)
            return Result.Failure<IsJobInProgressClosableQueryResult>(JobErrors.JobInProgressAlreadyCompleted);

        if (jobInProgress.OperatorTerminalId != operatorTerminalId || jobInProgress.Job.AssignedOperatorId != operatorId)
            return Result.Failure<IsJobInProgressClosableQueryResult>(OperatorTerminalErrors.Forbidden);

        var result = new IsJobInProgressClosableQueryResult
        {
            IsClosable = AllJobItemsReadWithRequestedQuantity(jobInProgress.Job),
            ItemsReadWithRequestedQuantity = CountJobItemsReadWithRequestedQuantity(jobInProgress.Job),
            TotalItems = jobInProgress.Job.JobItems.Count
        };

        return result;
    }

    private static bool AllJobItemsReadWithRequestedQuantity(Job job) =>
         job.JobItems.All(x => x.ItemStatus == (byte)JobItemStatus.ReadWithRequestedQuantity);

    private static int CountJobItemsReadWithRequestedQuantity(Job job) =>
         job.JobItems.Count(x => x.ItemStatus == (byte)JobItemStatus.ReadWithRequestedQuantity);
}
