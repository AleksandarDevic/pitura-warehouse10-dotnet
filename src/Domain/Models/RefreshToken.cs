namespace Domain.Models;

public record RefreshToken
{
    public string Value { get; init; } = null!;
    public required DateTime Expires { get; init; }
}
