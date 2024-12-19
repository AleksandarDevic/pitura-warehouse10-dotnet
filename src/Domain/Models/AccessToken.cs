namespace Domain.Models;

public record AccessToken
{
    public required string Value { get; init; }
    public required DateTime Expires { get; init; }
}
