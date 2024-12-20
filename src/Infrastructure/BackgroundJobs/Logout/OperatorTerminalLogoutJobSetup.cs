using Microsoft.Extensions.Options;
using Quartz;

namespace Infrastructure.BackgroundJobs.Logout;

internal sealed class OperatorTerminalLogoutJobSetup : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        const string jobName = nameof(OperatorTerminalLogoutJob);

        options.AddJob<OperatorTerminalLogoutJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure => configure
                .ForJob(jobName)
                .WithCronSchedule("0 0 3 ? * *"));
    }
}