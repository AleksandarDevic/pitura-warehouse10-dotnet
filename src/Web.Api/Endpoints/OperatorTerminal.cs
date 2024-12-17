using Application.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints;

public class OperatorTerminal : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {

        app.MapGet("/", () => "Hello World!"); // => is showing

        app.MapPost("terminal-operator/login", async (
            [FromBody] LoginCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<JwtResponse> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("TerminalOperator");

    }
}