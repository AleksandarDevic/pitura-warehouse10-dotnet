namespace Domain.Models;

public record TerminalResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
}
