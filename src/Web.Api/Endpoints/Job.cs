using Application.JobItems.CompleteJobItem;
using Application.JobItems.GetJobItems;
using Application.Jobs.ChooseJob;
using Application.Jobs.CompleteJobInProgress;
using Application.Jobs.GetAssignedJob;
using Application.Jobs.GetAvailableJobs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
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

        app.MapPost("job/{jobId}/choose", async (
            ISender sender,
            [FromRoute] long jobId,
            CancellationToken cancellationToken) =>
        {
            var command = new ChooseJobCommand(jobId);
            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Job)
        .RequireAuthorization();

        app.MapPost("job-in-progress/{jobInProgressId}/complete", async (
            ISender sender,
            [FromRoute] long jobInProgressId,
            [FromBody] CompleteJobInProgressRequest request,
            CancellationToken cancellationToken) =>
        {
            var command = new CompleteJobInProgressCommand(jobInProgressId, request.CompletitionType, request.Note);
            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.JobInProgress)
        .RequireAuthorization();

        app.MapGet("job/{jobId}/items", async (
            ISender sender,
            [FromRoute] long jobId,
            [AsParameters] BasePagedRequest request,
            CancellationToken cancellationToken) =>
        {
            var query = new GetJobItemsQuery
            {
                JobId = jobId,
                SearchTerm = request.SearchTerm,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                OrderBy = request.OrderBy,
                IsDescending = request.IsDescending
            };
            var result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Job)
        .RequireAuthorization();

        app.MapPost("job-in-progress/{jobInProgressId}/job-item/{jobItemId}/complete", async (
            ISender sender,
            [FromRoute] long jobInProgressId,
            [FromRoute] long jobItemId,
            [FromBody] CompleteJobItemRequest request,
            CancellationToken cancellationToken) =>
        {
            var command = new CompleteJobItemCommand(jobInProgressId, jobItemId, request.EnteredQuantity, request.Status);
            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.JobInProgress)
        .RequireAuthorization();
    }
}