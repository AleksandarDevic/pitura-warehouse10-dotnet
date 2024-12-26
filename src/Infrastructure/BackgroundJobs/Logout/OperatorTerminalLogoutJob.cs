using Application.Admin.LogoutJob;
using MediatR;
using Quartz;

namespace Infrastructure.BackgroundJobs.Logout;

[DisallowConcurrentExecution]
internal sealed class OperatorTerminalLogoutJob(ISender sender) : IJob
{

    public async Task Execute(IJobExecutionContext context)
    {
        var password = "root.123";
        var command = new LogoutJobCommand(password);

        await sender.Send(command, context.CancellationToken);
    }
}