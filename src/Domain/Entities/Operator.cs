namespace Domain.Entities;
public class Operator
{
    public short Id { get; set; }
    public required string Name { get; set; }
    public required string Password { get; set; }
    public bool IsActive { get; set; }

    // Navigation property
    public virtual ICollection<OperatorTerminal>? AssignedTerminals { get; set; }
    public virtual ICollection<Job>? Jobs { get; set; }
}
