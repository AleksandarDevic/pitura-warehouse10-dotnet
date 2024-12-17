using System.Security.Claims;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Login;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure.Authentication.Jwt;

public class JwtProvider(IDateTimeProvider dateTimeProvider, IOptions<JwtOptions> options) : IJwtProvider
{
    public readonly JwtOptions _options = options.Value;

    public JwtResponse Create(int operatorId, int terminalId)
    {
        var secretKey = _options.Secret;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claimTypeTerminal = "TerminalId";
        var claimTypeOperator = "OperatorId";
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
               new Claim(JwtRegisteredClaimNames.Sub,$"{claimTypeTerminal}: {terminalId} - {claimTypeOperator}: {operatorId}"),
               new Claim(claimTypeTerminal,$"{terminalId}"),
               new Claim(claimTypeOperator,$"{operatorId}"),
            ]),
            Expires = dateTimeProvider.UtcNow.AddMinutes(_options.AccessTokenExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _options.Issuer,
            Audience = _options.Audience
        };

        var handler = new JsonWebTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        var result = new JwtResponse
        {
            AccessToken = token,
            Expires = (DateTime)tokenDescriptor.Expires
        };

        return result;
    }
}
