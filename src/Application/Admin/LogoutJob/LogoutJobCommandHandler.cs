using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Admin.LogoutJob;

internal sealed class LogoutJobCommandHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider, ILogger<LogoutJobCommandHandler> logger) : ICommandHandler<LogoutJobCommand>
{
    public async Task<Result> Handle(LogoutJobCommand request, CancellationToken cancellationToken)
    {
        if (request.Password != "root.123")
            return Result.Failure<Result>(Error.Unauthorized("LogoutJob.Unauthorized", "Bad credentials."));

        logger.LogInformation("Job for 'Logout process' start");

        var sessionsForLogout = await dbContext.OperatorTerminalSessions
            .Where(x => x.LogoutDateTime == null)
            .Include(x => x.JobsInProgess)
        .ToListAsync(cancellationToken);

        if (sessionsForLogout.Count > 0)
        {
            var dateTimeNow = dateTimeProvider.Now;

            foreach (var session in sessionsForLogout)
            {
                session.LogoutDateTime = dateTimeNow;
                logger.LogWarning("LogoutDateTime set by background-job for OperatorTerminalId: {OperatorTerminalId}.", session.Id);
                var jobsInProgressForSession = session.JobsInProgess.Where(x => x.EndDateTime == null).ToList();
                foreach (var jobInProgress in jobsInProgressForSession)
                {
                    jobInProgress.EndDateTime = dateTimeNow;
                    jobInProgress.Note = "Ended with background-job.";
                    logger.LogWarning("LogoutDateTime set by background-job for JobInProgressId: {JobInProgressId} and OperatorTerminalId: {OperatorTerminalId}.", jobInProgress.Id, jobInProgress.OperatorTerminalId);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Job for 'Logout process' finish");

        return Result.Success();
    }
}
