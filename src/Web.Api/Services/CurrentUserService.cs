using System.Security.Claims;
using Application.Abstractions.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Web.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int OperatorTerminalId => int.Parse(httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    public short OperatorId => short.Parse(httpContextAccessor.HttpContext?.User?.FindFirstValue("OperatorId")!);

    public int TerminalId => int.Parse(httpContextAccessor.HttpContext?.User?.FindFirstValue("TerminalId")!);
}
