namespace Domain.Models;

public class JobResponse
{
    public long Id { get; set; }

    public string? Description { get; set; }

    public short? AssignedOperatorId { get; set; }

    public DateTime CreationDateTime { get; set; }

    public DateTime? DueDateTime { get; set; }
}
