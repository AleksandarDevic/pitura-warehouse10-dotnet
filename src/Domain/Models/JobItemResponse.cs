using Domain.Enums;

namespace Domain.Models;

public record JobItemResponse
{
    public long Id { get; set; }

    public long JobId { get; set; }

    public string? ItemDescription { get; set; }
    public string? RequiredField1 { get; set; }
    public string? RequiredField2 { get; set; }
    public double? RequiredField3 { get; set; }

    public int? JobInProgressId { get; set; }

    public double? ReadedField3 { get; set; }
    public JobItemStatus ItemStatus { get; set; }
}
