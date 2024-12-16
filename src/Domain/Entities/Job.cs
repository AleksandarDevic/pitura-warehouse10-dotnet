namespace Domain.Entities;
public class Job
{
    public long Id { get; set; }

    public string? Description { get; set; }

    public short? AssignedOperatorId { get; set; }
    public virtual Operator? AssignedOperator { get; set; }

    public byte Type { get; set; }

    public DateTime CreationDateTime { get; set; }

    public DateTime? DueDateTime { get; set; }

    public string? TakenOverByOperatorName { get; set; }

    public string? CompletedByOperatorName { get; set; }

    public byte CompletionType { get; set; }

    public string? LastNote { get; set; }

    public byte Field1Length { get; set; }

    public bool IsField1Required { get; set; }

    public byte Field2Length { get; set; }

    public bool IsField2Required { get; set; }

    public bool IsField3Required { get; set; }

    public byte ReadingType { get; set; }

    public byte ClosingType { get; set; }

    public int? LidderDocumentNumber { get; set; }

    public byte? LidderDocumentType { get; set; }

    public int? InventoryId { get; set; }

    public bool IsVerified { get; set; }

    public string? Client { get; set; }

    // Navigation properties
    public virtual ICollection<JobInProgress> JobsInProgress { get; set; } = [];

    public virtual ICollection<JobItem> JobItems { get; set; } = [];
}
