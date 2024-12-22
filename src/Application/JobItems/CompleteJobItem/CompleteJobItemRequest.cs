using Domain.Enums;

namespace Application.JobItems.CompleteJobItem;

public record CompleteJobItemRequest
{
    public long JobInProgressId { get; init; }
    public long JobItemId { get; init; }
    public double EnteredQuantity { get; init; }
    public JobItemStatus Status { get; init; }
}
