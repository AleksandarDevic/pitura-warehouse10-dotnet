using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Web.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? OperatorTerminalId => httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
