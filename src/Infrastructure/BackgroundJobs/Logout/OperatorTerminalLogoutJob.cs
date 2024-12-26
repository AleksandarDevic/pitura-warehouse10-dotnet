using Application.Admin.LogoutJob;
using MediatR;
using Microsoft.Extensions.Options;
using Quartz;

namespace Infrastructure.BackgroundJobs.Logout;

[DisallowConcurrentExecution]
internal sealed class OperatorTerminalLogoutJob(ISender sender, IOptions<AdminCredentialsOptions> options) : IJob
{
    private readonly AdminCredentialsOptions adminCredentials = options.Value;
    public async Task Execute(IJobExecutionContext context)
    {
        var password = adminCredentials.Secret;
        var command = new LogoutJobCommand(password);

        await sender.Send(command, context.CancellationToken);
    }
}