namespace Application.Login;

public record JwtResponse
{
    public required string AccessToken { get; init; }
    public required DateTime Expires { get; init; }
}
