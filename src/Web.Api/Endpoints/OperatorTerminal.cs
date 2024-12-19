using Application.Login;
using Application.Logout;
using Domain.Entities;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Authorization;
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
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<JwtResponse> result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                var refreshToken = result.Value.RefreshToken;
                httpContext.Response.Cookies.Append(HttpContextItemKeys.RefreshTokenCookie, refreshToken.Value, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = refreshToken.Expires,
                    SameSite = SameSiteMode.None,
                    Secure = true
                });
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("TerminalOperator");

        app.MapPost("terminal-operator/logout", async (
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var refreshToken = httpContext.Request.Cookies[HttpContextItemKeys.RefreshTokenCookie];
            if (refreshToken is null)
                return Results.Unauthorized();

            int operatorTerminalId = int.Parse(refreshToken);

            var command = new LogoutCommand(operatorTerminalId);
            Result result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
                httpContext.Response.Cookies.Delete(HttpContextItemKeys.RefreshTokenCookie);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("TerminalOperator")
        .RequireAuthorization();
    }

}