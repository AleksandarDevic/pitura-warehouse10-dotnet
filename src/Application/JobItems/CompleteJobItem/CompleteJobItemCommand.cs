using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.JobItems.CompleteJobItem;

public record CompleteJobItemCommand : ICommand
{
    public long JobInProgressId { get; init; }
    public long JobItemId { get; init; }
    public double EnteredQuantity { get; init; }
    public JobItemStatus Status { get; init; }
    public string Note { get; init; } = string.Empty;
}
