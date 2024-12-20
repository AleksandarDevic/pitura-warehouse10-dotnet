namespace Domain.Models;

public record OperatorResponse
{
    public short Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
}
