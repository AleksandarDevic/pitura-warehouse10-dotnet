using Domain.Enums;

namespace Application.JobItems.CompleteJobItem;

public record CompleteJobItemRequest
{
    public string RequiredFieldRead1 { get; set; } = null!;
    public string RequiredFieldRead2 { get; set; } = null!;
    public double RequiredFieldRead3 { get; init; }
    public JobItemStatus Status { get; init; }
}
