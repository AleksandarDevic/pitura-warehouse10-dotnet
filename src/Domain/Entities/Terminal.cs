namespace Domain.Entities;
public class Terminal
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }

    // Navigation property
    public virtual ICollection<OperatorTerminal> AssignedOperators { get; set; } = [];
}
