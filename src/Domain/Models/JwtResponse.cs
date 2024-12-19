namespace Domain.Models;

public record JwtResponse
{
    public AccessToken AccessToken { get; init; } = null!;
    public RefreshToken RefreshToken { get; init; } = null!;
}
