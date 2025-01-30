namespace Domain.Models;

public record RefreshToken
{
    public string Value { get; init; } = null!;
}

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = null!;
}
