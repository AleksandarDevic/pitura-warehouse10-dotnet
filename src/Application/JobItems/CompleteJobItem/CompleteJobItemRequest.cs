using Domain.Enums;

namespace Application.JobItems.CompleteJobItem;

public record CompleteJobItemRequest
{
    public double EnteredQuantity { get; init; }
    public JobItemStatus Status { get; init; }
}
