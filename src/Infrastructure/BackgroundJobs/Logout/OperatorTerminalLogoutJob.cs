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

        var sessionsForLogout = await applicationDbContext.OperatorTerminalSessions.Where(x => x.LogoutDateTime == null).ToListAsync(context.CancellationToken);
        foreach (var session in sessionsForLogout)
        {
            session.LogoutDateTime = dateTimeProvider.UtcNow;
            logger.LogWarning("LogoutDateTime set for OperatorTerminalId: {OperatorTerminalId}", session.Id);
        }

        await applicationDbContext.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Logout process for OperatorTerminal finish");
    }
}