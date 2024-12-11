namespace Domain.Entities;

public class JobItem
{
    public long Id { get; set; }

    public long JobId { get; set; }
    public virtual Job Job { get; set; } = null!;

    public string? ItemDescription { get; set; }
    public string? RequiredField1 { get; set; }
    public string? RequiredField2 { get; set; }
    public double? RequiredField3 { get; set; }

    public int? JobInProgressId { get; set; }
    public virtual JobInProgress? JobInProgress { get; set; }

    public double? ReadedField3 { get; set; }
    public byte? ItemStatus { get; set; }
}
