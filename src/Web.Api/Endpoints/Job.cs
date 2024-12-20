using Application.Jobs.GetAssignedJob;
using Application.Jobs.GetAvailableJobs;
using MediatR;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints;

public class Job : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {

        app.MapGet("job/available", async (
            ISender sender,
            [AsParameters] GetAvailableJobsQuery query,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Job)
        .RequireAuthorization();

        app.MapGet("job/assigned", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAssignedJobQuery();
            var result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Job)
        .RequireAuthorization();
    }
}