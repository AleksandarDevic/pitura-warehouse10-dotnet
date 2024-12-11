namespace Domain.Entities;
public class OperatorTerminal
{
    public int Id { get; set; }

    public int OperatorId { get; set; }
    public virtual Operator Operator { get; set; } = null!;

    public int TerminalId { get; set; }
    public virtual Terminal Terminal { get; set; } = null!;

    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }

    // Navigation property
    public virtual ICollection<JobInProgress>? JobsInProgess { get; set; }
}
