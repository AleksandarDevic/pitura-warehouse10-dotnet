namespace Domain.Models;

public record RefreshToken
{
    public string Value { get; init; } = null!;
}
