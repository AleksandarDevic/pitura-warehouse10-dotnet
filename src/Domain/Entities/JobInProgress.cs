namespace Domain.Entities;
public class JobInProgress
{
    public int Id { get; set; }

    public long JobId { get; set; }
    public virtual Job Job { get; set; } = null!;

    public int OperatorTerminalId { get; set; }
    public virtual OperatorTerminal OperatorTerminal { get; set; } = null!;

    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }

    public byte CompletionType { get; set; }
    public string? Note { get; set; }

    // Navigation properties
    public virtual ICollection<JobItem> JobItems { get; set; } = null!;
}
