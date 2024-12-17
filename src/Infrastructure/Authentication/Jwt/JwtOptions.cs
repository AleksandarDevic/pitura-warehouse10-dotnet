namespace Infrastructure.Authentication.Jwt;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Secret { get; init; }
    public required int AccessTokenExpirationInMinutes { get; init; }
    public required int RefreshTokenExpirationInDays { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}
