using Application.Admin.LogoutJob;
using Application.Login;
using Application.Logout;
using Application.OperatorTerminals.GetActiveTerminals;
using Application.OperatorTerminals.GetOperatorTerminalDetails;
using Application.TokenRefresh;
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
        app.MapPost("admin/prerequisite/logout-job", async (
            [FromBody] LogoutJobCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Admin);

        app.MapGet("terminal/all", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetActiveTerminalsQuery(), cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Terminal);

        app.MapPost("operator-terminal/login", async (
            [FromBody] LoginCommand command,
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                var refreshToken = result.Value.RefreshToken;
                SetRefreshTokenCookie(httpContext, refreshToken);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.OperatorTerminal);

        app.MapPost("operator-terminal/logout", async (
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var refreshToken = httpContext.Request.Cookies[HttpContextItemKeys.RefreshTokenCookie];
            if (refreshToken is null)
                return Results.Unauthorized();

            int operatorTerminalId = int.Parse(refreshToken);

            var command = new LogoutCommand(operatorTerminalId, true);
            Result result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
                DeleteRefreshTokenCookie(httpContext);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.OperatorTerminal);

        app.MapPost("operator-terminal/logout-expired-token", async (
            HttpContext httpContext,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var refreshToken = httpContext.Request.Cookies[HttpContextItemKeys.RefreshTokenCookie];
            if (refreshToken is null)
                return Results.NoContent();

            int operatorTerminalId = int.Parse(refreshToken);

            var command = new LogoutCommand(operatorTerminalId, false);
            Result result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
                DeleteRefreshTokenCookie(httpContext);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.OperatorTerminal);

        app.MapPost("operator-terminal/token-refresh", async (
            HttpContext httpContext,
            ISender sender,
            [FromBody] string? refreshToken,
            CancellationToken cancellationToken) =>
        {
                return Results.Unauthorized();
            // var refreshTokenFromCookie = httpContext.Request.Cookies[HttpContextItemKeys.RefreshTokenCookie];
            // if (refreshTokenFromCookie is not null)
            //     refreshToken = refreshTokenFromCookie;

            // if (refreshToken is null)
            //     return Results.Unauthorized();

            // int operatorTerminalId = int.Parse(refreshToken);

            // var command = new TokenRefreshCommand(operatorTerminalId);
            // var result = await sender.Send(command, cancellationToken);

            // if (result.IsSuccess)
            // {
            //     DeleteRefreshTokenCookie(httpContext);
            //     SetRefreshTokenCookie(httpContext, result.Value.RefreshToken);
            // }

            // return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.OperatorTerminal);

        app.MapGet("operator-terminal/details", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOperatorTerminalDetailsQuery();
            var result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.OperatorTerminal)
        .RequireAuthorization();
    }

    private static void DeleteRefreshTokenCookie(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(HttpContextItemKeys.RefreshTokenCookie);
    }

    private static void SetRefreshTokenCookie(HttpContext httpContext, RefreshToken refreshToken)
    {
        httpContext.Response.Cookies.Append(HttpContextItemKeys.RefreshTokenCookie, refreshToken.Value, new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(1),
            SameSite = SameSiteMode.None,
            Secure = true
        });
    }
}