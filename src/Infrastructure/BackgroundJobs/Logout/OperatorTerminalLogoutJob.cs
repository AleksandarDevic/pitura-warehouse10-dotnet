using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedKernel;

namespace Infrastructure.BackgroundJobs.Logout;

[DisallowConcurrentExecution]
internal sealed class OperatorTerminalLogoutJob(
    IApplicationDbContext applicationDbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<OperatorTerminalLogoutJob> logger) : IJob
{

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Logout process for OperatorTerminal start");

        var sessionsForLogout = await applicationDbContext.OperatorTerminalSessions
            .Where(x => x.LogoutDateTime == null)
            .Include(x => x.JobsInProgess)
        .ToListAsync(context.CancellationToken);

        if (sessionsForLogout.Count > 0)
        {
            var dateTimeNow = dateTimeProvider.UtcNow;

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

            await applicationDbContext.SaveChangesAsync(context.CancellationToken);
        }

        logger.LogInformation("Logout process for OperatorTerminal finish");
    }
}