using System.Security.Claims;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Login;
using Domain.Entities;
using Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure.Authentication.Jwt;

public class JwtProvider(IDateTimeProvider dateTimeProvider, IOptions<JwtOptions> options) : IJwtProvider
{
    public readonly JwtOptions _options = options.Value;

    public JwtResponse Create(OperatorTerminal operatorTerminal)
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
               new Claim(JwtRegisteredClaimNames.Sub, $"{operatorTerminal.Id}"),
               new Claim(claimTypeTerminal, $"{operatorTerminal.TerminalId}"),
               new Claim(claimTypeOperator, $"{operatorTerminal.OperatorId}"),
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
            AccessToken = new AccessToken
            {
                Value = token,
                Expires = (DateTime)tokenDescriptor.Expires
            },
            RefreshToken = new RefreshToken
            {
                Value = operatorTerminal.Id.ToString(),
                Expires = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationInDays)
            }
        };

        return result;
    }
}
