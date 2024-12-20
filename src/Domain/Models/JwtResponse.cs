namespace Domain.Models;

public record JwtResponse
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
}
