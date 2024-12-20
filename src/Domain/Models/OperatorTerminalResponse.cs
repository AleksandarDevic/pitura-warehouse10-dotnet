namespace Domain.Models;

public record OperatorTerminalResponse
{
    public int Id { get; set; }

    public short OperatorId { get; set; }
    public virtual OperatorResponse Operator { get; set; } = null!;

    public int TerminalId { get; set; }
    public virtual TerminalResponse Terminal { get; set; } = null!;
}
